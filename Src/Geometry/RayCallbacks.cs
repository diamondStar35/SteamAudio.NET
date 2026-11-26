using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace SteamAudio.Geometry;

internal sealed unsafe class RayCallbacks : IDisposable
{
    [ThreadStatic]
    private static int _depth;
    private readonly IRayTracer _tracer;
    private readonly Interop.Material* _materials;
    private readonly int _count;
    private GCHandle _root;
    private ExceptionDispatchInfo? _error;
    internal RayCallbacks(IRayTracer tracer, ReadOnlySpan<Interop.Material> materials)
    {
        ArgumentNullException.ThrowIfNull(tracer);
        if (materials.IsEmpty)
        {
            throw new ArgumentException("A custom scene needs acoustic materials.");
        }

        _tracer = tracer;
        _count = materials.Length;
        _materials = (Interop.Material*)NativeMemory.Alloc(checked((nuint)(sizeof(Interop.Material) * materials.Length)));
        if (_materials == null)
        {
            throw new OutOfMemoryException();
        }

        try
        {
            materials.CopyTo(new Span<Interop.Material>(_materials, materials.Length));
            _root = GCHandle.Alloc(this);
        }
        catch
        {
            NativeMemory.Free(_materials);
            throw;
        }
    }

    internal Interop.SceneSettings Settings => new()
    {
        Type = Interop.SceneType.Custom,
        ClosestHitCallback = &Closest,
        AnyHitCallback = &Any,
        BatchedClosestHitCallback = &ClosestBatch,
        BatchedAnyHitCallback = &AnyBatch,
        UserData = (void*)GCHandle.ToIntPtr(_root)
    };

    internal static void EnterCallback() => _depth++;
    internal static void ExitCallback() => _depth--;
    internal static void ThrowIfInside()
    {
        if (_depth != 0)
        {
            throw new InvalidOperationException("Do not call SDK controls or dispose resources from native ray callbacks.");
        }
    }

    internal void ThrowIfFailed() => Volatile.Read(ref _error)?.Throw();
    private void Fail(Exception error) => Interlocked.CompareExchange(ref _error, ExceptionDispatchInfo.Capture(error), null);
    private static RayCallbacks From(void* data) => (RayCallbacks)GCHandle.FromIntPtr((nint)data).Target!;
    private void Trace(Interop.Ray* ray, float min, float max, Interop.Hit* hit)
    {
        *hit = new()
        {
            Distance = float.PositiveInfinity,
            MaterialIndex = -1,
            ObjectIndex = -1,
            TriangleIndex = -1
        };
        _depth++;
        try
        {
            var value = _tracer.ClosestHit(in *ray, min, max);
            if (value is not { } result)
            {
                return;
            }

            if (!float.IsFinite(result.Distance) || result.Distance < min || result.Distance > max || (uint)result.MaterialIndex >= (uint)_count)
            {
                throw new InvalidOperationException("Custom ray callback returned an invalid hit.");
            }

            *hit = new()
            {
                Distance = result.Distance,
                Normal = result.Normal,
                MaterialIndex = result.MaterialIndex,
                TriangleIndex = result.TriangleIndex,
                ObjectIndex = result.ObjectIndex,
                Material = _materials + result.MaterialIndex
            };
        }
        catch (Exception error)
        {
            Fail(error);
        }
        finally
        {
            _depth--;
        }
    }

    private void Occlude(Interop.Ray* ray, float min, float max, byte* output)
    {
        *output = 0;
        _depth++;
        try
        {
            *output = _tracer.AnyHit(in *ray, min, max) ? (byte)1 : (byte)0;
        }
        catch (Exception error)
        {
            Fail(error);
        }
        finally
        {
            _depth--;
        }
    }

    [UnmanagedCallersOnly]
    private static void Closest(Interop.Ray* ray, float min, float max, Interop.Hit* hit, void* data) => From(data).Trace(ray, min, max, hit);
    [UnmanagedCallersOnly]
    private static void Any(Interop.Ray* ray, float min, float max, byte* output, void* data) => From(data).Occlude(ray, min, max, output);
    [UnmanagedCallersOnly]
    private static void ClosestBatch(
        int count,
        Interop.Ray* rays,
        float* mins,
        float* maxs,
        Interop.Hit* hits,
        void* data)
    {
        var state = From(data);
        for (var i = 0; i < count; i++)
        {
            state.Trace(rays + i, mins[i], maxs[i], hits + i);
        }
    }

    [UnmanagedCallersOnly]
    private static void AnyBatch(int count, Interop.Ray* rays, float* mins, float* maxs, byte* outputs, void* data)
    {
        var state = From(data);
        for (var i = 0; i < count; i++)
        {
            state.Occlude(rays + i, mins[i], maxs[i], outputs + i);
        }
    }

    public void Dispose()
    {
        if (_root.IsAllocated)
        {
            _root.Free();
        }

        NativeMemory.Free(_materials);
    }
}
