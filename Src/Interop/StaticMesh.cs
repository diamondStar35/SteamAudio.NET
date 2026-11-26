using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a static mesh. A static mesh represents a triangle mesh that does not change after it is
    /// created. A static mesh also contains an array of acoustic material properties, and a mapping between
    /// each of its triangles and their acoustic material properties. Static mesh objects should be used for
    /// scene geometry that is guaranteed to never change, such as rooms, buildings, or triangulated terrain. A
    /// scene may contain multiple static meshes, although typically one is sufficient.
    /// </summary>
    /// <param name="scene">The scene in which the static mesh should be created.</param>
    /// <param name="settings">The settings to use when creating the static mesh.</param>
    /// <param name="staticMesh">The created static mesh.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplStaticMeshCreate")]
    public static partial Error StaticMeshCreate(
        SceneHandle scene,
        StaticMeshSettings* settings,
        StaticMeshHandle* staticMesh);

    /// <summary>
    /// Retains an additional reference to a static mesh.
    /// </summary>
    /// <param name="staticMesh">The static mesh to retain a reference to.</param>
    /// <returns>The additional reference to the static mesh.</returns>
    [LibraryImport(Library, EntryPoint = "iplStaticMeshRetain")]
    public static partial StaticMeshHandle StaticMeshRetain(StaticMeshHandle staticMesh);

    /// <summary>
    /// Releases a reference to a static mesh.
    /// </summary>
    /// <param name="staticMesh">The static mesh to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplStaticMeshRelease")]
    public static partial void StaticMeshRelease(StaticMeshHandle* staticMesh);

    /// <summary>
    /// Loads a static mesh from a serialized object. Typically, the serialized object will be created from a
    /// byte array loaded from disk or over the network.
    /// </summary>
    /// <param name="scene">The scene in which the static mesh should be created.</param>
    /// <param name="serializedObject">The serialized object from which to load the scene.</param>
    /// <param name="progressCallback">
    /// Callback that reports the percentage of this function's work that has been completed. May be
    /// <c>NULL.</c>
    /// </param>
    /// <param name="progressCallbackUserData">
    /// Pointer to arbitrary data that will be passed to the progress callback. May be <c>NULL.</c>
    /// </param>
    /// <param name="staticMesh">The created static mesh.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplStaticMeshLoad")]
    public static partial Error StaticMeshLoad(
        SceneHandle scene,
        SerializedObjectHandle serializedObject,
        delegate* unmanaged<float, void*, void> progressCallback,
        void* progressCallbackUserData,
        StaticMeshHandle* staticMesh);

    /// <summary>
    /// Saves a static mesh to a serialized object. Typically, the serialized object will then be saved to disk.
    /// This function can only be called on a static mesh that is part of a scene created with
    /// <c>IPL_SCENETYPE_DEFAULT.</c>
    /// </summary>
    /// <param name="staticMesh">The static mesh to save.</param>
    /// <param name="serializedObject">The serialized object into which to save the static mesh.</param>
    [LibraryImport(Library, EntryPoint = "iplStaticMeshSave")]
    public static partial void StaticMeshSave(StaticMeshHandle staticMesh, SerializedObjectHandle serializedObject);

    /// <summary>
    /// Adds a static mesh to a scene. This function should be called after <c>iplStaticMeshCreate</c>, or at
    /// any point after <c>iplStaticMeshRemove</c>, for the static mesh to start affecting sound propagation.
    /// After calling this function, <c>iplSceneCommit</c> must be called for the changes to take effect.
    /// </summary>
    /// <param name="staticMesh">The static mesh to add.</param>
    /// <param name="scene">
    /// The scene to which to add the static mesh. This must be the scene which was passed when calling
    /// <c>iplStaticMeshCreate.</c>
    /// </param>
    [LibraryImport(Library, EntryPoint = "iplStaticMeshAdd")]
    public static partial void StaticMeshAdd(StaticMeshHandle staticMesh, SceneHandle scene);

    /// <summary>
    /// Removes a static mesh from a scene. After this function is called, the static mesh will stop affecting
    /// sound propagation, until it is added back using <c>iplStaticMeshAdd.</c> After calling this function,
    /// <c>iplSceneCommit</c> must be called for the changes to take effect.
    /// </summary>
    /// <param name="staticMesh">The static mesh to remove.</param>
    /// <param name="scene">
    /// The scene from which to remove the static mesh. This must be the scene which was passed when calling
    /// <c>iplStaticMeshCreate.</c>
    /// </param>
    [LibraryImport(Library, EntryPoint = "iplStaticMeshRemove")]
    public static partial void StaticMeshRemove(StaticMeshHandle staticMesh, SceneHandle scene);

    /// <summary>
    /// Loads the material data of specified object in the iplStaticMesh.
    /// </summary>
    /// <param name="staticMesh">The static mesh to update.</param>
    /// <param name="scene">
    /// The scene from which to update the static mesh. This must be the scene which was passed when calling
    /// <c>iplStaticMeshCreate.</c>
    /// </param>
    /// <param name="newMaterial">The material data of specified object.</param>
    /// <param name="index">
    /// The index of specified object. It means the object's index in the order it was exported.
    /// </param>
    [LibraryImport(Library, EntryPoint = "iplStaticMeshSetMaterial")]
    public static partial void StaticMeshSetMaterial(
        StaticMeshHandle staticMesh,
        SceneHandle scene,
        Material* newMaterial,
        int index);
}
