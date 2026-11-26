// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Describes a standard or custom speaker layout.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SpeakerLayout
{
    /// <summary>See <c>IPLSpeakerLayoutType.</c></summary>
    public SpeakerLayoutType Type;

    /// <summary>Number of speakers. Only for IPL_SPEAKERLAYOUTTYPE_CUSTOM.</summary>
    public int NumSpeakers;

    /// <summary>Array of unit-length directions for each speaker. Only for IPL_SPEAKERLAYOUTTYPE_CUSTOM.</summary>
    public Vector3* Speakers;
}

/// <summary>Settings used to create a panning effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct PanningEffectSettings
{
    /// <summary>The speaker layout to pan input audio to.</summary>
    public SpeakerLayout SpeakerLayout;
}

/// <summary>Parameters for applying a panning effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct PanningEffectParams
{
    /// <summary>Unit vector pointing from the listener towards the source.</summary>
    public Vector3 Direction;
}

/// <summary>Settings used to create a binaural effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct BinauralEffectSettings
{
    /// <summary>The HRTF to use.</summary>
    public HrtfHandle Hrtf;
}

/// <summary>Parameters for applying a binaural effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct BinauralEffectParams
{
    /// <summary>Unit vector pointing from the listener towards the source.</summary>
    public Vector3 Direction;

    /// <summary>The interpolation technique to use.</summary>
    public HrtfInterpolation Interpolation;

    /// <summary>Amount to blend input audio with spatialized audio. When set to 0, output audio is not spatialized at all and is close to input audio. If set to 1, output audio is fully spatialized.</summary>
    public float SpatialBlend;

    /// <summary>The HRTF to use.</summary>
    public HrtfHandle Hrtf;

    /// <summary>Base address of an array into which to write the left- and right-ear peak delays for the HRTF used to spatialize the input audio. Memory for this array must be allocated and managed by the caller. Can be NULL, in which case peak delays will not be written.</summary>
    public float* PeakDelays;
}

/// <summary>Settings used to create a virtual surround effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct VirtualSurroundEffectSettings
{
    /// <summary>The speaker layout that will be used by input audio buffers.</summary>
    public SpeakerLayout SpeakerLayout;

    /// <summary>The HRTF to use.</summary>
    public HrtfHandle Hrtf;
}

/// <summary>Parameters for applying a virtual surround effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct VirtualSurroundEffectParams
{
    /// <summary>The HRTF to use.</summary>
    public HrtfHandle Hrtf;
}

/// <summary>Settings used to create an Ambisonics encode effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsEncodeEffectSettings
{
    /// <summary>Maximum Ambisonics order to encode audio buffers to.</summary>
    public int MaxOrder;
}

/// <summary>Parameters for applying an Ambisonics encode effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsEncodeEffectParams
{
    /// <summary>Vector pointing from the listener towards the source. Need not be normalized; Steam Audio will automatically normalize this vector. If a zero-length vector is passed, the output will be order 0 (omnidirectional).</summary>
    public Vector3 Direction;

    /// <summary>Ambisonic order of the output buffer. May be less than the <c>maxOrder</c> specified when creating the effect, in which case the effect will generate fewer output channels, reducing CPU usage.</summary>
    public int Order;
}

/// <summary>Settings used to create an Ambisonics panning effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsPanningEffectSettings
{
    /// <summary>The speaker layout that will be used by output audio buffers.</summary>
    public SpeakerLayout SpeakerLayout;

    /// <summary>The maximum Ambisonics order that will be used by input audio buffers.</summary>
    public int MaxOrder;
}

/// <summary>Parameters for applying an Ambisonics panning effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsPanningEffectParams
{
    /// <summary>Ambisonic order of the input buffer. May be less than the <c>maxOrder</c> specified when creating the effect, in which case the effect will process fewer input channels, reducing CPU usage.</summary>
    public int Order;
}

/// <summary>Settings used to create an Ambisonics binaural effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsBinauralEffectSettings
{
    /// <summary>The HRTF to use.</summary>
    public HrtfHandle Hrtf;

    /// <summary>The maximum Ambisonics order that will be used by input audio buffers.</summary>
    public int MaxOrder;
}

