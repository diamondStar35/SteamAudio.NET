using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a simulation source.
    /// </summary>
    /// <param name="simulator">The simulator with which this source will be used.</param>
    /// <param name="settings">The settings to use for creating the source.</param>
    /// <param name="source">The created source.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplSourceCreate")]
    public static partial Error SourceCreate(
        SimulatorHandle simulator,
        SourceSettings* settings,
        SourceHandle* source);

    /// <summary>
    /// Retains an additional reference to a source.
    /// </summary>
    /// <param name="source">The source to retain a reference to.</param>
    /// <returns>The additional reference to the source.</returns>
    [LibraryImport(Library, EntryPoint = "iplSourceRetain")]
    public static partial SourceHandle SourceRetain(SourceHandle source);

    /// <summary>
    /// Releases a reference to a source.
    /// </summary>
    /// <param name="source">The source to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplSourceRelease")]
    public static partial void SourceRelease(SourceHandle* source);

    /// <summary>
    /// Adds a source to the set of sources processed by a simulator in subsequent simulations. Call
    /// <c>iplSimulatorCommit</c> after calling this function for the changes to take effect.
    /// </summary>
    /// <param name="simulator">The simulator being used.</param>
    /// <param name="source">The source to add.</param>
    [LibraryImport(Library, EntryPoint = "iplSourceAdd")]
    public static partial void SourceAdd(SourceHandle source, SimulatorHandle simulator);

    /// <summary>
    /// Removes a source from the set of sources processed by a simulator in subsequent simulations. Call
    /// <c>iplSimulatorCommit</c> after calling this function for the changes to take effect.
    /// </summary>
    /// <param name="simulator">The simulator being used.</param>
    /// <param name="source">The source to remove.</param>
    [LibraryImport(Library, EntryPoint = "iplSourceRemove")]
    public static partial void SourceRemove(SourceHandle source, SimulatorHandle simulator);

    /// <summary>
    /// Specifies simulation parameters for a source.
    /// </summary>
    /// <param name="source">The source to specify parameters for.</param>
    /// <param name="flags">
    /// The types of simulation for which to specify inputs. If, for example, direct and reflections simulations
    /// are being run on separate threads, you can call this function on the direct simulation thread with
    /// <c>IPL_SIMULATIONFLAGS_DIRECT</c>, and on the reflections simulation thread with
    /// <c>IPL_SIMULATIONFLAGS_REFLECTIONS</c>, without requiring any synchronization between the calls.
    /// </param>
    /// <param name="inputs">The input parameters to set.</param>
    [LibraryImport(Library, EntryPoint = "iplSourceSetInputs")]
    public static partial void SourceSetInputs(
        SourceHandle source,
        SimulationFlags flags,
        SimulationInputs* inputs);

    /// <summary>
    /// Retrieves simulation results for a source.
    /// </summary>
    /// <param name="source">The source to retrieve results for.</param>
    /// <param name="flags">The types of simulation for which to retrieve results.</param>
    /// <param name="outputs">The simulation results.</param>
    [LibraryImport(Library, EntryPoint = "iplSourceGetOutputs")]
    public static partial void SourceGetOutputs(
        SourceHandle source,
        SimulationFlags flags,
        SimulationOutputs* outputs);
}
