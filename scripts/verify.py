"""Check declarations, native layouts and published consumers without modifying source files."""

import argparse
import hashlib
import json
import os
import pathlib
import re
import shutil
import subprocess
import tempfile
from textwrap import dedent
import zipfile
import xml.etree.ElementTree as xml
from xml.sax.saxutils import escape

from build import CONFIGURATIONS, ROOT, SDK


def verify_api():
    """Compare all 216 signatures, then parse each XML documentation block."""
    header = (ROOT / "Native/include/phonon.h").read_text(encoding="utf-8")
    clean = re.sub("/\\*[\\s\\S]*?\\*/|//[^\\n]*", "", header)
    handles = set(re.findall("DECLARE_OPAQUE_HANDLE\\((IPL\\w+)\\);", clean))
    callbacks = {
        name: (ret, args)
        for ret, name, args in re.findall(
            "typedef\\s+(\\w+\\s*\\*?)\\s*\\(IPLCALL\\s*\\*\\s*(IPL\\w+)\\)\\((.*?)\\);",
            clean,
            re.S,
        )
    }

    def name(s):
        return s.removeprefix("IPL").replace("HRTF", "Hrtf").replace("SIMD", "Simd")

    primitives = {
        "void": "void",
        "char": "byte",
        "int": "int",
        "float": "float",
        "IPLint8": "sbyte",
        "IPLuint8": "byte",
        "IPLint16": "short",
        "IPLuint16": "ushort",
        "IPLint32": "int",
        "IPLuint32": "uint",
        "IPLint64": "long",
        "IPLuint64": "ulong",
        "IPLfloat32": "float",
        "IPLfloat64": "double",
        "IPLbyte": "byte",
        "IPLsize": "nuint",
        "IPLstring": "byte*",
        "IPLbool": "Bool",
        "IPLerror": "Error",
    }

    def ctype(s):
        s = re.sub("\\bconst\\b|\\bvolatile\\b", "", s).strip()
        stars = s.count("*")
        s = s.replace("*", "").strip()
        if s in callbacks:
            ret, args = callbacks[s]
            params = (
                []
                if args.strip() in ["", "void"]
                else [
                    ctype(re.sub("\\b\\w+\\s*$", "", x).strip())
                    for x in args.split(",")
                ]
            )
            return "delegate*unmanaged<" + ",".join(params + [ctype(ret)]) + ">"
        return (
            primitives.get(s, name(s) + ("Handle" if s in handles else ""))
            + "*" * stars
        )

    native = {
        n: (
            ctype(ret),
            (
                [ctype(re.sub("\\b\\w+\\s*$", "", p).strip()) for p in args.split(",")]
                if args.strip() not in ["", "void"]
                else []
            ),
        )
        for ret, n, args in re.findall(
            "IPLAPI\\s+([\\w* ]+?)\\s+IPLCALL\\s+(ipl\\w+)\\s*\\((.*?)\\);", clean, re.S
        )
    }
    source = "\n".join(
        p.read_text(encoding="utf-8") for p in (ROOT / "Src/Interop").glob("*.cs")
    )
    managed = {}

    def split(args):
        values = []
        start = 0
        depth = 0
        for i, char in enumerate(args):
            if char == "<":
                depth += 1
            elif char == ">":
                depth -= 1
            elif char == "," and depth == 0:
                values.append(args[start:i])
                start = i + 1
        if args.strip():
            values.append(args[start:])
        return values

    for entry, ret, args in re.findall(
        '\\[LibraryImport\\(Library, EntryPoint = "(ipl\\w+)"\\)\\]\\s*public static partial (.*?) \\w+\\((.*?)\\);',
        source,
        re.S,
    ):
        managed[entry] = (
            ret.strip(),
            [
                re.sub("\\s+", "", re.sub("\\s+@?\\w+$", "", p.strip()))
                for p in split(args)
            ],
        )
    if native != managed:
        for key in sorted(native.keys() | managed.keys()):
            if native.get(key) != managed.get(key):
                print(key, "SDK:", native.get(key), "C#:", managed.get(key))
        raise SystemExit("Native API signature mismatch")
    if len(native) != 216:
        raise SystemExit(
            "Unexpected SDK API surface; update the audited version explicitly"
        )
    for file in (ROOT / "Src").rglob("*.cs"):
        if "obj" in file.parts or "bin" in file.parts:
            continue
        docs = "\n".join(
            (
                line.split("///", 1)[1]
                for line in file.read_text(encoding="utf-8").splitlines()
                if "///" in line
            )
        )
        xml.fromstring("<documentation>" + docs + "</documentation>")
        if re.search("\\\\[A-Za-z]+|@(param|return|brief|remarks|c)\\b", docs):
            raise SystemExit("Unconverted Doxygen in " + str(file))
    print("216 native signatures match SDK headers; XML documentation is well formed.")


