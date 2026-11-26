namespace SteamAudio.Effects;

/// <summary>
/// Renders reflected sound using convolution, parametric reverb or a hybrid of both.
/// </summary>
public sealed unsafe class Reflection : Effect
{
    private readonly int _inputChannels;
    private readonly int _outputChannels;
    private readonly Interop.ReflectionEffectSettings _settings;

    /// <summary>
    /// Creates independent reflection rendering state for a fixed sample rate and frame size.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="audio">Fixed processing format.</param>
    /// <param name="settings">Reflection algorithm, impulse-response length and output channel count.</param>
    /// <param name="dependencies">
    /// Owners of native handles supplied in the settings. These references are retained until the effect is
    /// released.
    /// </param>
    public Reflection(
        Context context,
        AudioSettings audio,
        Interop.ReflectionEffectSettings settings,
        params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    {
        _inputChannels = 1;
        _outputChannels = settings.NumChannels;
        _settings = settings;
    }

    private static nint Create(Context context, AudioSettings audio, Interop.ReflectionEffectSettings settings)
    {
        using var contextLease = context.Acquire();
        var format = audio.Native;
        Interop.ReflectionEffectHandle result;
        SteamAudioException.Check(
            Interop.Api.ReflectionEffectCreate(new(contextLease.Value), &format, &settings, &result),
            "Create Reflection");
        return result.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.ReflectionEffectHandle(value);
        Interop.Api.ReflectionEffectRelease(&handle);
    }

    /// <summary>
    /// Processes one complete PCM block using this effect's configured frame size.
    /// </summary>
    /// <param name="parameters">
    /// Reflection algorithm and simulation results. Impulse-response handles remain borrowed from the simulator
    /// and must not race a simulation update.
    /// </param>
    /// <param name="input">Input with the configured channel and frame counts.</param>
    /// <param name="output">Output with the configured channel and frame counts.</param>
    /// <param name="mixer">Optional retained reflection mixer; null returns samples directly.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(
        Interop.ReflectionEffectParams parameters,
        AudioBuffer input,
        AudioBuffer output,
        ReflectionMixer? mixer = null)
    {
        Validate(input, output);
        if (input.Channels != _inputChannels || output.Channels != _outputChannels)
        {
            throw new ArgumentException("Effect channel dimensions do not match.");
        }

        mixer?.ValidateContribution(this, _settings);
        using var operation = Enter();
        using var effect = Acquire();
        using var sourceBuffer = input.Borrow();
        using var outputBuffer = output.Borrow();
        if (mixer is null)
        {
            return Interop.Api.ReflectionEffectApply(new(effect.Value), &parameters, sourceBuffer.Native, outputBuffer.Native, default);
        }

        using var mixing = mixer.Enter();
        using var target = mixer.Acquire();
        return Interop.Api.ReflectionEffectApply(
            new(effect.Value),
            &parameters,
            sourceBuffer.Native,
            outputBuffer.Native,
            new(target.Value));
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
            return Interop.Api.ReflectionEffectGetTailSize(new(effect.Value));
        }
    }

    /// <summary>
    /// Drains a complete tail block after input ends.
    /// </summary>
    /// <param name="output">Output with this effect's channel and frame counts.</param>
    /// <param name="mixer">Optional reflection mixer.</param>
    /// <returns>Whether further tail blocks remain.</returns>
    public Interop.AudioEffectState GetTail(AudioBuffer output, ReflectionMixer? mixer = null)
    {
        Validate(output);
        if (output.Channels != _outputChannels)
        {
            throw new ArgumentException("Incorrect output channels.");
        }

        mixer?.ValidateContribution(this, _settings);
        using var operation = Enter();
        using var effect = Acquire();
        using var outputBuffer = output.Borrow();
        if (mixer is null)
        {
            return Interop.Api.ReflectionEffectGetTail(new(effect.Value), outputBuffer.Native, default);
        }

        using var mixing = mixer.Enter();
        using var target = mixer.Acquire();
        return Interop.Api.ReflectionEffectGetTail(new(effect.Value), outputBuffer.Native, new(target.Value));
    }

    /// <summary>
    /// Clears stored effect state.
    /// </summary>
    public void Reset()
    {
        using var operation = Enter();
        using var effect = Acquire();
        Interop.Api.ReflectionEffectReset(new(effect.Value));
    }
}
