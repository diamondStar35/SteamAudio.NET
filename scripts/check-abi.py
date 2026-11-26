"""Compare native compiler ABI facts with managed unsafe sizeof/field addresses. Native compilation is CI-only."""
import argparse, json, os, pathlib, shutil, subprocess, sys
root=pathlib.Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--target',required=True);p.add_argument('--linkage',required=True);args=p.parse_args()
if os.environ.get('GITHUB_ACTIONS')!='true':raise SystemExit('Native probe compilation is restricted to CI')
out=root/'artifacts'/args.target/args.linkage;out.mkdir(parents=True,exist_ok=True)
managed=out/'managed-abi.json'
subprocess.run(['dotnet','run','--project',str(root/'Samples/Offline/Offline.csproj'),'-c','Release','--','--abi',str(managed)],check=True)
actual=json.loads(managed.read_text())
include=root/'Native/include'
if args.target in ['ios-arm64','android-arm64']:
    lines=['#include <stddef.h>','#include "phonon.h"']
    for name,layout in actual['structures'].items():
        lines.append('_Static_assert(sizeof('+name+') == '+str(layout['size'])+', "'+name+' size");')
        for field,offset in layout['fields'].items():lines.append('_Static_assert(offsetof('+name+', '+field+') == '+str(offset)+', "'+name+'.'+field+'");')
    source=out/'abi-assert.c';source.write_text('\n'.join(lines)+'\n')
    if args.target=='ios-arm64':
        sdk=subprocess.check_output(['xcrun','--sdk','iphoneos','--show-sdk-path'],text=True).strip()
        command=['xcrun','--sdk','iphoneos','clang','-std=c11','-arch','arm64','-isysroot',sdk,'-miphoneos-version-min=15.0']
    else:
        ndk=os.environ.get('ANDROID_NDK_HOME') or os.environ.get('ANDROID_NDK_ROOT')
        if not ndk:
            sdk=pathlib.Path(os.environ.get('ANDROID_HOME',os.environ.get('ANDROID_SDK_ROOT','/usr/local/lib/android/sdk')))
            ndk=str(sorted((sdk/'ndk').iterdir(),key=lambda path:tuple(int(x) for x in path.name.split('.')))[-1])
        command=[str(pathlib.Path(ndk)/'toolchains/llvm/prebuilt/linux-x86_64/bin/aarch64-linux-android26-clang'),'-std=c11']
    subprocess.run(command+['-I'+str(include),'-c',str(source),'-o',str(out/'abi.o')],check=True)
    if args.linkage=='static':
        probe=out/'mobile-link.c'
        probe.write_text('#include <stddef.h>\n#include "phonon.h"\nint main(void) { IPLContextSettings settings = {0}; IPLContext context = 0; settings.version = STEAMAUDIO_VERSION; IPLerror result = iplContextCreate(&settings, &context); if (context) iplContextRelease(&context); return result; }\n')
        libraries=list((root/'Native/static'/args.target).glob('*.a'))
        if not libraries:raise SystemExit('No mobile static archives to verify')
        link=command+['-I'+str(include),str(probe)]
        if args.target=='android-arm64':link+=['-Wl,--start-group']+list(map(str,libraries))+['-Wl,--end-group','-lc++_static','-lc++abi','-lunwind','-latomic','-llog','-landroid','-lm','-ldl']
        else:link+=list(map(str,libraries))+['-lc++']
        subprocess.run(link+['-o',str(out/'mobile-link')],check=True)
    (out/'abi-result.txt').write_text('All 64 structures and all field offsets matched the mobile target compiler. Static selections also linked the native context/dependency closure. Cross-compiled; not device-executed.\n')
else:
    executable=out/('abi.exe' if args.target.startswith('win-') else 'abi')
    command=['cl','/nologo','/std:c11','/I'+str(include),str(root/'Verification/abi.c'),'/Fe:'+str(executable),'/Fo:'+str(out/'abi.obj')] if args.target.startswith('win-') else ['cc','-std=c11','-I'+str(include),str(root/'Verification/abi.c'),'-o',str(executable)]
    subprocess.run(command,check=True)
    native=json.loads(subprocess.check_output([str(executable)],text=True))
    (out/'native-abi.json').write_text(json.dumps(native,indent=2))
    if native!=actual:
        for name in actual['structures']:
            if actual['structures'][name]!=native['structures'].get(name):print(name,actual['structures'][name],native['structures'].get(name))
        raise SystemExit('Native/managed ABI mismatch')
    print('All 64 structures and field offsets matched the native compiler.')
