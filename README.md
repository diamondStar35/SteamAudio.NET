# SteamAudio.NET

C# bindings for Valve's Steam Audio 4.8.1, with explicit native ownership, offline audio processing and NativeAOT publishing support.

This is a redesigned derivative of [Mirsario/SteamAudio.NET](https://github.com/Mirsario/SteamAudio.NET). The original project supplied the starting bindings; this repository replaces the external header-generated runtime layer, reorganizes the API and adds lifetime handling, verification and native build tooling. It is not an official Valve or Mirsario release. The original MIT copyright notice is preserved.

Steam Audio processes sound; it does not open speakers or microphones. You supply PCM blocks and use your audio engine to play the results. The library has no dependency on SoundFlow, OpenAL or MiniAudio.

## Getting started

Install the .NET 10 SDK and Python 3.11 or later. The library itself targets .NET 8 and later. From a source checkout:

```powershell
python scripts/fetch-sdk.py
dotnet build SteamAudio.NET.slnx -c Release
dotnet test Tests/Tests.csproj -c Release
dotnet run --project Samples/Offline -- output.wav
```

The fetch step downloads the pinned official SDK, verifies its SHA-256 and stages shared libraries. Normal builds never compile Steam Audio. The offline example writes a stereo WAV and verifies directional channel energy, effect tails and non-silent impulse-response reconstruction. It does not access an audio device.

Reference `Src/SteamAudio.NET.csproj` from a .NET project, or use a package produced from the native workflow artifacts. This fork has not been published to NuGet; the existing upstream NuGet packages should not be confused with this redesigned API.

## Headphone spatialization

```csharp
using System.Numerics;
using SteamAudio;
using SteamAudio.Effects;

using var context = new Context();
var audio = new AudioSettings(sampleRate: 48000, frameSize: 512);
using var hrtf = new Hrtf(context, audio);
using var binaural = new Binaural(context, hrtf);
using var input = new AudioBuffer(context, channels: 1, frames: 512);
using var output = new AudioBuffer(context, channels: 2, frames: 512);

input.Deinterleave(monoSamples); // exactly 512 samples
binaural.Process(new Vector3(-1, 0, 0), input, output);
output.Interleave(stereoSamples); // exactly 1024 samples
```

Coordinates are right-handed: +X right, +Y up and -Z forward. Buffers contain planar 32-bit floating-point samples internally. `Deinterleave` and `Interleave` copy explicitly; `Borrow()` exposes channel spans without copying. Keep the borrowed view alive until the final span access, then dispose it exactly once.

Each effect processes its configured number of frames. Accumulate smaller input chunks in your engine; this binding does not silently pad every chunk or add buffering latency. After input ends, call `GetTail` until it returns `TailComplete`. Call `Reset` when intentionally clearing history.

Context and HRTF references are retained by their children, so disposing the original owner does not free native dependencies still in use. A disposed owner rejects further operations. Effects and simulator operations reject concurrent use of the same mutable instance; independent instances can work in parallel. Keep allocation, file loading and simulation off the physical audio callback.

## Features and API layers

The managed API covers context/HRTF ownership, planar buffers, panning and binaural effects, ambisonic effects, direct sound, reflections and mixing, path effects, geometry, simulation, probes, baking, energy fields, impulse responses and reconstruction. Geometry supports custom managed ray tracing and indexed material updates.

`SteamAudio.Interop.Api` exposes all 216 SDK C functions, including acceleration-device APIs. Its structs contain exact native-width fields, typed pointers and unmanaged function pointers. This is an advanced interface: native pointers, handle parameters and callback user data keep the lifetimes described by the SDK. A wrapper taking native settings cannot infer the owners of arbitrary pointer members; pass required resource dependencies explicitly and keep external storage valid. Borrowed simulation outputs must not outlive or race their simulator updates.

XML documentation includes the official native descriptions, parameters, results and restrictions, supplemented by managed ownership rules. See [API and lifetime guidance](Docs/API.md), [migration](Docs/Migration.md) and [native publishing](Docs/Native.md).

## Shared libraries and NativeAOT

Ordinary desktop builds load `phonon.dll`, `libphonon.so` or `libphonon.dylib` from the application's `lib` directory first. NuGet/runtime probing is the fallback. Call `SteamAudio.Runtime.ConfigureDirectory(path)` before first use to select another shared-library directory.

NativeAOT publishing defaults to linking Phonon and its native dependency archives into the application image:

```powershell
dotnet publish MyApp.csproj -c Release -r win-x64 -p:PublishAot=true
```

The matching static workflow artifacts must be present, or supplied through `SteamAudioNativeDirectory`. Missing archives are a build error. To intentionally use a shared library with NativeAOT:

```powershell
dotnet publish MyApp.csproj -c Release -r win-x64 -p:PublishAot=true -p:SteamAudioStaticLink=false
```

For a source project reference, import `Src/SteamAudio.NET.targets` into the application project as well; NuGet consumers receive it automatically through `buildTransitive`.

Static imports bypass the runtime shared-library resolver. Static publishing requires the normal platform native linker and system runtime prerequisites; it removes the need to ship Phonon companion shared libraries, not the operating system's own libraries.

## Platforms and native builds

| Runtime | Shared assets | Static assets |
| --- | --- | --- |
| Windows x64 | Yes | Yes |
| Linux x64 | Yes | Yes |
| macOS x64 / ARM64 | Yes | Yes |
| Android ARM64 | Yes | Yes |
| iOS ARM64 | No | Yes |

These are build targets, not claims of physical-device testing. iOS uses static native references. Android shared libraries use Android packaging rules rather than an ordinary desktop filesystem directory. Android Mono AOT and experimental NativeAOT are distinct deployment modes; the binding does not turn one into the other. Mobile target jobs report cross-compiled ABI/link checks separately from device execution.

The **Native assets and verification** GitHub workflow accepts `linkage=static`, `dynamic` or `both`, plus comma-separated `targets` or `all`. The default builds both modes for all supported targets; iOS remains static-only. A dynamic-only selection containing iOS is rejected before any build starts. Native compilation is restricted to GitHub Actions.

The reproducible source-build profile uses the built-in CPU ray tracer, PFFFT, libmysofa and zlib. It includes HRTF, DSP, geometry, simulation, baking and reconstruction. IPP, MKL, Embree, Radeon Rays and TrueAudio Next are not bundled into this profile. Their APIs remain available when using an appropriate externally supplied SDK build. Standard three-band processing is supported; experimental octave-band ABI variants are not interchangeable.

## Verification and provenance

The tests use real native processing and procedural signals. They verify direction-dependent audio, gain, sample ordering, finite tails, geometry/material effects, callback lifetime and failures, retained resources, serialization and reconstruction. CI compares every native structure's size and field offsets with an independent C compiler probe, roots every import during NativeAOT publication and executes desktop consumers without a Phonon shared library for static mode.

Native artifacts record their target, linkage, SDK/dependency pins, hashes and notices. See [verification](Docs/Verification.md) for commands and platform limits.

C# code retains the original MIT license. Steam Audio declarations and translated SDK documentation are covered by Valve's Apache-2.0 license. Preserve [third-party notices](THIRD-PARTY-NOTICES.md) when redistributing native assets. Build integration and correctness fixes adapted from NVGT are attributed there.
