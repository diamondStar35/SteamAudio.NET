using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates an instanced mesh. An instanced mesh takes one scene and positions it within another scene. This
    /// is useful if you have the same object, like a pillar, that you want to instantiate multiple times within
    /// the same scene. A scene can be instantiated multiple times within another scene, without incurring any
    /// significant memory overhead. The instanced mesh can be moved, rotated, and scaled freely at any time,
    /// providing an easy way to implement dynamic objects whose motion can be described purely in terms of
    /// rigid-body transformations.
    /// </summary>
    /// <param name="scene">The scene in which the instanced mesh should be created.</param>
    /// <param name="settings">The settings used to create the instanced mesh.</param>
    /// <param name="instancedMesh">The created instanced mesh.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplInstancedMeshCreate")]
    public static partial Error InstancedMeshCreate(
        SceneHandle scene,
        InstancedMeshSettings* settings,
        InstancedMeshHandle* instancedMesh);

    /// <summary>
    /// Retains an additional reference to a instanced mesh.
    /// </summary>
    /// <param name="instancedMesh">The instanced mesh to retain a reference to.</param>
    /// <returns>The additional reference to the instanced mesh.</returns>
    [LibraryImport(Library, EntryPoint = "iplInstancedMeshRetain")]
    public static partial InstancedMeshHandle InstancedMeshRetain(InstancedMeshHandle instancedMesh);

    /// <summary>
    /// Releases a reference to a instanced mesh.
    /// </summary>
    /// <param name="instancedMesh">The instanced mesh to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplInstancedMeshRelease")]
    public static partial void InstancedMeshRelease(InstancedMeshHandle* instancedMesh);

    /// <summary>
    /// Adds an instanced mesh to a scene. This function should be called after <c>iplInstancedMeshCreate</c>,
    /// or at any point after <c>iplInstancedMeshRemove</c>, for the instanced mesh to start affecting sound
    /// propagation. After calling this function, <c>iplSceneCommit</c> must be called for the changes to take
    /// effect.
    /// </summary>
    /// <param name="instancedMesh">The instanced mesh to add.</param>
    /// <param name="scene">
    /// The scene to which to add the instanced mesh. This must be the scene which was passed when calling
    /// <c>iplInstancedMeshCreate.</c>
    /// </param>
    [LibraryImport(Library, EntryPoint = "iplInstancedMeshAdd")]
    public static partial void InstancedMeshAdd(InstancedMeshHandle instancedMesh, SceneHandle scene);

    /// <summary>
    /// Removes an instanced mesh from a scene. After this function is called, the instanced mesh will stop
    /// affecting sound propagation, until it is added back using <c>iplInstancedMeshAdd.</c> After calling this
    /// function, <c>iplSceneCommit</c> must be called for the changes to take effect.
    /// </summary>
    /// <param name="instancedMesh">The instanced mesh to remove.</param>
    /// <param name="scene">
    /// The scene from which to remove the instanced mesh. This must be the scene which was passed when calling
    /// <c>iplInstancedMeshCreate.</c>
    /// </param>
    [LibraryImport(Library, EntryPoint = "iplInstancedMeshRemove")]
    public static partial void InstancedMeshRemove(InstancedMeshHandle instancedMesh, SceneHandle scene);

    /// <summary>
    /// Updates the local-to-world transform of an instanced mesh within its parent scene. This function allows
    /// the instanced mesh to be moved, rotated, and scaled dynamically. After calling this function,
    /// <c>iplSceneCommit</c> must be called for the changes to take effect.
    /// </summary>
    /// <param name="instancedMesh">The instanced mesh whose transform is to be updated.</param>
    /// <param name="scene">The parent scene that contains the instanced mesh.</param>
    /// <param name="transform">The new 4x4 local-to-world transform matrix.</param>
    [LibraryImport(Library, EntryPoint = "iplInstancedMeshUpdateTransform")]
    public static partial void InstancedMeshUpdateTransform(
        InstancedMeshHandle instancedMesh,
        SceneHandle scene,
        Matrix4x4 transform);
}
