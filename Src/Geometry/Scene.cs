namespace SteamAudio.Geometry;

/// <summary>
/// Stores acoustic geometry used by direct-sound, reflection and path simulations.
/// </summary>
public sealed unsafe class Scene : Resource
{
    internal readonly nint ParentAddress;
    private readonly Interop.SceneType _type;

    /// <summary>
    /// Creates a scene using the selected ray tracer and retains its acceleration resources.
    /// </summary>
    /// <param name="parent">Context that owns this resource and is retained until its release.</param>
    /// <param name="settings">
    /// Ray-tracing implementation and callbacks. Custom callback code and user data must remain valid until the
    /// scene is released.
    /// </param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public Scene(Context parent, Interop.SceneSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
        _type = settings.Type;
    }

    /// <summary>
    /// Commits geometry additions, removals and material changes before simulation.
    /// </summary>
    public void Commit()
    {
        using var lease = Acquire();
        Interop.Api.SceneCommit(new(lease.Value));
    }

    /// <summary>
    /// Serializes a built-in CPU scene, including committed geometry.
    /// </summary>
    /// <param name="context">Original context used for serialization storage.</param>
    /// <returns>An independent serialized byte array.</returns>
    public byte[] Save(Context context)
    {
        if (_type != Interop.SceneType.Default)
        {
            throw new NotSupportedException("Only built-in CPU scenes support serialization.");
        }

        if (context.DangerousGetHandle() != ParentAddress)
        {
            throw new ArgumentException("Foreign context.");
        }

        using var storage = new SerializedObject(context);
        using var scene = Acquire();
        using var destination = storage.Acquire();
        Interop.Api.SceneSave(new(scene.Value), new(destination.Value));
        return storage.ToArray();
    }

    /// <summary>
    /// Loads a built-in CPU scene from serialized bytes. Loading consumes data synchronously.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="data">Complete serialized scene.</param>
    /// <returns>An independent scene owner.</returns>
    public static Scene Load(Context context, ReadOnlySpan<byte> data)
    {
        using var storage = new SerializedObject(context, data);
        using var contextLease = context.Acquire();
        using var input = storage.Acquire();
        var settings = new Interop.SceneSettings
        {
            Type = Interop.SceneType.Default
        };
        Interop.SceneHandle value;
        SteamAudioException.Check(
            Interop.Api.SceneLoad(new(contextLease.Value), &settings, new(input.Value), null, null, &value),
            "Load scene");
        return new(context, value.Value);
    }

    private Scene(Context context, nint value)
        : base(value, Release, context)
    {
        ParentAddress = context.DangerousGetHandle();
    }

    private readonly RayCallbacks? _callbacks;
    private sealed record Custom(nint Value, RayCallbacks Callbacks);

    /// <summary>
    /// Creates a custom ray-traced scene with rooted managed callbacks and copied native materials.
    /// </summary>
    /// <param name="context">Owning context.</param>
    /// <param name="tracer">Thread-safe geometry provider. Exceptions are deferred until simulation returns.</param>
    /// <param name="materials">Stable material table selected by callback hit indices.</param>
    public Scene(Context context, IRayTracer tracer, ReadOnlySpan<Interop.Material> materials)
        : this(context, CreateCustom(context, tracer, materials))
    {
    }

    private Scene(Context context, Custom creation)
        : base(
            creation.Value,
            value =>
    {
        try
        {
            Release(value);
        }
        finally
        {
            creation.Callbacks.Dispose();
        }
    },
        context)
    {
        ParentAddress = context.DangerousGetHandle();
        _callbacks = creation.Callbacks;
        _type = Interop.SceneType.Custom;
    }

    private static Custom CreateCustom(
        Context context,
        IRayTracer tracer,
        ReadOnlySpan<Interop.Material> materials)
    {
        var callbacks = new RayCallbacks(tracer, materials);
        try
        {
            return new(Create(context, callbacks.Settings), callbacks);
        }
        catch
        {
            callbacks.Dispose();
            throw;
        }
    }

    internal void ThrowIfCallbackFailed() => _callbacks?.ThrowIfFailed();
    private static nint Create(Context parent, Interop.SceneSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.SceneHandle value;
        SteamAudioException.Check(
            Interop.Api.SceneCreate(new Interop.ContextHandle(lease.Value), &settings, &value),
            "Create Scene");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.SceneHandle(value);
        Interop.Api.SceneRelease(&handle);
    }
}
