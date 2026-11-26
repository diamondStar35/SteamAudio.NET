namespace SteamAudio.Effects;
/// <summary>Owned AmbisonicsEncode processing state. Native parameter documentation is on Interop.AmbisonicsEncodeEffectParams.</summary>
/// <remarks>Raw pointer members in settings/parameters remain advanced caller-owned inputs; retain them for their documented SDK lifetime.</remarks>
public sealed unsafe class AmbisonicsEncode : Effect
{
    private readonly int _inputChannels;
    private readonly int _maximumOrder;
    private readonly int _outputChannels;
    /// <summary>Creates a fixed-block effect and retains supplied native-resource dependencies.</summary>
    /// <param name="context">Owning context.</param><param name="audio">Fixed processing format.</param><param name="settings">Native effect settings, with SDK-documented dimensions.</param><param name="dependencies">Resources referenced by settings, such as HRTF or acceleration devices.</param>
    public AmbisonicsEncode(Context context, AudioSettings audio, Interop.AmbisonicsEncodeEffectSettings settings, params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    { _maximumOrder = settings.MaxOrder; _inputChannels = 1; _outputChannels = (settings.MaxOrder + 1) * (settings.MaxOrder + 1); }
    private static nint Create(Context context, AudioSettings audio, Interop.AmbisonicsEncodeEffectSettings settings)
    {
        if (settings.MaxOrder < 0) throw new ArgumentOutOfRangeException(nameof(settings));
        _ = checked((settings.MaxOrder + 1) * (settings.MaxOrder + 1));
        using var c = context.Acquire(); var format = audio.Native; Interop.AmbisonicsEncodeEffectHandle result;
        SteamAudioException.Check(Interop.Api.AmbisonicsEncodeEffectCreate(new(c.Value), &format, &settings, &result), "Create AmbisonicsEncode"); return result.Value;
    }
    private static void Release(nint value) { var handle = new Interop.AmbisonicsEncodeEffectHandle(value); Interop.Api.AmbisonicsEncodeEffectRelease(&handle); }
    /// <summary>Processes one complete block. Native parameters preserve all SDK controls; pointed-to data must remain valid during this call.</summary>
    /// <param name="parameters">Effect parameters as documented by the SDK.</param><param name="input">Input with the configured channel and frame counts.</param><param name="output">Output with the configured channel and frame counts.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(Interop.AmbisonicsEncodeEffectParams parameters, AudioBuffer input, AudioBuffer output)
    {
        if ((uint)parameters.Order > (uint)_maximumOrder) throw new ArgumentOutOfRangeException(nameof(parameters));
        Validate(input, output); if (input.Channels != _inputChannels || output.Channels != _outputChannels) throw new ArgumentException("Effect channel dimensions do not match.");
        using var operation = Enter(); using var effect = Acquire(); using var src = input.Borrow(); using var dst = output.Borrow();
        return Interop.Api.AmbisonicsEncodeEffectApply(new(effect.Value), &parameters, src.Native, dst.Native);
    }
    /// <summary>Remaining tail samples per channel.</summary>
    public int TailSize { get { using var operation = Enter(); using var effect = Acquire(); return Interop.Api.AmbisonicsEncodeEffectGetTailSize(new(effect.Value)); } }
    /// <summary>Drains a complete tail block after input ends.</summary>
    /// <param name="output">Output with this effect's channel and frame counts.</param><returns>Whether further tail blocks remain.</returns>
    public Interop.AudioEffectState GetTail(AudioBuffer output)
    {
        Validate(output); if (output.Channels != _outputChannels) throw new ArgumentException("Incorrect output channels.");
        using var operation = Enter(); using var effect = Acquire(); using var dst = output.Borrow(); return Interop.Api.AmbisonicsEncodeEffectGetTail(new(effect.Value), dst.Native);
    }
    /// <summary>Clears stored effect state.</summary>
    public void Reset() { using var operation = Enter(); using var effect = Acquire(); Interop.Api.AmbisonicsEncodeEffectReset(new(effect.Value)); }
}
