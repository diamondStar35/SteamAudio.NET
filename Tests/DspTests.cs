using System.Numerics;
using SteamAudio;
using SteamAudio.Effects;
using Xunit;
using N = SteamAudio.Interop;

public class DspTests
{
    [Theory]
    [InlineData(N.HrtfInterpolation.Nearest)]
    [InlineData(N.HrtfInterpolation.Bilinear)]
    public unsafe void BinauralInterpolationAndAlternateHrtfMatchTheNativeCall(N.HrtfInterpolation interpolation)
    {
        using var context = new Context();
        using var hrtf = new Hrtf(context, new(48000, 256));
        using var alternate = new Hrtf(context, new(48000, 256));
        using var effect = new Binaural(context, hrtf);
        using var reference = new Binaural(context, hrtf);
        using var input = new AudioBuffer(context, 1, 256);
        using var output = new AudioBuffer(context, 2, 256);
        using var expected = new AudioBuffer(context, 2, 256);
        using (var data = input.Borrow()) data.Channel(0)[0] = 1;
        var direction = Vector3.Normalize(new Vector3(.3f, .1f, -.7f));
        var parameters = new N.BinauralEffectParams
        {
            Direction = new() { X = direction.X, Y = direction.Y, Z = direction.Z },
            Hrtf = new(alternate.DangerousGetHandle()),
            SpatialBlend = .65f,
            Interpolation = interpolation
        };
        using (var source = input.Borrow())
        using (var destination = expected.Borrow())
            fixed (float* sourceSamples = source.Channel(0))
            fixed (float* left = destination.Channel(0))
            fixed (float* right = destination.Channel(1))
            {
                float** sourceChannels = stackalloc float*[1] { sourceSamples };
                float** destinationChannels = stackalloc float*[2] { left, right };
                var nativeInput = new N.AudioBuffer { NumChannels = 1, NumSamples = 256, Data = sourceChannels };
                var nativeOutput = new N.AudioBuffer { NumChannels = 2, NumSamples = 256, Data = destinationChannels };
                N.Api.BinauralEffectApply(new(reference.DangerousGetHandle()), &parameters, &nativeInput, &nativeOutput);
            }
        effect.Process(direction, input, output, .65f, interpolation, alternate);
        var actualPcm = new float[512]; var expectedPcm = new float[512];
        output.Interleave(actualPcm); expected.Interleave(expectedPcm);
        Assert.Contains(actualPcm, sample => Math.Abs(sample) > .00001f);
        for (var i = 0; i < actualPcm.Length; i++) Assert.Equal(expectedPcm[i], actualPcm[i], 6);
        Assert.Throws<ArgumentOutOfRangeException>(() => effect.Process(direction, input, output, 1, (N.HrtfInterpolation)99));
        using var incompatible = new Hrtf(context, new(44100, 256));
        Assert.Throws<ArgumentException>(() => effect.Process(direction, input, output, 1, interpolation, incompatible));
        effect.Reset(); reference.Reset();
        effect.Process(direction, input, output);
        reference.Process(direction, input, expected, 1, N.HrtfInterpolation.Bilinear);
        output.Interleave(actualPcm); expected.Interleave(expectedPcm);
        Assert.Equal(expectedPcm, actualPcm);
    }

