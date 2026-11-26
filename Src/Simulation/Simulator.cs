namespace SteamAudio.Simulation;
/// <summary>Owns a native Simulator reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class Simulator : Resource
{
    internal readonly nint ParentAddress;
    internal readonly OperationGate Operations = new();
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="scene">Optional scene retained for the simulator lifetime.</param>
    /// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public Simulator(Context parent, Interop.SimulationSettings settings, Geometry.Scene? scene = null, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies, .. (scene == null ? Array.Empty<Resource>() : new Resource[] { scene })])
    { ParentAddress = parent.DangerousGetHandle(); _scene = scene; if (scene != null) { using var sim = Acquire(); using var geometry = scene.Acquire(); Interop.Api.SimulatorSetScene(new(sim.Value), new(geometry.Value)); } }
    private readonly Geometry.Scene? _scene;
    /// <summary>Commits source and probe changes before simulation.</summary>
    public void Commit() { using var operation = Operations.Enter(); using var lease = Acquire(); Interop.Api.SimulatorCommit(new(lease.Value)); }
    /// <summary>Runs direct-sound simulation synchronously. Do not call from the audio callback.</summary>
    public void RunDirect() { using var operation = Operations.Enter(); using var lease = Acquire(); Interop.Api.SimulatorRunDirect(new(lease.Value)); _scene?.ThrowIfCallbackFailed(); }
    /// <summary>Runs reflection simulation synchronously on the configured CPU workers.</summary>
    public void RunReflections() { using var operation = Operations.Enter(); using var lease = Acquire(); Interop.Api.SimulatorRunReflections(new(lease.Value)); _scene?.ThrowIfCallbackFailed(); }
    /// <summary>Runs pathing simulation using committed baked probe data.</summary>
    public void RunPathing() { using var operation = Operations.Enter(); using var lease = Acquire(); Interop.Api.SimulatorRunPathing(new(lease.Value)); _scene?.ThrowIfCallbackFailed(); }
    /// <summary>Sets listener and shared simulation controls for the selected algorithms.</summary>
    /// <param name="flags">Algorithms receiving these inputs.</param><param name="inputs">SDK inputs; retain callback user data throughout simulation.</param>
    public void SetSharedInputs(Interop.SimulationFlags flags, Interop.SimulationSharedInputs inputs)
    { using var operation = Operations.Enter(); using var lease = Acquire(); Interop.Api.SimulatorSetSharedInputs(new(lease.Value), flags, &inputs); }
    private static nint Create(Context parent, Interop.SimulationSettings settings)
    {
        using var lease = parent.Acquire(); Interop.SimulatorHandle value;
        SteamAudioException.Check(Interop.Api.SimulatorCreate(new Interop.ContextHandle(lease.Value), &settings, &value), "Create Simulator"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.SimulatorHandle(value); Interop.Api.SimulatorRelease(&handle); }
}
