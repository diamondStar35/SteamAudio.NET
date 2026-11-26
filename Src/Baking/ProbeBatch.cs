namespace SteamAudio.Baking;
/// <summary>Owns a native ProbeBatch reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class ProbeBatch : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public ProbeBatch(Context parent, params Resource[] dependencies)
        : base(Create(parent), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    /// <summary>Number of probes currently stored.</summary>
    public int Count { get { using var value = Acquire(); return Interop.Api.ProbeBatchGetNumProbes(new(value.Value)); } }
    /// <summary>Adds an authored probe sphere; commit before baking or simulation.</summary>
    /// <param name="probe">Position and influence radius in world units.</param>
    public void Add(Interop.Sphere probe)
    {
        if (!float.IsFinite(probe.Radius) || probe.Radius <= 0) throw new ArgumentOutOfRangeException(nameof(probe));
        using var value = Acquire(); Interop.Api.ProbeBatchAddProbe(new(value.Value), probe);
    }
    /// <summary>Commits changed probe positions and data.</summary>
    public void Commit() { using var value = Acquire(); Interop.Api.ProbeBatchCommit(new(value.Value)); }
    /// <summary>Saves probe and baked data into an independently owned byte array.</summary>
    /// <param name="context">Context to allocate serialization storage.</param><returns>Serialized probe batch.</returns>
    public byte[] Save(Context context)
    {
        if (context.DangerousGetHandle() != ParentAddress) throw new ArgumentException("Foreign context.");
        using var storage = new SerializedObject(context); using var value = Acquire(); using var destination = storage.Acquire();
        Interop.Api.ProbeBatchSave(new(value.Value), new(destination.Value)); return storage.ToArray();
    }
    /// <summary>Loads serialized probe and baked data.</summary>
    /// <param name="context">Owning context.</param><param name="data">Complete serialized bytes.</param><returns>An owned probe batch.</returns>
    public static ProbeBatch Load(Context context, ReadOnlySpan<byte> data)
    {
        using var storage = new SerializedObject(context, data); using var c = context.Acquire(); using var source = storage.Acquire(); Interop.ProbeBatchHandle value;
        SteamAudioException.Check(Interop.Api.ProbeBatchLoad(new(c.Value), new(source.Value), &value), "Load probe batch"); return new(context, value.Value);
    }
    private ProbeBatch(Context context, nint value) : base(value, Release, context) => ParentAddress = context.DangerousGetHandle();
    /// <summary>Copies baked energy data at one probe into an existing field.</summary>
    /// <param name="identifier">Baked data selection.</param><param name="index">Probe index.</param><param name="field">Destination field in the same context.</param>
    public void GetEnergyField(Interop.BakedDataIdentifier identifier, int index, EnergyField field)
    {
        if ((uint)index >= (uint)Count || field.ParentAddress != ParentAddress) throw new ArgumentException("Invalid probe index or context.");
        using var batch = Acquire(); using var output = field.Acquire(); Interop.Api.ProbeBatchGetEnergyField(new(batch.Value), &identifier, index, new(output.Value));
    }
    /// <summary>Copies three baked reverb times in seconds from a probe.</summary>
    /// <param name="identifier">Baked data selection.</param><param name="index">Probe index.</param><param name="times">Storage for three bands.</param>
    public void GetReverb(Interop.BakedDataIdentifier identifier, int index, Span<float> times)
    {
        if ((uint)index >= (uint)Count || times.Length < 3) throw new ArgumentException("Invalid probe index or output storage.");
        using var batch = Acquire(); fixed (float* values = times) Interop.Api.ProbeBatchGetReverb(new(batch.Value), &identifier, index, values);
    }
    private static nint Create(Context parent)
    {
        using var lease = parent.Acquire(); Interop.ProbeBatchHandle value;
        SteamAudioException.Check(Interop.Api.ProbeBatchCreate(new Interop.ContextHandle(lease.Value), &value), "Create ProbeBatch"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.ProbeBatchHandle(value); Interop.Api.ProbeBatchRelease(&handle); }
}
