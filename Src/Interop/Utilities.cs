// Native contracts and documentation derived from Valve Steam Audio 4.8.1 (Apache-2.0).
using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

/// <summary>Exact Steam Audio C ABI. Pointers and handles are borrowed; managed owners provide lifetime checks.</summary>
public static unsafe partial class Api
{
    /// <summary>Calculates the relative direction from the listener to a sound source. The returned direction vector is expressed in the listener's coordinate system.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="sourcePosition">World-space coordinates of the source.</param>
    /// <param name="listenerPosition">World-space coordinates of the listener.</param>
    /// <param name="listenerAhead">World-space unit-length vector pointing ahead relative to the listener.</param>
    /// <param name="listenerUp">World-space unit-length vector pointing up relative to the listener.</param>
    /// <returns>A unit-length vector in the listener's coordinate space, pointing from the listener to the source.</returns>
    [LibraryImport(Library, EntryPoint = "iplCalculateRelativeDirection")]
    public static partial Vector3 CalculateRelativeDirection(ContextHandle context, Vector3 sourcePosition, Vector3 listenerPosition, Vector3 listenerAhead, Vector3 listenerUp);

    /// <summary>Creates an Embree device.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="settings">The settings to use when creating the Embree device.</param>
    /// <param name="device">The created Embree device.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplEmbreeDeviceCreate")]
    public static partial Error EmbreeDeviceCreate(ContextHandle context, EmbreeDeviceSettings* settings, EmbreeDeviceHandle* device);

    /// <summary>Retains an additional reference to an Embree device.</summary>
    /// <param name="device">The Embree device to retain a reference to.</param>
    /// <returns>The additional reference to the Embree device.</returns>
    [LibraryImport(Library, EntryPoint = "iplEmbreeDeviceRetain")]
    public static partial EmbreeDeviceHandle EmbreeDeviceRetain(EmbreeDeviceHandle device);

    /// <summary>Releases a reference to an Embree device.</summary>
    /// <param name="device">The Embree device to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplEmbreeDeviceRelease")]
    public static partial void EmbreeDeviceRelease(EmbreeDeviceHandle* device);

    /// <summary>Creates an OpenCL device list. This involves listing all available OpenCL devices on the user's system.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="settings">The requirements that all listed OpenCL devices must satisfy.</param>
    /// <param name="deviceList">The created OpenCL device list.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceListCreate")]
    public static partial Error OpenCLDeviceListCreate(ContextHandle context, OpenCLDeviceSettings* settings, OpenCLDeviceListHandle* deviceList);

    /// <summary>Retains an additional reference to an OpenCL device list.</summary>
    /// <param name="deviceList">The OpenCL device list to retain a reference to.</param>
    /// <returns>The additional reference to the OpenCL device list.</returns>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceListRetain")]
    public static partial OpenCLDeviceListHandle OpenCLDeviceListRetain(OpenCLDeviceListHandle deviceList);

    /// <summary>Releases a reference to an OpenCL device list.</summary>
    /// <param name="deviceList">The OpenCL device list to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceListRelease")]
    public static partial void OpenCLDeviceListRelease(OpenCLDeviceListHandle* deviceList);

    /// <summary>OpenCLDeviceListGetNumDevices</summary>
    /// <returns>The number of devices in an OpenCL device list.</returns>
    /// <param name="deviceList">The OpenCL device list.</param>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceListGetNumDevices")]
    public static partial int OpenCLDeviceListGetNumDevices(OpenCLDeviceListHandle deviceList);

    /// <summary>Retrieves information about a specific device in an OpenCL device list.</summary>
    /// <param name="deviceList">The OpenCL device list.</param>
    /// <param name="index">The index of the device within the list.</param>
    /// <param name="deviceDesc">A descriptor for the properties of the specified OpenCL device.</param>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceListGetDeviceDesc")]
    public static partial void OpenCLDeviceListGetDeviceDesc(OpenCLDeviceListHandle deviceList, int index, OpenCLDeviceDesc* deviceDesc);

