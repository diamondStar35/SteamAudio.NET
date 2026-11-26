# Verification

Run the managed/native suite after fetching the official SDK:

```powershell
python scripts/fetch-sdk.py
python scripts/verify-api.py
dotnet test Tests/Tests.csproj -c Release
dotnet run --project Samples/Offline -c Release -- output.wav
```

Tests use actual SDK processing with impulses and authored geometry. Assertions check directional HRTF energy, exact sample ordering, gain after ramps, finite terminating tails, geometry occlusion, changed transmission after material edits, reconstruction energy, immutable serialized input, callback errors and native lifetime through forced GC. A closed-room test bakes reflections and checks measured energy, decay times and a serialized round trip. Another bakes paths and verifies usable simulation coefficients. Real-time reflection simulation is rendered through the shared convolution mixer and checked for finite, non-silent audio. No physical audio device is opened.

`Verification/abi.c` asks a C compiler for native sizes and field offsets. `Verification/AbiMeasurements.cs` uses managed unsafe sizeof and field addresses, without runtime reflection. Desktop CI executes both and compares their JSON results. Mobile CI cross-compiles static assertions against the target compiler; this proves target layout, not physical-device behavior.

The offline program roots all 216 imports for NativeAOT compilation, verifies actual directional output and reconstruction and writes a float WAV. Static CI inspects executable dependencies and runs a copied executable in a clean directory without Phonon shared libraries. Dynamic NativeAOT remains a separate supported check.

After packing, `scripts/check-package.py` checks native file hashes inside the NuGet archive. If Linux x64 was selected, CI also restores a separate application using only the packed `PackageReference`, then publishes and executes each selected linkage mode with NativeAOT. This checks the transitive build targets and dependency archives without relying on a source project reference. The same script accepts `--target win-x64`, `osx-x64` or `osx-arm64` when run on the matching desktop host.

Artifacts include test reports, compiler ABI measurements, published samples, dependency listings and WAV output. GPU hardware, perceived headphone quality and physical mobile-device behavior are not inferred from CPU/offline tests. The native workflow is manual and configurable; inspect the actual run results before treating a target as verified.
