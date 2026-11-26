using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a context object. A context must be created before creating any other API objects.
    /// </summary>
    /// <param name="settings">
    /// Pointer to the `IPLContextSettings` struct that specifies context creation parameters.
    /// </param>
    /// <param name="context">Handle to the created context object.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplContextCreate")]
    public static partial Error ContextCreate(ContextSettings* settings, ContextHandle* context);

    /// <summary>
    /// Retains an additional reference to a context. The context will not be destroyed until all references are
    /// released.
    /// </summary>
    /// <param name="context">The context to retain a reference to.</param>
    /// <returns>The additional reference to the context.</returns>
    [LibraryImport(Library, EntryPoint = "iplContextRetain")]
    public static partial ContextHandle ContextRetain(ContextHandle context);

    /// <summary>
    /// Releases a reference to a context. The context will not be destroyed until all references are released.
    /// </summary>
    /// <param name="context">The context to release.</param>
    [LibraryImport(Library, EntryPoint = "iplContextRelease")]
    public static partial void ContextRelease(ContextHandle* context);
}
