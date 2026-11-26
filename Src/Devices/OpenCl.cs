namespace SteamAudio.Devices;

/// <summary>
/// An explicitly selected OpenCL device. Construction may fail when the native profile or hardware lacks
/// support.
/// </summary>
public sealed unsafe class OpenCl : Resource
{
    /// <summary>
    /// Creates a device from an enumerated device list.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="devices">Enumerated devices.</param>
    /// <param name="index">Zero-based device index.</param>
    public OpenCl(Context context, OpenClDevices devices, int index)
        : base(Create(context, devices, index), Release, context, devices)
    {
    }

    private static nint Create(Context context, OpenClDevices devices, int index)
    {
        using var contextLease = context.Acquire();
        using var devicesLease = devices.Acquire();
        if ((uint)index >= (uint)Interop.Api.OpenCLDeviceListGetNumDevices(new(devicesLease.Value)))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        Interop.OpenCLDeviceHandle value;
        SteamAudioException.Check(
            Interop.Api.OpenCLDeviceCreate(new(contextLease.Value), new(devicesLease.Value), index, &value),
            "Create OpenCL device");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.OpenCLDeviceHandle(value);
        Interop.Api.OpenCLDeviceRelease(&handle);
    }
}
