namespace SteamAudio.Effects;

/// <summary>
/// Renders a multichannel speaker mix as stereo headphone audio using an HRTF.
/// </summary>
public sealed unsafe class VirtualSurround : Effect
{
    private readonly int _inputChannels;
    private readonly int _outputChannels;

    /// <summary>
    /// Creates independent virtual surround state for a fixed sample rate and frame size.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="audio">Fixed processing format.</param>
    /// <param name="settings">Input speaker layout and HRTF used to position its virtual speakers.</param>
    /// <param name="dependencies">
    /// Owners of native handles supplied in the settings. These references are retained until the effect is
    /// released.
    /// </param>
    public VirtualSurround(
        Context context,
        AudioSettings audio,
        Interop.VirtualSurroundEffectSettings settings,
        params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    {
        _inputChannels = LayoutChannels(settings.SpeakerLayout);
        _outputChannels = 2;
    }

    private static nint Create(
        Context context,
        AudioSettings audio,
        Interop.VirtualSurroundEffectSettings settings)
    {
        using var contextLease = context.Acquire();
        var format = audio.Native;
        Interop.VirtualSurroundEffectHandle result;
        SteamAudioException.Check(
            Interop.Api.VirtualSurroundEffectCreate(new(contextLease.Value), &format, &settings, &result),
            "Create VirtualSurround");
        return result.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.VirtualSurroundEffectHandle(value);
        Interop.Api.VirtualSurroundEffectRelease(&handle);
    }

    /// <summary>
    /// Processes one complete PCM block using this effect's configured frame size.
    /// </summary>
    /// <param name="parameters">HRTF used to render the virtual speakers for this block.</param>
    /// <param name="input">Input with the configured channel and frame counts.</param>
    /// <param name="output">Output with the configured channel and frame counts.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(
        Interop.VirtualSurroundEffectParams parameters,
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
        return Interop.Api.VirtualSurroundEffectApply(new(effect.Value), &parameters, sourceBuffer.Native, outputBuffer.Native);
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
            return Interop.Api.VirtualSurroundEffectGetTailSize(new(effect.Value));
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
        return Interop.Api.VirtualSurroundEffectGetTail(new(effect.Value), outputBuffer.Native);
    }

    /// <summary>
    /// Clears stored effect state.
    /// </summary>
    public void Reset()
    {
        using var operation = Enter();
        using var effect = Acquire();
        Interop.Api.VirtualSurroundEffectReset(new(effect.Value));
    }
}
