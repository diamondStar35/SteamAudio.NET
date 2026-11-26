namespace SteamAudio.Effects;
/// <summary>Owned Direct processing state. Native parameter documentation is on Interop.DirectEffectParams.</summary>
/// <remarks>Raw pointer members in settings/parameters remain advanced caller-owned inputs; retain them for their documented SDK lifetime.</remarks>
public sealed unsafe class Direct : Effect
{
    private readonly int _inputChannels;
    private readonly int _outputChannels;
    /// <summary>Creates a fixed-block effect and retains supplied native-resource dependencies.</summary>
    /// <param name="context">Owning context.</param><param name="audio">Fixed processing format.</param><param name="settings">Native effect settings, with SDK-documented dimensions.</param><param name="dependencies">Resources referenced by settings, such as HRTF or acceleration devices.</param>
    public Direct(Context context, AudioSettings audio, Interop.DirectEffectSettings settings, params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    { _inputChannels = settings.NumChannels; _outputChannels = settings.NumChannels; }
    private static nint Create(Context context, AudioSettings audio, Interop.DirectEffectSettings settings)
    {
        using var c = context.Acquire(); var format = audio.Native; Interop.DirectEffectHandle result;
        SteamAudioException.Check(Interop.Api.DirectEffectCreate(new(c.Value), &format, &settings, &result), "Create Direct"); return result.Value;
    }
    private static void Release(nint value) { var handle = new Interop.DirectEffectHandle(value); Interop.Api.DirectEffectRelease(&handle); }
    /// <summary>Processes one complete block. Native parameters preserve all SDK controls; pointed-to data must remain valid during this call.</summary>
    /// <param name="parameters">Effect parameters as documented by the SDK.</param><param name="input">Input with the configured channel and frame counts.</param><param name="output">Output with the configured channel and frame counts.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(Interop.DirectEffectParams parameters, AudioBuffer input, AudioBuffer output)
    {
        Validate(input, output); if (input.Channels != _inputChannels || output.Channels != _outputChannels) throw new ArgumentException("Effect channel dimensions do not match.");
        using var operation = Enter(); using var effect = Acquire(); using var src = input.Borrow(); using var dst = output.Borrow();
        return Interop.Api.DirectEffectApply(new(effect.Value), &parameters, src.Native, dst.Native);
    }
    /// <summary>Remaining tail samples per channel.</summary>
    public int TailSize { get { using var operation = Enter(); using var effect = Acquire(); return Interop.Api.DirectEffectGetTailSize(new(effect.Value)); } }
    /// <summary>Drains a complete tail block after input ends.</summary>
    /// <param name="output">Output with this effect's channel and frame counts.</param><returns>Whether further tail blocks remain.</returns>
    public Interop.AudioEffectState GetTail(AudioBuffer output)
    {
        Validate(output); if (output.Channels != _outputChannels) throw new ArgumentException("Incorrect output channels.");
        using var operation = Enter(); using var effect = Acquire(); using var dst = output.Borrow(); return Interop.Api.DirectEffectGetTail(new(effect.Value), dst.Native);
    }
    /// <summary>Clears stored effect state.</summary>
    public void Reset() { using var operation = Enter(); using var effect = Acquire(); Interop.Api.DirectEffectReset(new(effect.Value)); }
}
