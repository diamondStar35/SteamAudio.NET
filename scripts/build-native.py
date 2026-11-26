"""CI-only native builder. Resolve the complete static dependency closure with pinned vcpkg ports."""
import argparse, hashlib, json, os, pathlib, platform, shutil, subprocess, xml.sax.saxutils as xml
root=pathlib.Path(__file__).resolve().parents[1]
spec=json.loads((root/'Native/sdk.json').read_text())
p=argparse.ArgumentParser();p.add_argument('--target',choices=spec['targets'],required=True);p.add_argument('--linkage',choices=['static','dynamic'],required=True);args=p.parse_args()
if os.environ.get('GITHUB_ACTIONS')!='true': raise SystemExit('Native compilation is restricted to GitHub Actions. Use fetch-sdk.py for local development.')
if args.target=='ios-arm64' and args.linkage=='dynamic': raise SystemExit('iOS supports static linking only.')
def run(*argv,**kw): subprocess.run(list(map(str,argv)),check=True,**kw)
vcpkg=root/'Native/vcpkg'
if not (vcpkg/'.git').exists():
    vcpkg.mkdir(parents=True,exist_ok=True)
    run('git','-C',vcpkg,'init')
    run('git','-C',vcpkg,'remote','add','origin','https://github.com/microsoft/vcpkg.git')
run('git','-C',vcpkg,'fetch','--depth=1','origin',spec['vcpkg'])
run('git','-C',vcpkg,'checkout','--detach','FETCH_HEAD')
windows=platform.system()=='Windows'
run('cmd.exe','/d','/c',vcpkg/'bootstrap-vcpkg.bat','-disableMetrics') if windows else run('sh',vcpkg/'bootstrap-vcpkg.sh','-disableMetrics')
arch,osname=args.target.split('-')[-1],args.target.split('-')[0]
triplets=root/'.cache/triplets';triplets.mkdir(parents=True,exist_ok=True)
triplet=args.target+'-'+args.linkage
lines=['set(VCPKG_TARGET_ARCHITECTURE '+('x64' if arch=='x64' else 'arm64')+')','set(VCPKG_CRT_LINKAGE '+('static' if windows else 'dynamic')+')','set(VCPKG_LIBRARY_LINKAGE static)','set(VCPKG_BUILD_TYPE release)','set(VCPKG_CMAKE_CONFIGURE_OPTIONS -DCMAKE_POSITION_INDEPENDENT_CODE=ON)']
if args.linkage=='dynamic':lines+=['if(PORT STREQUAL "steam-audio")','  set(VCPKG_LIBRARY_LINKAGE dynamic)','endif()']
if osname in ['linux','android','ios','osx']:lines+=['set(VCPKG_CMAKE_SYSTEM_NAME '+{'linux':'Linux','android':'Android','ios':'iOS','osx':'Darwin'}[osname]+')']
if osname=='osx':lines+=['set(VCPKG_OSX_DEPLOYMENT_TARGET 12.0)','set(VCPKG_OSX_ARCHITECTURES '+('x86_64' if arch=='x64' else 'arm64')+')']
if osname=='ios':lines+=['set(VCPKG_OSX_DEPLOYMENT_TARGET 15.0)']
if osname=='android':lines+=['set(VCPKG_CMAKE_SYSTEM_VERSION 26)','set(VCPKG_CMAKE_CONFIGURE_OPTIONS "-DANDROID_ABI=arm64-v8a" "-DANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON" "-DCMAKE_POSITION_INDEPENDENT_CODE=ON")']
(triplets/(triplet+'.cmake')).write_text('\n'.join(lines)+'\n')
if osname=='android' and not os.environ.get('ANDROID_NDK_HOME'):
    sdk=pathlib.Path(os.environ.get('ANDROID_HOME',os.environ.get('ANDROID_SDK_ROOT','/usr/local/lib/android/sdk')))
    os.environ['ANDROID_NDK_HOME']=str(sorted((sdk/'ndk').iterdir(),key=lambda path:tuple(int(x) for x in path.name.split('.')))[-1])