/// <summary>Parameters for applying an Ambisonics binaural effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsBinauralEffectParams
{
    /// <summary>The HRTF to use.</summary>
    public HrtfHandle Hrtf;

    /// <summary>Ambisonic order of the input buffer. May be less than the <c>maxOrder</c> specified when creating the effect, in which case the effect will process fewer input channels, reducing CPU usage.</summary>
    public int Order;
}

/// <summary>Settings used to create an Ambisonics rotation effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsRotationEffectSettings
{
    /// <summary>The maximum Ambisonics order that will be used by input audio buffers.</summary>
    public int MaxOrder;
}

/// <summary>Parameters for applying an Ambisonics rotation effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsRotationEffectParams
{
    /// <summary>The orientation of the listener.</summary>
    public CoordinateSpace3 Orientation;

    /// <summary>Ambisonic order of the input and output buffers. May be less than the <c>maxOrder</c> specified when creating the effect, in which case the effect will process fewer channels, reducing CPU usage.</summary>
    public int Order;
}

/// <summary>Settings used to create an Ambisonics decode effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsDecodeEffectSettings
{
    /// <summary>The speaker layout that will be used by output audio buffers.</summary>
    public SpeakerLayout SpeakerLayout;

    /// <summary>The HRTF to use.</summary>
    public HrtfHandle Hrtf;

    /// <summary>The maximum Ambisonics order that will be used by input audio buffers.</summary>
    public int MaxOrder;
}

/// <summary>Parameters for applying an Ambisonics decode effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct AmbisonicsDecodeEffectParams
{
    /// <summary>Ambisonic order of the input buffer. May be less than the <c>maxOrder</c> specified when creating the effect, in which case the effect will process fewer input channels, reducing CPU usage.</summary>
    public int Order;

    /// <summary>The HRTF to use.</summary>
    public HrtfHandle Hrtf;

    /// <summary>The orientation of the listener.</summary>
    public CoordinateSpace3 Orientation;

    /// <summary>Whether to use binaural rendering or panning.</summary>
    public Bool Binaural;
}

/// <summary>Settings used to create a direct effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct DirectEffectSettings
{
    /// <summary>Number of channels that will be used by input and output buffers.</summary>
    public int NumChannels;
}

/// <summary>Parameters for applying a direct effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct DirectEffectParams
{
    /// <summary>Flags indicating which direct path effects to apply.</summary>
    public DirectEffectFlags Flags;

    /// <summary>Mode of applying transmission effect, if <c>IPL_DIRECTEFFECTFLAGS_APPLYTRANSMISSION</c> is enabled.</summary>
    public TransmissionType TransmissionType;

    /// <summary>Value of distance attenuation, between 0 and 1.</summary>
    public float DistanceAttenuation;

    /// <summary>3-band EQ coefficients for air absorption, each between 0 and 1.</summary>
    public fixed float AirAbsorption[3];

    /// <summary>Value of directivity term, between 0 and 1.</summary>
    public float Directivity;

    /// <summary>Value of occlusion factor, between 0 and 1.</summary>
    public float Occlusion;

    /// <summary>3-band EQ coefficients for transmission, each between 0 and 1.</summary>
    public fixed float Transmission[3];
}

/// <summary>Settings used to create a reflection effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ReflectionEffectSettings
{
    /// <summary>Type of reflection effect algorithm to use.</summary>
    public ReflectionEffectType Type;

    /// <summary>Number of samples per channel in the IR.</summary>
    public int IrSize;

    /// <summary>Number of channels in the IR.</summary>
    public int NumChannels;
}

