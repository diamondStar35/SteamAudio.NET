namespace SteamAudio.Devices;

/// <summary>
/// Owns GPU ray-tracing resources associated with an OpenCL device.
/// </summary>
public sealed unsafe class RadeonRays : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Creates Radeon Rays resources on the selected OpenCL device.
    /// </summary>
    /// <param name="parent">Parent device or simulator retained until this resource is released.</param>
    /// <param name="settings">Reserved device settings; initialize them to their default values.</param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public RadeonRays(OpenCl parent, Interop.RadeonRaysDeviceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
    }

    private static nint Create(OpenCl parent, Interop.RadeonRaysDeviceSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.RadeonRaysDeviceHandle value;
        SteamAudioException.Check(
            Interop.Api.RadeonRaysDeviceCreate(new Interop.OpenCLDeviceHandle(lease.Value), &settings, &value),
            "Create RadeonRaysDevice");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.RadeonRaysDeviceHandle(value);
        Interop.Api.RadeonRaysDeviceRelease(&handle);
    }
}
