// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>A distance attenuation model that can be used for modeling attenuation of sound over distance. Can be used with both direct and indirect sound propagation.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct DistanceAttenuationModel
{
    /// <summary>The type of distance attenuation model to use.</summary>
    public DistanceAttenuationModelType Type;

    /// <summary>When <c>type</c> is <c>IPL_DISTANCEATTENUATIONTYPE_INVERSEDISTANCE</c>, no distance attenuation is applied to any sound whose distance from the listener is less than this value.</summary>
    public float MinDistance;

    /// <summary>When <c>type</c> is <c>IPL_DISTANCEATTENUATIONTYPE_CALLBACK</c>, this function will be called whenever Steam Audio needs to evaluate distance attenuation.</summary>
    public delegate* unmanaged<float, void*, float> Callback;

    /// <summary>Pointer to arbitrary data that will be provided to the <c>callback</c> function whenever it is called. May be <c>NULL.</c></summary>
    public void* UserData;

    /// <summary>Set to <c>IPL_TRUE</c> to indicate that the distance attenuation model defined by the <c>callback</c> function has changed since the last time simulation was run. For example, the callback may be evaluating a curve defined in a GUI. If the user is editing the curve in real-time, set this to <c>IPL_TRUE</c> whenever the curve changes, so Steam Audio can update simulation results to match.</summary>
    public Bool Dirty;
}

/// <summary>An air absorption model that can be used for modeling frequency-dependent attenuation of sound over distance.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AirAbsorptionModel
{
    /// <summary>The type of air absorption model to use.</summary>
    public AirAbsorptionModelType Type;

    /// <summary>The exponential falloff coefficients to use when <c>type</c> is <c>IPL_AIRABSORPTIONTYPE_EXPONENTIAL.</c></summary>
    public fixed float Coefficients[3];

    /// <summary>When <c>type</c> is <c>IPL_AIRABSORPTIONTYPE_CALLBACK</c>, this function will be called whenever Steam Audio needs to evaluate air absorption.</summary>
    public delegate* unmanaged<float, int, void*, float> Callback;

    /// <summary>Pointer to arbitrary data that will be provided to the <c>callback</c> function whenever it is called. May be <c>NULL.</c></summary>
    public void* UserData;

    /// <summary>Set to <c>IPL_TRUE</c> to indicate that the air absorption model defined by the <c>callback</c> function has changed since the last time simulation was run. For example, the callback may be evaluating a set of curves defined in a GUI. If the user is editing the curves in real-time, set this to <c>IPL_TRUE</c> whenever the curves change, so Steam Audio can update simulation results to match.</summary>
    public Bool Dirty;
}

/// <summary>A directivity pattern that can be used to model changes in sound intensity as a function of the source's orientation. Can be used with both direct and indirect sound propagation. The default directivity model is a weighted dipole. This is a linear blend between an omnidirectional source (which emits sound with equal intensity in all directions), and a dipole oriented along the z-axis in the source's coordinate system (which focuses sound along the +z and -z axes). A callback function can be specified to implement any other arbitrary directivity pattern.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct Directivity
{
    /// <summary>How much of the dipole to blend into the directivity pattern. <c>0</c> = pure omnidirectional, <c>1</c> = pure dipole. <c>0.5f</c> results in a cardioid directivity pattern.</summary>
    public float DipoleWeight;

    /// <summary>How "sharp" the dipole is. Higher values result in sound being focused within a narrower range of directions.</summary>
    public float DipolePower;

    /// <summary>If non <c>NULL</c>, this function will be called whenever Steam Audio needs to evaluate a directivity pattern.</summary>
    public delegate* unmanaged<Vector3, void*, float> Callback;

    /// <summary>Pointer to arbitrary data that will be provided to the <c>callback</c> function whenever it is called. May be <c>NULL.</c></summary>
    public void* UserData;
}

/// <summary>A deviation model that can be used for modeling frequency-dependent attenuation of sound as it bends along the path from the source to the listener.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct DeviationModel
{
    /// <summary>The type of deviation model to use.</summary>
    public DeviationModelType Type;

    /// <summary>When <c>type</c> is <c>IPL_DEVIATIONTYPE_CALLBACK</c>, this function will be called whenever Steam Audio needs to evaluate deviation-based attenuation.</summary>
    public delegate* unmanaged<float, int, void*, float> Callback;

    /// <summary>Pointer to arbitrary data that will be provided to the <c>callback</c> function whenever it is called. May be <c>NULL.</c></summary>
    public void* UserData;
}

