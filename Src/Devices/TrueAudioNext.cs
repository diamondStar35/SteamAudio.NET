namespace SteamAudio.Devices;

/// <summary>
/// Owns GPU convolution resources associated with an OpenCL device.
/// </summary>
public sealed unsafe class TrueAudioNext : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Creates TrueAudio Next convolution resources on the selected OpenCL device.
    /// </summary>
    /// <param name="parent">Parent device or simulator retained until this resource is released.</param>
    /// <param name="settings">
    /// Frame size, impulse-response size, source capacity and Ambisonics order supported by the device.
    /// </param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public TrueAudioNext(
        OpenCl parent,
        Interop.TrueAudioNextDeviceSettings settings,
        params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
    }

    private static nint Create(OpenCl parent, Interop.TrueAudioNextDeviceSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.TrueAudioNextDeviceHandle value;
        SteamAudioException.Check(
            Interop.Api.TrueAudioNextDeviceCreate(new Interop.OpenCLDeviceHandle(lease.Value), &settings, &value),
            "Create TrueAudioNextDevice");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.TrueAudioNextDeviceHandle(value);
        Interop.Api.TrueAudioNextDeviceRelease(&handle);
    }
}
