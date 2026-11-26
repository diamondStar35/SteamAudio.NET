"""Download SDK binaries and build or collect native assets for the Build workflow."""

import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import urllib.request
import xml.etree.ElementTree as xml
import zipfile

ROOT = Path(__file__).resolve().parents[1]
SDK = json.loads((ROOT / "Native/sdk.json").read_text(encoding="utf-8"))
WORK = ROOT / "artifacts/build"
RUNNERS = {
    "win-x64": "windows-2022",
    "linux-x64": "ubuntu-24.04",
    "osx-x64": "macos-15-intel",
    "osx-arm64": "macos-15",
    "android-arm64": "ubuntu-24.04",
    "ios-arm64": "macos-15",
}
CONFIGURATIONS = ("Release", "Debug")
OPTIMIZATIONS = ("O2", "O1")
DEFAULT_OPTIMIZATION = "O2"


def build_profile(configuration, optimization):
    """Name the effective profile; Debug always disables optimization."""
    if configuration not in CONFIGURATIONS or optimization not in OPTIMIZATIONS:
        raise ValueError("Select Release/Debug and O1/O2.")
    return "Debug" if configuration == "Debug" else f"Release-{optimization}"


def release_build_type(optimization):
    return "MinSizeRel" if optimization == "O1" else "Release"


def run(*command):
    subprocess.run([str(argument) for argument in command], check=True)


def fetch_sdk():
    """Verify the official release archive and stage its shared libraries."""
    cache = WORK / "sdk"
    cache.mkdir(parents=True, exist_ok=True)
    archive = cache / f"steamaudio_{SDK['version']}.zip"
    if not archive.exists():
        urllib.request.urlretrieve(SDK["url"], archive)

    with archive.open("rb") as stream:
        if hashlib.file_digest(stream, "sha256").hexdigest() != SDK["sha256"]:
            raise RuntimeError("SDK SHA-256 mismatch")

    with zipfile.ZipFile(archive) as package:
        for entry in package.infolist():
            if not (cache / entry.filename).resolve().is_relative_to(cache.resolve()):
                raise RuntimeError("Unsafe SDK archive path")
        package.extractall(cache)

    folders = {
        "win-x64": "windows-x64",
        "linux-x64": "linux-x64",
        "osx-x64": "osx",
        "osx-arm64": "osx",
        "android-arm64": "android-armv8",
    }
    for target, folder in folders.items():
        destination = ROOT / "Native/runtimes" / target / "native"
        destination.mkdir(parents=True, exist_ok=True)
        for library in (cache / "steamaudio/lib" / folder).iterdir():
            if library.is_file() and library.suffix in {".dll", ".so", ".dylib"}:
                shutil.copy2(library, destination / library.name)

    shutil.copy2(
        cache / "steamaudio/THIRDPARTY.md", ROOT / "Native/notices/Valve-THIRDPARTY.md"
    )
    print(
        f"Verified Steam Audio {SDK['version']}; shared libraries are in Native/runtimes."
    )


def print_matrix():
    """Validate workflow inputs before any native jobs are scheduled."""
    selection = os.environ["TARGETS"].strip()
    targets = (
        SDK["targets"]
        if selection == "all"
        else list(dict.fromkeys(target.strip() for target in selection.split(",")))
    )
    linkage = os.environ["LINKAGE"]
    configuration = os.environ.get("CONFIGURATION", "Release")
    optimization = os.environ.get("OPTIMIZATION", DEFAULT_OPTIMIZATION)
    if configuration not in CONFIGURATIONS:
        raise SystemExit("Select Release or Debug.")
    if optimization not in OPTIMIZATIONS:
        raise SystemExit("Select O1 or O2.")
    if configuration == "Debug":
        optimization = DEFAULT_OPTIMIZATION
    profile = build_profile(configuration, optimization)
    if linkage not in {"static", "dynamic", "both"} or not targets:
        raise SystemExit("Select static, dynamic or both, and at least one target.")
    if any(target not in SDK["targets"] for target in targets):
        raise SystemExit("Unsupported runtime identifier.")
    if linkage == "dynamic" and "ios-arm64" in targets:
        raise SystemExit("iOS supports static linking only.")

    jobs = []
    for target in targets:
        modes = (
            ["static"]
            if target == "ios-arm64"
            else (["static", "dynamic"] if linkage == "both" else [linkage])
        )
        for mode in modes:
            jobs.append(
                {
                    "target": target,
                    "linkage": mode,
                    "configuration": configuration,
                    "optimization": optimization,
                    "profile": profile,
                    "runner": RUNNERS[target],
                }
            )
    print("value=" + json.dumps({"include": jobs}, separators=(",", ":")))


