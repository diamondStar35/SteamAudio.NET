namespace SteamAudio.Devices;

/// <summary>
/// Owns an enumerated list of OpenCL devices matching the requested hardware filters.
/// </summary>
public sealed unsafe class OpenClDevices : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Enumerates devices supported by the loaded OpenCL implementation.
    /// </summary>
    /// <param name="parent">Context that owns this resource and is retained until its release.</param>
    /// <param name="settings">
    /// Device type, reserved compute units and whether TrueAudio Next support is required.
    /// </param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public OpenClDevices(Context parent, Interop.OpenCLDeviceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
    }

    private static nint Create(Context parent, Interop.OpenCLDeviceSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.OpenCLDeviceListHandle value;
        SteamAudioException.Check(
            Interop.Api.OpenCLDeviceListCreate(new Interop.ContextHandle(lease.Value), &settings, &value),
            "Create OpenCLDeviceList");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.OpenCLDeviceListHandle(value);
        Interop.Api.OpenCLDeviceListRelease(&handle);
    }
}
