namespace SteamAudio.Geometry;

/// <summary>
/// Owns a native InstancedMesh reference.
/// </summary>
public sealed unsafe class InstancedMesh : Resource
{
    internal readonly nint ParentAddress;

    /// <summary>
    /// Creates this resource and retains its parent and explicitly supplied native dependencies.
    /// </summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">
    /// SDK settings. Pointer members must remain valid for their documented native lifetime.
    /// </param>
    /// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public InstancedMesh(
        SteamAudio.Geometry.Scene parent,
        Interop.InstancedMeshSettings settings,
        params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
    }

    /// <summary>
    /// Adds the instance to its retained parent scene; commit afterward.
    /// </summary>
    public void Add()
    {
        using var instance = Acquire();
        Interop.Api.InstancedMeshAdd(new(instance.Value), new(ParentAddress));
    }

    /// <summary>
    /// Removes the instance from its retained parent scene; commit afterward.
    /// </summary>
    public void Remove()
    {
        using var instance = Acquire();
        Interop.Api.InstancedMeshRemove(new(instance.Value), new(ParentAddress));
    }

    /// <summary>
    /// Updates the row-major local-to-world transform; commit afterward.
    /// </summary>
    /// <param name="transform">Four-by-four SDK transform matrix.</param>
    public void UpdateTransform(Interop.Matrix4x4 transform)
    {
        using var instance = Acquire();
        Interop.Api.InstancedMeshUpdateTransform(new(instance.Value), new(ParentAddress), transform);
    }

    private static nint Create(SteamAudio.Geometry.Scene parent, Interop.InstancedMeshSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.InstancedMeshHandle value;
        SteamAudioException.Check(
            Interop.Api.InstancedMeshCreate(new Interop.SceneHandle(lease.Value), &settings, &value),
            "Create InstancedMesh");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.InstancedMeshHandle(value);
        Interop.Api.InstancedMeshRelease(&handle);
    }
}
