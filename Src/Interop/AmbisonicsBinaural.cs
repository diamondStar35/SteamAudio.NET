using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates an Ambisonics binaural effect.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the Ambisonics binaural effect.</param>
    /// <param name="effect">The created Ambisonics binaural effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsBinauralEffectCreate")]
    public static partial Error AmbisonicsBinauralEffectCreate(
        ContextHandle context,
        AudioSettings* audioSettings,
        AmbisonicsBinauralEffectSettings* effectSettings,
        AmbisonicsBinauralEffectHandle* effect);

    /// <summary>
    /// Retains an additional reference to an Ambisonics binaural effect.
    /// </summary>
    /// <param name="effect">The Ambisonics binaural effect to retain a reference to.</param>
    /// <returns>The additional reference to the Ambisonics binaural effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsBinauralEffectRetain")]
    public static partial AmbisonicsBinauralEffectHandle AmbisonicsBinauralEffectRetain(AmbisonicsBinauralEffectHandle effect);

    /// <summary>
    /// Releases a reference to an Ambisonics binaural effect.
    /// </summary>
    /// <param name="effect">The Ambisonics binaural effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsBinauralEffectRelease")]
    public static partial void AmbisonicsBinauralEffectRelease(AmbisonicsBinauralEffectHandle* effect);

    /// <summary>
    /// Resets the internal processing state of an Ambisonics binaural effect.
    /// </summary>
    /// <param name="effect">The Ambisonics binaural effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsBinauralEffectReset")]
    public static partial void AmbisonicsBinauralEffectReset(AmbisonicsBinauralEffectHandle effect);

    /// <summary>
    /// Applies an Ambisonics binaural effect to an audio buffer. This effect CANNOT be applied in-place.
    /// </summary>
    /// <param name="effect">The Ambisonics binaural effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">
    /// The input audio buffer. Must have as many channels as needed for the Ambisonics order specified in the
    /// parameters.
    /// </param>
    /// <param name="out">The output audio buffer. Must have 2 channels.</param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsBinauralEffectApply")]
    public static partial AudioEffectState AmbisonicsBinauralEffectApply(
        AmbisonicsBinauralEffectHandle effect,
        AmbisonicsBinauralEffectParams* @params,
        AudioBuffer* @in,
        AudioBuffer* @out);

    /// <summary>
    /// Returns the number of tail samples remaining in an Ambisonics binaural effect's internal buffers. Tail
    /// samples are audio samples that should be played even after the input to the effect has stopped playing
    /// and no further input samples are available.
    /// </summary>
    /// <param name="effect">The Ambisonics binaural effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsBinauralEffectGetTailSize")]
    public static partial int AmbisonicsBinauralEffectGetTailSize(AmbisonicsBinauralEffectHandle effect);

    /// <summary>
    /// Retrieves a single frame of tail samples from an Ambisonics binaural effect's internal buffers. After
    /// the input to the Ambisonics binaural effect has stopped, this function must be called instead of
    /// <c>iplAmbisonicsBinauralEffectApply</c> until the return value indicates that no more tail samples
    /// remain.
    /// </summary>
    /// <param name="effect">The Ambisonics binaural effect.</param>
    /// <param name="out">The output audio buffer. Must have 2 channels.</param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsBinauralEffectGetTail")]
    public static partial AudioEffectState AmbisonicsBinauralEffectGetTail(
        AmbisonicsBinauralEffectHandle effect,
        AudioBuffer* @out);
}
