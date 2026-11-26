namespace SteamAudio.Effects;

/// <summary>
/// Renders diffracted sound paths from their frequency gains and directional coefficients.
/// </summary>
public sealed unsafe class Path : Effect
{
    private readonly int _inputChannels;
    private readonly int _maximumOrder;
    private readonly int _outputChannels;
    private readonly bool _spatialize;
    private bool _lastBinaural;

    /// <summary>
    /// Creates independent path rendering state for a fixed sample rate and frame size.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="audio">Fixed processing format.</param>
    /// <param name="settings">Maximum Ambisonics order, spatialization mode, speaker layout and HRTF.</param>
    /// <param name="dependencies">
    /// Owners of native handles supplied in the settings. These references are retained until the effect is
    /// released.
    /// </param>
    public Path(
        Context context,
        AudioSettings audio,
        Interop.PathEffectSettings settings,
        params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    {
        _maximumOrder = settings.MaxOrder;
        _inputChannels = 1;
        _spatialize = settings.Spatialize == Interop.Bool.True;
        _outputChannels = _spatialize ? LayoutChannels(settings.SpeakerLayout) : (settings.MaxOrder + 1) * (settings.MaxOrder + 1);
    }

    private static nint Create(Context context, AudioSettings audio, Interop.PathEffectSettings settings)
    {
        if (settings.MaxOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings));
        }

        _ = checked((settings.MaxOrder + 1) * (settings.MaxOrder + 1));
        if (settings.Spatialize == Interop.Bool.True)
        {
            _ = LayoutChannels(settings.SpeakerLayout);
            // The SDK allocates its convolution state even when the first block uses speaker panning.
            if (settings.Hrtf.Value == 0)
            {
                throw new ArgumentException("Spatialized path effects require an HRTF at creation.", nameof(settings));
            }
        }

        using var contextLease = context.Acquire();
        var format = audio.Native;
        Interop.PathEffectHandle result;
        SteamAudioException.Check(
            Interop.Api.PathEffectCreate(new(contextLease.Value), &format, &settings, &result),
            "Create Path");
        return result.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.PathEffectHandle(value);
        Interop.Api.PathEffectRelease(&handle);
    }

    /// <summary>
    /// Processes one complete PCM block using this effect's configured frame size.
    /// </summary>
    /// <param name="parameters">
    /// Path coefficients, frequency gains, listener orientation and rendering mode. Coefficient storage and the
    /// selected HRTF must remain valid throughout the call.
    /// </param>
    /// <param name="input">Mono input with the configured frame count.</param>
    /// <param name="output">
    /// Stereo for spatialized binaural processing, the configured speaker layout for panning, or the
    /// maximum-order Ambisonics channels when spatialization is disabled.
    /// </param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(
        Interop.PathEffectParams parameters,
        AudioBuffer input,
        AudioBuffer output)
    {
        if ((uint)parameters.Order > (uint)_maximumOrder)
        {
            throw new ArgumentOutOfRangeException(nameof(parameters));
        }

        var binaural = _spatialize && parameters.Binaural == Interop.Bool.True;
        if (parameters.ShCoeffs == null || (binaural && parameters.Hrtf.Value == 0))
        {
            throw new ArgumentException("Path coefficients and a binaural HRTF must be valid.", nameof(parameters));
        }

        Validate(input, output);
        if (input.Channels != _inputChannels || output.Channels != (binaural ? 2 : _outputChannels))
        {
            throw new ArgumentException("Effect channel dimensions do not match.");
        }

        using var operation = Enter();
        using var effect = Acquire();
        using var sourceBuffer = input.Borrow();
        using var outputBuffer = output.Borrow();
        _lastBinaural = binaural;
        return Interop.Api.PathEffectApply(new(effect.Value), &parameters, sourceBuffer.Native, outputBuffer.Native);
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
            return Interop.Api.PathEffectGetTailSize(new(effect.Value));
        }
    }

    /// <summary>
    /// Drains a complete tail block after input ends.
    /// </summary>
    /// <param name="output">Output matching the most recent processing mode and configured frame count.</param>
    /// <returns>Whether further tail blocks remain.</returns>
    public Interop.AudioEffectState GetTail(AudioBuffer output)
    {
        using var operation = Enter();
        Validate(output);
        if (output.Channels != (_lastBinaural ? 2 : _outputChannels))
        {
            throw new ArgumentException("Incorrect output channels.");
        }

        using var effect = Acquire();
        using var outputBuffer = output.Borrow();
        return Interop.Api.PathEffectGetTail(new(effect.Value), outputBuffer.Native);
    }

    /// <summary>
    /// Clears stored effect state.
    /// </summary>
    public void Reset()
    {
        using var operation = Enter();
        using var effect = Acquire();
        Interop.Api.PathEffectReset(new(effect.Value));
        _lastBinaural = false;
    }
}
