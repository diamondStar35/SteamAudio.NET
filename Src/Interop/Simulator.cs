using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates a simulator.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="settings">The settings to use when creating the simulator.</param>
    /// <param name="simulator">The created simulator.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplSimulatorCreate")]
    public static partial Error SimulatorCreate(
        ContextHandle context,
        SimulationSettings* settings,
        SimulatorHandle* simulator);

    /// <summary>
    /// Retains an additional reference to a simulator.
    /// </summary>
    /// <param name="simulator">The simulator to retain a reference to.</param>
    /// <returns>The additional reference to the simulator.</returns>
    [LibraryImport(Library, EntryPoint = "iplSimulatorRetain")]
    public static partial SimulatorHandle SimulatorRetain(SimulatorHandle simulator);

    /// <summary>
    /// Releases a reference to a simulator.
    /// </summary>
    /// <param name="simulator">The simulator to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorRelease")]
    public static partial void SimulatorRelease(SimulatorHandle* simulator);

    /// <summary>
    /// Specifies the scene within which all subsequent simulations should be run. Call
    /// <c>iplSimulatorCommit</c> after calling this function for the changes to take effect. This function
    /// cannot be called while any simulation is running.
    /// </summary>
    /// <param name="simulator">The simulator being used.</param>
    /// <param name="scene">The scene to use for simulations.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorSetScene")]
    public static partial void SimulatorSetScene(SimulatorHandle simulator, SceneHandle scene);

    /// <summary>
    /// Adds a probe batch for use in subsequent simulations. Sources that require baked data can then use the
    /// data contained in the specified probe batch. Call <c>iplSimulatorCommit</c> after calling this function
    /// for the changes to take effect. This function cannot be called while any simulation is running.
    /// </summary>
    /// <param name="probeBatch">The probe batch to add.</param>
    /// <param name="simulator">The simulator being used.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorAddProbeBatch")]
    public static partial void SimulatorAddProbeBatch(SimulatorHandle simulator, ProbeBatchHandle probeBatch);

    /// <summary>
    /// Removed a probe batch from use in subsequent simulations. Sources that require baked data will then stop
    /// using the data contained in the specified probe batch. Call <c>iplSimulatorCommit</c> after calling this
    /// function for the changes to take effect. This function cannot be called while any simulation is running.
    /// </summary>
    /// <param name="probeBatch">The probe batch to remove.</param>
    /// <param name="simulator">The simulator being used.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorRemoveProbeBatch")]
    public static partial void SimulatorRemoveProbeBatch(SimulatorHandle simulator, ProbeBatchHandle probeBatch);

    /// <summary>
    /// Specifies simulation parameters that are not associated with any particular source.
    /// </summary>
    /// <param name="simulator">The simulator being used.</param>
    /// <param name="flags">
    /// The types of simulation for which to specify shared inputs. If, for example, direct and reflections
    /// simulations are being run on separate threads, you can call this function on the direct simulation
    /// thread with <c>IPL_SIMULATIONFLAGS_DIRECT</c>, and on the reflections simulation thread with
    /// <c>IPL_SIMULATIONFLAGS_REFLECTIONS</c>, without requiring any synchronization between the calls.
    /// </param>
    /// <param name="sharedInputs">The shared input parameters to set.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorSetSharedInputs")]
    public static partial void SimulatorSetSharedInputs(
        SimulatorHandle simulator,
        SimulationFlags flags,
        SimulationSharedInputs* sharedInputs);

    /// <summary>
    /// Commits changes to the scene or probe batches used for simulation. Call this function after calling
    /// <c>iplSimulatorSetScene</c>, <c>iplSimulatorAddProbeBatch</c>, or <c>iplSimulatorRemoveProbeBatch</c>
    /// for the changes to take effect. This function cannot be called while any simulation is running.
    /// </summary>
    /// <param name="simulator">The simulator being used.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorCommit")]
    public static partial void SimulatorCommit(SimulatorHandle simulator);

    /// <summary>
    /// Runs a direct simulation for all sources added to the simulator. This may include distance attenuation,
    /// air absorption, directivity, occlusion, and transmission. This function should not be called from the
    /// audio processing thread if occlusion and/or transmission are enabled.
    /// </summary>
    /// <param name="simulator">The simulator being used.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorRunDirect")]
    public static partial void SimulatorRunDirect(SimulatorHandle simulator);

    /// <summary>
    /// Runs a reflections simulation for all sources added to the simulator. This function can be CPU
    /// intensive, and should be called from a separate thread in order to not block either the audio processing
    /// thread or the game's main update thread.
    /// </summary>
    /// <param name="simulator">The simulator being used.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorRunReflections")]
    public static partial void SimulatorRunReflections(SimulatorHandle simulator);

    /// <summary>
    /// Runs a pathing simulation for all sources added to the simulator. This function can be CPU intensive,
    /// and should be called from a separate thread in order to not block either the audio processing thread or
    /// the game's main update thread.
    /// </summary>
    /// <param name="simulator">The simulator being used.</param>
    [LibraryImport(Library, EntryPoint = "iplSimulatorRunPathing")]
    public static partial void SimulatorRunPathing(SimulatorHandle simulator);
}
