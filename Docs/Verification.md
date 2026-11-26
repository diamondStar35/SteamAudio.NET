# Verification

Run the managed/native suite after fetching the official SDK:

```powershell
python scripts/fetch-sdk.py
python scripts/verify-api.py
dotnet test Tests/Tests.csproj -c Release
dotnet run --project Samples/Offline -c Release -- output.wav
```

Tests use actual SDK processing with impulses and authored geometry. Assertions check directional HRTF energy, exact sample ordering, gain after ramps, finite terminating tails, geometry occlusion, changed transmission after material edits, reconstruction energy, immutable serialized input, callback errors and native lifetime through forced GC. No physical audio device is opened.

`Verification/abi.c` asks a C compiler for native sizes and field offsets. `Verification/AbiMeasurements.cs` uses managed unsafe sizeof and field addresses, without runtime reflection. Desktop CI executes both and compares their JSON results. Mobile CI cross-compiles static assertions against the target compiler; this proves target layout, not physical-device behavior.

The offline program roots all 216 imports for NativeAOT compilation, verifies actual directional output and reconstruction and writes a float WAV. Static CI inspects executable dependencies and runs a copied executable in a clean directory without Phonon shared libraries. Dynamic NativeAOT remains a separate supported check.

Artifacts include test reports, compiler ABI measurements, published samples, dependency listings and WAV output. GPU hardware, perceived headphone quality and physical mobile-device behavior are not inferred from CPU/offline tests. The native workflow is manual and configurable; inspect the actual run results before treating a target as verified.
