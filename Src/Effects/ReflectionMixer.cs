namespace SteamAudio.Effects;

/// <summary>
/// Accumulates compatible reflection effects before producing a shared output block.
/// </summary>
public sealed unsafe class ReflectionMixer : Effect
{
    private readonly int _channels;
    private readonly Interop.ReflectionEffectSettings _settings;

    /// <summary>
    /// Creates a mixer for the selected reflection format.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="audio">Fixed audio format.</param>
    /// <param name="settings">Reflection type, impulse response length and channels.</param>
    public ReflectionMixer(Context context, AudioSettings audio, Interop.ReflectionEffectSettings settings)
        : base(Create(context, audio, settings), Release, context, audio)
    {
        _channels = settings.NumChannels;
        _settings = settings;
    }

    internal void ValidateContribution(Reflection effect, Interop.ReflectionEffectSettings settings)
    {
        if (ContextAddress != effect.ContextAddress ||
            Settings != effect.Settings ||
            _settings.Type != settings.Type ||
            _settings.NumChannels != settings.NumChannels ||
            _settings.IrSize != settings.IrSize)
        {
            throw new ArgumentException("Reflection effect and mixer must share a context, audio format and reflection settings.");
        }
    }

    private static nint Create(Context context, AudioSettings audio, Interop.ReflectionEffectSettings settings)
    {
        using var contextLease = context.Acquire();
        var format = audio.Native;
        Interop.ReflectionMixerHandle value;
        SteamAudioException.Check(
            Interop.Api.ReflectionMixerCreate(new(contextLease.Value), &format, &settings, &value),
            "Create reflection mixer");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.ReflectionMixerHandle(value);
        Interop.Api.ReflectionMixerRelease(&handle);
    }

    /// <summary>
    /// Retrieves one mixed output block after contributing reflection effects have run.
    /// </summary>
    /// <param name="parameters">Reflection algorithm, channel count and impulse-response length matching the contributing effects.</param>
    /// <param name="output">Output with the configured channels and frame size.</param>
    /// <returns>Whether any tail remains.</returns>
    public Interop.AudioEffectState Process(Interop.ReflectionEffectParams parameters, AudioBuffer output)
    {
        Validate(output);
        if (output.Channels != _channels)
        {
            throw new ArgumentException("Incorrect mixer output channels.");
        }

        using var operation = Enter();
        using var handle = Acquire();
        using var buffer = output.Borrow();
        return Interop.Api.ReflectionMixerApply(new(handle.Value), &parameters, buffer.Native);
    }

    /// <summary>
    /// Clears accumulated contributions and the mixer's convolution history.
    /// </summary>
    public void Reset()
    {
        using var operation = Enter();
        using var handle = Acquire();
        Interop.Api.ReflectionMixerReset(new(handle.Value));
    }
}