    /// <summary>Creates an OpenCL device. The device is specified as an index into an OpenCL device list.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="deviceList">The OpenCL device list.</param>
    /// <param name="index">The index of the device within the list.</param>
    /// <param name="device">The created OpenCL device.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceCreate")]
    public static partial Error OpenCLDeviceCreate(ContextHandle context, OpenCLDeviceListHandle deviceList, int index, OpenCLDeviceHandle* device);

    /// <summary>Creates an OpenCL device from an existing OpenCL device created by your application. Steam Audio will use up to two command queues that you provide for enqueuing OpenCL computations.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="convolutionQueue">The <c>cl_command_queue</c> to use for enqueueing convolution work.</param>
    /// <param name="irUpdateQueue">The <c>cl_command_queue</c> to use for enqueueing IR update work.</param>
    /// <param name="device">The created OpenCL device.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceCreateFromExisting")]
    public static partial Error OpenCLDeviceCreateFromExisting(ContextHandle context, void* convolutionQueue, void* irUpdateQueue, OpenCLDeviceHandle* device);

    /// <summary>Retains an additional reference to an OpenCL device.</summary>
    /// <param name="device">The OpenCL device to retain a reference to.</param>
    /// <returns>The additional reference to the OpenCL device.</returns>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceRetain")]
    public static partial OpenCLDeviceHandle OpenCLDeviceRetain(OpenCLDeviceHandle device);

    /// <summary>Releases a reference to an OpenCL device.</summary>
    /// <param name="device">The OpenCL device to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplOpenCLDeviceRelease")]
    public static partial void OpenCLDeviceRelease(OpenCLDeviceHandle* device);

    /// <summary>Creates a Radeon Rays device.</summary>
    /// <param name="openCLDevice">The OpenCL device to use for running Radeon Rays.</param>
    /// <param name="settings">The settings to use when creating the Radeon Rays device.</param>
    /// <param name="rrDevice">The created Radeon Rays device.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplRadeonRaysDeviceCreate")]
    public static partial Error RadeonRaysDeviceCreate(OpenCLDeviceHandle openCLDevice, RadeonRaysDeviceSettings* settings, RadeonRaysDeviceHandle* rrDevice);

    /// <summary>Retains an additional reference to a Radeon Rays device.</summary>
    /// <param name="device">The Radeon Rays device to retain a reference to.</param>
    /// <returns>The additional reference to the Radeon Rays device.</returns>
    [LibraryImport(Library, EntryPoint = "iplRadeonRaysDeviceRetain")]
    public static partial RadeonRaysDeviceHandle RadeonRaysDeviceRetain(RadeonRaysDeviceHandle device);

    /// <summary>Releases a reference to a Radeon Rays device.</summary>
    /// <param name="device">The Radeon Rays device to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplRadeonRaysDeviceRelease")]
    public static partial void RadeonRaysDeviceRelease(RadeonRaysDeviceHandle* device);

    /// <summary>Creates a TrueAudio Next device.</summary>
    /// <param name="openCLDevice">The OpenCL device to use for running TrueAudio Next.</param>
    /// <param name="settings">The settings to use when creating the TrueAudio Next device.</param>
    /// <param name="tanDevice">The created TrueAudio Next device.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplTrueAudioNextDeviceCreate")]
    public static partial Error TrueAudioNextDeviceCreate(OpenCLDeviceHandle openCLDevice, TrueAudioNextDeviceSettings* settings, TrueAudioNextDeviceHandle* tanDevice);

    /// <summary>Retains an additional reference to a TrueAudio Next device.</summary>
    /// <param name="device">The TrueAudio Next device to retain a reference to.</param>
    /// <returns>The additional reference to the TrueAudio Next device.</returns>
    [LibraryImport(Library, EntryPoint = "iplTrueAudioNextDeviceRetain")]
    public static partial TrueAudioNextDeviceHandle TrueAudioNextDeviceRetain(TrueAudioNextDeviceHandle device);

