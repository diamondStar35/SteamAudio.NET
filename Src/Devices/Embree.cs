namespace SteamAudio.Devices;
/// <summary>Owns a native EmbreeDevice reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class Embree : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public Embree(Context parent, Interop.EmbreeDeviceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    private static nint Create(Context parent, Interop.EmbreeDeviceSettings settings)
    {
        using var lease = parent.Acquire(); Interop.EmbreeDeviceHandle value;
        SteamAudioException.Check(Interop.Api.EmbreeDeviceCreate(new Interop.ContextHandle(lease.Value), &settings, &value), "Create EmbreeDevice"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.EmbreeDeviceHandle(value); Interop.Api.EmbreeDeviceRelease(&handle); }
}
