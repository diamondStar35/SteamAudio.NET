using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>
/// Settings used to create an energy field.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EnergyFieldSettings
{
    /// <summary>
    /// Total duration (in seconds) of the energy field. This determines the number of bins in each channel and
    /// band.
    /// </summary>
    public float Duration;

    /// <summary>
    /// The Ambisonic order. This determines the number of channels.
    /// </summary>
    public int Order;
}

/// <summary>
/// Settings used to create an impulse response.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImpulseResponseSettings
{
    /// <summary>
    /// Total duration (in seconds) of the impulse response. This determines the number of samples in each
    /// channel.
    /// </summary>
    public float Duration;

    /// <summary>
    /// The Ambisonic order. This determines the number of channels.
    /// </summary>
    public int Order;

    /// <summary>
    /// The sampling rate. This, together with the duration, determines the number of samples in each channel.
    /// </summary>
    public int SamplingRate;
}

/// <summary>
/// Settings used to create a reconstructor.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ReconstructorSettings
{
    /// <summary>
    /// The largest possible duration (in seconds) of any impulse response that will be reconstructed using this
    /// reconstructor.
    /// </summary>
    public float MaxDuration;

    /// <summary>
    /// The largest possible Ambisonic order of any impulse response that will be reconstructed using this
    /// reconstructor.
    /// </summary>
    public int MaxOrder;

    /// <summary>
    /// The sampling rate of impulse responses reconstructed using this reconstructor.
    /// </summary>
    public int SamplingRate;
}

/// <summary>
/// The inputs for a single reconstruction operation.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ReconstructorInputs
{
    /// <summary>
    /// The energy field from which to reconstruct an impulse response.
    /// </summary>
    public EnergyFieldHandle EnergyField;
}

/// <summary>
/// Inputs common to all reconstruction operations specified in a single call to
/// <c>iplReconstructorReconstruct.</c>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ReconstructorSharedInputs
{
    /// <summary>
    /// Duration of impulse responses to reconstruct. Must be less than or equal to <c>maxDuration</c> specified
    /// in <c>IPLReconstructorSettings.</c>
    /// </summary>
    public float Duration;

    /// <summary>
    /// Ambisonic order of impulse responses to reconstruct. Must be less than or equal to <c>maxOrder</c>
    /// specified in <c>IPLReconstructorSettings.</c>
    /// </summary>
    public int Order;
}

/// <summary>
/// The outputs for a single reconstruction operation.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ReconstructorOutputs
{
    /// <summary>
    /// The impulse response object into which the reconstructed impulse repsonse should be stored.
    /// </summary>
    public ImpulseResponseHandle ImpulseResponse;
}

/// <summary>
/// Settings used to generate probes.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ProbeGenerationParams
{
    /// <summary>
    /// The algorithm to use for generating probes.
    /// </summary>
    public ProbeGenerationType Type;

    /// <summary>
    /// Spacing (in meters) between two neighboring probes. Only for
    /// <c>IPL_PROBEGENERATIONTYPE_UNIFORMFLOOR.</c>
    /// </summary>
    public float Spacing;

    /// <summary>
    /// Height (in meters) above the floor at which probes will be generated. Only for
    /// <c>IPL_PROBEGENERATIONTYPE_UNIFORMFLOOR.</c>
    /// </summary>
    public float Height;

    /// <summary>
    /// A transformation matrix that transforms an axis-aligned unit cube, with minimum and maximum vertices at
    /// (0, 0, 0) and (1, 1, 1), into a parallelopiped volume. Probes will be generated within this volume.
    /// </summary>
    public Matrix4x4 Transform;
}

/// <summary>
/// Identifies a "layer" of data stored in a probe batch. Each probe batch may store multiple layers of data,
/// such as reverb, static source reflections, or pathing. Each layer can be accessed using an identifier.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct BakedDataIdentifier
{
    /// <summary>
    /// The type of data stored.
    /// </summary>
    public BakedDataType Type;

    /// <summary>
    /// The way in which source and listener positions depend on probe position.
    /// </summary>
    public BakedDataVariation Variation;

    /// <summary>
    /// The static source (for <c>IPL_BAKEDDATAVARIATION_STATICSOURCE</c>) or static listener (for
    /// <c>IPL_BAKEDDATAVARIATION_STATICLISTENER</c>) used to generate baked data. Baked data is only stored for
    /// probes that lie within the radius of this sphere.
    /// </summary>
    public Sphere EndpointInfluence;
}

