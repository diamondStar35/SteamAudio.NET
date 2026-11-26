namespace SteamAudio.Baking;

/// <summary>
/// Stores reflected acoustic energy by direction, frequency band and time bin.
/// </summary>
public sealed unsafe class EnergyField : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Allocates an energy field for the requested duration and Ambisonics order.
    /// </summary>
    /// <param name="parent">Context that owns this resource and is retained until its release.</param>
    /// <param name="settings">Maximum duration in seconds and directional order of the stored field.</param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public EnergyField(Context parent, Interop.EnergyFieldSettings settings, params Resource[] dependencies)
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
            var handle = new Interop.EnergyFieldHandle(lease.Value);
            return new(
                lease,
                Interop.Api.EnergyFieldGetData(handle),
                checked(Interop.Api.EnergyFieldGetNumChannels(handle) * Interop.Api.EnergyFieldGetNumBins(handle) * Interop.Api.NumBands));
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
        Interop.Api.EnergyFieldReset(new(lease.Value));
    }

    /// <summary>
    /// Scales data into another compatible resource. SDK minimum-dimension behavior is preserved.
    /// </summary>
    /// <param name="scalar">Finite multiplier.</param>
    /// <param name="destination">Output belonging to the same native context.</param>
    /// <param name="accumulate">Add to destination instead of replacing it.</param>
    public void Scale(float scalar, EnergyField destination, bool accumulate = false)
    {
        if (!float.IsFinite(scalar) || ParentAddress != destination.ParentAddress)
        {
            throw new ArgumentException("Invalid scalar or foreign resource.");
        }

        using var source = Acquire();
        using var target = destination.Acquire();
        if (accumulate)
        {
            Interop.Api.EnergyFieldScaleAccum(new(source.Value), scalar, new(target.Value));
        }
        else
        {
            Interop.Api.EnergyFieldScale(new(source.Value), scalar, new(target.Value));
        }
    }

    /// <summary>
    /// Copies values to another resource, preserving the SDK's minimum-dimension behavior.
    /// </summary>
    /// <param name="destination">Output belonging to the same context.</param>
    public void CopyTo(EnergyField destination)
    {
        if (ParentAddress != destination.ParentAddress)
        {
            throw new ArgumentException("Foreign context.");
        }

        using var source = Acquire();
        using var target = destination.Acquire();
        Interop.Api.EnergyFieldCopy(new(source.Value), new(target.Value));
    }

    private static nint Create(Context parent, Interop.EnergyFieldSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.EnergyFieldHandle value;
        SteamAudioException.Check(
            Interop.Api.EnergyFieldCreate(new Interop.ContextHandle(lease.Value), &settings, &value),
            "Create EnergyField");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.EnergyFieldHandle(value);
        Interop.Api.EnergyFieldRelease(&handle);
    }
}
