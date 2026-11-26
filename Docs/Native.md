# Native builds and packaging

## Build workflow

The **Build** workflow accepts `linkage=static`, `dynamic` or `both`, and `targets=all` or comma-separated runtime identifiers:

```powershell
gh workflow run build.yml --repo diamondStar35/SteamAudio.NET --ref main -f linkage=both -f targets=all
gh workflow run build.yml --repo diamondStar35/SteamAudio.NET --ref main -f linkage=static -f targets=win-x64,linux-x64
```

An all-target build produces eleven combinations: both linkage modes for Windows x64, Linux x64, macOS x64, macOS ARM64 and Android ARM64, plus static iOS ARM64. A dynamic-only selection containing iOS is rejected.

Native source compilation is restricted to GitHub Actions. `python scripts/build.py sdk` downloads the official prebuilt shared libraries for local development. All temporary downloads, dependency checkouts and verification output belong under `artifacts/`.

## Asset layout

Extract the selected workflow assets into the checkout root:

```text
Native/
  runtimes/<rid>/native/    Shared libraries
  static/<rid>/            Static archives and Link.targets
  notices/<rid>/           Dependency license notices
```

These directories hold the libraries used by consumers. Temporary build output can be deleted independently. `SteamAudioNativeDirectory` selects an alternative asset root. A partial workflow selection produces a package containing only those targets and linkage modes.

`Native/sdk.json` pins the SDK release, archive hash and vcpkg revision. Each source-built artifact includes file hashes, dependency versions and the build configuration in `manifest.json`.

## Static archives

The CPU profile links four archives: Phonon, PFFFT, libmysofa and zlib. They are separate build inputs, linked automatically by `Link.targets`. No individual archive needs to be shipped beside the final static executable. FlatBuffers supplies build tools and headers; its archive is not a runtime link input.

Windows archives embed debug information with `/Z7`, making them larger than archives that reference a separate PDB with `/Zi`. Archive size includes object code and debug records and is not the size added to the executable. Link-time optimization and removal of unused sections determine the final image size.

The target defaults `SteamAudioStaticLink=true` for NativeAOT and iOS. It registers `phonon` as a direct import and supplies the native archives and platform system libraries to the linker. Missing static assets produce a build error. The Windows SDK import library is not a substitute for these static archives.

For shared NativeAOT publishing, set `SteamAudioStaticLink=false`. Ordinary builds use shared libraries. Desktop resolution first checks `lib/`; call `Runtime.ConfigureDirectory(path)` before first use to select another directory.

## Platform details

iOS uses static native references. Android shared libraries are packaged for `arm64-v8a`. Android Mono AOT and NativeAOT are separate deployment modes; the workflow's mobile ABI and native link checks do not exercise either on a physical device.

The source-build profile includes the built-in CPU ray tracer, HRTF rendering, SOFA loading, simulation, baking and reconstruction. IPP, MKL, Embree, Radeon Rays and TrueAudio Next are not bundled. Their declarations remain available for compatible external builds. Experimental octave-band builds use a different ABI and are not supported by these three-band bindings.

Preserve the dependency notices when redistributing native assets or statically linked applications.