/// <summary>
/// Parameters used to control how reflections data is baked.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ReflectionsBakeParams
{
    /// <summary>
    /// The scene in which the probes exist.
    /// </summary>
    public SceneHandle Scene;

    /// <summary>
    /// A probe batch containing the probes at which reflections data should be baked.
    /// </summary>
    public ProbeBatchHandle ProbeBatch;

    /// <summary>
    /// The type of scene being used.
    /// </summary>
    public SceneType SceneType;

    /// <summary>
    /// An identifier for the data layer that should be baked. The identifier determines what data is simulated
    /// and stored at each probe. If the probe batch already contains data with this identifier, it will be
    /// overwritten.
    /// </summary>
    public BakedDataIdentifier Identifier;

    /// <summary>
    /// The types of data to save for each probe.
    /// </summary>
    public ReflectionsBakeFlags BakeFlags;

    /// <summary>
    /// The number of rays to trace from each listener position when baking. Increasing this number results in
    /// improved accuracy, at the cost of increased bake times.
    /// </summary>
    public int NumRays;

    /// <summary>
    /// The number of directions to consider when generating diffusely-reflected rays when baking. Increasing
    /// this number results in slightly improved accuracy of diffuse reflections.
    /// </summary>
    public int NumDiffuseSamples;

    /// <summary>
    /// The number of times each ray is reflected off of solid geometry. Increasing this number results in
    /// longer reverb tails and improved accuracy, at the cost of increased bake times.
    /// </summary>
    public int NumBounces;

    /// <summary>
    /// The length (in seconds) of the impulse responses to simulate. Increasing this number allows the baked
    /// data to represent longer reverb tails (and hence larger spaces), at the cost of increased memory usage
    /// while baking.
    /// </summary>
    public float SimulatedDuration;

    /// <summary>
    /// The length (in seconds) of the impulse responses to save at each probe. Increasing this number allows
    /// the baked data to represent longer reverb tails (and hence larger spaces), at the cost of increased disk
    /// space usage and memory usage at run-time. It may be useful to set <c>savedDuration</c> to be less than
    /// <c>simulatedDuration</c>, especially if you plan to use hybrid reverb for rendering baked reflections.
    /// This way, the parametric reverb data is estimated using a longer IR, resulting in more accurate
    /// estimation, but only the early part of the IR can be saved for subsequent rendering.
    /// </summary>
    public float SavedDuration;

    /// <summary>
    /// Ambisonic order of the baked IRs.
    /// </summary>
    public int Order;

    /// <summary>
    /// Number of threads to use for baking.
    /// </summary>
    public int NumThreads;

    /// <summary>
    /// If using custom ray tracer callbacks, this the number of rays that will be passed to the callbacks every
    /// time rays need to be traced.
    /// </summary>
    public int RayBatchSize;

    /// <summary>
    /// When calculating how much sound energy reaches a surface directly from a source, any source that is
    /// closer than <c>irradianceMinDistance</c> to the surface is assumed to be at a distance of
    /// <c>irradianceMinDistance</c>, for the purposes of energy calculations.
    /// </summary>
    public float IrradianceMinDistance;

    /// <summary>
    /// If using Radeon Rays or if <c>identifier.variation</c> is <c>IPL_BAKEDDATAVARIATION_STATICLISTENER</c>,
    /// this is the number of probes for which data is baked simultaneously.
    /// </summary>
    public int BakeBatchSize;

    /// <summary>
    /// The OpenCL device, if using Radeon Rays.
    /// </summary>
    public OpenCLDeviceHandle OpenCLDevice;

    /// <summary>
    /// The Radeon Rays device, if using Radeon Rays.
    /// </summary>
    public RadeonRaysDeviceHandle RadeonRaysDevice;
}

/// <summary>
/// Parameters used to control how pathing data is baked.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct PathBakeParams
{
    /// <summary>
    /// The scene in which the probes exist.
    /// </summary>
    public SceneHandle Scene;

    /// <summary>
    /// A probe batch containing the probes for which pathing data should be baked.
    /// </summary>
    public ProbeBatchHandle ProbeBatch;

    /// <summary>
    /// An identifier for the data layer that should be baked. The identifier determines what data is simulated
    /// and stored at each probe. If the probe batch already contains data with this identifier, it will be
    /// overwritten.
    /// </summary>
    public BakedDataIdentifier Identifier;

    /// <summary>
    /// Number of point samples to use around each probe when testing whether one probe can see another. To
    /// determine if two probes are mutually visible, numSamples * numSamples rays are traced, from each point
    /// sample of the first probe, to every other point sample of the second probe.
    /// </summary>
    public int NumSamples;

    /// <summary>
    /// When testing for mutual visibility between a pair of probes, each probe is treated as a sphere of this
    /// radius (in meters), and point samples are generated within this sphere.
    /// </summary>
    public float Radius;

    /// <summary>
    /// When tracing rays to test for mutual visibility between a pair of probes, the fraction of rays that are
    /// unoccluded must be greater than this threshold for the pair of probes to be considered mutually visible.
    /// </summary>
    public float Threshold;

    /// <summary>
    /// If the distance between two probes is greater than this value, the probes are not considered mutually
    /// visible. Increasing this value can result in simpler paths, at the cost of increased bake times.
    /// </summary>
    public float VisRange;

    /// <summary>
    /// If the length of the path between two probes is greater than this value, the probes are considered to
    /// not have any path between them. Increasing this value allows sound to propagate over greater distances,
    /// at the cost of increased bake times and memory usage.
    /// </summary>
    public float PathRange;

    /// <summary>
    /// Number of threads to use for baking.
    /// </summary>
    public int NumThreads;
}
