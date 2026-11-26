using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SteamAudio;
using SteamAudio.Effects;
using N = SteamAudio.Interop;
internal static class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(N.Api))]
    private static void RootAllNativeImports() { }
    public static void Main(string[] args)
    {
        RootAllNativeImports();
        if (args is ["--abi", var file]) { AbiMeasurements.Write(file); return; }
        var destination = args.FirstOrDefault() ?? "steamaudio-offline.wav";
        using var context = new Context(); var settings = new AudioSettings(48000, 512);
        using var hrtf = new Hrtf(context, settings); using var effect = new Binaural(context, hrtf);
        using var input = new AudioBuffer(context, 1, 512); using var output = new AudioBuffer(context, 2, 512);
        double leftEnergy = 0, rightEnergy = 0;
        var samples = new List<float>(); var block = new float[1024];
        for (var index = 0; index < 96; index++)
        {
            using (var view = input.Borrow()) for (var i = 0; i < 512; i++) view.Channel(0)[i] = .15f * MathF.Sin((index * 512 + i) * .071f);
            effect.Process(index < 48 ? -Vector3.UnitX : Vector3.UnitX, input, output);
            output.Interleave(block); samples.AddRange(block);
            for (var i = 0; i < block.Length; i += 2)
            {
                if (!float.IsFinite(block[i]) || !float.IsFinite(block[i + 1])) throw new Exception("Non-finite HRTF output");
                if (index < 48) { leftEnergy += block[i] * block[i]; rightEnergy += block[i + 1] * block[i + 1]; }
            }
        }
        if (leftEnergy <= rightEnergy || rightEnergy <= 0) throw new Exception("Directional HRTF output check failed");
        for (var i = 0; i < 64; i++)
        {
            var state = effect.GetTail(output); output.Interleave(block); samples.AddRange(block);
            if (state == N.AudioEffectState.TailComplete) break;
            if (i == 63) throw new Exception("HRTF tail did not terminate");
        }
        using (var energy = new SteamAudio.Baking.EnergyField(context, new N.EnergyFieldSettings { Duration = .1f, Order = 0 }))
        using (var response = new SteamAudio.Baking.ImpulseResponse(context, new N.ImpulseResponseSettings { Duration = .1f, Order = 0, SamplingRate = 48000 }))
        using (var reconstructor = new SteamAudio.Baking.Reconstructor(context, new N.ReconstructorSettings { MaxDuration = .1f, MaxOrder = 0, SamplingRate = 48000 }))
        {
            using (var energyValues = energy.Borrow()) energyValues.Samples.Fill(.1f);
            reconstructor.Reconstruct(energy, response, .1f, 0);
            using var values = response.Borrow(); double total = 0; foreach (var sample in values.Samples) { if (!float.IsFinite(sample)) throw new Exception("Non-finite IR"); total += Math.Abs(sample); }
            if (total <= 0) throw new Exception("Reconstruction returned silence");
        }
        using var writer = new BinaryWriter(File.Create(destination));
        writer.Write("RIFF"u8); writer.Write(36 + samples.Count * 4); writer.Write("WAVEfmt "u8); writer.Write(16);
        writer.Write((short)3); writer.Write((short)2); writer.Write(48000); writer.Write(48000 * 8); writer.Write((short)8); writer.Write((short)32);
        writer.Write("data"u8); writer.Write(samples.Count * 4); foreach (var sample in samples) writer.Write(sample);
        Console.WriteLine($"Native verification passed: HRTF left/right energy {leftEnergy:F6}/{rightEnergy:F6}, tail drained, reconstructed IR is non-silent. Wrote {destination}");
    }
}
