using SteamAudio;
using SteamAudio.Baking;
using SteamAudio.Geometry;
using Xunit;
using N = SteamAudio.Interop;
public sealed unsafe class BakingTests
{
    private static StaticMesh Room(Scene scene)
    {
        N.Vector3[] vertices = [new() { X=-2,Y=-2,Z=-2 },new() { X=2,Y=-2,Z=-2 },new() { X=2,Y=2,Z=-2 },new() { X=-2,Y=2,Z=-2 },new() { X=-2,Y=-2,Z=2 },new() { X=2,Y=-2,Z=2 },new() { X=2,Y=2,Z=2 },new() { X=-2,Y=2,Z=2 }];
        int[] indices = [0,1,2,0,2,3,4,6,5,4,7,6,0,5,1,0,4,5,3,2,6,3,6,7,0,3,7,0,7,4,1,5,6,1,6,2];
        var triangles = new N.Triangle[12]; for(var t=0;t<12;t++) for(var i=0;i<3;i++) triangles[t].Indices[i]=indices[t*3+i];
        var material = new N.Material { Scattering=.2f }; for(var i=0;i<3;i++) material.Absorption[i]=.2f;
        return new(scene,vertices,triangles,new int[12],new[]{material});
    }
    [Fact] public void ReflectionBakeProducesMeasuredEnergyAndSerializableProbeData()
    {
        using var context = new Context(); using var scene = new Scene(context,new N.SceneSettings { Type=N.SceneType.Default });
        using var mesh = Room(scene); mesh.Add(); scene.Commit();
        using var probes = new ProbeBatch(context); probes.Add(new() { Radius=1 }); probes.Commit();
        var identifier = new N.BakedDataIdentifier { Type=N.BakedDataType.Reflections,Variation=N.BakedDataVariation.Reverb };
        var progressCalls=0;
        Baker.Reflections(context,scene,probes,new N.ReflectionsBakeParams { SceneType=N.SceneType.Default, Identifier=identifier, BakeFlags=N.ReflectionsBakeFlags.BakeConvolution|N.ReflectionsBakeFlags.BakeParametric, NumRays=128, NumDiffuseSamples=16, NumBounces=4, SimulatedDuration=.1f, SavedDuration=.1f, Order=0, NumThreads=1, RayBatchSize=16, IrradianceMinDistance=.1f, BakeBatchSize=1 }, _=>Interlocked.Increment(ref progressCalls));
        using var field = new EnergyField(context,new N.EnergyFieldSettings { Duration=.1f,Order=0 }); probes.GetEnergyField(identifier,0,field);
        using(var data=field.Borrow()) Assert.Contains(data.Samples.ToArray(),value=>value>0);
        var reverb=new float[3]; probes.GetReverb(identifier,0,reverb); Assert.All(reverb,value=>Assert.True(float.IsFinite(value)&&value>0));
        Assert.True(progressCalls>0); var bytes=probes.Save(context); using var restored=ProbeBatch.Load(context,bytes);
        using var copy=new EnergyField(context,new N.EnergyFieldSettings { Duration=.1f,Order=0 });restored.GetEnergyField(identifier,0,copy);
        using var original=field.Borrow();using var roundtrip=copy.Borrow();Assert.Equal(original.Samples.ToArray(),roundtrip.Samples.ToArray());
    }
    [Fact] public void PreCanceledBakingDoesNotEnterNativeCode()
    {
        using var context = new Context();using var scene=new Scene(context,new N.SceneSettings { Type=N.SceneType.Default });using var probes=new ProbeBatch(context);
        Assert.Throws<OperationCanceledException>(()=>Baker.Reflections(context,scene,probes,default,cancellationToken:new CancellationToken(true)));
        Assert.Equal(0,probes.Count);
    }
}
