using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates an HRTF. Calling this function is somewhat expensive; avoid creating HRTF objects in your audio
    /// thread at all if possible. This function is not thread-safe. Do not simultaneously call it from multiple
    /// threads.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="audioSettings">Global audio processing settings.</param>
    /// <param name="hrtfSettings">The settings used to create the HRTF object.</param>
    /// <param name="hrtf">The created HRTF object.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplHRTFCreate")]
    public static partial Error HrtfCreate(
        ContextHandle context,
        AudioSettings* audioSettings,
        HrtfSettings* hrtfSettings,
        HrtfHandle* hrtf);

    /// <summary>
    /// Retains an additional reference to an HRTF object.
    /// </summary>
    /// <param name="hrtf">The HRTF object to retain a reference to.</param>
    /// <returns>The additional reference to the HRTF object.</returns>
    [LibraryImport(Library, EntryPoint = "iplHRTFRetain")]
    public static partial HrtfHandle HrtfRetain(HrtfHandle hrtf);

    /// <summary>
    /// Releases a reference to an HRTF object.
    /// </summary>
    /// <param name="hrtf">The HRTF object to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplHRTFRelease")]
    public static partial void HrtfRelease(HrtfHandle* hrtf);
}
