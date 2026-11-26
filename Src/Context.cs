namespace SteamAudio;

/// <summary>
/// Initializes Steam Audio and owns the native state shared by its resources. Child resources retain the
/// context until they are released.
/// </summary>
public sealed unsafe class Context : Resource
{
    /// <summary>
    /// Creates a context for Steam Audio 4.8.1. Validation adds native diagnostics and is disabled by default.
    /// </summary>
    /// <param name="validation">Enable additional native API validation.</param>
    public Context(bool validation = false)
        : this(new Interop.ContextSettings
        {
            Version = Interop.Api.Version,
            SimdLevel = Interop.SimdLevel.Avx2,
            Flags = validation ? Interop.ContextFlags.Validation : 0
        })
    {
    }

    /// <summary>
    /// Creates a context with explicit native settings, including optional static function-pointer callbacks.
    /// </summary>
    /// <param name="settings">
    /// Settings copied for creation. Callback code must remain valid until all dependent resources end.
    /// </param>
    public Context(Interop.ContextSettings settings)
        : base(Create(settings), Release)
    {
    }

    private static nint Create(Interop.ContextSettings settings)
    {
        Interop.ContextHandle result;
        SteamAudioException.Check(Interop.Api.ContextCreate(&settings, &result), "Create context");
        return result.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.ContextHandle(value);
        Interop.Api.ContextRelease(&handle);
    }
}
