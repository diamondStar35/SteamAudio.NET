using System.Numerics;
using SteamAudio;
using SteamAudio.Effects;
using Xunit;
using N = SteamAudio.Interop;
public class DspTests
{
    [Fact] public void ABufferViewRetainsItsAllocationUntilTheViewEnds()
    {
        using var context = new Context(); var buffer = new AudioBuffer(context, 1, 8);
        using var view = buffer.Borrow(); buffer.Dispose(); view.Channel(0)[3] = .75f;
        Assert.Equal(.75f, view.Channel(0)[3]); Assert.Throws<ObjectDisposedException>(() => buffer.Clear());
    }
    [Fact] public void DirectEffectAppliesTheRequestedGainAfterItsRamp()
    {
        using var context = new Context(); using var effect = new Direct(context, new(48000, 512), new N.DirectEffectSettings { NumChannels = 1 });
        using var input = new AudioBuffer(context, 1, 512); using var output = new AudioBuffer(context, 1, 512);
        using (var view = input.Borrow()) view.Channel(0).Fill(.2f);
        var parameters = new N.DirectEffectParams { Flags = N.DirectEffectFlags.ApplyDistanceAttenuation, DistanceAttenuation = .25f };
        for (var i = 0; i < 4; i++) effect.Process(parameters, input, output);
        using var result = output.Borrow(); Assert.All(result.Channel(0).ToArray(), value => Assert.Equal(.05f, value, 5));
    }
    [Fact] public void PanningChangesChannelEnergyWithoutNeedingHrtf()
    {
        using var context = new Context(); using var effect = new Panning(context, new(48000, 512), new N.PanningEffectSettings { SpeakerLayout = new() { Type = N.SpeakerLayoutType.Stereo } });
        using var input = new AudioBuffer(context, 1, 512); using var output = new AudioBuffer(context, 2, 512);
        using (var view = input.Borrow()) view.Channel(0).Fill(.2f);
        for (var i = 0; i < 4; i++) effect.Process(new() { Direction = new() { X = -1 } }, input, output);
        using var result = output.Borrow(); var left = result.Channel(0).ToArray().Sum(x => x * x); var right = result.Channel(1).ToArray().Sum(x => x * x);
        Assert.True(left > .1f); Assert.True(left > right * 10);
    }
    [Fact] public void SerializedInputOwnsItsBytesBeyondCallerMutationAndGc()
    {
        using var context = new Context(); var input = Enumerable.Range(1, 64).Select(i => (byte)i).ToArray();
        using var storage = new SerializedObject(context, input); Array.Fill(input, (byte)99);
        GC.Collect(); GC.WaitForPendingFinalizers();
        Assert.Equal(Enumerable.Range(1, 64).Select(i => (byte)i), storage.ToArray());
    }
    [Fact] public void ReconstructionConvertsPositiveEnergyToANonSilentResponse()
    {
        using var context = new Context();
        using var field = new SteamAudio.Baking.EnergyField(context, new N.EnergyFieldSettings { Duration = .1f, Order = 0 });
        using var response = new SteamAudio.Baking.ImpulseResponse(context, new N.ImpulseResponseSettings { Duration = .1f, Order = 0, SamplingRate = 48000 });
        using var reconstructor = new SteamAudio.Baking.Reconstructor(context, new N.ReconstructorSettings { MaxDuration = .1f, MaxOrder = 0, SamplingRate = 48000 });
        using (var energy = field.Borrow()) energy.Samples.Fill(.1f);
        reconstructor.Reconstruct(field, response, .1f, 0);
        using var result = response.Borrow(); var values = result.Samples.ToArray();
        Assert.Contains(values, sample => Math.Abs(sample) > .000001); Assert.All(values, sample => Assert.True(float.IsFinite(sample)));
    }
    [Fact] public void InterleavingPreservesEveryChannelAndSample()
    {
        using var context = new Context(); using var buffer = new AudioBuffer(context, 2, 8);
        float[] samples = [1, 101, 2, 102, 3, 103, 4, 104, 5, 105, 6, 106, 7, 107, 8, 108];
        buffer.Deinterleave(samples); using (var view = buffer.Borrow()) { Assert.Equal(4, view.Channel(0)[3]); Assert.Equal(104, view.Channel(1)[3]); }
        var copy = new float[16]; buffer.Interleave(copy); Assert.Equal(samples, copy);
        Assert.Throws<ArgumentException>(() => buffer.Deinterleave(new float[15]));
    }
    [Fact] public void HrtfProducesDirectionalAudioAndItsTailDrains()
    {
        using var context = new Context(); var audio = new AudioSettings(48000, 512);
        using var hrtf = new Hrtf(context, audio); using var effect = new Binaural(context, hrtf);
        using var input = new AudioBuffer(context, 1, 512); using var output = new AudioBuffer(context, 2, 512);
        using (var samples = input.Borrow()) samples.Channel(0)[64] = .5f;
        effect.Process(-Vector3.UnitX, input, output); var left = new float[1024]; output.Interleave(left);
        effect.Reset(); effect.Process(Vector3.UnitX, input, output); var right = new float[1024]; output.Interleave(right);
        double Energy(float[] values, int channel) => values.Where((_, index) => index % 2 == channel).Sum(x => (double)x * x);
        Assert.True(Energy(left, 0) + Energy(left, 1) > .0001);
        Assert.True(Energy(left, 0) > Energy(left, 1)); Assert.True(Energy(right, 1) > Energy(right, 0));
        Assert.All(left.Concat(right), value => Assert.True(float.IsFinite(value)));
        var tail = N.AudioEffectState.TailRemaining; var blocks = 0;
        while (tail == N.AudioEffectState.TailRemaining && blocks++ < 32) tail = effect.GetTail(output);
        Assert.Equal(N.AudioEffectState.TailComplete, tail); Assert.Equal(0, effect.TailSize);
        effect.Reset(); input.Clear(); effect.Process(Vector3.UnitZ, input, output); output.Interleave(left); Assert.All(left, value => Assert.Equal(0, value));
    }
    [Fact] public void RetainedDependenciesSurviveTheirOriginalOwnersDisposal()
    {
        var context = new Context(); var audio = new AudioSettings(48000, 512); var hrtf = new Hrtf(context, audio);
        using var effect = new Binaural(context, hrtf); using var input = new AudioBuffer(context, 1, 512); using var output = new AudioBuffer(context, 2, 512);
        context.Dispose(); hrtf.Dispose(); GC.Collect(); GC.WaitForPendingFinalizers();
        using (var samples = input.Borrow()) samples.Channel(0)[16] = .5f;
        effect.Process(Vector3.UnitX, input, output); var values = new float[1024]; output.Interleave(values); Assert.Contains(values, value => Math.Abs(value) > .001);
        effect.Dispose(); Assert.Throws<ObjectDisposedException>(() => effect.Reset());
    }
    [Fact] public void BinauralProcessingAllocatesNothingAfterWarmup()
    {
        using var context = new Context(); using var hrtf = new Hrtf(context, new(48000, 512)); using var effect = new Binaural(context, hrtf);
        using var input = new AudioBuffer(context, 1, 512); using var output = new AudioBuffer(context, 2, 512);
        for (var i = 0; i < 16; i++) effect.Process(Vector3.UnitX, input, output);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++) effect.Process(Vector3.UnitX, input, output);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }
    [Fact] public void EnergyFieldsAndImpulseResponsesContainRealScalableData()
    {
        using var context = new Context();
        using var field = new SteamAudio.Baking.EnergyField(context, new N.EnergyFieldSettings { Duration = .1f, Order = 0 });
        using var copy = new SteamAudio.Baking.EnergyField(context, new N.EnergyFieldSettings { Duration = .1f, Order = 0 });
        using (var data = field.Borrow()) { Assert.True(data.Samples.Length >= 3); data.Samples[0] = 2; data.Samples[1] = 4; }
        field.Scale(.5f, copy); using (var data = copy.Borrow()) { Assert.Equal(1, data.Samples[0]); Assert.Equal(2, data.Samples[1]); }
        copy.Reset(); using (var data = copy.Borrow()) Assert.All(data.Samples.ToArray(), value => Assert.Equal(0, value));
        using var response = new SteamAudio.Baking.ImpulseResponse(context, new N.ImpulseResponseSettings { Duration = .1f, Order = 0, SamplingRate = 48000 });
        using (var data = response.Borrow()) { Assert.True(data.Samples.Length >= 4800); data.Samples[17] = 1; }
        response.Scale(.25f, response); using (var data = response.Borrow()) Assert.Equal(.25f, data.Samples[17]);
    }
}
