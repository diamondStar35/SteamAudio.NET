using System.Numerics;

namespace SteamAudio.Effects;

/// <summary>
/// Headphone HRTF spatialization of mono or stereo fixed blocks. Output always has two channels.
/// </summary>
public sealed unsafe class Binaural : Effect
{
    private readonly nint _hrtfAddress;

    /// <summary>
    /// Creates independent binaural convolution state and retains its HRTF.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="hrtf">Shared HRTF database; its format determines this effect's block size.</param>
    public Binaural(Context context, Hrtf hrtf)
        : base(Create(context, hrtf), Release, context, hrtf.Settings, hrtf)
    {
        _hrtfAddress = hrtf.DangerousGetHandle();
    }

    private static nint Create(Context context, Hrtf hrtf)
    {
        using var contextLease = context.Acquire();
        using var hrtfLease = hrtf.Acquire();
        var audio = hrtf.Settings.Native;
        var settings = new Interop.BinauralEffectSettings
        {
            Hrtf = new(hrtfLease.Value)
        };
        Interop.BinauralEffectHandle result;
        SteamAudioException.Check(
            Interop.Api.BinauralEffectCreate(new(contextLease.Value), &audio, &settings, &result),
            "Create binaural effect");
        return result.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.BinauralEffectHandle(value);
        Interop.Api.BinauralEffectRelease(&handle);
    }

    /// <summary>
    /// Spatializes one complete block. Direction uses +X right, +Y up and -Z forward. Input and output cannot
    /// alias.
    /// </summary>
    /// <param name="direction">Nonzero listener-relative direction; normalized before processing.</param>
    /// <param name="input">One or two input channels.</param>
    /// <param name="output">Two output channels.</param>
    /// <param name="spatialBlend">HRTF blend from zero to one.</param>
    /// <returns>Whether convolution tails remain after processing.</returns>
    public Interop.AudioEffectState Process(
        Vector3 direction,
        AudioBuffer input,
        AudioBuffer output,
        float spatialBlend = 1)
        => Process(direction, input, output, spatialBlend, Interop.HrtfInterpolation.Bilinear);

    /// <summary>Spatializes a complete block with explicitly selected HRTF interpolation.</summary>
    /// <param name="direction">Nonzero listener-relative direction.</param>
    /// <param name="input">Mono or stereo fixed-block input.</param>
    /// <param name="output">Stereo fixed-block output.</param>
    /// <param name="spatialBlend">Spatialization amount from zero through one.</param>
    /// <param name="interpolation">Nearest-neighbor or bilinear HRTF interpolation.</param>
    /// <returns>Whether convolution tail samples remain.</returns>
    public Interop.AudioEffectState Process(
        Vector3 direction,
        AudioBuffer input,
        AudioBuffer output,
        float spatialBlend,
        Interop.HrtfInterpolation interpolation)
        => ProcessCore(direction, input, output, spatialBlend, interpolation, _hrtfAddress);

    /// <summary>
    /// Processes with a compatible alternate HRTF retained for this call. This permits isolated control-thread
    /// warm-up without touching a rendering thread's HRTF interpolation workspace.
    /// Changing the HRIR length can rebuild native convolution storage; prepare such changes outside rendering.
    /// </summary>
    /// <param name="direction">Nonzero listener-relative direction.</param>
    /// <param name="input">Mono or stereo fixed-block input.</param>
    /// <param name="output">Stereo fixed-block output.</param>
    /// <param name="spatialBlend">Spatialization amount from zero through one.</param>
    /// <param name="interpolation">HRTF interpolation strategy.</param>
    /// <param name="hrtf">Alternate database with matching sampling rate and frame size.</param>
    /// <returns>Whether convolution tail samples remain.</returns>
    public Interop.AudioEffectState Process(
        Vector3 direction,
        AudioBuffer input,
        AudioBuffer output,
        float spatialBlend,
        Interop.HrtfInterpolation interpolation,
        Hrtf hrtf)
    {
        if (hrtf.Settings != Settings)
        {
            throw new ArgumentException("HRTF processing formats must match.", nameof(hrtf));
        }

        using var lease = hrtf.Acquire();
        return ProcessCore(direction, input, output, spatialBlend, interpolation, lease.Value);
    }

    private Interop.AudioEffectState ProcessCore(
        Vector3 direction,
        AudioBuffer input,
        AudioBuffer output,
        float spatialBlend,
        Interop.HrtfInterpolation interpolation,
        nint hrtfAddress)
    {
        if (!Enum.IsDefined(interpolation))
        {
            throw new ArgumentOutOfRangeException(nameof(interpolation));
        }

        Validate(input, output);
        if (input.Channels is < 1 or > 2 || output.Channels != 2)
        {
            throw new ArgumentException("Binaural processing needs mono/stereo input and stereo output.");
        }

        if (!float.IsFinite(direction.LengthSquared()) ||
            direction.LengthSquared() <= 0 ||
            !float.IsFinite(spatialBlend) ||
            spatialBlend is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        using var operation = Enter();
        using var effect = Acquire();
        using var source = input.Borrow();
        using var destination = output.Borrow();
        // The effect retains its default HRTF; the alternate-HRTF overload leases its database for the call.
        direction = Vector3.Normalize(direction);
        var parameters = new Interop.BinauralEffectParams
        {
            Direction = new()
            {
                X = direction.X,
                Y = direction.Y,
                Z = direction.Z
            },
            Interpolation = interpolation,
            SpatialBlend = spatialBlend,
            Hrtf = new(hrtfAddress)
        };
        return Interop.Api.BinauralEffectApply(new(effect.Value), &parameters, source.Native, destination.Native);
    }

    /// <summary>
    /// Remaining tail samples per channel.
    /// </summary>
    public int TailSize
    {
        get
        {
            using var operation = Enter();
            using var effect = Acquire();
            return Interop.Api.BinauralEffectGetTailSize(new(effect.Value));
        }
    }

    /// <summary>
    /// Drains one output block after input ends. Repeat until TailComplete; do not continue calling Process
    /// with padded input instead.
    /// </summary>
    /// <param name="output">Stereo output with the configured frame size.</param>
    /// <returns>TailRemaining or TailComplete.</returns>
    public Interop.AudioEffectState GetTail(AudioBuffer output)
    {
        Validate(output);
        if (output.Channels != 2)
        {
            throw new ArgumentException("Expected stereo output.");
        }

        using var operation = Enter();
        using var effect = Acquire();
        using var destination = output.Borrow();
        return Interop.Api.BinauralEffectGetTail(new(effect.Value), destination.Native);
    }

    /// <summary>
    /// Clears convolution state when reusing the effect after a discontinuity.
    /// </summary>
    public void Reset()
    {
        using var operation = Enter();
        using var effect = Acquire();
        Interop.Api.BinauralEffectReset(new(effect.Value));
    }
}