def triplet_settings(
    target, linkage, configuration="Release", optimization=DEFAULT_OPTIMIZATION
):
    """Keep dependencies static while allowing Phonon itself to be shared."""
    operating_system, architecture = target.split("-")
    build_profile(configuration, optimization)
    if configuration == "Debug":
        optimization = DEFAULT_OPTIMIZATION
    lines = [
        f"set(VCPKG_TARGET_ARCHITECTURE {architecture})",
        f"set(VCPKG_CRT_LINKAGE {'static' if operating_system == 'win' else 'dynamic'})",
        "set(VCPKG_LIBRARY_LINKAGE static)",
        "set(VCPKG_CMAKE_CONFIGURE_OPTIONS -DCMAKE_POSITION_INDEPENDENT_CODE=ON)",
    ]
    # vcpkg ports commonly install shared headers from their Release build.
    # Debug runs build the normal pair and stage only debug/lib and debug/bin.
    if configuration == "Release":
        lines.append("set(VCPKG_BUILD_TYPE release)")
    # Keep debug symbols disabled for either Release optimization preset.
    if operating_system == "win":
        inline_level = "1" if optimization == "O1" else "2"
        release_flags = f"/MT /{optimization} /Ob{inline_level} /Oi /Gy /DNDEBUG"
    else:
        level = "s" if optimization == "O1" else "3"
        release_flags = f"-O{level} -DNDEBUG -g0"
    lines.append("set(VCPKG_CMAKE_CONFIGURE_OPTIONS_RELEASE")
    lines.append(f'  "-DCMAKE_BUILD_TYPE:STRING={release_build_type(optimization)}"')
    for language in ["C", "CXX"]:
        for build_type in ["RELEASE", "MINSIZEREL"]:
            lines.append(
                f'  "-DCMAKE_{language}_FLAGS_{build_type}:STRING={release_flags}"'
            )
    if operating_system == "win":
        lines.append('  "-DCMAKE_MSVC_DEBUG_INFORMATION_FORMAT:STRING="')
        for target_type in ["EXE", "SHARED", "MODULE"]:
            for build_type in ["RELEASE", "MINSIZEREL"]:
                lines.append(
                    f'  "-DCMAKE_{target_type}_LINKER_FLAGS_{build_type}:STRING='
                    '/nologo /INCREMENTAL:NO /OPT:REF /OPT:ICF"'
                )
    lines.append(")")
    lines.append("set(VCPKG_CMAKE_CONFIGURE_OPTIONS_DEBUG")
    debug_flags = (
        "/MTd /Z7 /Ob0 /Od /RTC1 /D_DEBUG"
        if operating_system == "win"
        else "-O0 -g"
    )
    for language in ["C", "CXX"]:
        lines.append(f'  "-DCMAKE_{language}_FLAGS_DEBUG:STRING={debug_flags}"')
    lines.append(")")
    if operating_system in {"linux", "android"}:
        lines.extend(
            [
                'set(VCPKG_C_FLAGS "-ffunction-sections -fdata-sections")',
                'set(VCPKG_CXX_FLAGS "-ffunction-sections -fdata-sections")',
            ]
        )
    if linkage == "dynamic":
        lines.extend(
            [
                'if(PORT STREQUAL "steam-audio")',
                "  set(VCPKG_LIBRARY_LINKAGE dynamic)",
                "endif()",
            ]
        )

    system_names = {
        "linux": "Linux",
        "android": "Android",
        "ios": "iOS",
        "osx": "Darwin",
    }
    if operating_system in system_names:
        lines.append(f"set(VCPKG_CMAKE_SYSTEM_NAME {system_names[operating_system]})")
    if operating_system == "osx":
        lines.extend(
            [
                "set(VCPKG_OSX_DEPLOYMENT_TARGET 12.0)",
                f"set(VCPKG_OSX_ARCHITECTURES {'x86_64' if architecture == 'x64' else 'arm64'})",
            ]
        )
    elif operating_system == "ios":
        lines.append("set(VCPKG_OSX_DEPLOYMENT_TARGET 15.0)")
    elif operating_system == "android":
        lines.extend(
            [
                "set(VCPKG_CMAKE_SYSTEM_VERSION 26)",
                "set(VCPKG_CMAKE_CONFIGURE_OPTIONS "
                '"-DANDROID_ABI=arm64-v8a" "-DANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON" '
                '"-DCMAKE_POSITION_INDEPENDENT_CODE=ON")',
            ]
        )
    return "\n".join(lines) + "\n"


