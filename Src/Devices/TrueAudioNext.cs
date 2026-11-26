namespace SteamAudio.Devices;
/// <summary>Owns a native TrueAudioNextDevice reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class TrueAudioNext : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public TrueAudioNext(OpenCl parent, Interop.TrueAudioNextDeviceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    private static nint Create(OpenCl parent, Interop.TrueAudioNextDeviceSettings settings)
    {
        using var lease = parent.Acquire(); Interop.TrueAudioNextDeviceHandle value;
        SteamAudioException.Check(Interop.Api.TrueAudioNextDeviceCreate(new Interop.OpenCLDeviceHandle(lease.Value), &settings, &value), "Create TrueAudioNextDevice"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.TrueAudioNextDeviceHandle(value); Interop.Api.TrueAudioNextDeviceRelease(&handle); }
}
