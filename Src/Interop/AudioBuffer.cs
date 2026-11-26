// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Allocates an audio buffer.  All audio buffers are uncompressed PCM with 32-bit floating-point samples.  Internally, all audio buffers are stored deinterleaved for performance reasons. If your audio engine provides interleaved audio buffers, you must use <c>iplAudioBufferInterleave</c> and <c>iplAudioBufferDeinterleave</c> to explicitly convert to/from deinterleaved format. If your audio engine provides deinterleaved audio buffers, you can pass them directly using <c>IPLAudioBuffer</c>, thus avoiding the processing and memory overhead of an extra audio buffer.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="numChannels">Number of channels.</param>
    /// <param name="numSamples">Number of samples per channel.</param>
    /// <param name="audioBuffer">The audio buffer to allocate.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplAudioBufferAllocate")]
    public static partial Error AudioBufferAllocate(ContextHandle context, int numChannels, int numSamples, AudioBuffer* audioBuffer);

    /// <summary>Frees an audio buffer.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioBuffer">The audio buffer to free.</param>
    [LibraryImport(Library, EntryPoint = "iplAudioBufferFree")]
    public static partial void AudioBufferFree(ContextHandle context, AudioBuffer* audioBuffer);

    /// <summary>Reads samples from an audio buffer and interleaves them into a user-provided array.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="src">The audio buffer to read from.</param>
    /// <param name="dst">The interleaved array to write into.</param>
    [LibraryImport(Library, EntryPoint = "iplAudioBufferInterleave")]
    public static partial void AudioBufferInterleave(ContextHandle context, AudioBuffer* src, float* dst);

    /// <summary>Writes interleaved samples from a user-provided array into an audio buffer.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="src">The interleaved array to read from.</param>
    /// <param name="dst">The audio buffer to write into.</param>
    [LibraryImport(Library, EntryPoint = "iplAudioBufferDeinterleave")]
    public static partial void AudioBufferDeinterleave(ContextHandle context, float* src, AudioBuffer* dst);

    /// <summary>Mixes one audio buffer into another.  Both audio buffers must have the same number of channels and samples.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="in">The source audio buffer.</param>
    /// <param name="mix">The destination audio buffer, into which the source should be mixed.</param>
    [LibraryImport(Library, EntryPoint = "iplAudioBufferMix")]
    public static partial void AudioBufferMix(ContextHandle context, AudioBuffer* @in, AudioBuffer* mix);

    /// <summary>Downmixes a multi-channel audio buffer into a mono audio buffer.  Both audio buffers must have the same number of samples.  Downmixing is performed by summing up the source channels and dividing the result by the number of source channels. If this is not the desired downmixing behavior, we recommend that downmixing be performed manually.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="in">The source audio buffer.</param>
    /// <param name="out">The destination audio buffer.</param>
    [LibraryImport(Library, EntryPoint = "iplAudioBufferDownmix")]
    public static partial void AudioBufferDownmix(ContextHandle context, AudioBuffer* @in, AudioBuffer* @out);

    /// <summary>Converts an Ambisonic audio buffer from one Ambisonic format to another.  Both audio buffers must have the same number of samples.  This conversion can be applied in-place, i.e., <c>in</c> and <c>out</c> can be the same audio buffer.  Steam Audio's "native" Ambisonic format is N3D, so for best performance, keep all Ambisonic data in N3D format except when exchanging data with your audio engine.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="inType">Ambisonic format of <c>in.</c></param>
    /// <param name="outType">Ambisonic format that <c>out</c> should be in.</param>
    /// <param name="in">The source audio buffer.</param>
    /// <param name="out">The destination audio buffer.</param>
    [LibraryImport(Library, EntryPoint = "iplAudioBufferConvertAmbisonics")]
    public static partial void AudioBufferConvertAmbisonics(ContextHandle context, AmbisonicsType inType, AmbisonicsType outType, AudioBuffer* @in, AudioBuffer* @out);
}
