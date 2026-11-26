// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Borrowed native Context identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ContextHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public ContextHandle(nint value) => Value = value;
}

/// <summary>Borrowed native SerializedObject identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SerializedObjectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public SerializedObjectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native EmbreeDevice identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct EmbreeDeviceHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public EmbreeDeviceHandle(nint value) => Value = value;
}

/// <summary>Borrowed native OpenCLDeviceList identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct OpenCLDeviceListHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public OpenCLDeviceListHandle(nint value) => Value = value;
}

/// <summary>Borrowed native OpenCLDevice identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct OpenCLDeviceHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public OpenCLDeviceHandle(nint value) => Value = value;
}

/// <summary>Borrowed native RadeonRaysDevice identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct RadeonRaysDeviceHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public RadeonRaysDeviceHandle(nint value) => Value = value;
}

/// <summary>Borrowed native TrueAudioNextDevice identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TrueAudioNextDeviceHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public TrueAudioNextDeviceHandle(nint value) => Value = value;
}

/// <summary>Borrowed native Scene identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SceneHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public SceneHandle(nint value) => Value = value;
}

/// <summary>Borrowed native StaticMesh identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct StaticMeshHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public StaticMeshHandle(nint value) => Value = value;
}

/// <summary>Borrowed native InstancedMesh identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct InstancedMeshHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public InstancedMeshHandle(nint value) => Value = value;
}

/// <summary>Borrowed native Hrtf identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct HrtfHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public HrtfHandle(nint value) => Value = value;
}

/// <summary>Borrowed native PanningEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct PanningEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public PanningEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native BinauralEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct BinauralEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public BinauralEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native VirtualSurroundEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct VirtualSurroundEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public VirtualSurroundEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native AmbisonicsEncodeEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct AmbisonicsEncodeEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public AmbisonicsEncodeEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native AmbisonicsPanningEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct AmbisonicsPanningEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public AmbisonicsPanningEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native AmbisonicsBinauralEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct AmbisonicsBinauralEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public AmbisonicsBinauralEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native AmbisonicsRotationEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct AmbisonicsRotationEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public AmbisonicsRotationEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native AmbisonicsDecodeEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct AmbisonicsDecodeEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public AmbisonicsDecodeEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native DirectEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct DirectEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public DirectEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native ReflectionEffectIR identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ReflectionEffectIRHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public ReflectionEffectIRHandle(nint value) => Value = value;
}

/// <summary>Borrowed native ReflectionEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ReflectionEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public ReflectionEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native ReflectionMixer identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ReflectionMixerHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public ReflectionMixerHandle(nint value) => Value = value;
}

/// <summary>Borrowed native PathEffect identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct PathEffectHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public PathEffectHandle(nint value) => Value = value;
}

/// <summary>Borrowed native EnergyField identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct EnergyFieldHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public EnergyFieldHandle(nint value) => Value = value;
}

/// <summary>Borrowed native ImpulseResponse identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ImpulseResponseHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public ImpulseResponseHandle(nint value) => Value = value;
}

/// <summary>Borrowed native Reconstructor identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ReconstructorHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public ReconstructorHandle(nint value) => Value = value;
}

/// <summary>Borrowed native ProbeArray identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ProbeArrayHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public ProbeArrayHandle(nint value) => Value = value;
}

/// <summary>Borrowed native ProbeBatch identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ProbeBatchHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public ProbeBatchHandle(nint value) => Value = value;
}

/// <summary>Borrowed native Source identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SourceHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public SourceHandle(nint value) => Value = value;
}

/// <summary>Borrowed native Simulator identity. Use the managed owner for automatic lifetime management.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SimulatorHandle
{
    /// <summary>Native address; zero represents a null handle.</summary>
    public readonly nint Value;
    /// <summary>Wraps an existing address without retaining or releasing it.</summary>
    /// <param name="value">Native address.</param>
    public SimulatorHandle(nint value) => Value = value;
}
