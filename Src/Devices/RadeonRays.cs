namespace SteamAudio.Devices;
/// <summary>Owns a native RadeonRaysDevice reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class RadeonRays : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public RadeonRays(OpenCl parent, Interop.RadeonRaysDeviceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    private static nint Create(OpenCl parent, Interop.RadeonRaysDeviceSettings settings)
    {
        using var lease = parent.Acquire(); Interop.RadeonRaysDeviceHandle value;
        SteamAudioException.Check(Interop.Api.RadeonRaysDeviceCreate(new Interop.OpenCLDeviceHandle(lease.Value), &settings, &value), "Create RadeonRaysDevice"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.RadeonRaysDeviceHandle(value); Interop.Api.RadeonRaysDeviceRelease(&handle); }
}
