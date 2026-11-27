namespace SteamAudio.Effects;

/// <summary>
/// Fixed-block DSP state. One effect instance processes one operation at a time. Independent effects may
/// run concurrently when they do not share mutable native workspaces, including HRTF interpolation scratch.
/// </summary>
public abstract class Effect : Resource
{
    private int _busy;
    internal readonly nint ContextAddress;

    /// <summary>
    /// Fixed sample rate and frame size selected at creation.
    /// </summary>
    public AudioSettings Settings
    {
        get;
    }

    private protected Effect(
        nint handle,
        Action<nint> release,
        Context context,
        AudioSettings settings,
        params Resource[] resources)
        : base(handle, release, [context, .. resources])
    {
        Settings = settings;
        ContextAddress = context.DangerousGetHandle();
    }

    internal void Validate(AudioBuffer input, AudioBuffer output, bool inPlace = false)
    {
        if (!inPlace && ReferenceEquals(input, output))
        {
            throw new ArgumentException("This effect cannot process in place.");
        }

        Validate(output);
        Validate(input);
    }

    internal void Validate(AudioBuffer buffer)
    {
        if (buffer.ContextAddress != ContextAddress || buffer.Frames != Settings.FrameSize)
        {
            throw new ArgumentException("Buffer context or frame size does not match the effect.");
        }
    }

    internal static int LayoutChannels(Interop.SpeakerLayout layout) => layout.Type switch
    {
        Interop.SpeakerLayoutType.Mono => 1,
        Interop.SpeakerLayoutType.Stereo => 2,
        Interop.SpeakerLayoutType.Quadraphonic => 4,
        Interop.SpeakerLayoutType.Surround51 => 6,
        Interop.SpeakerLayoutType.Surround71 => 8,
        Interop.SpeakerLayoutType.Custom when layout.NumSpeakers > 0 => layout.NumSpeakers,
        _ => throw new ArgumentException("Invalid speaker layout.")
    };
    internal Operation Enter()
    {
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
        {
            throw new InvalidOperationException("Concurrent processing of one effect is not supported.");
        }

        return new(this);
    }

    internal readonly ref struct Operation(Effect effect)
    {
        public void Dispose() => Volatile.Write(ref effect._busy, 0);
    }
}
