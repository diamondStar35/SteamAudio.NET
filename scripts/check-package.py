"""Verify a packed consumer without a project reference or manually imported build targets."""
import argparse, hashlib, json, pathlib, shutil, subprocess, tempfile, zipfile
from xml.sax.saxutils import escape

root = pathlib.Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument('--package', type=pathlib.Path, required=True)
parser.add_argument('--target', required=True)
args = parser.parse_args()
package = args.package.resolve()
with zipfile.ZipFile(package) as archive:
    names = set(archive.namelist())
    # Prove the archives and shared objects survived artifact merging and packing unchanged.
    manifests = [name for name in names if name.endswith('/manifest.json') and name.startswith(('native/', 'runtimes/'))]
    for name in manifests:
        manifest = json.loads(archive.read(name))
        for filename, digest in manifest['files'].items():
            data = archive.read(str(pathlib.PurePosixPath(name).parent / filename))
            if hashlib.sha256(data).hexdigest() != digest:
                raise SystemExit('Packaged native hash mismatch: ' + filename)
    if not manifests:
        raise SystemExit('No verified native assets in the package')
    version = package.name.removeprefix('SteamAudio.NET.').removesuffix('.nupkg')
    modes = []
    if f'native/{args.target}/Link.targets' in names:
        modes.append('static')
    if any(name.startswith(f'runtimes/{args.target}/native/') for name in names):
        modes.append('dynamic')

if not modes:
    print(f'Native hashes verified. {args.target} consumer omitted: target was not selected.')
    raise SystemExit(0)

consumer = pathlib.Path(tempfile.mkdtemp(prefix='steamaudio-package-'))
(consumer/'NuGet.Config').write_text(f'''<configuration><packageSources><clear/>
  <add key="verified-local-package" value="{escape(str(package.parent), {'"': '&quot;'})}"/>
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json"/>
</packageSources></configuration>''')
shutil.copy2(root/'Samples/Offline/Program.cs', consumer/'Program.cs')
shutil.copy2(root/'Verification/AbiMeasurements.cs', consumer/'AbiMeasurements.cs')
(consumer/'Consumer.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <IsAotCompatible>true</IsAotCompatible><WarningsAsErrors>IL2026;IL3050</WarningsAsErrors>
  </PropertyGroup>
  <ItemGroup><PackageReference Include="SteamAudio.NET" Version="{version}" /></ItemGroup>
</Project>
''')

def run(command, **options):
    subprocess.run(list(map(str, command)), cwd=consumer, check=True, **options)

for mode in modes:
    output = root/'artifacts/package-verification'/args.target/mode
    run(['dotnet', 'publish', consumer/'Consumer.csproj', '-c', 'Release', '-r', args.target,
         '-p:PublishAot=true', '-p:SteamAudioStaticLink='+str(mode=='static').lower(),
         '-o', output])
    executable = output/('Consumer.exe' if args.target.startswith('win-') else 'Consumer')
    inspect = ['dumpbin', '/dependents'] if args.target.startswith('win-') else ['otool', '-L'] if args.target.startswith('osx-') else ['ldd']
    dependencies = subprocess.check_output([*inspect, str(executable)], text=True)
    (output/'dependencies.txt').write_text(dependencies)
    if mode == 'static':
        if 'phonon' in dependencies.lower():
            raise SystemExit('Packaged static consumer retains a Phonon shared dependency')
        isolated = consumer/'isolated'; isolated.mkdir(exist_ok=True)
        shutil.copy2(executable, isolated/executable.name)
        run([isolated/executable.name, output/'rendered.wav'])
    else:
        run([executable, output/'rendered.wav'])
print(f'Package hashes and {args.target} NativeAOT consumers passed: {", ".join(modes)}.')
