using Xunit;
using N = SteamAudio.Interop;
public sealed unsafe class AbiTests
{
    [Fact] public void NativeWidthsAndInlineArraysMatchTheSdkContract()
    {
        Assert.Equal(4, sizeof(N.Bool)); Assert.Equal(8, sizeof(N.ContextHandle));
        Assert.Equal(64, sizeof(N.Matrix4x4)); Assert.Equal(12, sizeof(N.Triangle));
        Assert.Equal(40, sizeof(N.ContextSettings)); Assert.Equal(40, sizeof(N.Hit));
        var path = Path.Combine(AppContext.BaseDirectory, "managed-abi.json"); AbiMeasurements.Write(path);
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
        foreach (var structure in document.RootElement.GetProperty("structures").EnumerateObject())
        {
            var size = structure.Value.GetProperty("size").GetInt32();
            foreach (var field in structure.Value.GetProperty("fields").EnumerateObject())
                Assert.InRange(field.Value.GetInt32(), 0, size - 1);
        }
    }
}
