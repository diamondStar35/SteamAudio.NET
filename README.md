# SteamAudio.NET

C# bindings for Steam Audio 4.8.1, with managed resource ownership and support for static and shared NativeAOT publishing. The library targets .NET 8 and later.

This project is derived from [Mirsario/SteamAudio.NET](https://github.com/Mirsario/SteamAudio.NET). It retains the original MIT license and replaces the generated bindings with direct interop declarations and a managed API for audio processing, geometry, simulation and baking.

## Build from source

The repository uses the .NET 10 SDK and Python 3.11 or later.

```powershell
python scripts/build.py sdk
dotnet build SteamAudio.NET.slnx -c Release
dotnet test Tests/Tests.csproj -c Release
```

The first command downloads and verifies the official SDK binaries. Native source compilation runs separately in GitHub Actions.

Reference `Src/SteamAudio.NET.csproj` from your application and import `Src/SteamAudio.NET.targets` into the application project. Packages produced by the workflow include that import automatically. This fork is not currently published to NuGet.

## Binaural rendering

```csharp
using System.Numerics;
using SteamAudio;
using SteamAudio.Effects;

using var context = new Context();
var format = new AudioSettings(sampleRate: 48000, frameSize: 512);
using var hrtf = new Hrtf(context, format);
using var binaural = new Binaural(context, hrtf);
using var input = new AudioBuffer(context, channels: 1, frames: 512);
using var output = new AudioBuffer(context, channels: 2, frames: 512);

input.Deinterleave(monoSamples);
binaural.Process(-Vector3.UnitX, input, output);
output.Interleave(stereoSamples);
```

Here `monoSamples` contains 512 samples and `stereoSamples` has space for 1024 samples. Directions use +X right, +Y up and -Z forward. `Borrow()` provides scoped access to planar channel spans. When input ends, drain `GetTail()` until it returns `TailComplete`.

The [offline example](Samples/Offline/Program.cs) renders a WAV without an audio device. The [API guide](Docs/API.md) covers ownership, callbacks, threading and simulation. See [migration](Docs/Migration.md) for changes from the original wrapper.

## Features

- Binaural rendering, speaker panning, virtual surround and Ambisonics encoding, rotation and decoding.
- Direct-sound filtering, reflection rendering and mixing, and path effects.
- Acoustic meshes, instancing, material updates and custom ray tracing.
- Direct, reflection and path simulation; probes, baking and impulse-response reconstruction.
- All 216 C API functions, with typed native handles and XML documentation.

## Native libraries and publishing

Ordinary desktop builds load the shared library from the application's `lib` directory. NativeAOT publishing links statically by default:

```powershell
dotnet publish MyApp.csproj -c Release -r win-x64 -p:PublishAot=true
```

Download the matching static assets from the **Build** workflow first. Set `SteamAudioStaticLink=false` to use shared libraries with NativeAOT. See [native builds and packaging](Docs/Native.md) for asset layout, platform requirements and workflow options.

Supported targets are Windows x64, Linux x64, macOS x64 and ARM64, Android ARM64, and iOS ARM64. iOS uses static linking. Desktop consumers are executed in CI; mobile checks cover cross-compilation and linking, without claiming device testing.

Source builds use the standard three-band CPU profile. Optional acceleration APIs are available for compatible external native builds. Test coverage and its limits are described in [verification](Docs/Verification.md).

## License

The C# library uses the [MIT license](LICENSE.md). Steam Audio and its documentation use [Apache-2.0](LICENSE.Natives.md). Native dependency notices are included with build artifacts and packages; see [third-party notices](THIRD-PARTY-NOTICES.md).