def verify_abi(target, linkage, configuration="Release"):
    """Execute desktop layout probes or cross-compile mobile layout assertions."""
    if os.environ.get("GITHUB_ACTIONS") != "true":
        raise SystemExit("Native probe compilation is restricted to CI")
    out = ROOT / "artifacts" / target / configuration / linkage
    out.mkdir(parents=True, exist_ok=True)
    managed = out / "managed-abi.json"
    subprocess.run(
        [
            "dotnet",
            "run",
            "--project",
            str(ROOT / "Samples/Offline/Offline.csproj"),
            "-c",
            configuration,
            "--",
            "--abi",
            str(managed),
        ],
        check=True,
    )
    actual = json.loads(managed.read_text(encoding="utf-8"))
    include = ROOT / "Native/include"
    if target in ["ios-arm64", "android-arm64"]:
        lines = ["#define __DOXYGEN__ 1", "#include <stddef.h>", '#include "phonon.h"']
        for name, layout in actual["structures"].items():
            lines.append(
                f'_Static_assert(sizeof({name}) == {layout["size"]}, "{name} size");'
            )
            for field, offset in layout["fields"].items():
                lines.append(
                    f'_Static_assert(offsetof({name}, {field}) == {offset}, "{name}.{field}");'
                )
        source = out / "abi-assert.c"
        source.write_text("\n".join(lines) + "\n")
        if target == "ios-arm64":
            sdk = subprocess.check_output(
                ["xcrun", "--sdk", "iphoneos", "--show-sdk-path"], text=True
            ).strip()
            command = [
                "xcrun",
                "--sdk",
                "iphoneos",
                "clang",
                "-std=c11",
                "-arch",
                "arm64",
                "-isysroot",
                sdk,
                "-miphoneos-version-min=15.0",
            ]
        else:
            ndk = os.environ.get("ANDROID_NDK_HOME") or os.environ.get(
                "ANDROID_NDK_ROOT"
            )
            if not ndk:
                sdk = pathlib.Path(
                    os.environ.get(
                        "ANDROID_HOME",
                        os.environ.get(
                            "ANDROID_SDK_ROOT", "/usr/local/lib/android/sdk"
                        ),
                    )
                )
                ndk = str(
                    sorted(
                        (sdk / "ndk").iterdir(),
                        key=lambda path: tuple((int(x) for x in path.name.split("."))),
                    )[-1]
                )
            command = [
                str(
                    pathlib.Path(ndk)
                    / "toolchains/llvm/prebuilt/linux-x86_64/bin/aarch64-linux-android26-clang"
                ),
                "-std=c11",
            ]
        subprocess.run(
            command
            + ["-I" + str(include), "-c", str(source), "-o", str(out / "abi.o")],
            check=True,
        )
        if linkage == "static":
            probe = out / "mobile-link.c"
            probe.write_text(dedent("""\
                    #include <stddef.h>
                    #include "phonon.h"

                    int main(void)
                    {
                        IPLContextSettings settings = {0};
                        IPLContext context = 0;
                        settings.version = STEAMAUDIO_VERSION;
                        IPLerror result = iplContextCreate(&settings, &context);
                        if (context)
                            iplContextRelease(&context);
                        return result;
                    }
                    """))
            libraries = list((ROOT / "Native/static" / target).glob("*.a"))
            if not libraries:
                raise SystemExit("No mobile static archives to verify")
            link = command + ["-I" + str(include), str(probe)]
            if target == "android-arm64":
                link += (
                    ["-Wl,--start-group"]
                    + list(map(str, libraries))
                    + [
                        "-Wl,--end-group",
                        "-lc++_static",
                        "-lc++abi",
                        "-lunwind",
                        "-latomic",
                        "-llog",
                        "-landroid",
                        "-lm",
                        "-ldl",
                    ]
                )
            else:
                link += list(map(str, libraries)) + ["-lc++"]
            subprocess.run(link + ["-o", str(out / "mobile-link")], check=True)
        (out / "abi-result.txt").write_text(
            "All 64 structures and all field offsets matched the mobile target compiler. Static selections also linked the native context/dependency closure. Cross-compiled; not device-executed.\n"
        )
    else:
        executable = out / ("abi.exe" if target.startswith("win-") else "abi")
        command = (
            [
                "cl",
                "/nologo",
                "/std:c11",
                "/I" + str(include),
                str(ROOT / "Verification/abi.c"),
                "/Fe:" + str(executable),
                "/Fo:" + str(out / "abi.obj"),
            ]
            if target.startswith("win-")
            else [
                "cc",
                "-std=c11",
                "-I" + str(include),
                str(ROOT / "Verification/abi.c"),
                "-o",
                str(executable),
            ]
        )
        subprocess.run(command, check=True)
        native = json.loads(subprocess.check_output([str(executable)], text=True))
        (out / "native-abi.json").write_text(json.dumps(native, indent=2))
        if native != actual:
            for name in actual["structures"]:
                if actual["structures"][name] != native["structures"].get(name):
                    print(
                        name, actual["structures"][name], native["structures"].get(name)
                    )
            raise SystemExit("Native/managed ABI mismatch")
        print("All 64 structures and field offsets matched the native compiler.")


