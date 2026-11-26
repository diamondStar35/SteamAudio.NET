using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates an Ambisonics encode effect.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the Ambisonics encode effect.</param>
    /// <param name="effect">The created Ambisonics encode effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsEncodeEffectCreate")]
    public static partial Error AmbisonicsEncodeEffectCreate(
        ContextHandle context,
        AudioSettings* audioSettings,
        AmbisonicsEncodeEffectSettings* effectSettings,
        AmbisonicsEncodeEffectHandle* effect);

    /// <summary>
    /// Retains an additional reference to an Ambisonics encode effect.
    /// </summary>
    /// <param name="effect">The Ambisonics encode effect to retain a reference to.</param>
    /// <returns>The additional reference to the Ambisonics encode effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsEncodeEffectRetain")]
    public static partial AmbisonicsEncodeEffectHandle AmbisonicsEncodeEffectRetain(AmbisonicsEncodeEffectHandle effect);

    /// <summary>
    /// Releases a reference to an Ambisonics encode effect.
    /// </summary>
    /// <param name="effect">The Ambisonics encode effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsEncodeEffectRelease")]
    public static partial void AmbisonicsEncodeEffectRelease(AmbisonicsEncodeEffectHandle* effect);

    /// <summary>
    /// Resets the internal processing state of an Ambisonics encode effect.
    /// </summary>
    /// <param name="effect">The Ambisonics encode effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsEncodeEffectReset")]
    public static partial void AmbisonicsEncodeEffectReset(AmbisonicsEncodeEffectHandle effect);

    /// <summary>
    /// Applies an Ambisonics encode effect to an audio buffer. This effect CANNOT be applied in-place.
    /// </summary>
    /// <param name="effect">The Ambisonics encode effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">The input audio buffer. Must be 1-channel.</param>
    /// <param name="out">
    /// The output audio buffer. Must have as many channels as needed for the Ambisonics order specified when
    /// creating the effect.
    /// </param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> to indicate that this effect does not generate any tail
    /// samples.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsEncodeEffectApply")]
    public static partial AudioEffectState AmbisonicsEncodeEffectApply(
        AmbisonicsEncodeEffectHandle effect,
        AmbisonicsEncodeEffectParams* @params,
        AudioBuffer* @in,
        AudioBuffer* @out);

    /// <summary>
    /// Returns the number of tail samples remaining in an Ambisonics encode effect's internal buffers. Tail
    /// samples are audio samples that should be played even after the input to the effect has stopped playing
    /// and no further input samples are available.
    /// </summary>
    /// <param name="effect">The Ambisonics encode effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsEncodeEffectGetTailSize")]
    public static partial int AmbisonicsEncodeEffectGetTailSize(AmbisonicsEncodeEffectHandle effect);

    /// <summary>
    /// Retrieves a single frame of tail samples from an Ambisonics encode effect's internal buffers. After the
    /// input to the Ambisonics encode effect has stopped, this function must be called instead of
    /// <c>iplAmbisonicsEncodeEffectApply</c> until the return value indicates that no more tail samples remain.
    /// </summary>
    /// <param name="effect">The Ambisonics encode effect.</param>
    /// <param name="out">
    /// The output audio buffer. Must have as many channels as needed for the Ambisonics order specified when
    /// creating the effect.
    /// </param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsEncodeEffectGetTail")]
    public static partial AudioEffectState AmbisonicsEncodeEffectGetTail(
        AmbisonicsEncodeEffectHandle effect,
        AudioBuffer* @out);
}
