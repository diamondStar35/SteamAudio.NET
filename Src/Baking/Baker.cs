using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace SteamAudio.Baking;

/// <summary>
/// Synchronous offline baking. A process-wide gate prevents competing SDK bake/cancellation operations.
/// </summary>
public static unsafe class Baker
{
    private static readonly object Gate = new();
    private sealed class Progress(Action<float>? callback)
    {
        internal ExceptionDispatchInfo? Error;
        internal void Invoke(float value)
        {
            Geometry.RayCallbacks.EnterCallback();
            try
            {
                callback?.Invoke(value);
            }
            catch (Exception error)
            {
                Interlocked.CompareExchange(ref Error, ExceptionDispatchInfo.Capture(error), null);
            }
            finally
            {
                Geometry.RayCallbacks.ExitCallback();
            }
        }
    }

    [UnmanagedCallersOnly]
    private static void Report(float value, void* userData) => ((Progress)GCHandle.FromIntPtr((nint)userData).Target!).Invoke(value);

    /// <summary>
    /// Bakes reflection data into committed probes. Geometry and simulation work runs outside the audio
    /// callback.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="scene">Committed geometry.</param>
    /// <param name="probes">Committed probe positions.</param>
    /// <param name="parameters">
    /// SDK sampling and bake settings; scene and batch handles are supplied by these owners.
    /// </param>
    /// <param name="progress">Optional progress from zero to one; exceptions are deferred.</param>
    /// <param name="cancellationToken">Requests the SDK's cooperative cancellation.</param>
    public static void Reflections(
        Context context,
        Geometry.Scene scene,
        ProbeBatch probes,
        Interop.ReflectionsBakeParams parameters,
        Action<float>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Geometry.RayCallbacks.ThrowIfInside();
        cancellationToken.ThrowIfCancellationRequested();
        lock (Gate)
        {
            using var contextLease = context.Acquire();
            using var s = scene.Acquire();
            using var b = probes.Acquire();
            if (scene.ParentAddress != contextLease.Value || probes.ParentAddress != contextLease.Value)
            {
                throw new ArgumentException("Baking resources belong to different contexts.");
            }

            parameters.Scene = new(s.Value);
            parameters.ProbeBatch = new(b.Value);
            var state = new Progress(progress);
            var root = GCHandle.Alloc(state);
            var address = contextLease.Value;
            try
            {
                using var registration = cancellationToken.Register(() => Interop.Api.ReflectionsBakerCancelBake(new(address)));
                Interop.Api.ReflectionsBakerBake(new(contextLease.Value), &parameters, &Report, (void*)GCHandle.ToIntPtr(root));
                state.Error?.Throw();
                scene.ThrowIfCallbackFailed();
                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                root.Free();
            }
        }
    }

    /// <summary>
    /// Bakes path visibility/connectivity into a committed probe batch.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="scene">Committed geometry.</param>
    /// <param name="probes">Committed probes.</param>
    /// <param name="parameters">Path-baking controls; scene and batch handles are supplied.</param>
    /// <param name="progress">Optional progress callback with deferred exceptions.</param>
    /// <param name="cancellationToken">Cooperative SDK cancellation.</param>
    public static void Paths(
        Context context,
        Geometry.Scene scene,
        ProbeBatch probes,
        Interop.PathBakeParams parameters,
        Action<float>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Geometry.RayCallbacks.ThrowIfInside();
        cancellationToken.ThrowIfCancellationRequested();
        lock (Gate)
        {
            using var contextLease = context.Acquire();
            using var s = scene.Acquire();
            using var b = probes.Acquire();
            if (scene.ParentAddress != contextLease.Value || probes.ParentAddress != contextLease.Value)
            {
                throw new ArgumentException("Foreign baking resources.");
            }

            parameters.Scene = new(s.Value);
            parameters.ProbeBatch = new(b.Value);
            var state = new Progress(progress);
            var root = GCHandle.Alloc(state);
            var address = contextLease.Value;
            try
            {
                using var registration = cancellationToken.Register(() => Interop.Api.PathBakerCancelBake(new(address)));
                Interop.Api.PathBakerBake(new(contextLease.Value), &parameters, &Report, (void*)GCHandle.ToIntPtr(root));
                state.Error?.Throw();
                scene.ThrowIfCallbackFailed();
                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                root.Free();
            }
        }
    }
}