def stage_static(installed, target, windows, configuration="Release"):
    """Copy dependency archives and write the consumer's platform link inputs."""
    output = ROOT / "Native/static" / target
    output.mkdir(parents=True, exist_ok=True)
    # Phonon must precede the archives that satisfy its references on GNU linkers.
    names = (
        ["phonon.lib", "pffft.lib", "mysofa.lib", "zs.lib"]
        if windows
        else ["libphonon.a", "libpffft.a", "libmysofa.a", "libz.a"]
    )
    library_directory = installed / (
        "debug/lib" if configuration == "Debug" else "lib"
    )
    libraries = []
    for name in names:
        canonical = Path(name)
        candidates = [library_directory / name]
        if configuration == "Debug":
            candidates.append(
                library_directory / (canonical.stem + "d" + canonical.suffix)
            )
        library = next(
            (candidate for candidate in candidates if candidate.is_file()), None
        )
        if library is None:
            raise RuntimeError(f"Required static archive is missing: {name}")
        libraries.append((library, name))

    project = xml.Element("Project")
    items = xml.SubElement(project, "ItemGroup")
    for library, name in libraries:
        shutil.copy2(library, output / name)
        location = "$(MSBuildThisFileDirectory)" + name
        xml.SubElement(items, "NativeLibrary", Include=location)
        if target == "ios-arm64":
            reference = xml.SubElement(
                items,
                "NativeReference",
                Include=location,
                Condition="'$(PublishAot)' != 'true'",
            )
            xml.SubElement(reference, "Kind").text = "Static"
            xml.SubElement(reference, "ForceLoad").text = "true"

    system_libraries = {
        "linux": ["stdc++", "m", "dl", "pthread"],
        "osx": ["c++"],
        "ios": ["c++"],
        "android": ["log", "android", "c++_static", "c++abi", "unwind", "atomic"],
    }
    if windows:
        xml.SubElement(items, "NativeLibrary", Include="delayimp.lib")
    else:
        for library in system_libraries[target.split("-")[0]]:
            xml.SubElement(items, "NativeSystemLibrary", Include=library)
    xml.indent(project, space="  ")
    (output / "Link.targets").write_text(
        xml.tostring(project, encoding="unicode") + "\n"
    )
    return output


