// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Creates an impulse response.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="settings">The settings to use when creating the impulse response.</param>
    /// <param name="impulseResponse">The created impulse response.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseCreate")]
    public static partial Error ImpulseResponseCreate(ContextHandle context, ImpulseResponseSettings* settings, ImpulseResponseHandle* impulseResponse);

    /// <summary>Retains an additional reference to an impulse response.</summary>
    /// <param name="impulseResponse">The impulse response to retain a reference to.</param>
    /// <returns>The additional reference to the impulse response.</returns>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseRetain")]
    public static partial ImpulseResponseHandle ImpulseResponseRetain(ImpulseResponseHandle impulseResponse);

    /// <summary>Releases a reference to an impulse response.</summary>
    /// <param name="impulseResponse">The impulse response to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseRelease")]
    public static partial void ImpulseResponseRelease(ImpulseResponseHandle* impulseResponse);

    /// <summary>Returns the number of channels in the impulse response.</summary>
    /// <param name="impulseResponse">The impulse response.</param>
    /// <returns>The number of channels in the impulse response.</returns>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseGetNumChannels")]
    public static partial int ImpulseResponseGetNumChannels(ImpulseResponseHandle impulseResponse);

    /// <summary>Returns the number of samples in the impulse response.</summary>
    /// <param name="impulseResponse">The impulse response.</param>
    /// <returns>The number of samples in the impulse response.</returns>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseGetNumSamples")]
    public static partial int ImpulseResponseGetNumSamples(ImpulseResponseHandle impulseResponse);

    /// <summary>Returns a pointer to the data stored in the impulse response.</summary>
    /// <param name="impulseResponse">The impulse response.</param>
    /// <returns>Pointer to #channels * #samples IPLfloat32 values stored in the impulse response, in row-major order.</returns>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseGetData")]
    public static partial float* ImpulseResponseGetData(ImpulseResponseHandle impulseResponse);

    /// <summary>Returns a pointer to the data stored in the impulse response for the given channel.</summary>
    /// <param name="impulseResponse">The impulse response.</param>
    /// <param name="channelIndex">Index of the channel.</param>
    /// <returns>Pointer to #samples IPLfloat32 values stored in the impulse response for the given channel, in row-major order.</returns>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseGetChannel")]
    public static partial float* ImpulseResponseGetChannel(ImpulseResponseHandle impulseResponse, int channelIndex);

    /// <summary>Resets all values stored in an impulse response to zero.</summary>
    /// <param name="impulseResponse">The impulse response.</param>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseReset")]
    public static partial void ImpulseResponseReset(ImpulseResponseHandle impulseResponse);

    /// <summary>Copies data from one impulse response into another.  If the source and destination impulse responses have different numbers of channels, only the smaller of the two numbers of channels will be copied.  If the source and destination impulse responses have different numbers of samples, only the smaller of the two numbers of samples will be copied.</summary>
    /// <param name="src">The source impulse response.</param>
    /// <param name="dst">The destination impulse response.</param>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseCopy")]
    public static partial void ImpulseResponseCopy(ImpulseResponseHandle src, ImpulseResponseHandle dst);

    /// <summary>Swaps the data contained in one impulse response with the data contained in another impulse response.  The two impulse responses may contain different numbers of channels or samples.</summary>
    /// <param name="ir1">The first impulse response.</param>
    /// <param name="ir2">The second impulse response.</param>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseSwap")]
    public static partial void ImpulseResponseSwap(ImpulseResponseHandle ir1, ImpulseResponseHandle ir2);

    /// <summary>Adds the values stored in two impulse responses, and stores the result in a third impulse response.  If the impulse responses have different numbers of channels, only the smallest of the three numbers of channels will be added.  If the impulse responses have different numbers of samples, only the smallest of the three numbers of samples will be added.  This function can be used for in-place addition, i.e. <c>out</c> may be equal to <c>in1</c> or <c>in2.</c></summary>
    /// <param name="in1">The first input impulse response.</param>
    /// <param name="in2">The second input impulse response.</param>
    /// <param name="out">The output impulse response.</param>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseAdd")]
    public static partial void ImpulseResponseAdd(ImpulseResponseHandle in1, ImpulseResponseHandle in2, ImpulseResponseHandle @out);

    /// <summary>Scales the values stored in an impulse response by a scalar, and stores the result in a second impulse response.  If the impulse responses have different numbers of channels, only the smallest of the two numbers of channels will be scaled.  If the impulse responses have different numbers of samples, only the smallest of the two numbers of samples will be scaled.  This function can be used for in-place scaling, i.e. <c>out</c> may be equal to <c>in.</c></summary>
    /// <param name="in">The input impulse response.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="out">The output impulse response.</param>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseScale")]
    public static partial void ImpulseResponseScale(ImpulseResponseHandle @in, float scalar, ImpulseResponseHandle @out);

    /// <summary>Scales the values stored in an impulse response by a scalar, and adds the result to a second impulse response.  If the impulse responses have different numbers of channels, only the smallest of the two numbers of channels will be added.  If the impulse responses have different numbers of samples, only the smallest of the two numbers of samples will be added.  This function can be used for in-place operation, i.e. <c>out</c> may be equal to <c>in.</c></summary>
    /// <param name="in">The input impulse response.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="out">The output impulse response.</param>
    [LibraryImport(Library, EntryPoint = "iplImpulseResponseScaleAccum")]
    public static partial void ImpulseResponseScaleAccum(ImpulseResponseHandle @in, float scalar, ImpulseResponseHandle @out);
}
