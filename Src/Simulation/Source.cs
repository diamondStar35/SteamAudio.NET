namespace SteamAudio.Simulation;

/// <summary>
/// Stores one source's simulation inputs and its latest direct, reflection and pathing results.
/// </summary>
public sealed unsafe class Source : Resource
{
    internal readonly nint ParentAddress;
    private readonly Simulator _simulator;

    /// <summary>
    /// Creates a source associated with a simulator. Add it and commit before running simulation.
    /// </summary>
    /// <param name="parent">Parent device or simulator retained until this resource is released.</param>
    /// <param name="settings">Algorithms whose inputs and outputs this source will use.</param>
    /// <param name="dependencies">
    /// Additional resource owners retained for this resource's lifetime. External pointer storage remains the
    /// caller's responsibility.
    /// </param>
    public Source(Simulator parent, Interop.SourceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies])
    {
        ParentAddress = parent.DangerousGetHandle();
        _simulator = parent;
    }

    /// <summary>
    /// Adds the source to its retained simulator. Commit the simulator afterward.
    /// </summary>
    public void Add()
    {
        using var operation = _simulator.Operations.Enter();
        using var lease = Acquire();
        Interop.Api.SourceAdd(new(lease.Value), new(ParentAddress));
    }

    /// <summary>
    /// Removes the source from simulation. Commit the simulator afterward.
    /// </summary>
    public void Remove()
    {
        using var operation = _simulator.Operations.Enter();
        using var lease = Acquire();
        Interop.Api.SourceRemove(new(lease.Value), new(ParentAddress));
    }

    /// <summary>
    /// Copies source inputs. Any callback or pointer data must remain valid until the next input update and
    /// simulation completion.
    /// </summary>
    /// <param name="inputs">Source controls including flags selecting their algorithms.</param>
    public void SetInputs(Interop.SimulationInputs inputs)
    {
        using var operation = _simulator.Operations.Enter();
        using var lease = Acquire();
        Interop.Api.SourceSetInputs(new(lease.Value), inputs.Flags, &inputs);
    }

    /// <summary>
    /// Retrieves the latest simulation outputs. Native impulse-response pointers remain owned by the simulator.
    /// </summary>
    /// <param name="flags">Algorithms whose outputs are requested.</param>
    /// <returns>A snapshot containing the SDK's borrowed output pointers.</returns>
    public Interop.SimulationOutputs GetOutputs(Interop.SimulationFlags flags)
    {
        using var operation = _simulator.Operations.Enter();
        using var lease = Acquire();
        Interop.SimulationOutputs outputs;
        Interop.Api.SourceGetOutputs(new(lease.Value), flags, &outputs);
        return outputs;
    }

    private static nint Create(Simulator parent, Interop.SourceSettings settings)
    {
        using var lease = parent.Acquire();
        Interop.SourceHandle value;
        SteamAudioException.Check(
            Interop.Api.SourceCreate(new Interop.SimulatorHandle(lease.Value), &settings, &value),
            "Create Source");
        return value.Value;
    }

    private static void Release(nint value)
    {
        var handle = new Interop.SourceHandle(value);
        Interop.Api.SourceRelease(&handle);
    }
}
