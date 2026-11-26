using System.Runtime.InteropServices;
namespace SteamAudio;
/// <summary>Owned planar F32 storage. Borrow a view for scoped channel access; keep processing away from allocation and I/O.</summary>
public sealed unsafe class AudioBuffer : Resource
{
    internal struct Allocation { internal Interop.ContextHandle Context; internal Interop.AudioBuffer Buffer; }
    /// <summary>Number of planar channels.</summary>
    public int Channels { get; }
    /// <summary>Samples per channel.</summary>
    public int Frames { get; }
    internal nint ContextAddress { get; }
    /// <summary>Allocates and clears a planar native buffer.</summary>
    /// <param name="context">Owning context.</param><param name="channels">Positive number of channels.</param><param name="frames">Positive samples per channel.</param>
    public AudioBuffer(Context context, int channels, int frames) : base(Create(context, channels, frames), Release, context)
    { Channels = channels; Frames = frames; ContextAddress = context.DangerousGetHandle(); Clear(); }
    private static nint Create(Context context, int channels, int frames)
    {
        if (channels <= 0 || frames <= 0) throw new ArgumentOutOfRangeException(nameof(channels));
        _ = checked(channels * frames); using var owner = context.Acquire();
        var allocation = (Allocation*)NativeMemory.AllocZeroed((nuint)sizeof(Allocation));
        if (allocation == null) throw new OutOfMemoryException(); allocation->Context = new(owner.Value);
        try { SteamAudioException.Check(Interop.Api.AudioBufferAllocate(allocation->Context, channels, frames, &allocation->Buffer), "Allocate audio buffer"); return (nint)allocation; }
        catch { NativeMemory.Free(allocation); throw; }
    }
    private static void Release(nint value) { var allocation = (Allocation*)value; Interop.Api.AudioBufferFree(allocation->Context, &allocation->Buffer); NativeMemory.Free(allocation); }
    /// <summary>Retains the native allocation for the lifetime of a stack-only view.</summary>
    /// <returns>A view that must be disposed after the last use of its spans.</returns>
    public BufferView Borrow() => new(this);
    /// <summary>Clears every channel without allocating.</summary>
    public void Clear() { using var view = Borrow(); for (var channel = 0; channel < Channels; channel++) view.Channel(channel).Clear(); }
    /// <summary>Copies complete interleaved frames into planar storage.</summary>
    /// <param name="input">Exactly Channels multiplied by Frames samples.</param>
    public void Deinterleave(ReadOnlySpan<float> input)
    {
        if (input.Length != Channels * Frames) throw new ArgumentException("Incorrect interleaved sample count.", nameof(input));
        using var view = Borrow(); fixed (float* data = input) Interop.Api.AudioBufferDeinterleave(new(ContextAddress), data, view.Native);
    }
    /// <summary>Copies planar channels into complete interleaved frames.</summary>
    /// <param name="output">Storage for exactly Channels multiplied by Frames samples.</param>
    public void Interleave(Span<float> output)
    {
        if (output.Length != Channels * Frames) throw new ArgumentException("Incorrect interleaved sample count.", nameof(output));
        using var view = Borrow(); fixed (float* data = output) Interop.Api.AudioBufferInterleave(new(ContextAddress), view.Native, data);
    }
}
/// <summary>A retained stack-only view. Spans are borrowed and must not be used after Dispose.</summary>
public readonly unsafe ref struct BufferView
{
    private readonly Resource.Lease _lease;
    internal Interop.AudioBuffer* Native => &((AudioBuffer.Allocation*)_lease.Value)->Buffer;
    internal BufferView(AudioBuffer owner) => _lease = owner.Acquire();
    /// <summary>Returns one complete planar channel without copying.</summary>
    /// <param name="index">Zero-based channel index.</param><returns>Borrowed samples valid until this view is disposed.</returns>
    public Span<float> Channel(int index)
    {
        if ((uint)index >= (uint)Native->NumChannels) throw new ArgumentOutOfRangeException(nameof(index));
        return new(Native->Data[index], Native->NumSamples);
    }
    /// <summary>Releases this view's native retention. Call once, after all spans are no longer used.</summary>
    public void Dispose() => _lease.Dispose();
}
