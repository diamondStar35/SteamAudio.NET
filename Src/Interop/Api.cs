using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>
/// Exact Steam Audio C ABI, using the standard platform calling convention.
/// </summary>
public static unsafe partial class Api
{
    /// <summary>
    /// Logical import name, also used by NativeAOT DirectPInvoke.
    /// </summary>
    public const string Library = "phonon";

    /// <summary>
    /// Steam Audio API version passed when creating a context.
    /// </summary>
    public const uint Version = 0x040801;

    /// <summary>
    /// Frequency bands in the supported standard SDK ABI.
    /// </summary>
    public const int NumBands = 3;
}