    [Fact]
    public unsafe void SpatializedPathsSwitchBetweenStereoHeadphonesAndTheSpeakerLayout()
    {
        using var context = new Context();
        using var hrtf = new Hrtf(context, new(48000, 512));
        var handle = new N.HrtfHandle(hrtf.DangerousGetHandle());
        using var path = new SteamAudio.Effects.Path(
            context,
            new(48000, 512),
            new N.PathEffectSettings
            {
                MaxOrder = 0,
                Spatialize = N.Bool.True,
                SpeakerLayout = new()
                {
                    Type = N.SpeakerLayoutType.Quadraphonic
                },
                Hrtf = handle
            },
            hrtf);
        using var input = new AudioBuffer(context, 1, 512);
        using var headphones = new AudioBuffer(context, 2, 512);
        using var speakers = new AudioBuffer(context, 4, 512);
        using (var data = input.Borrow())
        {
            data.Channel(0).Fill(.2f);
        }

        var coefficient = 1f;
        var parameters = new N.PathEffectParams
        {
            ShCoeffs = &coefficient,
            Order = 0,
            Binaural = N.Bool.True,
            Hrtf = handle,
            Listener = new()
            {
                Right = new()
                {
                    X = 1
                },
                Up = new()
                {
                    Y = 1
                },
                Ahead = new()
                {
                    Z = -1
                }
            }
        };
        for (var i = 0; i < 3; i++)
        {
            parameters.EqCoeffs[i] = 1;
        }

        for (var i = 0; i < 4; i++)
        {
            path.Process(parameters, input, headphones);
        }

        using (var data = headphones.Borrow())
        {
            Assert.Contains(data.Channel(0).ToArray(), value => Math.Abs(value) > .00001f);
        }

        var complete = false;
        for (var i = 0; i < 64; i++)
        {
            if (path.GetTail(headphones) == N.AudioEffectState.TailComplete)
            {
                complete = true;
                break;
            }
        }

        Assert.True(complete);
        path.Reset();
        parameters.Binaural = N.Bool.False;
        for (var i = 0; i < 4; i++)
        {
            path.Process(parameters, input, speakers);
        }

        using (var data = speakers.Borrow())
        {
            for (var channel = 0; channel < 4; channel++)
            {
                Assert.Contains(
                    data.Channel(channel).ToArray(),
                    value => float.IsFinite(value) && Math.Abs(value) > .00001f);
            }
        }
    }

    [Fact]
    public void AmbisonicOrderCannotExceedTheAllocatedEffectDimensions()
    {
        using var context = new Context();
        using var effect = new AmbisonicsEncode(
            context,
            new(48000, 512),
            new N.AmbisonicsEncodeEffectSettings
            {
                MaxOrder = 1
            });
        using var input = new AudioBuffer(context, 1, 512);
        using var output = new AudioBuffer(context, 4, 512);
        Assert.Throws<ArgumentOutOfRangeException>(() => effect.Process(
                new()
                {
                    Order = 2,
                    Direction = new()
                    {
                        X = 1
                    }
                },
                input,
                output
            ));
        using (var samples = input.Borrow())
        {
            samples.Channel(0)[0] = .5f;
        }

        effect.Process(
            new()
            {
                Order = 1,
                Direction = new()
                {
                    X = 1
                }
            },
            input,
            output);
        using var result = output.Borrow();
        Assert.Contains(result.Channel(0).ToArray(), value => Math.Abs(value) > .001);
    }

    [Fact]
    public void ABufferViewRetainsItsAllocationUntilTheViewEnds()
    {
        using var context = new Context();
        var buffer = new AudioBuffer(context, 1, 8);
        using var view = buffer.Borrow();
        buffer.Dispose();
        view.Channel(0)[3] = .75f;
        Assert.Equal(.75f, view.Channel(0)[3]);
        Assert.Throws<ObjectDisposedException>(() => buffer.Clear());
    }

    [Fact]
    public void DirectEffectAppliesTheRequestedGainAfterItsRamp()
    {
        using var context = new Context();
        using var effect = new Direct(
            context,
            new(48000, 512),
            new N.DirectEffectSettings
            {
                NumChannels = 1
            });
        using var input = new AudioBuffer(context, 1, 512);
        using var output = new AudioBuffer(context, 1, 512);
        using (var view = input.Borrow())
        {
            view.Channel(0).Fill(.2f);
        }

        var parameters = new N.DirectEffectParams
        {
            Flags = N.DirectEffectFlags.ApplyDistanceAttenuation,
            DistanceAttenuation = .25f
        };
        for (var i = 0; i < 4; i++)
        {
            effect.Process(parameters, input, output);
        }

        using var result = output.Borrow();
        Assert.All(result.Channel(0).ToArray(), value => Assert.Equal(.05f, value, 5));
    }

