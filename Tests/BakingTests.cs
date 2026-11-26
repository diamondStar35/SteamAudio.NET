using SteamAudio;
using SteamAudio.Baking;
using SteamAudio.Geometry;
using Xunit;
using N = SteamAudio.Interop;

public sealed unsafe class BakingTests
{
    private static StaticMesh Room(Scene scene)
    {
        N.Vector3[] vertices = [new()
        {
            X = -2,
            Y = -2,
            Z = -2
        }, new()
        {
            X = 2,
            Y = -2,
            Z = -2
        }, new()
        {
            X = 2,
            Y = 2,
            Z = -2
        }, new()
        {
            X = -2,
            Y = 2,
            Z = -2
        }, new()
        {
            X = -2,
            Y = -2,
            Z = 2
        }, new()
        {
            X = 2,
            Y = -2,
            Z = 2
        }, new()
        {
            X = 2,
            Y = 2,
            Z = 2
        }, new()
        {
            X = -2,
            Y = 2,
            Z = 2
        }
        ];
        int[] indices = [0, 1, 2, 0, 2, 3, 4, 6, 5, 4, 7, 6, 0, 5, 1, 0, 4, 5, 3, 2, 6, 3, 6, 7, 0, 3, 7, 0, 7, 4, 1, 5, 6, 1, 6, 2];
        var triangles = new N.Triangle[12];
        for (var t = 0; t < 12; t++)
        {
            for (var i = 0; i < 3; i++)
            {
                triangles[t].Indices[i] = indices[t * 3 + i];
            }
        }

        var material = new N.Material
        {
            Scattering = .2f
        };
        for (var i = 0; i < 3; i++)
        {
            material.Absorption[i] = .2f;
        }

        return new(
            scene,
            vertices,
            triangles,
            new int[12],
            new[]
            {
                material
            });
    }

    [Fact]
    public void BakedPathsProduceUsableSimulationCoefficients()
    {
        using var context = new Context();
        using var scene = new Scene(
            context,
            new N.SceneSettings
            {
                Type = N.SceneType.Default
            });
        using var mesh = Room(scene);
        mesh.Add();
        scene.Commit();
        using var probes = new ProbeBatch(context);
        probes.Add(new()
        {
            Center = new()
            {
                X = -1
            },
            Radius = 2
        });
        probes.Add(new()
        {
            Center = new()
            {
                X = 1
            },
            Radius = 2
        });
        probes.Commit();
        var identifier = new N.BakedDataIdentifier
        {
            Type = N.BakedDataType.Pathing,
            Variation = N.BakedDataVariation.Dynamic
        };
        Baker.Paths(
            context,
            scene,
            probes,
            new N.PathBakeParams
            {
                Identifier = identifier,
                NumSamples = 4,
                Radius = .1f,
                Threshold = .5f,
                VisRange = 4,
                PathRange = 8,
                NumThreads = 1
            });
        var batch = new N.ProbeBatchHandle(probes.DangerousGetHandle());
        Assert.True(N.Api.ProbeBatchGetDataSize(batch, &identifier) > 0);
        using var simulation = new SteamAudio.Simulation.Simulator(
            context,
            new N.SimulationSettings
            {
                Flags = N.SimulationFlags.Pathing,
                SceneType = N.SceneType.Default,
                MaxOrder = 0,
                MaxNumSources = 1,
                NumThreads = 1,
                NumVisSamples = 4,
                SamplingRate = 48000,
                FrameSize = 512
            },
            scene,
            probes);
        simulation.AddProbeBatch(probes);
        using var source = new SteamAudio.Simulation.Source(
            simulation,
            new N.SourceSettings
            {
                Flags = N.SimulationFlags.Pathing
            },
            probes);
        source.Add();
        simulation.Commit();
        simulation.SetSharedInputs(
            N.SimulationFlags.Pathing,
            new()
            {
                Listener = new()
                {
                    Origin = new()
                    {
                        X = 1
                    },
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
                },
                Order = 0
            });
        source.SetInputs(new()
        {
            Flags = N.SimulationFlags.Pathing,
            Source = new()
            {
                Origin = new()
                {
                    X = -1
                },
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
            },
            PathingProbes = batch,
            PathingOrder = 0,
            VisRadius = .1f,
            VisThreshold = .5f,
            VisRange = 4
        });
        simulation.RunPathing();
        var output = source.GetOutputs(N.SimulationFlags.Pathing);
        Assert.True(output.Pathing.ShCoeffs != null);
        Assert.True(float.IsFinite(output.Pathing.ShCoeffs[0]));
        Assert.True(Math.Abs(output.Pathing.ShCoeffs[0]) > .000001f);
        source.Remove();
        simulation.RemoveProbeBatch(probes);
        simulation.Commit();
    }

