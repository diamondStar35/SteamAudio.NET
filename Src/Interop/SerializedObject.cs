using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a serialized object.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="settings">The settings to use when creating the serialized object.</param>
    /// <param name="serializedObject">The created serialized object.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplSerializedObjectCreate")]
    public static partial Error SerializedObjectCreate(
        ContextHandle context,
        SerializedObjectSettings* settings,
        SerializedObjectHandle* serializedObject);

    /// <summary>
    /// Retains an additional reference to a serialized object.
    /// </summary>
    /// <param name="serializedObject">The serialized object to retain a reference to.</param>
    /// <returns>The additional reference to the serialized object.</returns>
    [LibraryImport(Library, EntryPoint = "iplSerializedObjectRetain")]
    public static partial SerializedObjectHandle SerializedObjectRetain(SerializedObjectHandle serializedObject);

    /// <summary>
    /// Releases a reference to a serialized object.
    /// </summary>
    /// <param name="serializedObject">The serialized object to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplSerializedObjectRelease")]
    public static partial void SerializedObjectRelease(SerializedObjectHandle* serializedObject);

    /// <summary>
    /// SerializedObjectGetSize
    /// </summary>
    /// <returns>The size in bytes of the serialized data contained in a serialized object.</returns>
    /// <param name="serializedObject">The serialized object.</param>
    [LibraryImport(Library, EntryPoint = "iplSerializedObjectGetSize")]
    public static partial nuint SerializedObjectGetSize(SerializedObjectHandle serializedObject);

    /// <summary>
    /// SerializedObjectGetData
    /// </summary>
    /// <returns>A pointer to a byte array of serialized data contained in a serialized object.</returns>
    /// <param name="serializedObject">The serialized object.</param>
    [LibraryImport(Library, EntryPoint = "iplSerializedObjectGetData")]
    public static partial byte* SerializedObjectGetData(SerializedObjectHandle serializedObject);
}
