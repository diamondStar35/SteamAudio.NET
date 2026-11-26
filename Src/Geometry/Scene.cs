namespace SteamAudio.Geometry;
/// <summary>Owns a native Scene reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class Scene : Resource
{
    internal readonly nint ParentAddress;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public Scene(Context parent, Interop.SceneSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies]) => ParentAddress = parent.DangerousGetHandle();
    /// <summary>Commits geometry additions, removals and material changes before simulation.</summary>
    public void Commit() { using var lease = Acquire(); Interop.Api.SceneCommit(new(lease.Value)); }
    /// <summary>Serializes a built-in CPU scene, including committed geometry.</summary>
    /// <param name="context">Original context used for serialization storage.</param><returns>An independent serialized byte array.</returns>
    public byte[] Save(Context context)
    {
        if (context.DangerousGetHandle() != ParentAddress) throw new ArgumentException("Foreign context.");
        using var storage = new SerializedObject(context); using var scene = Acquire(); using var destination = storage.Acquire();
        Interop.Api.SceneSave(new(scene.Value), new(destination.Value)); return storage.ToArray();
    }
    /// <summary>Loads a built-in CPU scene from serialized bytes. Loading consumes data synchronously.</summary>
    /// <param name="context">Owning context.</param><param name="data">Complete serialized scene.</param><returns>An independent scene owner.</returns>
    public static Scene Load(Context context, ReadOnlySpan<byte> data)
    {
        using var storage = new SerializedObject(context, data); using var c = context.Acquire(); using var input = storage.Acquire();
        var settings = new Interop.SceneSettings { Type = Interop.SceneType.Default }; Interop.SceneHandle value;
        SteamAudioException.Check(Interop.Api.SceneLoad(new(c.Value), &settings, new(input.Value), null, null, &value), "Load scene"); return new(context, value.Value);
    }
    private Scene(Context context, nint value) : base(value, Release, context) => ParentAddress = context.DangerousGetHandle();
    private readonly RayCallbacks? _callbacks;
    private sealed record Custom(nint Value, RayCallbacks Callbacks);
    /// <summary>Creates a custom ray-traced scene with rooted managed callbacks and copied native materials.</summary>
    /// <param name="context">Owning context.</param><param name="tracer">Thread-safe geometry provider. Exceptions are deferred until simulation returns.</param><param name="materials">Stable material table selected by callback hit indices.</param>
    public Scene(Context context, IRayTracer tracer, ReadOnlySpan<Interop.Material> materials) : this(context, CreateCustom(context, tracer, materials)) { }
    private Scene(Context context, Custom creation) : base(creation.Value, value => { try { Release(value); } finally { creation.Callbacks.Dispose(); } }, context)
    { ParentAddress = context.DangerousGetHandle(); _callbacks = creation.Callbacks; }
    private static Custom CreateCustom(Context context, IRayTracer tracer, ReadOnlySpan<Interop.Material> materials)
    {
        var callbacks = new RayCallbacks(tracer, materials);
        try { return new(Create(context, callbacks.Settings), callbacks); }
        catch { callbacks.Dispose(); throw; }
    }
    internal void ThrowIfCallbackFailed() => _callbacks?.ThrowIfFailed();
    private static nint Create(Context parent, Interop.SceneSettings settings)
    {
        using var lease = parent.Acquire(); Interop.SceneHandle value;
        SteamAudioException.Check(Interop.Api.SceneCreate(new Interop.ContextHandle(lease.Value), &settings, &value), "Create Scene"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.SceneHandle(value); Interop.Api.SceneRelease(&handle); }
}