/// <summary>Settings used to create a simulator.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SimulationSettings
{
    /// <summary>The types of simulation that this simulator will be used for.</summary>
    public SimulationFlags Flags;

    /// <summary>The type of scene that will be used for simulations via <c>iplSimulatorSetScene.</c> The scene type cannot change during the lifetime of a simulator object.</summary>
    public SceneType SceneType;

    /// <summary>The type of reflections effect that will be used to render the results of reflections simulation. The reflections effect type cannot change during the lifetime of a simulator object.</summary>
    public ReflectionEffectType ReflectionType;

    /// <summary>The maximum number of point samples to consider when calculating occlusion using the volumetric occlusion algorithm. Different sources can use different numbers of samples, and the number of samples can change between simulation runs, but this is the maximum value. Increasing this value results in smoother occlusion transitions, at the cost of increased CPU usage.</summary>
    public int MaxNumOcclusionSamples;

    /// <summary>The maximum number of rays to trace from the listener when simulating reflections. You can use different numbers of rays between simulation runs, but this is the maximum value. Increasing this value results in more accurate reflections, at the cost of increased CPU usage.</summary>
    public int MaxNumRays;

    /// <summary>The number of directions to sample when generating diffusely reflected rays. Increasing this value may increase the accuracy of diffuse reflections.</summary>
    public int NumDiffuseSamples;

    /// <summary>The maximum length (in seconds) of impulse responses generated by reflection simulations. You can change this value betweeen simulation runs, but this is the maximum value. Increasing this value results in longer, more accurate reverb tails, at the cost of increased CPU and memory usage.</summary>
    public float MaxDuration;

    /// <summary>The maximum Ambisonic order of impulse responses generated by reflection simulations. You can change this value between simulation runs, but this is the maximum value. Increasing this value results in more accurate directional variations in the impulse responses, at the cost of increased CPU and memory usage.</summary>
    public int MaxOrder;

    /// <summary>The maximum number of sources for which reflection simulations will be run at any given time.</summary>
    public int MaxNumSources;

    /// <summary>The number of threads used for real-time reflection simulations.</summary>
    public int NumThreads;

    /// <summary>If using custom ray tracer callbacks, this the number of rays that will be passed to the callbacks every time rays need to be traced.</summary>
    public int RayBatchSize;

    /// <summary>The number of point samples to consider when calculating probe-to-probe visibility for pathing simulations. Baked paths may end up being occluded by dynamic objects, in which case you can configure the simulator to look for alternate paths in real time. This process will involve checking visibility between probes.</summary>
    public int NumVisSamples;

    /// <summary>The sampling rate (in Hz) used for audio processing.</summary>
    public int SamplingRate;

    /// <summary>The size (in samples) of the audio buffers used for audio processing.</summary>
    public int FrameSize;

    /// <summary>The OpenCL device being used. Only necessary if \sceneType is <c>IPL_SCENETYPE_RADEONRAYS</c>, or <c>reflectionType</c> is <c>IPL_REFLECTIONEFFECTTYPE_TAN.</c></summary>
    public OpenCLDeviceHandle OpenCLDevice;

    /// <summary>The Radeon Rays device being used. Only necessary if \sceneType is <c>IPL_SCENETYPE_RADEONRAYS.</c></summary>
    public RadeonRaysDeviceHandle RadeonRaysDevice;

    /// <summary>The TrueAudio Next device being used. Only necessary if <c>reflectionType</c> is <c>IPL_REFLECTIONEFFECTTYPE_TAN.</c></summary>
    public TrueAudioNextDeviceHandle TanDevice;
}

/// <summary>Settings used to create a source.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SourceSettings
{
    /// <summary>The types of simulation that may be run for this source.</summary>
    public SimulationFlags Flags;
}

