namespace SteamAudio.Baking;
/// <summary>Owns a native EnergyField reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class EnergyField : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public EnergyField(Context parent, Interop.EnergyFieldSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    /// <summary>Returns a retained view of all data in SDK row-major order.</summary>
    /// <returns>Contiguous samples; keep the view alive while accessing its span.</returns>
    public FloatData Borrow()
    {
        var lease = Acquire();
        try { var handle = new Interop.EnergyFieldHandle(lease.Value); return new(lease, Interop.Api.EnergyFieldGetData(handle), checked(Interop.Api.EnergyFieldGetNumChannels(handle) * Interop.Api.EnergyFieldGetNumBins(handle) * Interop.Api.NumBands)); }
        catch { lease.Dispose(); throw; }
    }
    /// <summary>Zeros stored values.</summary>
    public void Reset() { using var lease = Acquire(); Interop.Api.EnergyFieldReset(new(lease.Value)); }
    /// <summary>Scales data into another compatible resource. SDK minimum-dimension behavior is preserved.</summary>
    /// <param name="scalar">Finite multiplier.</param><param name="destination">Output belonging to the same native context.</param><param name="accumulate">Add to destination instead of replacing it.</param>
    public void Scale(float scalar, EnergyField destination, bool accumulate = false)
    {
        if (!float.IsFinite(scalar) || ParentAddress != destination.ParentAddress) throw new ArgumentException("Invalid scalar or foreign resource.");
        using var source = Acquire(); using var target = destination.Acquire();
        if (accumulate) Interop.Api.EnergyFieldScaleAccum(new(source.Value), scalar, new(target.Value));
        else Interop.Api.EnergyFieldScale(new(source.Value), scalar, new(target.Value));
    }
    /// <summary>Copies values to another resource, preserving the SDK's minimum-dimension behavior.</summary>
    /// <param name="destination">Output belonging to the same context.</param>
    public void CopyTo(EnergyField destination)
    {
        if (ParentAddress != destination.ParentAddress) throw new ArgumentException("Foreign context.");
        using var source = Acquire(); using var target = destination.Acquire(); Interop.Api.EnergyFieldCopy(new(source.Value), new(target.Value));
    }
    private static nint Create(Context parent, Interop.EnergyFieldSettings settings)
    {
        using var lease = parent.Acquire(); Interop.EnergyFieldHandle value;
        SteamAudioException.Check(Interop.Api.EnergyFieldCreate(new Interop.ContextHandle(lease.Value), &settings, &value), "Create EnergyField"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.EnergyFieldHandle(value); Interop.Api.EnergyFieldRelease(&handle); }
}
