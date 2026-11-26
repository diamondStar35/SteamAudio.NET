# API and lifetime guidance

The public API is split into `SteamAudio` for context/HRTF/buffers, `Effects`, `Geometry`, `Simulation`, `Baking`, `Devices`, and `Interop`.

## Native contracts

`Interop.Api` uses handwritten `LibraryImport` declarations. The .NET SDK generates its normal interop stubs; there is no external header-to-runtime binding generator. SDK headers remain checked in as the audit source. Function pointers use the platform ABI on the supported 64-bit targets. `IPLsize` is `nuint`, native Boolean enums occupy four bytes and any-hit callback outputs remain single bytes. Strings are null-terminated UTF-8 pointers; managed SOFA path helpers encode UTF-8 explicitly.

`python scripts/verify.py api` independently compares every raw function signature with the C header and validates documentation XML. CI compares structure layouts against native compiler facts. Do not change the SDK pin, band count or layouts without updating and running that verification.

## Ownership

Every managed resource owns one native reference. Dependencies have retained safe-handle leases until the dependent native object is released. Each active processing call or borrowed buffer/data view also retains its allocation. Logical disposal is separate from delayed physical release: a view can remain valid after its original owner is disposed, while new calls on that owner are rejected.

A resource's `DangerousGetHandle()` is a borrowed address, not a retained reference. Native-settings constructors accept explicit resource dependencies for handles contained inside native settings. The library cannot discover the lifetime of arbitrary user pointers. Do not dispose or replace their external storage while native code may use it.

`SerializedObject` copies input bytes into retained unmanaged storage because the SDK borrows serialized input memory. Its exported arrays are independent snapshots. Native serialization destinations should be created empty. `FloatData` and `BufferView` are stack-only views: dispose exactly once and do not use copied spans afterward.

## Threading and callbacks

Effects and their native history belong to one processing operation at a time. A simulator and its source-control operations share a nonblocking operation gate. No global audio lock prevents independent effects or simulators from running concurrently.

Managed custom ray callbacks can be invoked concurrently on native workers. Their provider and material table remain rooted through the scene's retained native lifetime. They must not call SDK controls or dispose resources. Exceptions are contained at the unmanaged boundary and rethrown after the simulation call returns; no exception unwinds through C++.

Progress callbacks during synchronous baking use the same reentry restriction and deferred exception handling. Bake cancellation is cooperative. A process-wide bake gate prevents overlapping SDK bake/cancel operations. Callback pointers supplied directly through Interop are advanced caller contracts; static unmanaged callback code and user data must remain valid for the SDK lifetime.

## Processing

Buffers are planar F32. Processing calls preserve a fixed frame count; interleave/deinterleave operations copy explicitly. The common binaural wrapper validates finite, nonzero direction vectors and normalizes them. Raw parameter wrappers preserve the native controls and restrictions documented on their corresponding Interop types. In-place restrictions are enforced conservatively by effect owners.

Finish effects through `GetTail`, not by treating every short input chunk as a complete padded block. Native effect tails are not an automatic audio-device lifetime. Geometry, probe edits and source membership require explicit commits before simulation. Borrowed reflection/pathing output pointers must be consumed under the caller's simulation/render synchronization.
