namespace SteamAudio.Effects;

/// <summary>
/// Positions a mono signal in a speaker layout using a listener-relative direction.
/// </summary>
public sealed unsafe class Panning : Effect
{
    private readonly int _inputChannels;
    private readonly int _outputChannels;

    /// <summary>
    /// Creates independent speaker panning state for a fixed sample rate and frame size.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="audio">Fixed processing format.</param>
    /// <param name="settings">Speaker layout receiving the panned signal.</param>
    /// <param name="dependencies">
    /// Owners of native handles supplied in the settings. These references are retained until the effect is
    /// released.
    /// </param>
    public Panning(
        Context context,
        AudioSettings audio,
        Interop.PanningEffectSettings settings,
        params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    {
        _inputChannels = 1;
        _outputChannels = LayoutChannels(settings.SpeakerLayout);
    }

    private static nint Create(Context context, AudioSettings audio, Interop.PanningEffectSettings settings)
    {
        using var contextLease = context.Acquire();
        var format = audio.Native;
        Interop.PanningEffectHandle result;
        SteamAudioException.Check(
            Interop.Api.PanningEffectCreate(new(contextLease.Value), &format, &settings, &result),
            "Create Panning");
        return result.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.PanningEffectHandle(value);
        Interop.Api.PanningEffectRelease(&handle);
    }

    /// <summary>
    /// Processes one complete PCM block using this effect's configured frame size.
    /// </summary>
    /// <param name="parameters">
    /// Listener-relative direction, with positive X to the right, positive Y upward and negative Z forward.
    /// </param>
    /// <param name="input">Input with the configured channel and frame counts.</param>
    /// <param name="output">Output with the configured channel and frame counts.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(
        Interop.PanningEffectParams parameters,
        AudioBuffer input,
        AudioBuffer output)
    {
        Validate(input, output);
        if (input.Channels != _inputChannels || output.Channels != _outputChannels)
        {
            throw new ArgumentException("Effect channel dimensions do not match.");
        }

        using var operation = Enter();
        using var effect = Acquire();
        using var sourceBuffer = input.Borrow();
        using var outputBuffer = output.Borrow();
        return Interop.Api.PanningEffectApply(new(effect.Value), &parameters, sourceBuffer.Native, outputBuffer.Native);
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
            return Interop.Api.PanningEffectGetTailSize(new(effect.Value));
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
        return Interop.Api.PanningEffectGetTail(new(effect.Value), outputBuffer.Native);
    }

    /// <summary>
    /// Clears stored effect state.
    /// </summary>
    public void Reset()
    {
        using var operation = Enter();
        using var effect = Acquire();
        Interop.Api.PanningEffectReset(new(effect.Value));
    }
}
