using Xunit;
using N = SteamAudio.Interop;
public sealed unsafe class AbiTests
{
    [Fact] public void NativeWidthsAndInlineArraysMatchTheSdkContract()
    {
        Assert.Equal(4, sizeof(N.Bool)); Assert.Equal(8, sizeof(N.ContextHandle));
        Assert.Equal(64, sizeof(N.Matrix4x4)); Assert.Equal(12, sizeof(N.Triangle));
        Assert.Equal(40, sizeof(N.ContextSettings)); Assert.Equal(40, sizeof(N.Hit));
        AbiMeasurements.Write(Path.Combine(AppContext.BaseDirectory, "managed-abi.json"));
    }
}
