using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a reconstructor.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="settings">The settings to use when creating the reconstructor.</param>
    /// <param name="reconstructor">The created reconstructor.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplReconstructorCreate")]
    public static partial Error ReconstructorCreate(
        ContextHandle context,
        ReconstructorSettings* settings,
        ReconstructorHandle* reconstructor);

    /// <summary>
    /// Retains an additional reference to a reconstructor.
    /// </summary>
    /// <param name="reconstructor">The reconstructor to retain a reference to.</param>
    /// <returns>The additional reference to the reconstructor.</returns>
    [LibraryImport(Library, EntryPoint = "iplReconstructorRetain")]
    public static partial ReconstructorHandle ReconstructorRetain(ReconstructorHandle reconstructor);

    /// <summary>
    /// Releases a reference to a reconstructor.
    /// </summary>
    /// <param name="reconstructor">The reconstructor to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplReconstructorRelease")]
    public static partial void ReconstructorRelease(ReconstructorHandle* reconstructor);

    /// <summary>
    /// Reconstructs one or more impulse responses as a single batch of work.
    /// </summary>
    /// <param name="reconstructor">The reconstructor to use.</param>
    /// <param name="numInputs">The number of impulse responses to reconstruct.</param>
    /// <param name="inputs">
    /// Pointer to <c>numInputs</c> input structures, each describing a single energy field from which to
    /// reconstruct a corresponding impulse response.
    /// </param>
    /// <param name="sharedInputs">
    /// Pointer to a single shared input structure, describing common inputs used across all impulse response
    /// reconstruction operations in this call.
    /// </param>
    /// <param name="outputs">
    /// Pointer to <c>numInputs</c> output structures, each describing a single impulse response object into
    /// which to write the corresponding reconstructed impulse response.
    /// </param>
    [LibraryImport(Library, EntryPoint = "iplReconstructorReconstruct")]
    public static partial void ReconstructorReconstruct(
        ReconstructorHandle reconstructor,
        int numInputs,
        ReconstructorInputs* inputs,
        ReconstructorSharedInputs* sharedInputs,
        ReconstructorOutputs* outputs);
}