    [Fact]
    public void ReflectionBakeProducesMeasuredEnergyAndSerializableProbeData()
    {
        using var context = new Context();
        using var scene = new Scene(
            context,
            new N.SceneSettings
            {
                Type = N.SceneType.Default
            });
        using var mesh = Room(scene);
        mesh.Add();
        scene.Commit();
        using var probes = new ProbeBatch(context);
        probes.Add(new()
        {
            Radius = 1
        });
        probes.Commit();
        var identifier = new N.BakedDataIdentifier
        {
            Type = N.BakedDataType.Reflections,
            Variation = N.BakedDataVariation.Reverb
        };
        var progressCalls = 0;
        Baker.Reflections(
            context,
            scene,
            probes,
            new N.ReflectionsBakeParams
            {
                SceneType = N.SceneType.Default,
                Identifier = identifier,
                BakeFlags = N.ReflectionsBakeFlags.BakeConvolution | N.ReflectionsBakeFlags.BakeParametric,
                NumRays = 128,
                NumDiffuseSamples = 16,
                NumBounces = 4,
                SimulatedDuration = .1f,
                SavedDuration = .1f,
                Order = 0,
                NumThreads = 1,
                RayBatchSize = 16,
                IrradianceMinDistance = .1f,
                BakeBatchSize = 1
            },
            _ => Interlocked.Increment(ref progressCalls));
        using var field = new EnergyField(
            context,
            new N.EnergyFieldSettings
            {
                Duration = .1f,
                Order = 0
            });
        probes.GetEnergyField(identifier, 0, field);
        using (var data = field.Borrow())
        {
            Assert.Contains(data.Samples.ToArray(), value => value > 0);
        }

        var reverb = new float[3];
        probes.GetReverb(identifier, 0, reverb);
        Assert.All(reverb, value => Assert.True(float.IsFinite(value) && value > 0));
        Assert.True(progressCalls > 0);
        var bytes = probes.Save(context);
        using var restored = ProbeBatch.Load(context, bytes);
        using var copy = new EnergyField(
            context,
            new N.EnergyFieldSettings
            {
                Duration = .1f,
                Order = 0
            });
        restored.GetEnergyField(identifier, 0, copy);
        using var original = field.Borrow();
        using var roundtrip = copy.Borrow();
        Assert.Equal(original.Samples.ToArray(), roundtrip.Samples.ToArray());
    }

    [Fact]
    public void SimulatedReflectionsRenderThroughTheSharedMixer()
    {
        using var context = new Context();
        using var scene = new Scene(
            context,
            new N.SceneSettings
            {
                Type = N.SceneType.Default
            });
        using var mesh = Room(scene);
        mesh.Add();
        scene.Commit();
        using var simulator = new SteamAudio.Simulation.Simulator(
            context,
            new N.SimulationSettings
            {
                Flags = N.SimulationFlags.Reflections,
                SceneType = N.SceneType.Default,
                ReflectionType = N.ReflectionEffectType.Convolution,
                MaxNumRays = 128,
                NumDiffuseSamples = 16,
                MaxDuration = .1f,
                MaxOrder = 0,
                MaxNumSources = 1,
                NumThreads = 1,
                RayBatchSize = 16,
                SamplingRate = 48000,
                FrameSize = 512
            },
            scene);
        using var source = new SteamAudio.Simulation.Source(
            simulator,
            new N.SourceSettings
            {
                Flags = N.SimulationFlags.Reflections
            });
        source.Add();
        simulator.Commit();
        var basis = new N.CoordinateSpace3
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
        };
        simulator.SetSharedInputs(
            N.SimulationFlags.Reflections,
            new()
            {
                Listener = basis,
                NumRays = 128,
                NumBounces = 4,
                Duration = .1f,
                Order = 0,
                IrradianceMinDistance = .1f
            });
        basis.Origin.X = 1;
        source.SetInputs(new()
        {
            Flags = N.SimulationFlags.Reflections,
            Source = basis
        });
        simulator.RunReflections();
        var parameters = source.GetOutputs(N.SimulationFlags.Reflections).Reflections;
        Assert.NotEqual(0, parameters.Ir.Value);
        var settings = new N.ReflectionEffectSettings
        {
            Type = N.ReflectionEffectType.Convolution,
            NumChannels = 1,
            IrSize = 4800
        };
        using var effect = new SteamAudio.Effects.Reflection(context, new(48000, 512), settings, simulator, source);
        using var mixer = new SteamAudio.Effects.ReflectionMixer(context, new(48000, 512), settings);
        using var input = new AudioBuffer(context, 1, 512);
        using var output = new AudioBuffer(context, 1, 512);
        double energy = 0;
        for (var block = 0; block < 16; block++)
        {
            using (var data = input.Borrow())
            {
                data.Channel(0).Clear();
                if (block == 0)
                {
                    data.Channel(0)[0] = 1;
                }
            }

            effect.Process(parameters, input, output, mixer);
            mixer.Process(parameters, output);
            using var rendered = output.Borrow();
            foreach (var value in rendered.Channel(0))
            {
                Assert.True(float.IsFinite(value));
                energy += value * value;
            }
        }

        Assert.True(energy > .000001, $"Expected reflected audio energy, got {energy}.");
        mixer.Reset();
        effect.Reset();
        mixer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => effect.Process(parameters, input, output, mixer));
        source.Remove();
        simulator.Commit();
    }

    [Fact]
    public void PreCanceledBakingDoesNotEnterNativeCode()
    {
        using var context = new Context();
        using var scene = new Scene(
            context,
            new N.SceneSettings
            {
                Type = N.SceneType.Default
            });
        using var probes = new ProbeBatch(context);
        Assert.Throws<OperationCanceledException>(() => Baker.Reflections(context, scene, probes, default, cancellationToken: new CancellationToken(true)));
        Assert.Equal(0, probes.Count);
    }
}
