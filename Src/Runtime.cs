using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SteamAudio;

/// <summary>
/// Shared-library search configuration. NativeAOT direct static imports bypass the resolver.
/// </summary>
public static class Runtime
{
    private static readonly object Gate = new();
    private static string? _directory;
    private static nint _library;

    /// <summary>
    /// Sets an explicit shared-library directory before first use. Default is the application's lib directory.
    /// </summary>
    /// <param name="directory">
    /// Absolute or relative directory containing the matching architecture's native library.
    /// </param>
    public static void ConfigureDirectory(string directory)
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);
        lock (Gate)
        {
            if (_library != 0)
            {
                throw new InvalidOperationException("Steam Audio is already loaded.");
            }

            _directory = Path.GetFullPath(directory);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage",
        "CA2255",
        Justification = "The native resolver must be installed before the first native call.")]
    [ModuleInitializer]
    internal static void Initialize() => NativeLibrary.SetDllImportResolver(typeof(Runtime).Assembly, Resolve);
    private static nint Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (name != Interop.Api.Library)
        {
            return 0;
        }

        lock (Gate)
        {
            if (_library != 0)
            {
                return _library;
            }

            if (OperatingSystem.IsIOS())
            {
                return NativeLibrary.GetMainProgramHandle();
            }

            var file = OperatingSystem.IsWindows() ? "phonon.dll" : OperatingSystem.IsMacOS() ? "libphonon.dylib" : "libphonon.so";
            var path = Path.Combine(_directory ?? Path.Combine(AppContext.BaseDirectory, "lib"), file);
            if (NativeLibrary.TryLoad(path, out _library))
            {
                return _library;
            }

            if (_directory != null)
            {
                throw new DllNotFoundException($"Steam Audio 4.8.1 for {RuntimeInformation.ProcessArchitecture} was not found at {path}.");
            }

            // Return control to the runtime for standard NuGet native probing and Android package resolution.
            return 0;
        }
    }
}