binary_cache=root/'.cache/vcpkg-binary-cache';binary_cache.mkdir(parents=True,exist_ok=True)
os.environ['VCPKG_DEFAULT_BINARY_CACHE']=str(binary_cache)
install=root/'.cache/vcpkg-installed'/triplet
run(vcpkg/('vcpkg.exe' if windows else 'vcpkg'),'install','--triplet='+triplet,'--overlay-triplets='+str(triplets),'--x-manifest-root='+str(root/'Native'),'--x-install-root='+str(install))
installed=install/triplet
if args.linkage=='static':
    output=root/'Native/static'/args.target;output.mkdir(parents=True,exist_ok=True)
    # GNU linkers scan archives from left to right. Phonon must precede its FFT/SOFA
    # dependencies; libmysofa in turn precedes zlib. Filesystem enumeration is not an order.
    libraries=sorted((installed/'lib').glob('*.lib' if windows else '*.a'),key=lambda file:('phonon' not in file.name,file.name))
    if not any('phonon' in file.name for file in libraries): raise RuntimeError('Static phonon archive missing')
    for library in libraries:shutil.copy2(library,output/library.name)
    items=['<NativeLibrary Include="$(MSBuildThisFileDirectory)'+xml.escape(file.name)+'" />' for file in libraries]
    if osname=='ios':items+=['<NativeReference Include="$(MSBuildThisFileDirectory)'+xml.escape(file.name)+'" Condition="&apos;$(PublishAot)&apos; != &apos;true&apos;"><Kind>Static</Kind><ForceLoad>true</ForceLoad></NativeReference>' for file in libraries]
    if osname=='win':items+=['<NativeLibrary Include="delayimp.lib" />']
    elif osname=='linux':items+=['<NativeSystemLibrary Include="'+lib+'" />' for lib in ['stdc++','m','dl','pthread']]
    elif osname in ['ios','osx']:items+=['<NativeSystemLibrary Include="c++" />']
    elif osname=='android':items+=['<NativeSystemLibrary Include="'+lib+'" />' for lib in ['log','android','c++_static','c++abi','unwind','atomic']]
    (output/'Link.targets').write_text('<Project><ItemGroup>'+''.join(items)+'</ItemGroup></Project>')
else:
    output=root/'Native/runtimes'/args.target/'native';output.mkdir(parents=True,exist_ok=True)
    for folder in ['bin','lib']:
        for file in (installed/folder).glob('*'):
            if file.suffix in ['.dll','.dylib','.so']:shutil.copy2(file,output/file.name)
    if not any('phonon' in file.name for file in output.iterdir()):raise RuntimeError('Shared phonon library missing')
notices=root/'Native/notices'/args.target;notices.mkdir(parents=True,exist_ok=True)
for file in (installed/'share').rglob('copyright'):
    shutil.copy2(file,notices/(file.parent.name+'-copyright.txt'))
(output/'manifest.json').write_text(json.dumps({'sdk':spec,'target':args.target,'linkage':args.linkage,'patches':{file.name:hashlib.sha256(file.read_bytes()).hexdigest() for file in (root/'Native/ports/steam-audio').glob('*.patch')},'dependency_status':(installed/'vcpkg/status').read_text() if (installed/'vcpkg/status').exists() else '', 'triplet':(triplets/(triplet+'.cmake')).read_text(),'files':{f.name:hashlib.sha256(f.read_bytes()).hexdigest() for f in output.iterdir() if f.is_file()}},indent=2)+'\n')
print('Native artifact: '+str(output))

if args.linkage=='static' and args.target not in ['ios-arm64','android-arm64']:
    sources=list((vcpkg/'buildtrees/steam-audio/src').rglob('array_math.h'))
    if not sources:raise RuntimeError('Native source for numerical regression is missing')
    probe=root/'.cache/simd-probe';probe.mkdir(parents=True,exist_ok=True)
    cmake=['cmake_minimum_required(VERSION 3.20)','project(SimdProbe LANGUAGES CXX)','set(CMAKE_CXX_STANDARD 14)','set(CMAKE_MSVC_RUNTIME_LIBRARY MultiThreaded)','add_executable(simd "'+(root/'Verification/simd.cpp').as_posix()+'")','target_include_directories(simd PRIVATE "'+sources[0].parent.as_posix()+'")']
    cmake+=['target_link_libraries(simd PRIVATE '+' '.join('"'+file.as_posix()+'"' for file in (output).glob('*.lib' if windows else '*.a'))+')']
    if osname=='linux':cmake+=['target_link_libraries(simd PRIVATE stdc++ m dl pthread)']
    elif osname=='win':cmake+=['target_link_libraries(simd PRIVATE delayimp)']
    (probe/'CMakeLists.txt').write_text('\n'.join(cmake)+'\n')
    run('cmake','-S',probe,'-B',probe/'build','-DCMAKE_BUILD_TYPE=Release')
    run('cmake','--build',probe/'build','--config','Release')
    candidates=list((probe/'build').rglob('simd.exe' if windows else 'simd'))
    executable=next(file for file in candidates if file.is_file())
    run(executable)
