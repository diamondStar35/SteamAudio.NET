using System.Runtime.InteropServices;

namespace SteamAudio.Interop;

public static unsafe partial class Api
{
    /// <summary>
    /// Creates an energy field.
    /// </summary>
    /// <param name="context">The context used to initialize Steam Audio.</param>
    /// <param name="settings">The settings to use when creating the energy field.</param>
    /// <param name="energyField">The created energy field.</param>
    /// <returns>Status code indicating whether or not the operation succeeded.</returns>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldCreate")]
    public static partial Error EnergyFieldCreate(
        ContextHandle context,
        EnergyFieldSettings* settings,
        EnergyFieldHandle* energyField);

    /// <summary>
    /// Retains an additional reference to an energy field.
    /// </summary>
    /// <param name="energyField">The energy field to retain a reference to.</param>
    /// <returns>The additional reference to the energy field.</returns>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldRetain")]
    public static partial EnergyFieldHandle EnergyFieldRetain(EnergyFieldHandle energyField);

    /// <summary>
    /// Releases a reference to an energy field.
    /// </summary>
    /// <param name="energyField">The energy field to release a reference to.</param>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldRelease")]
    public static partial void EnergyFieldRelease(EnergyFieldHandle* energyField);

    /// <summary>
    /// Returns the number of channels in the energy field.
    /// </summary>
    /// <param name="energyField">The energy field.</param>
    /// <returns>The number of channels in the energy field.</returns>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldGetNumChannels")]
    public static partial int EnergyFieldGetNumChannels(EnergyFieldHandle energyField);

    /// <summary>
    /// Returns the number of bins in the energy field.
    /// </summary>
    /// <param name="energyField">The energy field.</param>
    /// <returns>The number of bins in the energy field.</returns>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldGetNumBins")]
    public static partial int EnergyFieldGetNumBins(EnergyFieldHandle energyField);

    /// <summary>
    /// Returns a pointer to the data stored in the energy field.
    /// </summary>
    /// <param name="energyField">The energy field.</param>
    /// <returns>
    /// Pointer to #channels * #bands * #bins IPLfloat32 values stored in the energy field, in row-major order.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldGetData")]
    public static partial float* EnergyFieldGetData(EnergyFieldHandle energyField);

    /// <summary>
    /// Returns a pointer to the data stored in the energy field for the given channel.
    /// </summary>
    /// <param name="energyField">The energy field.</param>
    /// <param name="channelIndex">Index of the channel.</param>
    /// <returns>
    /// Pointer to #bands * #bins IPLfloat32 values stored in the energy field for the given channel, in
    /// row-major order.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldGetChannel")]
    public static partial float* EnergyFieldGetChannel(EnergyFieldHandle energyField, int channelIndex);

    /// <summary>
    /// Returns a pointer to the data stored in the energy field for the given channel and band.
    /// </summary>
    /// <param name="energyField">The energy field.</param>
    /// <param name="channelIndex">Index of the channel.</param>
    /// <param name="bandIndex">Index of the band.</param>
    /// <returns>
    /// Pointer to #bins IPLfloat32 values stored in the energy field for the given channel and band, in
    /// row-major order.
    /// </returns>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldGetBand")]
    public static partial float* EnergyFieldGetBand(EnergyFieldHandle energyField, int channelIndex, int bandIndex);

    /// <summary>
    /// Resets all values stored in an energy field to zero.
    /// </summary>
    /// <param name="energyField">The energy field.</param>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldReset")]
    public static partial void EnergyFieldReset(EnergyFieldHandle energyField);

    /// <summary>
    /// Copies data from one energy field into another. If the source and destination energy fields have
    /// different numbers of channels, only the smaller of the two numbers of channels will be copied. If the
    /// source and destination energy fields have different numbers of bins, only the smaller of the two numbers
    /// of bins will be copied.
    /// </summary>
    /// <param name="src">The source energy field.</param>
    /// <param name="dst">The destination energy field.</param>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldCopy")]
    public static partial void EnergyFieldCopy(EnergyFieldHandle src, EnergyFieldHandle dst);

    /// <summary>
    /// Swaps the data contained in one energy field with the data contained in another energy field. The two
    /// energy fields may contain different numbers of channels or bins.
    /// </summary>
    /// <param name="a">The first energy field.</param>
    /// <param name="b">The second energy field.</param>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldSwap")]
    public static partial void EnergyFieldSwap(EnergyFieldHandle a, EnergyFieldHandle b);

    /// <summary>
    /// Adds the values stored in two energy fields, and stores the result in a third energy field. If the
    /// energy fields have different numbers of channels, only the smallest of the three numbers of channels
    /// will be added. If the energy fields have different numbers of bins, only the smallest of the three
    /// numbers of bins will be added. This function can be used for in-place addition, i.e. <c>out</c> may be
    /// equal to <c>in1</c> or <c>in2.</c>
    /// </summary>
    /// <param name="in1">The first input energy field.</param>
    /// <param name="in2">The second input energy field.</param>
    /// <param name="out">The output energy field.</param>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldAdd")]
    public static partial void EnergyFieldAdd(EnergyFieldHandle in1, EnergyFieldHandle in2, EnergyFieldHandle @out);

    /// <summary>
    /// Scales the values stored in an energy field by a scalar, and stores the result in a second energy field.
    /// If the energy fields have different numbers of channels, only the smallest of the two numbers of
    /// channels will be scaled. If the energy fields have different numbers of bins, only the smallest of the
    /// two numbers of bins will be scaled. This function can be used for in-place scaling, i.e. <c>out</c> may
    /// be equal to <c>in.</c>
    /// </summary>
    /// <param name="in">The input energy field.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="out">The output energy field.</param>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldScale")]
    public static partial void EnergyFieldScale(EnergyFieldHandle @in, float scalar, EnergyFieldHandle @out);

    /// <summary>
    /// Scales the values stored in an energy field by a scalar, and adds the result to a second energy field.
    /// If the energy fields have different numbers of channels, only the smallest of the two numbers of
    /// channels will be added. If the energy fields have different numbers of bins, only the smallest of the
    /// two numbers of bins will be added. This function can be used for in-place operation, i.e. <c>out</c> may
    /// be equal to <c>in.</c>
    /// </summary>
    /// <param name="in">The input energy field.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="out">The output energy field.</param>
    [LibraryImport(Library, EntryPoint = "iplEnergyFieldScaleAccum")]
    public static partial void EnergyFieldScaleAccum(EnergyFieldHandle @in, float scalar, EnergyFieldHandle @out);
}
