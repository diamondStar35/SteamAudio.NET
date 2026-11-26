# Native builds and packaging

## Build workflow

The **Build** workflow accepts `configuration=Release` or `Debug`, `optimization=O2` or `O1`, `linkage=static`, `dynamic` or `both`, and `targets=all` or comma-separated runtime identifiers. Release with O2 is the default. The optimization selection applies only to Release; Debug always disables optimization.

```powershell
gh workflow run build.yml --repo diamondStar35/SteamAudio.NET --ref main -f configuration=Release -f optimization=O2 -f linkage=both -f targets=all
gh workflow run build.yml --repo diamondStar35/SteamAudio.NET --ref main -f configuration=Release -f optimization=O1 -f linkage=static -f targets=win-x64
gh workflow run build.yml --repo diamondStar35/SteamAudio.NET --ref main -f configuration=Debug -f linkage=both -f targets=win-x64,linux-x64
```

An all-target build produces eleven combinations: both linkage modes for Windows x64, Linux x64, macOS x64, macOS ARM64 and Android ARM64, plus static iOS ARM64. A dynamic-only selection containing iOS is rejected.

Caches and artifacts distinguish `Release-O1`, `Release-O2` and `Debug`. Debug packages use `5.0.0-debug.<run-id>`; Release packages use `5.0.0-ci.O1.<run-id>` or `5.0.0-ci.O2.<run-id>`. Manifests record the effective profile and optimization. Each run packages only its selected profile. Debug runs build vcpkg's usual Release/Debug pair to obtain the shared headers, then stage only Debug libraries and symbols; their unused Release side uses the default O2 preset regardless of the optimization selector.

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

Release optimization applies consistently to Phonon and its dependencies:

| Selection | Windows | Unix | CMake configuration |
| --- | --- | --- | --- |
| O1: smaller code | `/O1 /Ob1 /Oi /Gy` | `-Os` | `MinSizeRel` |
| O2: favor speed | `/O2 /Ob2 /Oi /Gy` | `-O3` | `Release` |

MSVC's stock `/O1` and `/O2` both default to `/Ob2`. This library's O1 preset deliberately uses `/Ob1` to limit inlining and explicitly enables `/Oi` intrinsics. The O2 preset permits the compiler's normal `/Ob2` inlining. See Microsoft's [optimization reference](https://learn.microsoft.com/en-us/cpp/build/reference/o1-o2-minimize-size-maximize-speed) and [inlining reference](https://learn.microsoft.com/en-us/cpp/build/reference/ob-inline-function-expansion).

Linux and Android use separate function/data sections so the final linker can discard unused code. Both Release presets omit debug information and Windows `/DEBUG` linking. The source patch removes Steam Audio's forced `/Ox` and Android `-O3`, so they cannot override the selected preset. It also removes forced `/GL` and `/LTCG`; selecting O2 does not silently turn on link-time optimization. SIMD, instruction-set and floating-point settings are the same for both presets. Actual binary size and processing-time differences depend on the workload and must be measured.

Debug disables optimization and enables assertions and symbols. Windows Debug archives use `/MTd /Od /Z7`; shared builds also provide PDB files. Unix Debug uses `-O0 -g`. Debug archive filenames are normalized so consumers use the same link targets for either configuration.

Windows Debug static archives require the debug CRT. NativeAOT's standard runtime uses the release CRT, so Windows static NativeAOT checks run only in Release workflows. A managed application can still be debugged with Release native assets, or use the Debug shared library. Other desktop configurations run both static and dynamic NativeAOT checks when selected. Existing local archives retain their original configuration until replaced with newly built assets.

The target defaults `SteamAudioStaticLink=true` for NativeAOT and iOS. It registers `phonon` as a direct import and supplies the native archives and platform system libraries to the linker. Missing static assets produce a build error. The Windows SDK import library is not a substitute for these static archives.

For shared NativeAOT publishing, set `SteamAudioStaticLink=false`. Ordinary builds use shared libraries. Desktop resolution first checks `lib/`; call `Runtime.ConfigureDirectory(path)` before first use to select another directory.

## Platform details

iOS uses static native references. Android shared libraries are packaged for `arm64-v8a`. Android Mono AOT and NativeAOT are separate deployment modes; the workflow's mobile ABI and native link checks do not exercise either on a physical device.

The source-build profile includes the built-in CPU ray tracer, HRTF rendering, SOFA loading, simulation, baking and reconstruction. IPP, MKL, Embree, Radeon Rays and TrueAudio Next are not bundled. Their declarations remain available for compatible external builds. Experimental octave-band builds use a different ABI and are not supported by these three-band bindings.

Preserve the dependency notices when redistributing native assets or statically linked applications.
