// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Creates an empty probe batch.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="probeBatch">The created probe batch.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchCreate")]
    public static partial Error ProbeBatchCreate(ContextHandle context, ProbeBatchHandle* probeBatch);

    /// <summary>Retains an additional reference to a probe batch.</summary>
    /// <param name="probeBatch">The probe batch to retain a reference to.</param>
    /// <returns>The additional reference to the probe batch.</returns>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchRetain")]
    public static partial ProbeBatchHandle ProbeBatchRetain(ProbeBatchHandle probeBatch);

    /// <summary>Releases a reference to a probe batch.</summary>
    /// <param name="probeBatch">The probe batch to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchRelease")]
    public static partial void ProbeBatchRelease(ProbeBatchHandle* probeBatch);

    /// <summary>Loads a probe batch from a serialized object. Typically, the serialized object will be created from a byte array loaded from disk or over the network.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="serializedObject">The serialized object from which to load the probe batch.</param>
    /// <param name="probeBatch">The created probe batch.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchLoad")]
    public static partial Error ProbeBatchLoad(ContextHandle context, SerializedObjectHandle serializedObject, ProbeBatchHandle* probeBatch);

    /// <summary>Saves a probe batch to a serialized object. Typically, the serialized object will then be saved to disk.</summary>
    /// <param name="probeBatch">The probe batch to save.</param>
    /// <param name="serializedObject">The serialized object into which to save the probe batch.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchSave")]
    public static partial void ProbeBatchSave(ProbeBatchHandle probeBatch, SerializedObjectHandle serializedObject);

    /// <summary>ProbeBatchGetNumProbes</summary>
    /// <returns>The number of probes in a probe batch.</returns>
    /// <param name="probeBatch">The probe batch.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchGetNumProbes")]
    public static partial int ProbeBatchGetNumProbes(ProbeBatchHandle probeBatch);

    /// <summary>Adds a probe to a batch. The new probe will be added as the last probe in the batch.</summary>
    /// <param name="probeBatch">The probe batch.</param>
    /// <param name="probe">The probe to add.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchAddProbe")]
    public static partial void ProbeBatchAddProbe(ProbeBatchHandle probeBatch, Sphere probe);

    /// <summary>Adds every probe in an array to a batch. The new probes will be added, in order, at the end of the batch.</summary>
    /// <param name="probeBatch">The probe batch.</param>
    /// <param name="probeArray">The probe array containing the probes to add.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchAddProbeArray")]
    public static partial void ProbeBatchAddProbeArray(ProbeBatchHandle probeBatch, ProbeArrayHandle probeArray);

    /// <summary>Removes a probe from a batch.</summary>
    /// <param name="probeBatch">The probe batch.</param>
    /// <param name="index">Index of the probe to remove.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchRemoveProbe")]
    public static partial void ProbeBatchRemoveProbe(ProbeBatchHandle probeBatch, int index);

    /// <summary>Commits all changes made to a probe batch since this function was last called (or since the probe batch was first created, if this function was never called). This function must be called after adding, removing, or updating any probes in the batch, for the changes to take effect.</summary>
    /// <param name="probeBatch">The probe batch.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchCommit")]
    public static partial void ProbeBatchCommit(ProbeBatchHandle probeBatch);

    /// <summary>Deletes a specific layer of data from a probe batch.</summary>
    /// <param name="probeBatch">The probe batch.</param>
    /// <param name="identifier">The identifier of the baked data layer to delete.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchRemoveData")]
    public static partial void ProbeBatchRemoveData(ProbeBatchHandle probeBatch, BakedDataIdentifier* identifier);

    /// <summary>ProbeBatchGetDataSize</summary>
    /// <returns>The size (in bytes) of a specific baked data layer in a probe batch.</returns>
    /// <param name="probeBatch">The probe batch.</param>
    /// <param name="identifier">The identifier of the baked data layer.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchGetDataSize")]
    public static partial nuint ProbeBatchGetDataSize(ProbeBatchHandle probeBatch, BakedDataIdentifier* identifier);

    /// <summary>Retrieves a single energy field in a specific baked data layer of a specific probe in a probe batch.</summary>
    /// <param name="probeBatch">The probe batch.</param>
    /// <param name="identifier">The identifier of the baked data layer.</param>
    /// <param name="probeIndex">The index of the probe.</param>
    /// <param name="energyField">The energy field into which to copy the baked energy field.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchGetEnergyField")]
    public static partial void ProbeBatchGetEnergyField(ProbeBatchHandle probeBatch, BakedDataIdentifier* identifier, int probeIndex, EnergyFieldHandle energyField);

    /// <summary>Retrieves a single array of parametric reverb times in a specific baked data layer of a specific probe in a probe batch.</summary>
    /// <param name="probeBatch">The probe batch.</param>
    /// <param name="identifier">The identifier of the baked data layer.</param>
    /// <param name="probeIndex">The index of the probe.</param>
    /// <param name="reverbTimes">Pointer to an array containing at least 3 IPLfloat32 values, into which the baked reverb times will be copied.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeBatchGetReverb")]
    public static partial void ProbeBatchGetReverb(ProbeBatchHandle probeBatch, BakedDataIdentifier* identifier, int probeIndex, float* reverbTimes);
}
