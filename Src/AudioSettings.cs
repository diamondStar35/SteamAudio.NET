namespace SteamAudio;
/// <summary>Immutable sample rate and fixed processing frame size. Values must match every buffer and effect in a processing graph.</summary>
public readonly record struct AudioSettings
{
    /// <summary>Samples per second per channel.</summary>
    public int SampleRate { get; }
    /// <summary>Complete samples per channel in each processing call.</summary>
    public int FrameSize { get; }
    /// <summary>Validates and captures a fixed processing format.</summary>
    /// <param name="sampleRate">Positive sample rate in Hz.</param><param name="frameSize">Positive block length in frames.</param>
    public AudioSettings(int sampleRate, int frameSize)
    {
        if (sampleRate <= 0 || frameSize <= 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        SampleRate = sampleRate; FrameSize = frameSize;
    }
    internal Interop.AudioSettings Native => SampleRate > 0 && FrameSize > 0 ? new() { SamplingRate = SampleRate, FrameSize = FrameSize } : throw new ArgumentException("Audio settings must be initialized.");
}
