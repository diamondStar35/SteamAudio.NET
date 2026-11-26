using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates an empty probe array.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="probeArray">The created probe array.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplProbeArrayCreate")]
    public static partial Error ProbeArrayCreate(ContextHandle context, ProbeArrayHandle* probeArray);

    /// <summary>
    /// Retains an additional reference to a probe array.
    /// </summary>
    /// <param name="probeArray">The probe array to retain a reference to.</param>
    /// <returns>The additional reference to the probe array.</returns>
    [LibraryImport(Library, EntryPoint = "iplProbeArrayRetain")]
    public static partial ProbeArrayHandle ProbeArrayRetain(ProbeArrayHandle probeArray);

    /// <summary>
    /// Releases a reference to a probe array.
    /// </summary>
    /// <param name="probeArray">The probe array to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeArrayRelease")]
    public static partial void ProbeArrayRelease(ProbeArrayHandle* probeArray);

    /// <summary>
    /// Generates probes and adds them to a probe array.
    /// </summary>
    /// <param name="scene">The scene in which to generate probes.</param>
    /// <param name="params">Parameters to use for generating probes.</param>
    /// <param name="probeArray">The array into which to add the generated probes.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeArrayGenerateProbes")]
    public static partial void ProbeArrayGenerateProbes(
        ProbeArrayHandle probeArray,
        SceneHandle scene,
        ProbeGenerationParams* @params);

    /// <summary>
    /// ProbeArrayGetNumProbes
    /// </summary>
    /// <returns>The number of probes in a probe array.</returns>
    /// <param name="probeArray">The probe array.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeArrayGetNumProbes")]
    public static partial int ProbeArrayGetNumProbes(ProbeArrayHandle probeArray);

    /// <summary>
    /// ProbeArrayGetProbe
    /// </summary>
    /// <returns>The probe at a given index in a probe array.</returns>
    /// <param name="probeArray">The probe array.</param>
    /// <param name="index">Index of the probe within the array.</param>
    [LibraryImport(Library, EntryPoint = "iplProbeArrayGetProbe")]
    public static partial Sphere ProbeArrayGetProbe(ProbeArrayHandle probeArray, int index);
}
