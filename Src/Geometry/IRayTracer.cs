namespace SteamAudio.Geometry;
/// <summary>Managed custom geometry callbacks. Implementations may be called concurrently by native simulation workers.</summary>
public interface IRayTracer
{
    /// <summary>Finds the nearest hit in the requested distance interval.</summary>
    /// <param name="ray">World-space ray.</param><param name="minimum">Minimum hit distance.</param><param name="maximum">Maximum hit distance.</param><returns>A hit, or null if the ray escapes.</returns>
    RayHit? ClosestHit(in Interop.Ray ray, float minimum, float maximum);
    /// <summary>Tests whether any surface blocks the ray interval.</summary>
    /// <param name="ray">World-space ray.</param><param name="minimum">Minimum distance.</param><param name="maximum">Maximum distance.</param><returns>True if blocked.</returns>
    bool AnyHit(in Interop.Ray ray, float minimum, float maximum);
}
/// <summary>A copied ray result. MaterialIndex selects the stable native material table supplied to the custom scene.</summary>
/// <param name="Distance">Nonnegative distance along the ray.</param><param name="Normal">Surface normal.</param><param name="MaterialIndex">Index into the scene's acoustic materials.</param><param name="TriangleIndex">Optional geometry identifier.</param><param name="ObjectIndex">Optional object identifier.</param>
public readonly record struct RayHit(float Distance, Interop.Vector3 Normal, int MaterialIndex = 0, int TriangleIndex = 0, int ObjectIndex = 0);
