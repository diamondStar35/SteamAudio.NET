namespace SteamAudio.Devices;

/// <summary>
/// Owns the Embree CPU ray-tracing device used by compatible scenes.
/// </summary>
public sealed unsafe class Embree : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Creates an Embree device. The loaded native library must include Embree support.
    /// </summary>
    /// <param name="parent">Context that owns this resource and is retained until its release.</param>
    /// <param name="settings">Reserved device settings; initialize them to their default values.</param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public Embree(Context parent, Interop.EmbreeDeviceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
    }

    private static nint Create(Context parent, Interop.EmbreeDeviceSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.EmbreeDeviceHandle value;
        SteamAudioException.Check(
            Interop.Api.EmbreeDeviceCreate(new Interop.ContextHandle(lease.Value), &settings, &value),
            "Create EmbreeDevice");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.EmbreeDeviceHandle(value);
        Interop.Api.EmbreeDeviceRelease(&handle);
    }
}
