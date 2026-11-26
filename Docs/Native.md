# Native assets and publishing

## Reproducible inputs

`Native/sdk.json` pins Valve SDK 4.8.1, source commit, SDK archive SHA-256 and vcpkg revision. `scripts/fetch-sdk.py` stages official shared binaries without native compilation. The source-build overlay is adapted from NVGT with its attribution retained. It selects CPU-only dependencies and includes the unaligned complex multiply-accumulate correction. Performance/thread-priority patches from NVGT are not included.

## Workflow selection

```powershell
gh workflow run native.yml --repo diamondStar35/SteamAudio.NET --ref main -f linkage=both -f targets=all
gh workflow run native.yml --repo diamondStar35/SteamAudio.NET --ref main -f linkage=static -f targets=win-x64,linux-x64
gh workflow run native.yml --repo diamondStar35/SteamAudio.NET --ref main -f linkage=dynamic -f targets=osx-arm64,android-arm64
```

`both` produces eleven combinations across six RIDs, because iOS is static-only. Invalid RIDs, modes and dynamic-only iOS combinations fail in the selection job. Dependencies are pinned and cached. A source checkout needs Python 3.11+ to fetch prebuilt assets; source compilation is permitted only in Actions.

Downloaded artifacts retain `Native/static/<rid>`, `Native/runtimes/<rid>/native` and notices. Extract them into the repository root for project-reference publishing. Set `SteamAudioNativeDirectory` to a different asset root when needed. Partial artifacts contain only the selected mode/target and must not be advertised as an all-platform package.

## Static linkage

The transitive target defaults `SteamAudioStaticLink=true` when `PublishAot=true`, and for iOS. It registers the logical `phonon` module as DirectPInvoke and imports the per-RID link metadata. That metadata includes Phonon, its dependency archives and required platform linker inputs. The Windows SDK's import library is not substituted for a true static archive.

The resolver loads shared libraries for ordinary builds and for an explicit `SteamAudioStaticLink=false`. iOS resolves statically linked symbols through the application image. Android requires packaging native assets for arm64-v8a; Mono AOT and NativeAOT are distinct host modes. Cross-target ABI verification does not imply device execution.

NuGet consumers receive the transitive target automatically. Source project-reference applications must import the target explicitly, as the offline sample does. Packaging the library does not compile native code or publish a package to any registry.

## Feature profile

Source-built static/shared assets use standard three-band CPU processing. HRTF data and SOFA support are retained. Optional accelerators are disabled and unsupported creation attempts return native status through SteamAudioException. Loading a compatible external build can enable the already-bound accelerator APIs; it does not change ABI assumptions.

## Dependency notices

Preserve the license files packaged under notices. Static linking includes third-party components in the application image. System runtimes and platform SDK prerequisites are documented separately from Phonon assets.