    [Fact]
    public void PanningChangesChannelEnergyWithoutNeedingHrtf()
    {
        using var context = new Context();
        using var effect = new Panning(
            context,
            new(48000, 512),
            new N.PanningEffectSettings
            {
                SpeakerLayout = new()
                {
                    Type = N.SpeakerLayoutType.Stereo
                }
            });
        using var input = new AudioBuffer(context, 1, 512);
        using var output = new AudioBuffer(context, 2, 512);
        using (var view = input.Borrow())
        {
            view.Channel(0).Fill(.2f);
        }

        for (var i = 0; i < 4; i++)
        {
            effect.Process(
                new()
                {
                    Direction = new()
                    {
                        X = -1
                    }
                },
                input,
                output);
        }

        using var result = output.Borrow();
        var left = result.Channel(0).ToArray().Sum(x => x * x);
        var right = result.Channel(1).ToArray().Sum(x => x * x);
        Assert.True(left > .1f);
        Assert.True(left > right * 10);
    }

    [Fact]
    public void SerializedInputOwnsItsBytesBeyondCallerMutationAndGc()
    {
        using var context = new Context();
        var input = Enumerable.Range(1, 64).Select(i => (byte)i).ToArray();
        using var storage = new SerializedObject(context, input);
        Array.Fill(input, (byte)99);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Assert.Equal(Enumerable.Range(1, 64).Select(i => (byte)i), storage.ToArray());
    }

    [Fact]
    public void ReconstructionConvertsPositiveEnergyToANonSilentResponse()
    {
        using var context = new Context();
        using var field = new SteamAudio.Baking.EnergyField(
            context,
            new N.EnergyFieldSettings
            {
                Duration = .1f,
                Order = 0
            });
        using var response = new SteamAudio.Baking.ImpulseResponse(
            context,
            new N.ImpulseResponseSettings
            {
                Duration = .1f,
                Order = 0,
                SamplingRate = 48000
            });
        using var reconstructor = new SteamAudio.Baking.Reconstructor(
            context,
            new N.ReconstructorSettings
            {
                MaxDuration = .1f,
                MaxOrder = 0,
                SamplingRate = 48000
            });
        using (var energy = field.Borrow())
        {
            energy.Samples.Fill(.1f);
        }

        reconstructor.Reconstruct(field, response, .1f, 0);
        using var result = response.Borrow();
        var values = result.Samples.ToArray();
        Assert.Contains(values, sample => Math.Abs(sample) > .000001);
        Assert.All(values, sample => Assert.True(float.IsFinite(sample)));
    }

    [Fact]
    public void InterleavingPreservesEveryChannelAndSample()
    {
        using var context = new Context();
        using var buffer = new AudioBuffer(context, 2, 8);
        float[] samples = [1, 101, 2, 102, 3, 103, 4, 104, 5, 105, 6, 106, 7, 107, 8, 108];
        buffer.Deinterleave(samples);
        using (var view = buffer.Borrow())
        {
            Assert.Equal(4, view.Channel(0)[3]);
            Assert.Equal(104, view.Channel(1)[3]);
        }

        var copy = new float[16];
        buffer.Interleave(copy);
        Assert.Equal(samples, copy);
        Assert.Throws<ArgumentException>(() => buffer.Deinterleave(new float[15]));
    }

