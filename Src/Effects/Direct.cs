namespace SteamAudio.Effects;

/// <summary>
/// Applies distance attenuation, air absorption, directivity, occlusion and transmission to PCM audio.
/// </summary>
public sealed unsafe class Direct : Effect
{
    private readonly int _inputChannels;
    private readonly int _outputChannels;

    /// <summary>
    /// Creates independent direct-sound filtering state for a fixed sample rate and frame size.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="audio">Fixed processing format.</param>
    /// <param name="settings">Channel count shared by the input and output buffers.</param>
    /// <param name="dependencies">
    /// Owners of native handles supplied in the settings. These references are retained until the effect is
    /// released.
    /// </param>
    public Direct(
        Context context,
        AudioSettings audio,
        Interop.DirectEffectSettings settings,
        params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    {
        _inputChannels = settings.NumChannels;
        _outputChannels = settings.NumChannels;
    }

    private static nint Create(Context context, AudioSettings audio, Interop.DirectEffectSettings settings)
    {
        using var contextLease = context.Acquire();
        var format = audio.Native;
        Interop.DirectEffectHandle result;
        SteamAudioException.Check(
            Interop.Api.DirectEffectCreate(new(contextLease.Value), &format, &settings, &result),
            "Create Direct");
        return result.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.DirectEffectHandle(value);
        Interop.Api.DirectEffectRelease(&handle);
    }

    /// <summary>
    /// Processes one complete PCM block using this effect's configured frame size.
    /// </summary>
    /// <param name="parameters">Attenuation values and flags selecting which direct-sound filters to apply.</param>
    /// <param name="input">Input with the configured channel and frame counts.</param>
    /// <param name="output">Output with the configured channel and frame counts.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(
        Interop.DirectEffectParams parameters,
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
        return Interop.Api.DirectEffectApply(new(effect.Value), &parameters, sourceBuffer.Native, outputBuffer.Native);
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
            return Interop.Api.DirectEffectGetTailSize(new(effect.Value));
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
        return Interop.Api.DirectEffectGetTail(new(effect.Value), outputBuffer.Native);
    }

    /// <summary>
    /// Clears stored effect state.
    /// </summary>
    public void Reset()
    {
        using var operation = Enter();
        using var effect = Acquire();
        Interop.Api.DirectEffectReset(new(effect.Value));
    }
}
