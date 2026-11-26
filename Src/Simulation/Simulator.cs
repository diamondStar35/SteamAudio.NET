namespace SteamAudio.Simulation;

/// <summary>
/// Coordinates direct-sound, reflection and path simulations for a committed set of sources.
/// </summary>
public sealed unsafe class Simulator : Resource
{
    internal readonly nint ParentAddress;
    internal readonly OperationGate Operations = new();

    /// <summary>
    /// Creates simulation state with fixed algorithm limits and an optional retained scene.
    /// </summary>
    /// <param name="parent">Context that owns this resource and is retained until its release.</param>
    /// <param name="settings">
    /// Enabled algorithms, ray-tracing backend, sampling limits and audio format. These allocation limits
    /// remain fixed for the simulator lifetime.
    /// </param>
    /// <param name="scene">Optional scene retained for the simulator lifetime.</param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public Simulator(
        Context parent,
        Interop.SimulationSettings settings,
        Geometry.Scene? scene = null,
        params Resource[] dependencies)
        : base(
            Create(parent, settings),
            Release,
            [parent, ..dependencies, ..(scene == null ? Array.Empty<Resource>() : new Resource[]
            {
                scene
            })])
    {
        ParentAddress = parent.DangerousGetHandle();
        _scene = scene;
        if (scene != null)
        {
            if (scene.ParentAddress != ParentAddress)
            {
                throw new ArgumentException("Scene belongs to another context.", nameof(scene));
            }

            using var sim = Acquire();
            using var geometry = scene.Acquire();
            Interop.Api.SimulatorSetScene(new(sim.Value), new(geometry.Value));
        }
    }

    private readonly Geometry.Scene? _scene;

    /// <summary>
    /// Commits source and probe changes before simulation.
    /// </summary>
    public void Commit()
    {
        using var operation = Operations.Enter();
        using var lease = Acquire();
        Interop.Api.SimulatorCommit(new(lease.Value));
    }

    /// <summary>
    /// Adds baked probe data to this simulator. Commit before running simulation.
    /// </summary>
    /// <param name="probes">
    /// Probe batch from the same context. Retain it until removal has been committed; supply it as a
    /// constructor dependency to retain it automatically.
    /// </param>
    public void AddProbeBatch(Baking.ProbeBatch probes)
    {
        if (probes.ParentAddress != ParentAddress)
        {
            throw new ArgumentException("Probe batch belongs to another context.", nameof(probes));
        }

        using var operation = Operations.Enter();
        using var simulator = Acquire();
        using var batch = probes.Acquire();
        Interop.Api.SimulatorAddProbeBatch(new(simulator.Value), new(batch.Value));
    }

    /// <summary>
    /// Removes baked probe data from this simulator. Commit before releasing its owner.
    /// </summary>
    /// <param name="probes">Previously added probe batch from the same context.</param>
    public void RemoveProbeBatch(Baking.ProbeBatch probes)
    {
        if (probes.ParentAddress != ParentAddress)
        {
            throw new ArgumentException("Probe batch belongs to another context.", nameof(probes));
        }

        using var operation = Operations.Enter();
        using var simulator = Acquire();
        using var batch = probes.Acquire();
        Interop.Api.SimulatorRemoveProbeBatch(new(simulator.Value), new(batch.Value));
    }

    /// <summary>
    /// Runs direct-sound simulation synchronously. Do not call from the audio callback.
    /// </summary>
    public void RunDirect()
    {
        using var operation = Operations.Enter();
        using var lease = Acquire();
        Interop.Api.SimulatorRunDirect(new(lease.Value));
        _scene?.ThrowIfCallbackFailed();
    }

    /// <summary>
    /// Runs reflection simulation synchronously on the configured CPU workers.
    /// </summary>
    public void RunReflections()
    {
        using var operation = Operations.Enter();
        using var lease = Acquire();
        Interop.Api.SimulatorRunReflections(new(lease.Value));
        _scene?.ThrowIfCallbackFailed();
    }

    /// <summary>
    /// Runs pathing simulation using committed baked probe data.
    /// </summary>
    public void RunPathing()
    {
        using var operation = Operations.Enter();
        using var lease = Acquire();
        Interop.Api.SimulatorRunPathing(new(lease.Value));
        _scene?.ThrowIfCallbackFailed();
    }

    /// <summary>
    /// Sets listener and shared simulation controls for the selected algorithms.
    /// </summary>
    /// <param name="flags">Algorithms receiving these inputs.</param>
    /// <param name="inputs">SDK inputs; retain callback user data throughout simulation.</param>
    public void SetSharedInputs(Interop.SimulationFlags flags, Interop.SimulationSharedInputs inputs)
    {
        using var operation = Operations.Enter();
        using var lease = Acquire();
        Interop.Api.SimulatorSetSharedInputs(new(lease.Value), flags, &inputs);
    }

    private static nint Create(Context parent, Interop.SimulationSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.SimulatorHandle value;
        SteamAudioException.Check(
            Interop.Api.SimulatorCreate(new Interop.ContextHandle(lease.Value), &settings, &value),
            "Create Simulator");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.SimulatorHandle(value);
        Interop.Api.SimulatorRelease(&handle);
    }
}