/// <summary>Simulation parameters for a source.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SimulationInputs
{
    /// <summary>The types of simulation to run for this source.</summary>
    public SimulationFlags Flags;

    /// <summary>The types of direct simulation to run for this source.</summary>
    public DirectSimulationFlags DirectFlags;

    /// <summary>The position and orientation of this source.</summary>
    public CoordinateSpace3 Source;

    /// <summary>The distance attenuation model to use for this source.</summary>
    public DistanceAttenuationModel DistanceAttenuationModel;

    /// <summary>The air absorption model to use for this source.</summary>
    public AirAbsorptionModel AirAbsorptionModel;

    /// <summary>The directivity pattern to use for this source.</summary>
    public Directivity Directivity;

    /// <summary>The occlusion algorithm to use for this source.</summary>
    public OcclusionType OcclusionType;

    /// <summary>If using volumetric occlusion, the source is modeled as a sphere with this radius.</summary>
    public float OcclusionRadius;

    /// <summary>If using volumetric occlusion, this is the number of point samples to consider when tracing rays. This value can change between simulation runs.</summary>
    public int NumOcclusionSamples;

    /// <summary>If using parametric or hybrid reverb for rendering reflections, the reverb decay times for each frequency band are scaled by these values. Set to <c>{1.0f, 1.0f, 1.0f}</c> to use the simulated values without modification.</summary>
    public fixed float ReverbScale[3];

    /// <summary>If using hybrid reverb for rendering reflections, this is the length (in seconds) of impulse response to use for convolution reverb. The rest of the impulse response will be used for parametric reverb estimation only. Increasing this value results in more accurate reflections, at the cost of increased CPU usage.</summary>
    public float HybridReverbTransitionTime;

    /// <summary>If using hybrid reverb for rendering reflections, this is the amount of overlap between the convolution and parametric parts. To ensure smooth transitions from the early convolution part to the late parametric part, the two are cross-faded towards the end of the convolution part. For example, if <c>hybridReverbTransitionTime</c> is <c>1.0f</c>, and <c>hybridReverbOverlapPercent</c> is <c>0.25f</c>, then the first 0.75 seconds are pure convolution, the next 0.25 seconds are a blend between convolution and parametric, and the portion of the tail beyond 1.0 second is pure parametric.</summary>
    public float HybridReverbOverlapPercent;

    /// <summary>If <c>IPL_TRUE</c>, this source will used baked data for reflections simulation.</summary>
    public Bool Baked;

    /// <summary>The identifier used to specify which layer of baked data to use for simulating reflections for this source.</summary>
    public BakedDataIdentifier BakedDataIdentifier;

    /// <summary>The probe batch within which to find paths from this source to the listener.</summary>
    public ProbeBatchHandle PathingProbes;

    /// <summary>When testing for mutual visibility between a pair of probes, each probe is treated as a sphere of this radius (in meters), and point samples are generated within this sphere.</summary>
    public float VisRadius;

    /// <summary>When tracing rays to test for mutual visibility between a pair of probes, the fraction of rays that are unoccluded must be greater than this threshold for the pair of probes to be considered mutually visible.</summary>
    public float VisThreshold;

    /// <summary>If the distance between two probes is greater than this value, the probes are not considered mutually visible. Increasing this value can result in simpler paths, at the cost of increased CPU usage.</summary>
    public float VisRange;

    /// <summary>If simulating pathing, this is the Ambisonic order used for representing path directionality. Higher values result in more precise spatialization of paths, at the cost of increased CPU usage.</summary>
    public int PathingOrder;

    /// <summary>If <c>IPL_TRUE</c>, baked paths are tested for visibility. This is useful if your scene has dynamic objects that might occlude baked paths.</summary>
    public Bool EnableValidation;

    /// <summary>If <c>IPL_TRUE</c>, and <c>enableValidation</c> is <c>IPL_TRUE</c>, then if a baked path is occluded by dynamic geometry, path finding is re-run in real-time to find alternate paths that take into account the dynamic geometry.</summary>
    public Bool FindAlternatePaths;

    /// <summary>If simulating transmission, this is the maximum number of surfaces, starting from the closest surface to the listener, whose transmission coefficients will be considered when calculating the total amount of sound transmitted. Increasing this value will result in more accurate results when multiple surfaces lie between the source and the listener, at the cost of increased CPU usage.</summary>
    public int NumTransmissionRays;

    /// <summary>The deviation model to use for this source. Only used when simulating pathing.</summary>
    public DeviationModel* DeviationModel;
}

/// <summary>Simulation parameters that are not specific to any source.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SimulationSharedInputs
{
    /// <summary>The position and orientation of the listener.</summary>
    public CoordinateSpace3 Listener;

    /// <summary>The number of rays to trace from the listener. Increasing this value results in more accurate reflections, at the cost of increased CPU usage.</summary>
    public int NumRays;

    /// <summary>The number of times each ray traced from the listener is reflected when it encounters a solid object. Increasing this value results in longer, more accurate reverb tails, at the cost of increased CPU usage during simulation.</summary>
    public int NumBounces;

    /// <summary>The duration (in seconds) of the impulse responses generated when simulating reflections. Increasing this value results in longer, more accurate reverb tails, at the cost of increased CPU usage during audio processing.</summary>
    public float Duration;

    /// <summary>The Ambisonic order of the impulse responses generated when simulating reflections. Increasing this value results in more accurate directional variation of reflected sound, at the cost of increased CPU usage during audio processing.</summary>
    public int Order;

    /// <summary>When calculating how much sound energy reaches a surface directly from a source, any source that is closer than <c>irradianceMinDistance</c> to the surface is assumed to be at a distance of <c>irradianceMinDistance</c>, for the purposes of energy calculations.</summary>
    public float IrradianceMinDistance;

    /// <summary>Callback for visualizing valid path segments during call to <c>iplSimulatorRunPathing.</c></summary>
    public delegate* unmanaged<Vector3, Vector3, Bool, void*, void> PathingVisCallback;

    /// <summary>Pointer to arbitrary user-specified data provided when calling the function that will call this callback.</summary>
    public void* PathingUserData;
}

/// <summary>Simulation results for a source.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SimulationOutputs
{
    /// <summary>Direct path simulation results.</summary>
    public DirectEffectParams Direct;

    /// <summary>Reflection simulation results.</summary>
    public ReflectionEffectParams Reflections;

    /// <summary>Pathing simulation results.</summary>
    public PathEffectParams Pathing;
}
