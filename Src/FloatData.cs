namespace SteamAudio;
/// <summary>A retained view of contiguous native F32 data. Do not use its span after disposing the view.</summary>
public readonly unsafe ref struct FloatData
{
    private readonly Resource.Lease _lease;
    /// <summary>Borrowed native samples in the SDK's documented row-major order.</summary>
    public Span<float> Samples { get; }
    internal FloatData(Resource.Lease lease, float* pointer, int length) { _lease = lease; Samples = new(pointer, length); }
    /// <summary>Ends this view's native retention. Dispose exactly once after the final span access.</summary>
    public void Dispose() => _lease.Dispose();
}
