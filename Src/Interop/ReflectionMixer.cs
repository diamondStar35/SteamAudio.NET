// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Creates a reflection effect mixer.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="effectSettings">The settings used when creating the reflection effects that will be mixed into this reflection mixer.</param>
    /// <param name="mixer">The created reflection mixer.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplReflectionMixerCreate")]
    public static partial Error ReflectionMixerCreate(ContextHandle context, AudioSettings* audioSettings, ReflectionEffectSettings* effectSettings, ReflectionMixerHandle* mixer);

    /// <summary>Retains an additional reference to a reflection mixer.</summary>
    /// <param name="mixer">The reflection mixer to retain a reference to.</param>
    /// <returns>The additional reference to the reflection mixer.</returns>
    [LibraryImport(Library, EntryPoint = "iplReflectionMixerRetain")]
    public static partial ReflectionMixerHandle ReflectionMixerRetain(ReflectionMixerHandle mixer);

    /// <summary>Releases a reference to a reflection mixer.</summary>
    /// <param name="mixer">The reflection mixer to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplReflectionMixerRelease")]
    public static partial void ReflectionMixerRelease(ReflectionMixerHandle* mixer);

    /// <summary>Resets the internal processing state of a reflection mixer.</summary>
    /// <param name="mixer">The reflection mixer to reset.</param>
    [LibraryImport(Library, EntryPoint = "iplReflectionMixerReset")]
    public static partial void ReflectionMixerReset(ReflectionMixerHandle mixer);

    /// <summary>Retrieves the contents of a reflection mixer and places it into an audio buffer.</summary>
    /// <param name="mixer">The reflection mixer to retrieve audio from.</param>
    /// <param name="params">Parameters for applying the effect.</param>
    /// <param name="out">The output audio buffer. Must have as many channels as the impulse response specified when creating the mixer.</param>
    /// <returns><c>IPL_AUDIOEFFECTSTATE_TAILREMAINING</c> if any tail samples remain in the effect's internal buffers, or <c>IPL_AUDIOEFFECTSTATE_TAILCOMPLETE</c> otherwise.</returns>
    [LibraryImport(Library, EntryPoint = "iplReflectionMixerApply")]
    public static partial AudioEffectState ReflectionMixerApply(ReflectionMixerHandle mixer, ReflectionEffectParams* @params, AudioBuffer* @out);
}
