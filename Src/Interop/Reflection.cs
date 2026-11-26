// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Creates a reflection effect.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings to use when creating the reflection effect.</param>
    /// <param name="effect">The created reflection effect.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplReflectionEffectCreate")]
    public static partial Error ReflectionEffectCreate(ContextHandle context, AudioSettings* audioSettings, ReflectionEffectSettings* effectSettings, ReflectionEffectHandle* effect);

    /// <summary>Retains an additional reference to a reflection effect.</summary>
    /// <param name="effect">The reflection effect to retain a reference to.</param>
    /// <returns>The additional reference to the reflection effect.</returns>
    [LibraryImport(Library, EntryPoint = "iplReflectionEffectRetain")]
    public static partial ReflectionEffectHandle ReflectionEffectRetain(ReflectionEffectHandle effect);

    /// <summary>Releases a reference to a reflection effect.</summary>
    /// <param name="effect">The reflection effect to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplReflectionEffectRelease")]
    public static partial void ReflectionEffectRelease(ReflectionEffectHandle* effect);

    /// <summary>Resets the internal processing state of a reflection effect.</summary>
    /// <param name="effect">The reflection effect to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplReflectionEffectReset")]
    public static partial void ReflectionEffectReset(ReflectionEffectHandle effect);

    /// <summary>Applies a reflection effect to an audio buffer.  This effect CANNOT be applied in-place.</summary>
    /// <param name="effect">The reflection effect to apply.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="in">The input audio buffer. Must have 1 channel.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as the impulse response specified when creating the effect (for convolution, hybrid, and TAN) or at least 1 channel (for parametric).</param>
    /// <param name="mixer">If this is non-null, then the output of this effect will be mixed into the given mixer object instead of being returned in the <c>out</c> parameter. The mixed output can be retrieved elsewhere in the audio pipeline using <c>iplReflectionMixerApply.</c> This can have a performance benefit if using convolution. If using TAN, specifying a mixer is required.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers, or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.</returns>
    [LibraryImport(Library, EntryPoint = "iplReflectionEffectApply")]
    public static partial AudioEffectState ReflectionEffectApply(ReflectionEffectHandle effect, ReflectionEffectParams* @params, AudioBuffer* @in, AudioBuffer* @out, ReflectionMixerHandle mixer);

    /// <summary>Returns the number of tail samples remaining in a reflection effect's internal buffers.  Tail samples are audio samples that should be played even after the input to the effect has stopped playing and no further input samples are available.</summary>
    /// <param name="effect">The reflection effect.</param>
    /// <returns>The number of tail samples remaining.</returns>
    [LibraryImport(Library, EntryPoint = "iplReflectionEffectGetTailSize")]
    public static partial int ReflectionEffectGetTailSize(ReflectionEffectHandle effect);

    /// <summary>Retrieves a single frame of tail samples from a reflection effect's internal buffers.  After the input to the reflection effect has stopped, this function must be called instead of <c>iplReflectionEffectApply</c> until the return value indicates that no more tail samples remain.</summary>
    /// <param name="effect">The reflection effect.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as the impulse response specified when creating the effect (for convolution, hybrid, and TAN) or at least 1 channel (for parametric).</param>
    /// <param name="mixer">If this is non-null, then the tail samples will be mixed into the given mixer object instead of being returned in the <c>out</c> parameter. The mixed output can be retrieved elsewhere in the audio pipeline using <c>iplReflectionMixerApply.</c> This can have a performance benefit if using convolution. If using TAN, specifying a mixer is required.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers, or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.</returns>
    [LibraryImport(Library, EntryPoint = "iplReflectionEffectGetTail")]
    public static partial AudioEffectState ReflectionEffectGetTail(ReflectionEffectHandle effect, AudioBuffer* @out, ReflectionMixerHandle mixer);
}