def verify_aot(target, static_link, configuration="Release"):
    """Publish the offline sample and execute it with the selected linkage."""
    if target == "win-x64" and static_link and configuration == "Debug":
        raise SystemExit(
            "Windows static NativeAOT uses Release native assets; "
            "the debug CRT is incompatible with its release runtime."
        )
    mode = "static" if static_link else "dynamic"
    out = ROOT / "artifacts" / target / configuration / (mode + "-aot")
    command = [
        "dotnet",
        "publish",
        str(ROOT / "Samples/Offline/Offline.csproj"),
        "-c",
        configuration,
        "-r",
        target,
        "-p:PublishAot=true",
        "-p:SteamAudioStaticLink=" + str(static_link).lower(),
        "-o",
        str(out),
    ]
    subprocess.run(command, check=True)
    exe = out / ("Offline.exe" if target.startswith("win-") else "Offline")
    inspect = (
        ["dumpbin", "/dependents", str(exe)]
        if target.startswith("win-")
        else (
            ["otool", "-L", str(exe)]
            if target.startswith("osx-")
            else ["ldd", str(exe)]
        )
    )
    deps = subprocess.check_output(inspect, text=True)
    (out / "dependencies.txt").write_text(deps)
    if static_link:
        if "phonon" in deps.lower() or (
            (out / "lib").exists()
            and any(("phonon" in f.name.lower() for f in (out / "lib").iterdir()))
        ):
            raise SystemExit("Static executable retains a Phonon shared dependency")
        with tempfile.TemporaryDirectory(prefix="steamaudio-static-") as temporary:
            clean = pathlib.Path(temporary)
            shutil.copy2(exe, clean / exe.name)
            subprocess.run(
                [str(clean / exe.name), str(out / "rendered.wav")],
                cwd=clean,
                check=True,
            )
    else:
        subprocess.run([str(exe), str(out / "rendered.wav")], cwd=out, check=True)
    print("NativeAOT " + mode + " execution and dependency checks passed.")


