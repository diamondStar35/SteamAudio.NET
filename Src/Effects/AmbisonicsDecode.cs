namespace SteamAudio.Effects;

/// <summary>
/// Rotates and decodes Ambisonics audio for speakers or stereo headphones.
/// </summary>
public sealed unsafe class AmbisonicsDecode : Effect
{
    private readonly int _inputChannels;
    private readonly int _maximumOrder;
    private readonly int _outputChannels;

    /// <summary>
    /// Creates independent Ambisonics decoding state for a fixed sample rate and frame size.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="audio">Fixed processing format.</param>
    /// <param name="settings">
    /// Maximum Ambisonics order, speaker layout and optional HRTF for headphone rendering.
    /// </param>
    /// <param name="dependencies">
    /// Owners of native handles supplied in the settings. These references are retained until the effect is
    /// released.
    /// </param>
    public AmbisonicsDecode(
        Context context,
        AudioSettings audio,
        Interop.AmbisonicsDecodeEffectSettings settings,
        params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    {
        _maximumOrder = settings.MaxOrder;
        _inputChannels = (settings.MaxOrder + 1) * (settings.MaxOrder + 1);
        _outputChannels = LayoutChannels(settings.SpeakerLayout);
    }

    private static nint Create(
        Context context,
        AudioSettings audio,
        Interop.AmbisonicsDecodeEffectSettings settings)
    {
        if (settings.MaxOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings));
        }

        _ = checked((settings.MaxOrder + 1) * (settings.MaxOrder + 1));
        using var contextLease = context.Acquire();
        var format = audio.Native;
        Interop.AmbisonicsDecodeEffectHandle result;
        SteamAudioException.Check(
            Interop.Api.AmbisonicsDecodeEffectCreate(new(contextLease.Value), &format, &settings, &result),
            "Create AmbisonicsDecode");
        return result.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.AmbisonicsDecodeEffectHandle(value);
        Interop.Api.AmbisonicsDecodeEffectRelease(&handle);
    }

    /// <summary>
    /// Processes one complete PCM block using this effect's configured frame size.
    /// </summary>
    /// <param name="parameters">
    /// Listener orientation, Ambisonics order and choice of speaker or binaural rendering.
    /// </param>
    /// <param name="input">Input with the configured channel and frame counts.</param>
    /// <param name="output">Output with the configured channel and frame counts.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(
        Interop.AmbisonicsDecodeEffectParams parameters,
        AudioBuffer input,
        AudioBuffer output)
    {
        if ((uint)parameters.Order > (uint)_maximumOrder)
        {
            throw new ArgumentOutOfRangeException(nameof(parameters));
        }

        Validate(input, output);
        if (input.Channels != _inputChannels || output.Channels != _outputChannels)
        {
            throw new ArgumentException("Effect channel dimensions do not match.");
        }

        using var operation = Enter();
        using var effect = Acquire();
        using var sourceBuffer = input.Borrow();
        using var outputBuffer = output.Borrow();
        return Interop.Api.AmbisonicsDecodeEffectApply(new(effect.Value), &parameters, sourceBuffer.Native, outputBuffer.Native);
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
            return Interop.Api.AmbisonicsDecodeEffectGetTailSize(new(effect.Value));
        }
    }

    /// <summary>
    /// Drains a complete tail block after input ends.
    /// </summary>
    /// <param name="output">Output with this effect's channel and frame counts.</param>
    /// <returns>Whether further tail blocks remain.</returns>
    public Interop.AudioEffectState GetTail(AudioBuffer output)
    {
        Validate(output);
        if (output.Channels != _outputChannels)
        {
            throw new ArgumentException("Incorrect output channels.");
        }

        using var operation = Enter();
        using var effect = Acquire();
        using var outputBuffer = output.Borrow();
        return Interop.Api.AmbisonicsDecodeEffectGetTail(new(effect.Value), outputBuffer.Native);
    }

    /// <summary>
    /// Clears stored effect state.
    /// </summary>
    public void Reset()
    {
        using var operation = Enter();
        using var effect = Acquire();
        Interop.Api.AmbisonicsDecodeEffectReset(new(effect.Value));
    }
}
