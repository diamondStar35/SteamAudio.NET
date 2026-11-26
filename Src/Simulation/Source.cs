namespace SteamAudio.Simulation;
/// <summary>Owns a native Source reference. The complete native controls and their documentation are available through Interop.Api.</summary>
public sealed unsafe partial class Source : Resource
{
    internal readonly nint ParentAddress;
    private readonly Simulator _simulator;
    /// <summary>Creates this resource and retains its parent and explicitly supplied native dependencies.</summary>
    /// <param name="parent">Native parent used for creation.</param>
    /// <param name="settings">SDK settings. Pointer members must remain valid for their documented native lifetime.</param>
/// <param name="dependencies">Resources referenced by pointer or handle settings.</param>
    public Source(Simulator parent, Interop.SourceSettings settings, params Resource[] dependencies)
        : base(Create(parent, settings), Release, [parent, .. dependencies]) { ParentAddress = parent.DangerousGetHandle(); _simulator = parent; }
    /// <summary>Adds the source to its retained simulator. Commit the simulator afterward.</summary>
    public void Add() { using var operation = _simulator.Operations.Enter(); using var lease = Acquire(); Interop.Api.SourceAdd(new(lease.Value), new(ParentAddress)); }
    /// <summary>Removes the source from simulation. Commit the simulator afterward.</summary>
    public void Remove() { using var operation = _simulator.Operations.Enter(); using var lease = Acquire(); Interop.Api.SourceRemove(new(lease.Value), new(ParentAddress)); }
    /// <summary>Copies source inputs. Any callback or pointer data must remain valid until the next input update and simulation completion.</summary>
    /// <param name="inputs">Source controls including flags selecting their algorithms.</param>
    public void SetInputs(Interop.SimulationInputs inputs) { using var operation = _simulator.Operations.Enter(); using var lease = Acquire(); Interop.Api.SourceSetInputs(new(lease.Value), inputs.Flags, &inputs); }
    /// <summary>Retrieves the latest simulation outputs. Native impulse-response pointers remain owned by the simulator.</summary>
    /// <param name="flags">Algorithms whose outputs are requested.</param><returns>A snapshot containing the SDK's borrowed output pointers.</returns>
    public Interop.SimulationOutputs GetOutputs(Interop.SimulationFlags flags)
    { using var operation = _simulator.Operations.Enter(); using var lease = Acquire(); Interop.SimulationOutputs outputs; Interop.Api.SourceGetOutputs(new(lease.Value), flags, &outputs); return outputs; }
    private static nint Create(Simulator parent, Interop.SourceSettings settings)
    {
        using var lease = parent.Acquire(); Interop.SourceHandle value;
        SteamAudioException.Check(Interop.Api.SourceCreate(new Interop.SimulatorHandle(lease.Value), &settings, &value), "Create Source"); return value.Value;
    }
    private static void Release(nint value) { var handle = new Interop.SourceHandle(value); Interop.Api.SourceRelease(&handle); }
}
