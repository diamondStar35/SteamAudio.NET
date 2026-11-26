using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a binaural effect.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the binaural effect.</param>
    /// <param name="effect">The created binaural effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplBinauralEffectCreate")]
    public static partial Error BinauralEffectCreate(
        ContextHandle context,
        AudioSettings* audioSettings,
        BinauralEffectSettings* effectSettings,
        BinauralEffectHandle* effect);

    /// <summary>
    /// Retains an additional reference to a binaural effect.
    /// </summary>
    /// <param name="effect">The binaural effect to retain a reference to.</param>
    /// <returns>The additional reference to the binaural effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplBinauralEffectRetain")]
    public static partial BinauralEffectHandle BinauralEffectRetain(BinauralEffectHandle effect);

    /// <summary>
    /// Releases a reference to a binaural effect.
    /// </summary>
    /// <param name="effect">The binaural effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplBinauralEffectRelease")]
    public static partial void BinauralEffectRelease(BinauralEffectHandle* effect);

    /// <summary>
    /// Resets the internal processing state of a binaural effect.
    /// </summary>
    /// <param name="effect">The binaural effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplBinauralEffectReset")]
    public static partial void BinauralEffectReset(BinauralEffectHandle effect);

    /// <summary>
    /// Applies a binaural effect to an audio buffer. This effect CANNOT be applied in-place.
    /// </summary>
    /// <param name="effect">The binaural effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">The input audio buffer. Must be 1- or 2-channel.</param>
    /// <param name="out">The output audio buffer. Must be 2-channel.</param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplBinauralEffectApply")]
    public static partial AudioEffectState BinauralEffectApply(
        BinauralEffectHandle effect,
        BinauralEffectParams* @params,
        AudioBuffer* @in,
        AudioBuffer* @out);

    /// <summary>
    /// Returns the number of tail samples remaining in a binaural effect's internal buffers. Tail samples are
    /// audio samples that should be played even after the input to the effect has stopped playing and no
    /// further input samples are available.
    /// </summary>
    /// <param name="effect">The binaural effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplBinauralEffectGetTailSize")]
    public static partial int BinauralEffectGetTailSize(BinauralEffectHandle effect);

    /// <summary>
    /// Retrieves a single frame of tail samples from a binaural effect's internal buffers. After the input to
    /// the binaural effect has stopped, this function must be called instead of <c>iplBinauralEffectApply</c>
    /// until the return value indicates that no more tail samples remain.
    /// </summary>
    /// <param name="effect">The binaural effect.</param>
    /// <param name="out">The output audio buffer. Must be 2-channel.</param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplBinauralEffectGetTail")]
    public static partial AudioEffectState BinauralEffectGetTail(BinauralEffectHandle effect, AudioBuffer* @out);
}
