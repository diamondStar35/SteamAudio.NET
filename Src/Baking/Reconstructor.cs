namespace SteamAudio.Baking;
/// <summary>Owns a native Reconstructor reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class Reconstructor : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public Reconstructor(Context parent, Interop.ReconstructorSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    /// <summary>Reconstructs a measured energy field into a time-domain impulse response.</summary>
    /// <param name="energy">Input field from the same context.</param><param name="output">Output response with sufficient duration, order and sampling rate.</param><param name="duration">Reconstruction duration in seconds.</param><param name="order">Ambisonic order, no greater than creation limits.</param>
    public void Reconstruct(EnergyField energy, ImpulseResponse output, float duration, int order)
    {
        if (!float.IsFinite(duration) || duration <= 0 || order < 0 || energy.ParentAddress != ParentAddress || output.ParentAddress != ParentAddress) throw new ArgumentException("Invalid reconstruction controls or context.");
        using var r = Acquire(); using var e = energy.Acquire(); using var o = output.Acquire();
        var input = new Interop.ReconstructorInputs { EnergyField = new(e.Value) };
        var settings = new Interop.ReconstructorSharedInputs { Duration = duration, Order = order };
        var destination = new Interop.ReconstructorOutputs { ImpulseResponse = new(o.Value) };
        Interop.Api.ReconstructorReconstruct(new(r.Value), 1, &input, &settings, &destination);
    }
    private static nint Create(Context parent, Interop.ReconstructorSettings settings)
    {
        using var lease = parent.Acquire(); Interop.ReconstructorHandle value;
        SteamAudioException.Check(Interop.Api.ReconstructorCreate(new Interop.ContextHandle(lease.Value), &settings, &value), "Create Reconstructor"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.ReconstructorHandle(value); Interop.Api.ReconstructorRelease(&handle); }
}
