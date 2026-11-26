namespace SteamAudio.Devices;
/// <summary>Owns a native OpenCLDeviceList reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class OpenClDevices : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public OpenClDevices(Context parent, Interop.OpenCLDeviceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    private static nint Create(Context parent, Interop.OpenCLDeviceSettings settings)
    {
        using var lease = parent.Acquire(); Interop.OpenCLDeviceListHandle value;
        SteamAudioException.Check(Interop.Api.OpenCLDeviceListCreate(new Interop.ContextHandle(lease.Value), &settings, &value), "Create OpenCLDeviceList"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.OpenCLDeviceListHandle(value); Interop.Api.OpenCLDeviceListRelease(&handle); }
}