def _verify_package(package, target, configuration, temporary):
    package = package.resolve()
    with zipfile.ZipFile(package) as archive:
        names = set(archive.namelist())
        manifests = [
            name
            for name in names
            if name.endswith("/manifest.json")
            and name.startswith(("native/", "runtimes/"))
        ]
        for name in manifests:
            manifest = json.loads(archive.read(name))
            if manifest.get("configuration", "Release") != configuration:
                raise SystemExit(
                    "Package native assets do not match the selected configuration"
                )
            for filename, digest in manifest["files"].items():
                data = archive.read(str(pathlib.PurePosixPath(name).parent / filename))
                if hashlib.sha256(data).hexdigest() != digest:
                    raise SystemExit("Packaged native hash mismatch: " + filename)
        if not manifests:
            raise SystemExit("No verified native assets in the package")
        version = package.name.removeprefix("SteamAudio.NET.").removesuffix(".nupkg")
        modes = []
        if f"native/{target}/Link.targets" in names:
            modes.append("static")
        if any((name.startswith(f"runtimes/{target}/native/") for name in names)):
            modes.append("dynamic")
    if not modes:
        print(
            f"Native hashes verified. {target} consumer omitted: target was not selected."
        )
        raise SystemExit(0)
    consumer = pathlib.Path(temporary)
    (consumer / "NuGet.Config").write_text(dedent(f"""\
            <configuration>
              <packageSources>
                <clear/>
                <add key="verified-local-package" value="{escape(str(package.parent), {'"': '&quot;'})}"/>
                <add key="nuget.org" value="https://api.nuget.org/v3/index.json"/>
              </packageSources>
            </configuration>
            """))
    shutil.copy2(ROOT / "Samples/Offline/Program.cs", consumer / "Program.cs")
    shutil.copy2(
        ROOT / "Verification/AbiMeasurements.cs", consumer / "AbiMeasurements.cs"
    )
    (consumer / "Consumer.csproj").write_text(dedent(f"""\
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net10.0</TargetFramework>
                <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <IsAotCompatible>true</IsAotCompatible>
                <WarningsAsErrors>IL2026;IL3050</WarningsAsErrors>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="SteamAudio.NET" Version="{version}" />
              </ItemGroup>
            </Project>
            """))

    def run(command, **options):
        subprocess.run(list(map(str, command)), cwd=consumer, check=True, **options)

    verified_modes = []
    for mode in modes:
        if target == "win-x64" and mode == "static" and configuration == "Debug":
            print(
                "Windows Debug static archives require the debug CRT; "
                "static NativeAOT verification uses Release assets."
            )
            continue
        output = ROOT / "artifacts/package-verification" / target / configuration / mode
        run(
            [
                "dotnet",
                "publish",
                consumer / "Consumer.csproj",
                "-c",
                configuration,
                "-r",
                target,
                "-p:PublishAot=true",
                "-p:SteamAudioStaticLink=" + str(mode == "static").lower(),
                "-o",
                output,
            ]
        )
        executable = output / (
            "Consumer.exe" if target.startswith("win-") else "Consumer"
        )
        inspect = (
            ["dumpbin", "/dependents"]
            if target.startswith("win-")
            else ["otool", "-L"] if target.startswith("osx-") else ["ldd"]
        )
        dependencies = subprocess.check_output([*inspect, str(executable)], text=True)
        (output / "dependencies.txt").write_text(dependencies)
        if mode == "static":
            if "phonon" in dependencies.lower():
                raise SystemExit(
                    "Packaged static consumer retains a Phonon shared dependency"
                )
            isolated = consumer / "isolated"
            isolated.mkdir(exist_ok=True)
            shutil.copy2(executable, isolated / executable.name)
            run([isolated / executable.name, output / "rendered.wav"])
        else:
            run([executable, output / "rendered.wav"])
        verified_modes.append(mode)
    print(
        f"Package hashes verified. {target} NativeAOT consumers passed: "
        f"{', '.join(verified_modes) or 'none selected'}."
    )


def verify_package(package, target, configuration="Release"):
    """Restore an independent consumer and verify the packaged build targets."""
    with tempfile.TemporaryDirectory(prefix="steamaudio-package-") as temporary:
        _verify_package(package, target, configuration, temporary)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser(
        "api", help="compare all declarations and documentation with the pinned headers"
    )
    abi = commands.add_parser("abi", help="compare compiler and managed layouts in CI")
    abi.add_argument("--target", required=True, choices=SDK["targets"])
    abi.add_argument("--linkage", required=True, choices=["static", "dynamic"])
    aot = commands.add_parser(
        "aot", help="publish and execute the offline sample with NativeAOT"
    )
    desktop_targets = [
        target
        for target in SDK["targets"]
        if not target.startswith(("ios-", "android-"))
    ]
    aot.add_argument("--target", required=True, choices=desktop_targets)
    aot.add_argument("--static", action="store_true")
    package = commands.add_parser(
        "package", help="verify package hashes and transitive publishing"
    )
    package.add_argument("--package", type=pathlib.Path, required=True)
    package.add_argument("--target", required=True, choices=desktop_targets)
    for command in [abi, aot, package]:
        command.add_argument(
            "--configuration", default="Release", choices=CONFIGURATIONS
        )
    arguments = parser.parse_args()
    if arguments.command == "api":
        verify_api()
    elif arguments.command == "abi":
        verify_abi(arguments.target, arguments.linkage, arguments.configuration)
    elif arguments.command == "aot":
        verify_aot(arguments.target, arguments.static, arguments.configuration)
    else:
        verify_package(arguments.package, arguments.target, arguments.configuration)


if __name__ == "__main__":
    main()
