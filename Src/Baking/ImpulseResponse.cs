namespace SteamAudio.Baking;

/// <summary>
/// Stores a multichannel time-domain impulse response for reconstruction and analysis.
/// </summary>
public sealed unsafe class ImpulseResponse : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Allocates an impulse response with fixed duration, sample rate and Ambisonics order.
    /// </summary>
    /// <param name="parent">Context that owns this resource and is retained until its release.</param>
    /// <param name="settings">Duration in seconds, sample rate and directional order of the response.</param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public ImpulseResponse(
        Context parent,
        Interop.ImpulseResponseSettings settings,
        params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
    }

    /// <summary>
    /// Returns a retained view of all data in SDK row-major order.
    /// </summary>
    /// <returns>Contiguous samples; keep the view alive while accessing its span.</returns>
    public FloatData Borrow()
    {
        var lease = Acquire();
        try
        {
            var handle = new Interop.ImpulseResponseHandle(lease.Value);
            return new(
                lease,
                Interop.Api.ImpulseResponseGetData(handle),
                checked(Interop.Api.ImpulseResponseGetNumChannels(handle) * Interop.Api.ImpulseResponseGetNumSamples(handle)));
        }
        catch
        {
            lease.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Zeros stored values.
    /// </summary>
    public void Reset()
    {
        using var lease = Acquire();
        Interop.Api.ImpulseResponseReset(new(lease.Value));
    }

    /// <summary>
    /// Scales data into another compatible resource. SDK minimum-dimension behavior is preserved.
    /// </summary>
    /// <param name="scalar">Finite multiplier.</param>
    /// <param name="destination">Output belonging to the same native context.</param>
    /// <param name="accumulate">Add to destination instead of replacing it.</param>
    public void Scale(float scalar, ImpulseResponse destination, bool accumulate = false)
    {
        if (!float.IsFinite(scalar) || ParentAddress != destination.ParentAddress)
        {
            throw new ArgumentException("Invalid scalar or foreign resource.");
        }

        using var source = Acquire();
        using var target = destination.Acquire();
        if (accumulate)
        {
            Interop.Api.ImpulseResponseScaleAccum(new(source.Value), scalar, new(target.Value));
        }
        else
        {
            Interop.Api.ImpulseResponseScale(new(source.Value), scalar, new(target.Value));
        }
    }

    /// <summary>
    /// Copies values to another resource, preserving the SDK's minimum-dimension behavior.
    /// </summary>
    /// <param name="destination">Output belonging to the same context.</param>
    public void CopyTo(ImpulseResponse destination)
    {
        if (ParentAddress != destination.ParentAddress)
        {
            throw new ArgumentException("Foreign context.");
        }

        using var source = Acquire();
        using var target = destination.Acquire();
        Interop.Api.ImpulseResponseCopy(new(source.Value), new(target.Value));
    }

    private static nint Create(Context parent, Interop.ImpulseResponseSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.ImpulseResponseHandle value;
        SteamAudioException.Check(
            Interop.Api.ImpulseResponseCreate(new Interop.ContextHandle(lease.Value), &settings, &value),
            "Create ImpulseResponse");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.ImpulseResponseHandle(value);
        Interop.Api.ImpulseResponseRelease(&handle);
    }
}
