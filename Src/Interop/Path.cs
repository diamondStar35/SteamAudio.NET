// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Creates a path effect.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the path effect.</param>
    /// <param name="effect">The created path effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplPathEffectCreate")]
    public static partial Error PathEffectCreate(ContextHandle context, AudioSettings* audioSettings, PathEffectSettings* effectSettings, PathEffectHandle* effect);

    /// <summary>Retains an additional reference to a path effect.</summary>
    /// <param name="effect">The path effect to retain a reference to.</param>
    /// <returns>The additional reference to the path effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplPathEffectRetain")]
    public static partial PathEffectHandle PathEffectRetain(PathEffectHandle effect);

    /// <summary>Releases a reference to a path effect.</summary>
    /// <param name="effect">The path effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplPathEffectRelease")]
    public static partial void PathEffectRelease(PathEffectHandle* effect);

    /// <summary>Resets the internal processing state of a path effect.</summary>
    /// <param name="effect">The path effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplPathEffectReset")]
    public static partial void PathEffectReset(PathEffectHandle effect);

    /// <summary>Applies a path effect to an audio buffer.  This effect CANNOT be applied in-place.</summary>
    /// <param name="effect">The path effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">The input audio buffer. Must have 1 channel.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as needed for the Ambisonics order specified when creating the effect.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers, or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.</returns>
    [LibraryImport(Library, EntryPoint = "iplPathEffectApply")]
    public static partial AudioEffectState PathEffectApply(PathEffectHandle effect, PathEffectParams* @params, AudioBuffer* @in, AudioBuffer* @out);

    /// <summary>Returns the number of tail samples remaining in a path effect's internal buffers.  Tail samples are audio samples that should be played even after the input to the effect has stopped playing and no further input samples are available.</summary>
    /// <param name="effect">The path effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplPathEffectGetTailSize")]
    public static partial int PathEffectGetTailSize(PathEffectHandle effect);

    /// <summary>Retrieves a single frame of tail samples from a path effect's internal buffers.  After the input to the path effect has stopped, this function must be called instead of <c>iplPathEffectApply</c> until the return value indicates that no more tail samples remain.</summary>
    /// <param name="effect">The path effect.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as needed for the Ambisonics order specified when creating the effect.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers, or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.</returns>
    [LibraryImport(Library, EntryPoint = "iplPathEffectGetTail")]
    public static partial AudioEffectState PathEffectGetTail(PathEffectHandle effect, AudioBuffer* @out);
}