    [Fact]
    public void HrtfProducesDirectionalAudioAndItsTailDrains()
    {
        using var context = new Context();
        var audio = new AudioSettings(48000, 512);
        using var hrtf = new Hrtf(context, audio);
        using var effect = new Binaural(context, hrtf);
        using var input = new AudioBuffer(context, 1, 512);
        using var output = new AudioBuffer(context, 2, 512);
        using (var samples = input.Borrow())
        {
            samples.Channel(0)[64] = .5f;
        }

        effect.Process(-Vector3.UnitX, input, output);
        var left = new float[1024];
        output.Interleave(left);
        effect.Reset();
        effect.Process(Vector3.UnitX, input, output);
        var right = new float[1024];
        output.Interleave(right);
        double Energy(float[] values, int channel) => values.Where((_, index) => index % 2 == channel).Sum(x => (double)x * x);
        Assert.True(Energy(left, 0) + Energy(left, 1) > .0001);
        Assert.True(Energy(left, 0) > Energy(left, 1));
        Assert.True(Energy(right, 1) > Energy(right, 0));
        Assert.All(left.Concat(right), value => Assert.True(float.IsFinite(value)));
        var tail = N.AudioEffectState.TailRemaining;
        var blocks = 0;
        while (tail == N.AudioEffectState.TailRemaining && blocks++ < 32)
        {
            tail = effect.GetTail(output);
        }

        Assert.Equal(N.AudioEffectState.TailComplete, tail);
        Assert.Equal(0, effect.TailSize);
        effect.Reset();
        input.Clear();
        effect.Process(Vector3.UnitZ, input, output);
        output.Interleave(left);
        Assert.All(left, value => Assert.Equal(0, value));
    }

    [Fact]
    public void RetainedDependenciesSurviveTheirOriginalOwnersDisposal()
    {
        var context = new Context();
        var audio = new AudioSettings(48000, 512);
        var hrtf = new Hrtf(context, audio);
        using var effect = new Binaural(context, hrtf);
        using var input = new AudioBuffer(context, 1, 512);
        using var output = new AudioBuffer(context, 2, 512);
        context.Dispose();
        hrtf.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        using (var samples = input.Borrow())
        {
            samples.Channel(0)[16] = .5f;
        }

        effect.Process(Vector3.UnitX, input, output);
        var values = new float[1024];
        output.Interleave(values);
        Assert.Contains(values, value => Math.Abs(value) > .001);
        effect.Dispose();
        Assert.Throws<ObjectDisposedException>(() => effect.Reset());
    }

    [Fact]
    public void BinauralProcessingAllocatesNothingAfterWarmup()
    {
        using var context = new Context();
        using var hrtf = new Hrtf(context, new(48000, 512));
        using var effect = new Binaural(context, hrtf);
        using var input = new AudioBuffer(context, 1, 512);
        using var output = new AudioBuffer(context, 2, 512);
        for (var i = 0; i < 16; i++)
        {
            effect.Process(Vector3.UnitX, input, output);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++)
        {
            effect.Process(Vector3.UnitX, input, output);
        }

        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    [Fact]
    public void EnergyFieldsAndImpulseResponsesContainRealScalableData()
    {
        using var context = new Context();
        using var field = new SteamAudio.Baking.EnergyField(
            context,
            new N.EnergyFieldSettings
            {
                Duration = .1f,
                Order = 0
            });
        using var copy = new SteamAudio.Baking.EnergyField(
            context,
            new N.EnergyFieldSettings
            {
                Duration = .1f,
                Order = 0
            });
        using (var data = field.Borrow())
        {
            Assert.True(data.Samples.Length >= 3);
            data.Samples[0] = 2;
            data.Samples[1] = 4;
        }

        field.Scale(.5f, copy);
        using (var data = copy.Borrow())
        {
            Assert.Equal(1, data.Samples[0]);
            Assert.Equal(2, data.Samples[1]);
        }

        copy.Reset();
        using (var data = copy.Borrow())
        {
            Assert.All(data.Samples.ToArray(), value => Assert.Equal(0, value));
        }

        using var response = new SteamAudio.Baking.ImpulseResponse(
            context,
            new N.ImpulseResponseSettings
            {
                Duration = .1f,
                Order = 0,
                SamplingRate = 48000
            });
        using (var data = response.Borrow())
        {
            Assert.True(data.Samples.Length >= 4800);
            data.Samples[17] = 1;
        }

        response.Scale(.25f, response);
        using (var data = response.Borrow())
        {
            Assert.Equal(.25f, data.Samples[17]);
        }
    }
}