def stage_shared(installed, target, configuration="Release"):
    output = ROOT / "Native/runtimes" / target / "native"
    output.mkdir(parents=True, exist_ok=True)
    source = installed / "debug" if configuration == "Debug" else installed
    for folder in ["bin", "lib"]:
        for library in (source / folder).glob("*"):
            if library.suffix in {".dll", ".dylib", ".so"} or (
                configuration == "Debug" and library.suffix == ".pdb"
            ):
                shutil.copy2(library, output / library.name)
    if not any("phonon" in library.name for library in output.iterdir()):
        raise RuntimeError("Shared Phonon library is missing")
    return output


def verify_accumulation(
    output, target, windows, configuration="Release", optimization=DEFAULT_OPTIMIZATION
):
    """Exercise the corrected unaligned SIMD path in the actual static archive."""
    probe = WORK / "simd" / build_profile(configuration, optimization)
    probe.mkdir(parents=True, exist_ok=True)
    libraries = " ".join(
        f'"{library.as_posix()}"'
        for library in output.glob("*.lib" if windows else "*.a")
    )
    lines = [
        "cmake_minimum_required(VERSION 3.20)",
        "project(AccumulationCheck LANGUAGES CXX)",
        "set(CMAKE_CXX_STANDARD 14)",
        "set(CMAKE_MSVC_RUNTIME_LIBRARY "
        + ("MultiThreadedDebug)" if configuration == "Debug" else "MultiThreaded)"),
        f'add_executable(simd "{(ROOT / "Verification/simd.cpp").as_posix()}")',
        f"target_link_libraries(simd PRIVATE {libraries})",
    ]
    if target == "linux-x64":
        lines.append("target_link_libraries(simd PRIVATE stdc++ m dl pthread)")
    elif windows:
        lines.append("target_link_libraries(simd PRIVATE delayimp)")
    (probe / "CMakeLists.txt").write_text("\n".join(lines) + "\n")
    native_configuration = (
        "Debug" if configuration == "Debug" else release_build_type(optimization)
    )
    run(
        "cmake", "-S", probe, "-B", probe / "build",
        f"-DCMAKE_BUILD_TYPE={native_configuration}",
    )
    run("cmake", "--build", probe / "build", "--config", native_configuration)
    executable = next(
        path
        for path in (probe / "build").rglob("simd.exe" if windows else "simd")
        if path.is_file()
    )
    run(executable)


