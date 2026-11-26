using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a virtual surround effect.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the virtual surround effect.</param>
    /// <param name="effect">The created virtual surround effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplVirtualSurroundEffectCreate")]
    public static partial Error VirtualSurroundEffectCreate(
        ContextHandle context,
        AudioSettings* audioSettings,
        VirtualSurroundEffectSettings* effectSettings,
        VirtualSurroundEffectHandle* effect);

    /// <summary>
    /// Retains an additional reference to a virtual surround effect.
    /// </summary>
    /// <param name="effect">The virtual surround effect to retain a reference to.</param>
    /// <returns>The additional reference to the virtual surround effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplVirtualSurroundEffectRetain")]
    public static partial VirtualSurroundEffectHandle VirtualSurroundEffectRetain(VirtualSurroundEffectHandle effect);

    /// <summary>
    /// Releases a reference to a virtual surround effect.
    /// </summary>
    /// <param name="effect">The virtual surround effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplVirtualSurroundEffectRelease")]
    public static partial void VirtualSurroundEffectRelease(VirtualSurroundEffectHandle* effect);

    /// <summary>
    /// Resets the internal processing state of a virtual surround effect.
    /// </summary>
    /// <param name="effect">The virtual surround effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplVirtualSurroundEffectReset")]
    public static partial void VirtualSurroundEffectReset(VirtualSurroundEffectHandle effect);

    /// <summary>
    /// Applies a virtual surround effect to an audio buffer. This effect CANNOT be applied in-place.
    /// </summary>
    /// <param name="effect">The virtual surround effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">
    /// The input audio buffer. Must have as many channels as needed for the speaker layout specified when
    /// creating the virtual surround effect.
    /// </param>
    /// <param name="out">The output audio buffer. Must be 2-channel.</param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplVirtualSurroundEffectApply")]
    public static partial AudioEffectState VirtualSurroundEffectApply(
        VirtualSurroundEffectHandle effect,
        VirtualSurroundEffectParams* @params,
        AudioBuffer* @in,
        AudioBuffer* @out);

    /// <summary>
    /// Returns the number of tail samples remaining in a virtual surround effect's internal buffers. Tail
    /// samples are audio samples that should be played even after the input to the effect has stopped playing
    /// and no further input samples are available.
    /// </summary>
    /// <param name="effect">The virtual surround effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplVirtualSurroundEffectGetTailSize")]
    public static partial int VirtualSurroundEffectGetTailSize(VirtualSurroundEffectHandle effect);

    /// <summary>
    /// Retrieves a single frame of tail samples from a virtual surround effect's internal buffers. After the
    /// input to the virtual surround effect has stopped, this function must be called instead of
    /// <c>iplVirtualSurroundEffectApply</c> until the return value indicates that no more tail samples remain.
    /// </summary>
    /// <param name="effect">The virtual surround effect.</param>
    /// <param name="out">The output audio buffer. Must be 2-channel.</param>
    /// <returns>
    /// <c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers,
    /// or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplVirtualSurroundEffectGetTail")]
    public static partial AudioEffectState VirtualSurroundEffectGetTail(
        VirtualSurroundEffectHandle effect,
        AudioBuffer* @out);
}
