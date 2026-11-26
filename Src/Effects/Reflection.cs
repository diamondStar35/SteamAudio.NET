namespace SteamAudio.Effects;
/// <summary>Owned Reflection processing state. Native parameter documentation is on Interop.ReflectionEffectParams.</summary>
/// <remarks>Raw pointer members in settings/parameters remain advanced caller-owned inputs; retain them for their documented SDK lifetime.</remarks>
public sealed unsafe class Reflection : Effect
{
    private readonly int _inputChannels;
    private readonly int _outputChannels;
    private readonly Interop.ReflectionEffectSettings _settings;
    /// <summary>Creates a fixed-block effect and retains supplied native-resource dependencies.</summary>
    /// <param name="context">Owning context.</param><param name="audio">Fixed processing format.</param><param name="settings">Native effect settings, with SDK-documented dimensions.</param><param name="dependencies">Resources referenced by settings, such as HRTF or acceleration devices.</param>
    public Reflection(Context context, AudioSettings audio, Interop.ReflectionEffectSettings settings, params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    { _inputChannels = 1; _outputChannels = settings.NumChannels; _settings = settings; }
    private static nint Create(Context context, AudioSettings audio, Interop.ReflectionEffectSettings settings)
    {
        using var c = context.Acquire(); var format = audio.Native; Interop.ReflectionEffectHandle result;
        SteamAudioException.Check(Interop.Api.ReflectionEffectCreate(new(c.Value), &format, &settings, &result), "Create Reflection"); return result.Value;
    }
    private static void Release(nint value) { var handle = new Interop.ReflectionEffectHandle(value); Interop.Api.ReflectionEffectRelease(&handle); }
    /// <summary>Processes one complete block. Native parameters preserve all SDK controls; pointed-to data must remain valid during this call.</summary>
    /// <param name="parameters">Effect parameters as documented by the SDK.</param><param name="input">Input with the configured channel and frame counts.</param><param name="output">Output with the configured channel and frame counts.</param>
    /// <param name="mixer">Optional retained reflection mixer; null returns samples directly.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(Interop.ReflectionEffectParams parameters, AudioBuffer input, AudioBuffer output, ReflectionMixer? mixer = null)
    {
        Validate(input, output); if (input.Channels != _inputChannels || output.Channels != _outputChannels) throw new ArgumentException("Effect channel dimensions do not match.");
        mixer?.ValidateContribution(this, _settings);
        using var operation = Enter(); using var effect = Acquire(); using var src = input.Borrow(); using var dst = output.Borrow();
        if (mixer is null) return Interop.Api.ReflectionEffectApply(new(effect.Value), &parameters, src.Native, dst.Native, default);
        using var mixing = mixer.Enter(); using var target = mixer.Acquire();
        return Interop.Api.ReflectionEffectApply(new(effect.Value), &parameters, src.Native, dst.Native, new(target.Value));
    }
    /// <summary>Remaining tail samples per channel.</summary>
    public int TailSize { get { using var operation = Enter(); using var effect = Acquire(); return Interop.Api.ReflectionEffectGetTailSize(new(effect.Value)); } }
    /// <summary>Drains a complete tail block after input ends.</summary>
    /// <param name="output">Output with this effect's channel and frame counts.</param><param name="mixer">Optional reflection mixer.</param><returns>Whether further tail blocks remain.</returns>
    public Interop.AudioEffectState GetTail(AudioBuffer output, ReflectionMixer? mixer = null)
    {
        Validate(output); if (output.Channels != _outputChannels) throw new ArgumentException("Incorrect output channels.");
        mixer?.ValidateContribution(this, _settings);
        using var operation = Enter(); using var effect = Acquire(); using var dst = output.Borrow();
        if (mixer is null) return Interop.Api.ReflectionEffectGetTail(new(effect.Value), dst.Native, default);
        using var mixing = mixer.Enter(); using var target = mixer.Acquire();
        return Interop.Api.ReflectionEffectGetTail(new(effect.Value), dst.Native, new(target.Value));
    }
    /// <summary>Clears stored effect state.</summary>
    public void Reset() { using var operation = Enter(); using var effect = Acquire(); Interop.Api.ReflectionEffectReset(new(effect.Value)); }
}