def build_native(
    target, linkage, configuration="Release", optimization=DEFAULT_OPTIMIZATION
):
    if os.environ.get("GITHUB_ACTIONS") != "true":
        raise SystemExit(
            "Native compilation runs in GitHub Actions. Use 'build.py sdk' locally."
        )
    if target == "ios-arm64" and linkage == "dynamic":
        raise SystemExit("iOS supports static linking only.")
    profile = build_profile(configuration, optimization)
    if configuration == "Debug":
        optimization = DEFAULT_OPTIMIZATION

    windows = platform.system() == "Windows"
    vcpkg = WORK / "vcpkg"
    if not (vcpkg / ".git").exists():
        vcpkg.mkdir(parents=True, exist_ok=True)
        run("git", "-C", vcpkg, "init")
        run(
            "git",
            "-C",
            vcpkg,
            "remote",
            "add",
            "origin",
            "https://github.com/microsoft/vcpkg.git",
        )
    run("git", "-C", vcpkg, "fetch", "--depth=1", "origin", SDK["vcpkg"])
    run("git", "-C", vcpkg, "checkout", "--detach", "FETCH_HEAD")
    if windows:
        run("cmd.exe", "/d", "/c", vcpkg / "bootstrap-vcpkg.bat", "-disableMetrics")
    else:
        run("sh", vcpkg / "bootstrap-vcpkg.sh", "-disableMetrics")

    if target == "android-arm64" and not os.environ.get("ANDROID_NDK_HOME"):
        android = Path(
            os.environ.get(
                "ANDROID_HOME",
                os.environ.get("ANDROID_SDK_ROOT", "/usr/local/lib/android/sdk"),
            )
        )
        versions = sorted(
            (android / "ndk").iterdir(),
            key=lambda path: tuple(map(int, path.name.split("."))),
        )
        os.environ["ANDROID_NDK_HOME"] = str(versions[-1])

    triplets = WORK / "triplets"
    triplets.mkdir(parents=True, exist_ok=True)
    triplet = f"{target}-{linkage}-{profile.lower()}"
    settings = triplet_settings(target, linkage, configuration, optimization)
    (triplets / f"{triplet}.cmake").write_text(settings)
    binary_cache = WORK / "vcpkg-binary-cache"
    binary_cache.mkdir(parents=True, exist_ok=True)
    os.environ["VCPKG_DEFAULT_BINARY_CACHE"] = str(binary_cache)
    install = WORK / "installed" / triplet
    run(
        vcpkg / ("vcpkg.exe" if windows else "vcpkg"),
        "install",
        f"--triplet={triplet}",
        f"--overlay-triplets={triplets}",
        f"--x-manifest-root={ROOT / 'Native'}",
        f"--x-install-root={install}",
    )

    installed = install / triplet
    output = (
        stage_static(installed, target, windows, configuration)
        if linkage == "static"
        else stage_shared(installed, target, configuration)
    )
    notices = ROOT / "Native/notices" / target
    notices.mkdir(parents=True, exist_ok=True)
    for notice in (installed / "share").rglob("copyright"):
        shutil.copy2(notice, notices / f"{notice.parent.name}-copyright.txt")

    manifest = {
        "sdk": SDK,
        "target": target,
        "linkage": linkage,
        "configuration": configuration,
        "optimization": "Disabled" if configuration == "Debug" else optimization,
        "profile": profile,
        "patches": {
            patch.name: hashlib.sha256(patch.read_bytes()).hexdigest()
            for patch in (ROOT / "Native/ports/steam-audio").glob("*.patch")
        },
        "dependency_status": (install / "vcpkg/status").read_text(encoding="utf-8"),
        "triplet": settings,
        "files": {
            path.name: hashlib.sha256(path.read_bytes()).hexdigest()
            for path in output.iterdir()
            if path.is_file() and path.name != "manifest.json"
        },
    }
    (output / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    if linkage == "static" and target not in {"ios-arm64", "android-arm64"}:
        verify_accumulation(output, target, windows, configuration, optimization)
    print(f"Native assets: {output}")


def collect(target, linkage):
    """Upload only the selected target and linkage, plus verification results."""
    output = ROOT / "artifacts/upload"
    paths = [
        f"Native/{'static' if linkage == 'static' else 'runtimes'}/{target}",
        "Native/notices",
        f"artifacts/{target}",
        "Tests/TestResults",
    ]
    for relative in paths:
        source = ROOT / relative
        if source.exists():
            shutil.copytree(source, output / relative, dirs_exist_ok=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("sdk", help="download and verify prebuilt SDK libraries")
    commands.add_parser(
        "matrix", help="validate workflow inputs and emit its job matrix"
    )
    for name in ["native", "collect"]:
        command = commands.add_parser(name)
        command.add_argument("--target", required=True, choices=SDK["targets"])
        command.add_argument("--linkage", required=True, choices=["static", "dynamic"])
        if name == "native":
            command.add_argument(
                "--configuration", default="Release", choices=CONFIGURATIONS
            )
            command.add_argument(
                "--optimization", default=DEFAULT_OPTIMIZATION, choices=OPTIMIZATIONS,
                help="Release optimization; ignored for Debug",
            )
    arguments = parser.parse_args()
    if arguments.command == "sdk":
        fetch_sdk()
    elif arguments.command == "matrix":
        print_matrix()
    elif arguments.command == "native":
        build_native(
            arguments.target, arguments.linkage,
            arguments.configuration, arguments.optimization,
        )
    else:
        collect(arguments.target, arguments.linkage)


if __name__ == "__main__":
    main()
