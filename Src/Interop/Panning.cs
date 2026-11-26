using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a panning effect.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the panning effect.</param>
    /// <param name="effect">The created panning effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplPanningEffectCreate")]
    public static partial Error PanningEffectCreate(
        ContextHandle context,
        AudioSettings* audioSettings,
        PanningEffectSettings* effectSettings,
        PanningEffectHandle* effect);

    /// <summary>
    /// Retains an additional reference to a panning effect.
    /// </summary>
    /// <param name="effect">The panning effect to retain a reference to.</param>
    /// <returns>The additional reference to the panning effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplPanningEffectRetain")]
    public static partial PanningEffectHandle PanningEffectRetain(PanningEffectHandle effect);

    /// <summary>
    /// Releases a reference to a panning effect.
    /// </summary>
    /// <param name="effect">The panning effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplPanningEffectRelease")]
    public static partial void PanningEffectRelease(PanningEffectHandle* effect);

    /// <summary>
    /// Resets the internal processing state of a panning effect.
    /// </summary>
    /// <param name="effect">The panning effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplPanningEffectReset")]
    public static partial void PanningEffectReset(PanningEffectHandle effect);

    /// <summary>
    /// Applies a panning effect to an audio buffer. This effect CANNOT be applied in-place.
    /// </summary>
    /// <param name="effect">The panning effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">The input audio buffer. Must be 1-channel.</param>
    /// <param name="out">
    /// The output audio buffer. Must have as many channels as needed for the speaker layout specified when
    /// creating the panning effect. For example, if the speaker layout is
    /// <c>IPL_SPEAKERLAYOUTTYPE_SURROUND_5_1</c>, the output buffer must contain 6 channels.
    /// </param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> to indicate that this effect does not generate any tail
    /// samples.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplPanningEffectApply")]
    public static partial AudioEffectState PanningEffectApply(
        PanningEffectHandle effect,
        PanningEffectParams* @params,
        AudioBuffer* @in,
        AudioBuffer* @out);

    /// <summary>
    /// Returns the number of tail samples remaining in a panning effect's internal buffers. Tail samples are
    /// audio samples that should be played even after the input to the effect has stopped playing and no
    /// further input samples are available.
    /// </summary>
    /// <param name="effect">The panning effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplPanningEffectGetTailSize")]
    public static partial int PanningEffectGetTailSize(PanningEffectHandle effect);

    /// <summary>
    /// Retrieves a single frame of tail samples from a panning effect's internal buffers. After the input to
    /// the panning effect has stopped, this function must be called instead of <c>iplPanningEffectApply</c>
    /// until the return value indicates that no more tail samples remain.
    /// </summary>
    /// <param name="effect">The panning effect.</param>
    /// <param name="out">
    /// The output audio buffer. Must have as many channels as needed for the speaker layout specified when
    /// creating the panning effect. For example, if the speaker layout is
    /// <c>IPL_SPEAKERLAYOUTTYPE_SURROUND_5_1</c>, the output buffer must contain 6 channels.
    /// </param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplPanningEffectGetTail")]
    public static partial AudioEffectState PanningEffectGetTail(PanningEffectHandle effect, AudioBuffer* @out);
}
