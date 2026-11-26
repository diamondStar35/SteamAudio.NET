// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Creates an Ambisonics rotation effect.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the Ambisonics rotation effect.</param>
    /// <param name="effect">The created Ambisonics rotation effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsRotationEffectCreate")]
    public static partial Error AmbisonicsRotationEffectCreate(ContextHandle context, AudioSettings* audioSettings, AmbisonicsRotationEffectSettings* effectSettings, AmbisonicsRotationEffectHandle* effect);

    /// <summary>Retains an additional reference to an Ambisonics rotation effect.</summary>
    /// <param name="effect">The Ambisonics rotation effect to retain a reference to.</param>
    /// <returns>The additional reference to the Ambisonics rotation effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsRotationEffectRetain")]
    public static partial AmbisonicsRotationEffectHandle AmbisonicsRotationEffectRetain(AmbisonicsRotationEffectHandle effect);

    /// <summary>Releases a reference to an Ambisonics rotation effect.</summary>
    /// <param name="effect">The Ambisonics rotation effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsRotationEffectRelease")]
    public static partial void AmbisonicsRotationEffectRelease(AmbisonicsRotationEffectHandle* effect);

    /// <summary>Resets the internal processing state of an Ambisonics rotation effect.</summary>
    /// <param name="effect">The Ambisonics rotation effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsRotationEffectReset")]
    public static partial void AmbisonicsRotationEffectReset(AmbisonicsRotationEffectHandle effect);

    /// <summary>Applies an Ambisonics rotation effect to an audio buffer.  This effect CANNOT be applied in-place.</summary>
    /// <param name="effect">The Ambisonics rotation effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">The input audio buffer. Must have as many channels as needed for the Ambisonics order specified when creating the effect.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as needed for the Ambisonics order specified when creating the effect.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> to indicate that this effect does not generate any tail samples.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsRotationEffectApply")]
    public static partial AudioEffectState AmbisonicsRotationEffectApply(AmbisonicsRotationEffectHandle effect, AmbisonicsRotationEffectParams* @params, AudioBuffer* @in, AudioBuffer* @out);

    /// <summary>Returns the number of tail samples remaining in an Ambisonics rotation effect's internal buffers.  Tail samples are audio samples that should be played even after the input to the effect has stopped playing and no further input samples are available.</summary>
    /// <param name="effect">The Ambisonics rotation effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsRotationEffectGetTailSize")]
    public static partial int AmbisonicsRotationEffectGetTailSize(AmbisonicsRotationEffectHandle effect);

    /// <summary>Retrieves a single frame of tail samples from an Ambisonics rotation effect's internal buffers.  After the input to the Ambisonics rotation effect has stopped, this function must be called instead of <c>iplAmbisonicsRotationEffectApply</c> until the return value indicates that no more tail samples remain.</summary>
    /// <param name="effect">The Ambisonics rotation effect.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as needed for the Ambisonics order specified when creating the effect.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers, or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsRotationEffectGetTail")]
    public static partial AudioEffectState AmbisonicsRotationEffectGetTail(AmbisonicsRotationEffectHandle effect, AudioBuffer* @out);
}
