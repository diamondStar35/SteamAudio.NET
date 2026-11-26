namespace SteamAudio.Baking;

/// <summary>
/// Converts simulated or baked acoustic energy into time-domain impulse responses.
/// </summary>
public sealed unsafe class Reconstructor : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Creates reconstruction state for a fixed sample rate and maximum response dimensions.
    /// </summary>
    /// <param name="parent">Context that owns this resource and is retained until its release.</param>
    /// <param name="settings">Maximum duration, maximum Ambisonics order and output sample rate.</param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public Reconstructor(Context parent, Interop.ReconstructorSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
    }

    /// <summary>
    /// Reconstructs a measured energy field into a time-domain impulse response.
    /// </summary>
    /// <param name="energy">Input field from the same context.</param>
    /// <param name="output">Output response with sufficient duration, order and sampling rate.</param>
    /// <param name="duration">Reconstruction duration in seconds.</param>
    /// <param name="order">Ambisonic order, no greater than creation limits.</param>
    public void Reconstruct(EnergyField energy, ImpulseResponse output, float duration, int order)
    {
        if (!float.IsFinite(duration) || duration <= 0 || order < 0 || energy.ParentAddress != ParentAddress || output.ParentAddress != ParentAddress)
        {
            throw new ArgumentException("Invalid reconstruction controls or context.");
        }

        using var reconstructorLease = Acquire();
        using var energyLease = energy.Acquire();
        using var outputLease = output.Acquire();
        var input = new Interop.ReconstructorInputs
        {
            EnergyField = new(energyLease.Value)
        };
        var settings = new Interop.ReconstructorSharedInputs
        {
            Duration = duration,
            Order = order
        };
        var destination = new Interop.ReconstructorOutputs
        {
            ImpulseResponse = new(outputLease.Value)
        };
        Interop.Api.ReconstructorReconstruct(new(reconstructorLease.Value), 1, &input, &settings, &destination);
    }

    private static nint Create(Context parent, Interop.ReconstructorSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.ReconstructorHandle value;
        SteamAudioException.Check(
            Interop.Api.ReconstructorCreate(new Interop.ContextHandle(lease.Value), &settings, &value),
            "Create Reconstructor");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.ReconstructorHandle(value);
        Interop.Api.ReconstructorRelease(&handle);
    }
}
