"""Publish, inspect and execute a NativeAOT consumer. Static mode must have no Phonon shared dependency."""
import argparse,json,os,pathlib,shutil,subprocess,tempfile
root=pathlib.Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--target',required=True);p.add_argument('--static',action='store_true');args=p.parse_args()
mode='static' if args.static else 'dynamic';out=root/'artifacts'/args.target/(mode+'-aot')
command=['dotnet','publish',str(root/'Samples/Offline/Offline.csproj'),'-c','Release','-r',args.target,'-p:PublishAot=true','-p:SteamAudioStaticLink='+str(args.static).lower(),'-o',str(out)]
subprocess.run(command,check=True)
exe=out/('Offline.exe' if args.target.startswith('win-') else 'Offline')
inspect=['dumpbin','/dependents',str(exe)] if args.target.startswith('win-') else ['otool','-L',str(exe)] if args.target.startswith('osx-') else ['ldd',str(exe)]
deps=subprocess.check_output(inspect,text=True);(out/'dependencies.txt').write_text(deps)
if args.static:
    if 'phonon' in deps.lower() or (out/'lib').exists() and any('phonon' in f.name.lower() for f in (out/'lib').iterdir()):raise SystemExit('Static executable retains a Phonon shared dependency')
    clean=pathlib.Path(tempfile.mkdtemp(prefix='steamaudio-static-'));shutil.copy2(exe,clean/exe.name)
    subprocess.run([str(clean/exe.name),str(out/'rendered.wav')],cwd=clean,check=True)
else:subprocess.run([str(exe),str(out/'rendered.wav')],cwd=out,check=True)
print('NativeAOT '+mode+' execution and dependency checks passed.')
