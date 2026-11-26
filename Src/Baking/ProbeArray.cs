namespace SteamAudio.Baking;

/// <summary>
/// Generates acoustic probe positions inside a transformed placement volume.
/// </summary>
public sealed unsafe class ProbeArray : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Creates an empty array to receive generated probe positions.
    /// </summary>
    /// <param name="parent">Context that owns this resource and is retained until its release.</param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public ProbeArray(Context parent, params Resource[] dependencies)
        : base(Create(parent), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
    }

    /// <summary>
    /// Generates probes using committed scene geometry.
    /// </summary>
    /// <param name="scene">Scene used for placement.</param>
    /// <param name="parameters">Placement strategy, spacing, height and volume transform.</param>
    public void Generate(Geometry.Scene scene, Interop.ProbeGenerationParams parameters)
    {
        using var array = Acquire();
        using var geometry = scene.Acquire();
        Interop.Api.ProbeArrayGenerateProbes(new(array.Value), new(geometry.Value), &parameters);
    }

    /// <summary>
    /// Number of generated probes.
    /// </summary>
    public int Count
    {
        get
        {
            using var array = Acquire();
            return Interop.Api.ProbeArrayGetNumProbes(new(array.Value));
        }
    }

    /// <summary>
    /// Retrieves a generated probe sphere.
    /// </summary>
    /// <param name="index">Zero-based probe index.</param>
    /// <returns>Copied sphere.</returns>
    public Interop.Sphere GetProbe(int index)
    {
        if ((uint)index >= (uint)Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        using var array = Acquire();
        return Interop.Api.ProbeArrayGetProbe(new(array.Value), index);
    }

    private static nint Create(Context parent)
    {
        using var lease = parent.Acquire();
        Interop.ProbeArrayHandle value;
        SteamAudioException.Check(
            Interop.Api.ProbeArrayCreate(new Interop.ContextHandle(lease.Value), &value),
            "Create ProbeArray");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.ProbeArrayHandle(value);
        Interop.Api.ProbeArrayRelease(&handle);
    }
}