/// <summary>Parameters for applying a reflection effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ReflectionEffectParams
{
    /// <summary>Type of reflection effect algorithm to use.</summary>
    public ReflectionEffectType Type;

    /// <summary>The impulse response. For <c>IPL_REFLECTIONEFFECTTYPE_CONVOLUTION</c> or <c>IPL_REFLECTIONEFFECTTYPE_HYBRID.</c></summary>
    public ReflectionEffectIRHandle Ir;

    /// <summary>3-band reverb decay times (RT60). For <c>IPL_REFLECTIONEFFECTTYPE_PARAMETRIC</c> or <c>IPL_REFLECTIONEFFECTTYPE_HYBRID.</c></summary>
    public fixed float ReverbTimes[3];

    /// <summary>3-band EQ coefficients applied to the parametric part to ensure smooth transition. For <c>IPL_REFLECTIONEFFECTTYPE_HYBRID.</c></summary>
    public fixed float Eq[3];

    /// <summary>Samples after which parametric part starts. For <c>IPL_REFLECTIONEFFECTTYPE_HYBRID.</c></summary>
    public int Delay;

    /// <summary>Number of IR channels to process. May be less than the number of channels specified when creating the effect, in which case CPU usage will be reduced.</summary>
    public int NumChannels;

    /// <summary>Number of IR samples per channel to process. May be less than the number of samples specified when creating the effect, in which case CPU usage will be reduced.</summary>
    public int IrSize;

    /// <summary>The TrueAudio Next device to use for convolution processing. For <c>IPL_REFLECTIONEFFECTTYPE_TAN.</c></summary>
    public TrueAudioNextDeviceHandle TanDevice;

    /// <summary>The TrueAudio Next slot index to use for convolution processing. The slot identifies the IR to use. For <c>IPL_REFLECTIONEFFECTTYPE_TAN.</c></summary>
    public int TanSlot;
}

/// <summary>Settings used to create a path effect.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct PathEffectSettings
{
    /// <summary>The maximum Ambisonics order that will be used by output audio buffers.</summary>
    public int MaxOrder;

    /// <summary>If <c>IPL_TRUE</c>, then this effect will render spatialized audio into the output buffer. If <c>IPL_FALSE</c>, this effect will render un-spatialized (and un-rotated) Ambisonic audio. Setting this to <c>IPL_FALSE</c> is mainly useful only if you plan to mix multiple Ambisonic buffers and/or apply additional processing to the Ambisonic audio before spatialization. If you plan to immediately spatialize the output of the path effect, setting this value to <c>IPL_TRUE</c> can result in significant performance improvements.</summary>
    public Bool Spatialize;

    /// <summary>The speaker layout to use when spatializing. Only used if <c>spatialize</c> is <c>IPL_TRUE.</c></summary>
    public SpeakerLayout SpeakerLayout;

    /// <summary>The HRTF to use when spatializing. Only used if <c>spatialize</c> is <c>IPL_TRUE.</c></summary>
    public HrtfHandle Hrtf;
}

/// <summary>Parameters for applying a path effect to an audio buffer.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct PathEffectParams
{
    /// <summary>3-band EQ coefficients for modeling frequency-dependent attenuation caused by paths bending around obstacles.</summary>
    public fixed float EqCoeffs[3];

    /// <summary>Ambisonic coefficients for modeling the directional distribution of sound reaching the listener. The coefficients are specified in world-space, and must be rotated to match the listener's orientation separately.</summary>
    public float* ShCoeffs;

    /// <summary>Ambisonic order of the output buffer. May be less than the maximum order specified when creating the effect, in which case higher-order <c>shCoeffs</c> will be ignored, and CPU usage will be reduced.</summary>
    public int Order;

    /// <summary>If <c>IPL_TRUE</c>, spatialize using HRTF-based binaural rendering. Only used if <c>spatialize</c> was set to <c>IPL_TRUE</c> in <c>IPLPathEffectSettings.</c></summary>
    public Bool Binaural;

    /// <summary>The HRTF to use when spatializing. Only used if <c>spatialize</c> was set to <c>IPL_TRUE</c> in <c>IPLPathEffectSettings</c> and <c>binaural</c> is set to <c>IPL_TRUE.</c></summary>
    public HrtfHandle Hrtf;

    /// <summary>The position and orientation of the listener. Only used if <c>spatialize</c> was set to <c>IPL_TRUE</c> in <c>IPLPathEffectSettings</c> and <c>binaural</c> is set to <c>IPL_TRUE.</c></summary>
    public CoordinateSpace3 Listener;

    /// <summary>If <c>IPL_TRUE</c>, the values in <c>eqCoeffs</c> will be normalized before being used, i.e., each value in <c>eqCoeffs</c> will be divided by the largest value in <c>eqCoeffs.</c> This can help counteract overly-aggressive filtering due to a physics-based deviation model. If <c>IPL_FALSE</c>, the values in <c>eqCoeffs</c> will be used as-is.</summary>
    public Bool NormalizeEQ;
}
