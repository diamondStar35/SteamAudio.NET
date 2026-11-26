# Migrating from the original IPL API

This is a breaking API redesign. The assembly remains SteamAudio.NET and its minimum managed target remains net8.0, but there is no nested `SteamAudio.IPL` compatibility class.

Use `Context`, `Hrtf`, `AudioBuffer` and the named owners in `SteamAudio.Effects`, `Geometry`, `Simulation`, `Baking` and `Devices` for owned native resources. Replace manual `Create`/`Release(ref handle)` sequences with `using` lifetimes. Use `Interop.Api` and the typed `*Handle` structures when direct SDK access is necessary.

Native settings and parameter structs live in `SteamAudio.Interop`. Fields preserve SDK layouts. Managed strings and delegates are no longer embedded in raw native structs: use UTF-8 pointers and unmanaged function pointers, or the managed SOFA/custom-ray/baking helpers. Use `nuint` for machine-sized quantities. The standard band count is three.

`AudioBuffer.Data` is not a managed array. Use `Borrow().Channel(index)` within a retained view, or explicit interleave/deinterleave helpers. The binaural wrapper takes a System.Numerics.Vector3 direction and retains its HRTF without exposing its handle for each block.

Source code using the old IPL names must be migrated explicitly. This phase does not modify any consuming game or audio engine. Remove stale references to the retired CodeGenerator/Natives projects and use the current solution and native asset targets.
