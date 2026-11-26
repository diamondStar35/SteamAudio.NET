// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Creates an Ambisonics panning effect.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the Ambisonics panning effect.</param>
    /// <param name="effect">The created Ambisonics panning effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsPanningEffectCreate")]
    public static partial Error AmbisonicsPanningEffectCreate(ContextHandle context, AudioSettings* audioSettings, AmbisonicsPanningEffectSettings* effectSettings, AmbisonicsPanningEffectHandle* effect);

    /// <summary>Retains an additional reference to an Ambisonics panning effect.</summary>
    /// <param name="effect">The Ambisonics panning effect to retain a reference to.</param>
    /// <returns>The additional reference to the Ambisonics panning effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsPanningEffectRetain")]
    public static partial AmbisonicsPanningEffectHandle AmbisonicsPanningEffectRetain(AmbisonicsPanningEffectHandle effect);

    /// <summary>Releases a reference to an Ambisonics panning effect.</summary>
    /// <param name="effect">The Ambisonics panning effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsPanningEffectRelease")]
    public static partial void AmbisonicsPanningEffectRelease(AmbisonicsPanningEffectHandle* effect);

    /// <summary>Resets the internal processing state of an Ambisonics panning effect.</summary>
    /// <param name="effect">The Ambisonics panning effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsPanningEffectReset")]
    public static partial void AmbisonicsPanningEffectReset(AmbisonicsPanningEffectHandle effect);

    /// <summary>Applies an Ambisonics panning effect to an audio buffer.  This effect CANNOT be applied in-place.</summary>
    /// <param name="effect">The Ambisonics panning effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">The input audio buffer. Must have as many channels as needed for the Ambisonics order specified in the parameters.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as needed for the speaker layout specified when creating the effect.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> to indicate that this effect does not generate any tail samples.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsPanningEffectApply")]
    public static partial AudioEffectState AmbisonicsPanningEffectApply(AmbisonicsPanningEffectHandle effect, AmbisonicsPanningEffectParams* @params, AudioBuffer* @in, AudioBuffer* @out);

    /// <summary>Returns the number of tail samples remaining in an Ambisonics panning effect's internal buffers.  Tail samples are audio samples that should be played even after the input to the effect has stopped playing and no further input samples are available.</summary>
    /// <param name="effect">The Ambisonics panning effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsPanningEffectGetTailSize")]
    public static partial int AmbisonicsPanningEffectGetTailSize(AmbisonicsPanningEffectHandle effect);

    /// <summary>Retrieves a single frame of tail samples from a Ambisonics panning effect's internal buffers.  After the input to the Ambisonics panning effect has stopped, this function must be called instead of <c>iplAmbisonicsPanningEffectApply</c> until the return value indicates that no more tail samples remain.</summary>
    /// <param name="effect">The Ambisonics panning effect.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as needed for the speaker layout specified when creating the effect.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers, or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.</returns>
    [LibraryImport(Library, EntryPoint = "iplAmbisonicsPanningEffectGetTail")]
    public static partial AudioEffectState AmbisonicsPanningEffectGetTail(AmbisonicsPanningEffectHandle effect, AudioBuffer* @out);
}
