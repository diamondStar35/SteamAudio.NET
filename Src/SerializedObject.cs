using System.Runtime.InteropServices;
namespace SteamAudio;
/// <summary>Owns Steam Audio's serialization storage. Exported arrays are independent of native lifetime.</summary>
public sealed unsafe class SerializedObject : Resource
{
    /// <summary>Creates empty serialization storage or copies existing serialized data.</summary>
    /// <param name="context">Owning context.</param><param name="data">Optional complete serialized bytes.</param>
    public SerializedObject(Context context, ReadOnlySpan<byte> data = default) : this(context, Create(context, data)) { }
    private sealed record Creation(nint Handle, nint Bytes);
    private SerializedObject(Context context, Creation creation) : base(creation.Handle, value =>
    { try { Release(value); } finally { NativeMemory.Free((void*)creation.Bytes); } }, context) { }
    private static Creation Create(Context context, ReadOnlySpan<byte> data)
    {
        using var c = context.Acquire();
        var bytes = data.IsEmpty ? null : (byte*)NativeMemory.Alloc((nuint)data.Length);
        if (!data.IsEmpty && bytes == null) throw new OutOfMemoryException();
        try
        {
            data.CopyTo(new Span<byte>(bytes, data.Length));
            var settings = new Interop.SerializedObjectSettings { Data = bytes, Size = (nuint)data.Length };
            Interop.SerializedObjectHandle result;
            SteamAudioException.Check(Interop.Api.SerializedObjectCreate(new(c.Value), &settings, &result), "Create serialized object");
            return new(result.Value, (nint)bytes);
        }
        catch { NativeMemory.Free(bytes); throw; }
    }

    private static void Release(nint value) { var handle = new Interop.SerializedObjectHandle(value); Interop.Api.SerializedObjectRelease(&handle); }
    /// <summary>Copies the complete serialized object into managed storage.</summary>
    /// <returns>An owned byte array.</returns>
    public byte[] ToArray() { using var value = Acquire(); return new ReadOnlySpan<byte>(Interop.Api.SerializedObjectGetData(new(value.Value)), checked((int)Interop.Api.SerializedObjectGetSize(new(value.Value)))).ToArray(); }
}
