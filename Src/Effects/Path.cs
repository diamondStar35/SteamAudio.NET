namespace SteamAudio.Effects;
/// <summary>Owned Path processing state. Native parameter documentation is on Interop.PathEffectParams.</summary>
/// <remarks>Raw pointer members in settings/parameters remain advanced caller-owned inputs; retain them for their documented SDK lifetime.</remarks>
public sealed unsafe class Path : Effect
{
    private readonly int _inputChannels;
    private readonly int _maximumOrder;
    private readonly int _outputChannels;
    private readonly bool _spatialize;
    private bool _lastBinaural;
    /// <summary>Creates a fixed-block effect and retains supplied native-resource dependencies.</summary>
    /// <param name="context">Owning context.</param><param name="audio">Fixed processing format.</param><param name="settings">Native effect settings, with SDK-documented dimensions.</param><param name="dependencies">Resources referenced by settings, such as HRTF or acceleration devices.</param>
    public Path(Context context, AudioSettings audio, Interop.PathEffectSettings settings, params Resource[] dependencies)
        : base(Create(context, audio, settings), Release, context, audio, dependencies)
    { _maximumOrder = settings.MaxOrder; _inputChannels = 1; _spatialize = settings.Spatialize == Interop.Bool.True; _outputChannels = _spatialize ? LayoutChannels(settings.SpeakerLayout) : (settings.MaxOrder + 1) * (settings.MaxOrder + 1); }
    private static nint Create(Context context, AudioSettings audio, Interop.PathEffectSettings settings)
    {
        if (settings.MaxOrder < 0) throw new ArgumentOutOfRangeException(nameof(settings));
        _ = checked((settings.MaxOrder + 1) * (settings.MaxOrder + 1));
        if (settings.Spatialize == Interop.Bool.True)
        {
            _ = LayoutChannels(settings.SpeakerLayout);
            // The SDK allocates its convolution state even when the first block uses speaker panning.
            if (settings.Hrtf.Value == 0) throw new ArgumentException("Spatialized path effects require an HRTF at creation.", nameof(settings));
        }
        using var c = context.Acquire(); var format = audio.Native; Interop.PathEffectHandle result;
        SteamAudioException.Check(Interop.Api.PathEffectCreate(new(c.Value), &format, &settings, &result), "Create Path"); return result.Value;
    }
    private static void Release(nint value) { var handle = new Interop.PathEffectHandle(value); Interop.Api.PathEffectRelease(&handle); }
    /// <summary>Processes one complete block. Native parameters preserve all SDK controls; pointed-to data must remain valid during this call.</summary>
    /// <param name="parameters">Effect parameters as documented by the SDK.</param><param name="input">Mono input with the configured frame count.</param><param name="output">Stereo for spatialized binaural processing, the configured speaker layout for panning, or the maximum-order Ambisonics channels when spatialization is disabled.</param>
    /// <returns>Whether an effect tail remains.</returns>
    public Interop.AudioEffectState Process(Interop.PathEffectParams parameters, AudioBuffer input, AudioBuffer output)
    {
        if ((uint)parameters.Order > (uint)_maximumOrder) throw new ArgumentOutOfRangeException(nameof(parameters));
        var binaural = _spatialize && parameters.Binaural == Interop.Bool.True;
        if (parameters.ShCoeffs == null || (binaural && parameters.Hrtf.Value == 0)) throw new ArgumentException("Path coefficients and a binaural HRTF must be valid.", nameof(parameters));
        Validate(input, output); if (input.Channels != _inputChannels || output.Channels != (binaural ? 2 : _outputChannels)) throw new ArgumentException("Effect channel dimensions do not match.");
        using var operation = Enter(); using var effect = Acquire(); using var src = input.Borrow(); using var dst = output.Borrow();
        _lastBinaural = binaural;
        return Interop.Api.PathEffectApply(new(effect.Value), &parameters, src.Native, dst.Native);
    }
    /// <summary>Remaining tail samples per channel.</summary>
    public int TailSize { get { using var operation = Enter(); using var effect = Acquire(); return Interop.Api.PathEffectGetTailSize(new(effect.Value)); } }
    /// <summary>Drains a complete tail block after input ends.</summary>
    /// <param name="output">Output matching the most recent processing mode and configured frame count.</param><returns>Whether further tail blocks remain.</returns>
    public Interop.AudioEffectState GetTail(AudioBuffer output)
    {
        using var operation = Enter();
        Validate(output); if (output.Channels != (_lastBinaural ? 2 : _outputChannels)) throw new ArgumentException("Incorrect output channels.");
        using var effect = Acquire(); using var dst = output.Borrow(); return Interop.Api.PathEffectGetTail(new(effect.Value), dst.Native);
    }
    /// <summary>Clears stored effect state.</summary>
    public void Reset() { using var operation = Enter(); using var effect = Acquire(); Interop.Api.PathEffectReset(new(effect.Value)); _lastBinaural = false; }
}