    /// <summary>Releases a reference to a TrueAudio Next device.</summary>
    /// <param name="device">The TrueAudio Next device to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplTrueAudioNextDeviceRelease")]
    public static partial void TrueAudioNextDeviceRelease(TrueAudioNextDeviceHandle* device);

    /// <summary>Bakes a single layer of reflections data in a probe batch.  Only one bake can be in progress at any point in time.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="params">Parameters to use for baking reflections data.</param>
    /// <param name="progressCallback">(Optional) This function will be called by Steam Audio to notify your application as the bake progresses. Use this to display a progress bar or some other indicator that the bake is running.</param>
    /// <param name="userData">(Optional) Pointer to arbitrary data that will be sent to the progress callback when Steam Audio calls it.</param>
    [LibraryImport(Library, EntryPoint = "iplReflectionsBakerBake")]
    public static partial void ReflectionsBakerBake(ContextHandle context, ReflectionsBakeParams* @params, delegate* unmanaged<float, void*, void> progressCallback, void* userData);

    /// <summary>Cancels any running bakes of reflections data.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    [LibraryImport(Library, EntryPoint = "iplReflectionsBakerCancelBake")]
    public static partial void ReflectionsBakerCancelBake(ContextHandle context);

    /// <summary>Bakes a single layer of pathing data in a probe batch.  Only one bake can be in progress at any point in time.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="params">Parameters to use for baking pathing data.</param>
    /// <param name="progressCallback">(Optional) This function will be called by Steam Audio to notify your application as the bake progresses. Use this to display a progress bar or some other indicator that the bake is running.</param>
    /// <param name="userData">(Optional) Pointer to arbitrary data that will be sent to the progress callback when Steam Audio calls it.</param>
    [LibraryImport(Library, EntryPoint = "iplPathBakerBake")]
    public static partial void PathBakerBake(ContextHandle context, PathBakeParams* @params, delegate* unmanaged<float, void*, void> progressCallback, void* userData);

    /// <summary>Cancels any running bakes of pathing data.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    [LibraryImport(Library, EntryPoint = "iplPathBakerCancelBake")]
    public static partial void PathBakerCancelBake(ContextHandle context);

    /// <summary>Calculates the distance attenuation between a source and a listener.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="source">The position of the source.</param>
    /// <param name="listener">The position of the listener.</param>
    /// <param name="model">The distance attenuation model to use.</param>
    /// <returns>The distance attenuation to apply, between <c>0</c> and <c>1.</c></returns>
    [LibraryImport(Library, EntryPoint = "iplDistanceAttenuationCalculate")]
    public static partial float DistanceAttenuationCalculate(ContextHandle context, Vector3 source, Vector3 listener, DistanceAttenuationModel* model);

    /// <summary>Calculates the air absorption coefficients between a source and a listener.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="source">The position of the source.</param>
    /// <param name="listener">The position of the listener.</param>
    /// <param name="model">The air absorption model to use.</param>
    /// <param name="airAbsorption">The 3-band air absorption coefficients, each between <c>0</c> and <c>1.</c></param>
    [LibraryImport(Library, EntryPoint = "iplAirAbsorptionCalculate")]
    public static partial void AirAbsorptionCalculate(ContextHandle context, Vector3 source, Vector3 listener, AirAbsorptionModel* model, float* airAbsorption);

    /// <summary>Calculates the attenuation of a source due to its directivity pattern and orientation relative to a listener.</summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="source">The position and orientation of the source.</param>
    /// <param name="listener">The position of the listener.</param>
    /// <param name="model">The directivity pattern to use.</param>
    /// <returns>The directivity value to apply, between <c>0</c> and <c>1.</c></returns>
    [LibraryImport(Library, EntryPoint = "iplDirectivityCalculate")]
    public static partial float DirectivityCalculate(ContextHandle context, CoordinateSpace3 source, Vector3 listener, Directivity* model);
}
