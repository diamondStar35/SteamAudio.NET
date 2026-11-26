namespace SteamAudio.Geometry;
/// <summary>Immutable copied mesh geometry with updateable indexed acoustic materials. Add explicitly, then commit the scene.</summary>
public sealed unsafe class StaticMesh : Resource
{
    private readonly nint _scene;
    private readonly int _materials;
    /// <summary>Copies vertices, triangles and acoustic materials into native mesh storage.</summary>
    /// <param name="scene">Parent scene.</param><param name="vertices">Positions in scene coordinates.</param><param name="triangles">Counter-clockwise vertex indices.</param><param name="materialIndices">One material index per triangle.</param><param name="materials">Acoustic materials referenced by triangles.</param>
    public StaticMesh(Scene scene, ReadOnlySpan<Interop.Vector3> vertices, ReadOnlySpan<Interop.Triangle> triangles, ReadOnlySpan<int> materialIndices, ReadOnlySpan<Interop.Material> materials)
        : base(Create(scene, vertices, triangles, materialIndices, materials), Release, scene)
    { _scene = scene.DangerousGetHandle(); _materials = materials.Length; }
    private static nint Create(Scene scene, ReadOnlySpan<Interop.Vector3> vertices, ReadOnlySpan<Interop.Triangle> triangles, ReadOnlySpan<int> indices, ReadOnlySpan<Interop.Material> materials)
    {
        if (vertices.IsEmpty || triangles.IsEmpty || materials.IsEmpty || indices.Length != triangles.Length) throw new ArgumentException("Mesh arrays must be nonempty and provide one material index per triangle.");
        foreach (var triangle in triangles) for (var i = 0; i < 3; i++) if ((uint)triangle.Indices[i] >= (uint)vertices.Length) throw new ArgumentException("Triangle index outside vertices.");
        foreach (var index in indices) if ((uint)index >= (uint)materials.Length) throw new ArgumentException("Material index outside materials.");
        using var lease = scene.Acquire();
        fixed (Interop.Vector3* v = vertices) fixed (Interop.Triangle* t = triangles) fixed (int* i = indices) fixed (Interop.Material* m = materials)
        {
            var settings = new Interop.StaticMeshSettings { NumVertices = vertices.Length, NumTriangles = triangles.Length, NumMaterials = materials.Length, Vertices = v, Triangles = t, MaterialIndices = i, Materials = m };
            Interop.StaticMeshHandle result; SteamAudioException.Check(Interop.Api.StaticMeshCreate(new(lease.Value), &settings, &result), "Create static mesh"); return result.Value;
        }
    }
    private static void Release(nint value) { var handle = new Interop.StaticMeshHandle(value); Interop.Api.StaticMeshRelease(&handle); }
    /// <summary>Adds the mesh to its parent scene. Commit before simulation.</summary>
    public void Add() { using var lease = Acquire(); Interop.Api.StaticMeshAdd(new(lease.Value), new(_scene)); }
    /// <summary>Removes the mesh from its parent scene. Commit before simulation.</summary>
    public void Remove() { using var lease = Acquire(); Interop.Api.StaticMeshRemove(new(lease.Value), new(_scene)); }
    /// <summary>Updates an acoustic material by index. Commit the scene before the next simulation.</summary>
    /// <param name="index">Index into the original materials array.</param><param name="material">Replacement acoustic properties.</param>
    public void SetMaterial(int index, Interop.Material material)
    {
        if ((uint)index >= (uint)_materials) throw new ArgumentOutOfRangeException(nameof(index));
        using var lease = Acquire(); Interop.Api.StaticMeshSetMaterial(new(lease.Value), new(_scene), &material, index);
    }
}
