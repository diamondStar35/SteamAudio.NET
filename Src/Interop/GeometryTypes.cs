// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Settings used to create a Radeon Rays device.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct RadeonRaysDeviceSettings
{
    /// <summary>reserved</summary>
    public byte Reserved;
}

/// <summary>A triangle in 3D space. Triangles are specified by their three vertices, which are in turn specified using indices into a vertex array. Steam Audio uses a counter-clockwise winding order. This means that when looking at the triangle such that the normal is pointing towards you, the vertices are specified in counter-clockwise order. Each triangle must be specified using three vertices; triangle strip or fan representations are not supported.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct Triangle
{
    /// <summary>Indices of the three vertices of this triangle.</summary>
    public fixed int Indices[3];
}

/// <summary>The acoustic properties of a surface. You can specify the acoustic material properties of each triangle, although typically many triangles will share a common material. The acoustic material properties are specified for three frequency bands with center frequencies of 400 Hz, 2.5 KHz, and 15 KHz. Below are the acoustic material properties for a few standard materials. ```cpp {"generic",{0.10f,0.20f,0.30f,0.05f,0.100f,0.050f,0.030f}} {"brick",{0.03f,0.04f,0.07f,0.05f,0.015f,0.015f,0.015f}} {"concrete",{0.05f,0.07f,0.08f,0.05f,0.015f,0.002f,0.001f}} {"ceramic",{0.01f,0.02f,0.02f,0.05f,0.060f,0.044f,0.011f}} {"gravel",{0.60f,0.70f,0.80f,0.05f,0.031f,0.012f,0.008f}}, {"carpet",{0.24f,0.69f,0.73f,0.05f,0.020f,0.005f,0.003f}} {"glass",{0.06f,0.03f,0.02f,0.05f,0.060f,0.044f,0.011f}} {"plaster",{0.12f,0.06f,0.04f,0.05f,0.056f,0.056f,0.004f}} {"wood",{0.11f,0.07f,0.06f,0.05f,0.070f,0.014f,0.005f}} {"metal",{0.20f,0.07f,0.06f,0.05f,0.200f,0.025f,0.010f}} {"rock",{0.13f,0.20f,0.24f,0.05f,0.015f,0.002f,0.001f}} ```</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct Material
{
    /// <summary>Fraction of sound energy absorbed at low, middle, high frequencies. Between 0.0 and 1.0.</summary>
    public fixed float Absorption[3];

    /// <summary>Fraction of sound energy scattered in a random direction on reflection. Between 0.0 (pure specular) and 1.0 (pure diffuse).</summary>
    public float Scattering;

    /// <summary>Fraction of sound energy transmitted through at low, middle, high frequencies. Between 0.0 and 1.0. Only used for direct occlusion calculations.</summary>
    public fixed float Transmission[3];
}

/// <summary>A ray in 3D space.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct Ray
{
    /// <summary>Origin of the ray.</summary>
    public Vector3 Origin;

    /// <summary>Unit vector direction of the ray.</summary>
    public Vector3 Direction;
}

/// <summary>Information about a ray's intersection with 3D geometry. This information should be provided by ray tracer callbacks when using <c>IPL_SCENETYPE_CUSTOM.</c> Not all fields are required.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct Hit
{
    /// <summary>Distance along the ray from origin to hit point. Set to <c>INFINITY</c> if nothing was hit.</summary>
    public float Distance;

    /// <summary>(Optional) Index of the primitive hit by the ray. <c>-1</c> if not provided.</summary>
    public int TriangleIndex;

    /// <summary>(Optional) Index of the scene object hit by the ray. <c>-1</c> if not provided.</summary>
    public int ObjectIndex;

    /// <summary>(Optional) Index of the material associated with the primitive hit by the ray. <c>-1</c> if not provided.</summary>
    public int MaterialIndex;

    /// <summary>Unit length surface normal at the hit point. Ignored if nothing was hit.</summary>
    public Vector3 Normal;

    /// <summary>Pointer to the material at the hit point. Ignored if nothing was hit.</summary>
    public Material* Material;
}

/// <summary>Settings used to create a scene.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SceneSettings
{
    /// <summary>Type of scene to create.</summary>
    public SceneType Type;

    /// <summary>Callback for finding the closest hit along a ray. Only for <c>IPL_SCENETYPE_CUSTOM.</c></summary>
    public delegate* unmanaged<Ray*, float, float, Hit*, void*, void> ClosestHitCallback;

    /// <summary>Callback for finding whether a ray hits anything. Only for <c>IPL_SCENETYPE_CUSTOM.</c></summary>
    public delegate* unmanaged<Ray*, float, float, byte*, void*, void> AnyHitCallback;

    /// <summary>Callback for finding the closest hit along a batch of rays. Only for <c>IPL_SCENETYPE_CUSTOM.</c></summary>
    public delegate* unmanaged<int, Ray*, float*, float*, Hit*, void*, void> BatchedClosestHitCallback;

    /// <summary>Callback for finding whether a batch of rays hits anything. Only for <c>IPL_SCENETYPE_CUSTOM.</c></summary>
    public delegate* unmanaged<int, Ray*, float*, float*, byte*, void*, void> BatchedAnyHitCallback;

    /// <summary>Arbitrary user-provided data for use by ray tracing callbacks. Only for <c>IPL_SCENETYPE_CUSTOM.</c></summary>
    public void* UserData;

    /// <summary>Handle to an Embree device. Only for <c>IPL_SCENETYPE_EMBREE.</c></summary>
    public EmbreeDeviceHandle EmbreeDevice;

    /// <summary>Handle to a Radeon Rays device. Only for <c>IPL_SCENETYPE_RADEONRAYS.</c></summary>
    public RadeonRaysDeviceHandle RadeonRaysDevice;
}

/// <summary>Settings used to create a static mesh.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct StaticMeshSettings
{
    /// <summary>Number of vertices.</summary>
    public int NumVertices;

    /// <summary>Number of triangles.</summary>
    public int NumTriangles;

    /// <summary>Number of materials.</summary>
    public int NumMaterials;

    /// <summary>Array containing vertices.</summary>
    public Vector3* Vertices;

    /// <summary>Array containing (indexed) triangles.</summary>
    public Triangle* Triangles;

    /// <summary>Array containing, for each triangle, the index of the associated material.</summary>
    public int* MaterialIndices;

    /// <summary>Array of materials.</summary>
    public Material* Materials;
}

/// <summary>Settings used to create an instanced mesh.</summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct InstancedMeshSettings
{
    /// <summary>Handle to the scene to be instantiated.</summary>
    public SceneHandle SubScene;

    /// <summary>Local-to-world transform that places the instance within the parent scene.</summary>
    public Matrix4x4 Transform;
}
