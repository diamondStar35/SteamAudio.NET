using System.Text;
namespace SteamAudio;
/// <summary>Immutable HRTF database shared by compatible binaural effects. Creation performs loading outside audio processing.</summary>
public sealed unsafe class Hrtf : Resource
{
    /// <summary>The fixed audio format for which this database was created.</summary>
    public AudioSettings Settings { get; }
    /// <summary>Creates Steam Audio's built-in HRTF database.</summary>
    /// <param name="context">Owning SDK context.</param><param name="settings">Processing format.</param><param name="volume">Nonnegative HRTF gain multiplier.</param>
    public Hrtf(Context context, AudioSettings settings, float volume = 1) : this(context, settings, new Interop.HrtfSettings { Type = Interop.HrtfType.Default, Volume = volume }) { }
    private Hrtf(Context context, AudioSettings audio, Interop.HrtfSettings settings) : base(Create(context, audio, settings), Release, context) => Settings = audio;
    /// <summary>Loads a SOFA database from an UTF-8 filesystem path during construction.</summary>
    /// <param name="context">Owning context.</param><param name="audio">Processing format.</param><param name="path">Existing SOFA file.</param>
    /// <returns>An independently owned HRTF database.</returns>
    public static Hrtf LoadSofa(Context context, AudioSettings audio, string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        var bytes = Encoding.UTF8.GetBytes(Path.GetFullPath(path) + "\0");
        fixed (byte* pointer = bytes) return new(context, audio, new Interop.HrtfSettings { Type = Interop.HrtfType.Sofa, SofaFileName = pointer, Volume = 1 });
    }
    /// <summary>Loads a SOFA database from bytes. The SDK consumes the bytes synchronously; no span is retained.</summary>
    /// <param name="context">Owning context.</param><param name="audio">Processing format.</param><param name="data">Complete SOFA contents.</param>
    /// <returns>An independently owned HRTF database.</returns>
    public static Hrtf LoadSofa(Context context, AudioSettings audio, ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) throw new ArgumentException("SOFA data is empty.", nameof(data));
        fixed (byte* pointer = data) return new(context, audio, new Interop.HrtfSettings { Type = Interop.HrtfType.Sofa, SofaData = pointer, SofaDataSize = data.Length, Volume = 1 });
    }
    private static nint Create(Context context, AudioSettings audio, Interop.HrtfSettings settings)
    {
        if (!float.IsFinite(settings.Volume) || settings.Volume < 0) throw new ArgumentOutOfRangeException(nameof(settings));
        using var lease = context.Acquire(); var format = audio.Native; Interop.HrtfHandle handle;
        SteamAudioException.Check(Interop.Api.HrtfCreate(new(lease.Value), &format, &settings, &handle), "Create HRTF"); return handle.Value;
    }
    private static void Release(nint value) { var handle = new Interop.HrtfHandle(value); Interop.Api.HrtfRelease(&handle); }
}
