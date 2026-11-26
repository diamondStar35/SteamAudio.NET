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
            jobs.append({"target": target, "linkage": mode, "runner": RUNNERS[target]})
    print("value=" + json.dumps({"include": jobs}, separators=(",", ":")))


def triplet_settings(target, linkage):
    """Keep dependencies static while allowing Phonon itself to be shared."""
    operating_system, architecture = target.split("-")
    lines = [
        f"set(VCPKG_TARGET_ARCHITECTURE {architecture})",
        f"set(VCPKG_CRT_LINKAGE {'static' if operating_system == 'win' else 'dynamic'})",
        "set(VCPKG_LIBRARY_LINKAGE static)",
        "set(VCPKG_BUILD_TYPE release)",
        "set(VCPKG_CMAKE_CONFIGURE_OPTIONS -DCMAKE_POSITION_INDEPENDENT_CODE=ON)",
    ]
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


def stage_static(installed, target, windows):
    """Copy dependency archives and write the consumer's platform link inputs."""
    output = ROOT / "Native/static" / target
    output.mkdir(parents=True, exist_ok=True)
    # Phonon must precede the archives that satisfy its references on GNU linkers.
    names = (
        ["phonon.lib", "pffft.lib", "mysofa.lib", "zs.lib"]
        if windows
        else ["libphonon.a", "libpffft.a", "libmysofa.a", "libz.a"]
    )
    libraries = [installed / "lib" / name for name in names]
    for library in libraries:
        if not library.is_file():
            raise RuntimeError(f"Required static archive is missing: {library.name}")

    project = xml.Element("Project")
    items = xml.SubElement(project, "ItemGroup")
    for library in libraries:
        shutil.copy2(library, output / library.name)
        location = "$(MSBuildThisFileDirectory)" + library.name
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


def stage_shared(installed, target):
    output = ROOT / "Native/runtimes" / target / "native"
    output.mkdir(parents=True, exist_ok=True)
    for folder in ["bin", "lib"]:
        for library in (installed / folder).glob("*"):
            if library.suffix in {".dll", ".dylib", ".so"}:
                shutil.copy2(library, output / library.name)
    if not any("phonon" in library.name for library in output.iterdir()):
        raise RuntimeError("Shared Phonon library is missing")
    return output


def verify_accumulation(output, target, windows):
    """Exercise the corrected unaligned SIMD path in the actual static archive."""
    probe = WORK / "simd"
    probe.mkdir(parents=True, exist_ok=True)
    libraries = " ".join(
        f'"{library.as_posix()}"'
        for library in output.glob("*.lib" if windows else "*.a")
    )
    lines = [
        "cmake_minimum_required(VERSION 3.20)",
        "project(AccumulationCheck LANGUAGES CXX)",
        "set(CMAKE_CXX_STANDARD 14)",
        "set(CMAKE_MSVC_RUNTIME_LIBRARY MultiThreaded)",
        f'add_executable(simd "{(ROOT / "Verification/simd.cpp").as_posix()}")',
        f"target_link_libraries(simd PRIVATE {libraries})",
    ]
    if target == "linux-x64":
        lines.append("target_link_libraries(simd PRIVATE stdc++ m dl pthread)")
    elif windows:
        lines.append("target_link_libraries(simd PRIVATE delayimp)")
    (probe / "CMakeLists.txt").write_text("\n".join(lines) + "\n")
    run("cmake", "-S", probe, "-B", probe / "build", "-DCMAKE_BUILD_TYPE=Release")
    run("cmake", "--build", probe / "build", "--config", "Release")
    executable = next(
        path
        for path in (probe / "build").rglob("simd.exe" if windows else "simd")
        if path.is_file()
    )
    run(executable)


def build_native(target, linkage):
    if os.environ.get("GITHUB_ACTIONS") != "true":
        raise SystemExit(
            "Native compilation runs in GitHub Actions. Use 'build.py sdk' locally."
        )
    if target == "ios-arm64" and linkage == "dynamic":
        raise SystemExit("iOS supports static linking only.")

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
    triplet = f"{target}-{linkage}"
    configuration = triplet_settings(target, linkage)
    (triplets / f"{triplet}.cmake").write_text(configuration)
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
        stage_static(installed, target, windows)
        if linkage == "static"
        else stage_shared(installed, target)
    )
    notices = ROOT / "Native/notices" / target
    notices.mkdir(parents=True, exist_ok=True)
    for notice in (installed / "share").rglob("copyright"):
        shutil.copy2(notice, notices / f"{notice.parent.name}-copyright.txt")

    manifest = {
        "sdk": SDK,
        "target": target,
        "linkage": linkage,
        "patches": {
            patch.name: hashlib.sha256(patch.read_bytes()).hexdigest()
            for patch in (ROOT / "Native/ports/steam-audio").glob("*.patch")
        },
        "dependency_status": (install / "vcpkg/status").read_text(encoding="utf-8"),
        "triplet": configuration,
        "files": {
            path.name: hashlib.sha256(path.read_bytes()).hexdigest()
            for path in output.iterdir()
            if path.is_file() and path.name != "manifest.json"
        },
    }
    (output / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    if linkage == "static" and target not in {"ios-arm64", "android-arm64"}:
        verify_accumulation(output, target, windows)
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
    arguments = parser.parse_args()
    if arguments.command == "sdk":
        fetch_sdk()
    elif arguments.command == "matrix":
        print_matrix()
    elif arguments.command == "native":
        build_native(arguments.target, arguments.linkage)
    else:
        collect(arguments.target, arguments.linkage)


if __name__ == "__main__":
    main()
