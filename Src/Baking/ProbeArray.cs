namespace SteamAudio.Baking;
/// <summary>Owns a native ProbeArray reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class ProbeArray : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public ProbeArray(Context parent, params Resource[] dependencies)
        : base(Create(parent), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    /// <summary>Generates probes using committed scene geometry.</summary>
    /// <param name="scene">Scene used for placement.</param><param name="parameters">Placement strategy, spacing, height and volume transform.</param>
    public void Generate(Geometry.Scene scene, Interop.ProbeGenerationParams parameters)
    { using var array = Acquire(); using var geometry = scene.Acquire(); Interop.Api.ProbeArrayGenerateProbes(new(array.Value), new(geometry.Value), &parameters); }
    /// <summary>Number of generated probes.</summary>
    public int Count { get { using var array = Acquire(); return Interop.Api.ProbeArrayGetNumProbes(new(array.Value)); } }
    /// <summary>Retrieves a generated probe sphere.</summary>
    /// <param name="index">Zero-based probe index.</param><returns>Copied sphere.</returns>
    public Interop.Sphere GetProbe(int index)
    { if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index)); using var array = Acquire(); return Interop.Api.ProbeArrayGetProbe(new(array.Value), index); }
    private static nint Create(Context parent)
    {
        using var lease = parent.Acquire(); Interop.ProbeArrayHandle value;
        SteamAudioException.Check(Interop.Api.ProbeArrayCreate(new Interop.ContextHandle(lease.Value), &value), "Create ProbeArray"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.ProbeArrayHandle(value); Interop.Api.ProbeArrayRelease(&handle); }
}
