using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a direct effect.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the direct effect.</param>
    /// <param name="effect">The created direct effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplDirectEffectCreate")]
    public static partial Error DirectEffectCreate(
        ContextHandle context,
        AudioSettings* audioSettings,
        DirectEffectSettings* effectSettings,
        DirectEffectHandle* effect);

    /// <summary>
    /// Retains an additional reference to a direct effect.
    /// </summary>
    /// <param name="effect">The direct effect to retain a reference to.</param>
    /// <returns>The additional reference to the direct effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplDirectEffectRetain")]
    public static partial DirectEffectHandle DirectEffectRetain(DirectEffectHandle effect);

    /// <summary>
    /// Releases a reference to a direct effect.
    /// </summary>
    /// <param name="effect">The direct effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplDirectEffectRelease")]
    public static partial void DirectEffectRelease(DirectEffectHandle* effect);

    /// <summary>
    /// Resets the internal processing state of a direct effect.
    /// </summary>
    /// <param name="effect">The direct effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplDirectEffectReset")]
    public static partial void DirectEffectReset(DirectEffectHandle effect);

    /// <summary>
    /// Applies a direct effect to an audio buffer. This effect CAN be applied in-place.
    /// </summary>
    /// <param name="effect">The direct effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">
    /// The input audio buffer. Must have as many channels as specified when creating the effect.
    /// </param>
    /// <param name="out">
    /// The output audio buffer. Must have as many channels as specified when creating the effect.
    /// </param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplDirectEffectApply")]
    public static partial AudioEffectState DirectEffectApply(
        DirectEffectHandle effect,
        DirectEffectParams* @params,
        AudioBuffer* @in,
        AudioBuffer* @out);

    /// <summary>
    /// Returns the number of tail samples remaining in a direct effect's internal buffers. Tail samples are
    /// audio samples that should be played even after the input to the effect has stopped playing and no
    /// further input samples are available.
    /// </summary>
    /// <param name="effect">The direct effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplDirectEffectGetTailSize")]
    public static partial int DirectEffectGetTailSize(DirectEffectHandle effect);

    /// <summary>
    /// Retrieves a single frame of tail samples from a direct effect's internal buffers. After the input to the
    /// direct effect has stopped, this function must be called instead of <c>iplDirectEffectApply</c> until the
    /// return value indicates that no more tail samples remain.
    /// </summary>
    /// <param name="effect">The direct effect.</param>
    /// <param name="out">
    /// The output audio buffer. Must have as many channels as specified when creating the effect.
    /// </param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplDirectEffectGetTail")]
    public static partial AudioEffectState DirectEffectGetTail(DirectEffectHandle effect, AudioBuffer* @out);
}
