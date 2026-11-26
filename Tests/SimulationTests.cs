using SteamAudio;
using SteamAudio.Geometry;
using SteamAudio.Simulation;
using Xunit;
using N = SteamAudio.Interop;

public sealed class SimulationTests
{
    private sealed class Wall : IRayTracer
    {
        public int Calls;
        public bool Broken;
        public RayHit? ClosestHit(in N.Ray ray, float minimum, float maximum)
        {
            Interlocked.Increment(ref Calls);
            if (Broken)
            {
                throw new InvalidOperationException("geometry failure");
            }

            return null;
        }

        public bool AnyHit(in N.Ray ray, float minimum, float maximum)
        {
            Interlocked.Increment(ref Calls);
            if (Broken)
            {
                throw new InvalidOperationException("geometry failure");
            }

            return true;
        }
    }

    private static N.CoordinateSpace3 Space(float z = 0) => new()
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
        },
        Origin = new()
        {
            Z = z
        }
    };
    [Fact]
    public unsafe void CommittedGeometryAndMaterialChangesAlterSimulationResults()
    {
        using var context = new Context();
        using var scene = new Scene(
            context,
            new N.SceneSettings
            {
                Type = N.SceneType.Default
            });
        N.Vector3[] vertices = [new()
        {
            X = -2,
            Y = -2,
            Z = -1
        }, new()
        {
            X = 2,
            Y = -2,
            Z = -1
        }, new()
        {
            Y = 2,
            Z = -1
        }
        ];
        var triangle = new N.Triangle();
        triangle.Indices[1] = 1;
        triangle.Indices[2] = 2;
        var material = new N.Material();
        for (var i = 0; i < 3; i++)
        {
            material.Transmission[i] = .1f;
        }

        using var mesh = new StaticMesh(
            scene,
            vertices,
            new[]
            {
                triangle
            },
            new[]
            {
                0
            },
            new[]
            {
                material
            });
        mesh.Add();
        scene.Commit();
        using var simulator = new Simulator(
            context,
            new N.SimulationSettings
            {
                Flags = N.SimulationFlags.Direct,
                SceneType = N.SceneType.Default,
                MaxNumOcclusionSamples = 8,
                MaxNumSources = 1
            },
            scene);
        using var source = new Source(
            simulator,
            new N.SourceSettings
            {
                Flags = N.SimulationFlags.Direct
            });
        source.Add();
        simulator.SetSharedInputs(
            N.SimulationFlags.Direct,
            new()
            {
                Listener = Space()
            });
        simulator.Commit();
        source.SetInputs(new()
        {
            Flags = N.SimulationFlags.Direct,
            DirectFlags = N.DirectSimulationFlags.Occlusion | N.DirectSimulationFlags.Transmission,
            Source = Space(-2),
            OcclusionType = N.OcclusionType.Raycast,
            NumOcclusionSamples = 1,
            NumTransmissionRays = 4
        });
        simulator.RunDirect();
        var blocked = source.GetOutputs(N.SimulationFlags.Direct);
        Assert.Equal(0, blocked.Direct.Occlusion);
        var lowTransmission = blocked.Direct.Transmission[0];
        for (var i = 0; i < 3; i++)
        {
            material.Transmission[i] = .8f;
        }

        mesh.SetMaterial(0, material);
        scene.Commit();
        simulator.RunDirect();
        var updated = source.GetOutputs(N.SimulationFlags.Direct);
        Assert.True(updated.Direct.Transmission[0] > lowTransmission + .1f);
        mesh.Remove();
        scene.Commit();
        simulator.RunDirect();
        Assert.Equal(1, source.GetOutputs(N.SimulationFlags.Direct).Direct.Occlusion);
        source.Remove();
        simulator.Commit();
    }

    [Fact]
    public void RootedCustomRayCallbacksSurviveGcAndProduceOcclusion()
    {
        using var context = new Context();
        var tracer = new Wall();
        var scene = new Scene(context, tracer, new N.Material[1]);
        using var simulator = new Simulator(
            context,
            new N.SimulationSettings
            {
                Flags = N.SimulationFlags.Direct,
                SceneType = N.SceneType.Custom,
                MaxNumOcclusionSamples = 8,
                MaxNumSources = 1
            },
            scene);
        using var source = new Source(
            simulator,
            new N.SourceSettings
            {
                Flags = N.SimulationFlags.Direct
            });
        source.Add();
        simulator.SetSharedInputs(
            N.SimulationFlags.Direct,
            new()
            {
                Listener = Space()
            });
        simulator.Commit();
        source.SetInputs(new()
        {
            Flags = N.SimulationFlags.Direct,
            DirectFlags = N.DirectSimulationFlags.Occlusion,
            Source = Space(-2),
            OcclusionType = N.OcclusionType.Raycast,
            NumOcclusionSamples = 1
        });
        scene.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        simulator.RunDirect();
        Assert.True(tracer.Calls > 0);
        Assert.Equal(0, source.GetOutputs(N.SimulationFlags.Direct).Direct.Occlusion);
        tracer.Broken = true;
        Assert.Throws<InvalidOperationException>(() => simulator.RunDirect());
        source.Remove();
        simulator.Commit();
    }

    [Fact]
    public void ProbeBatchSerializationPreservesAuthoredProbes()
    {
        using var context = new Context();
        using var batch = new SteamAudio.Baking.ProbeBatch(context);
        batch.Add(new()
        {
            Center = new()
            {
                X = 1,
                Y = 2,
                Z = 3
            },
            Radius = 2
        });
        batch.Add(new()
        {
            Center = new()
            {
                X = -1
            },
            Radius = 1
        });
        batch.Commit();
        var bytes = batch.Save(context);
        Assert.True(bytes.Length > 32);
        using var restored = SteamAudio.Baking.ProbeBatch.Load(context, bytes);
        Assert.Equal(2, restored.Count);
    }

    [Fact]
    public unsafe void StaticMeshesValidateGeometryAndSupportMaterialUpdates()
    {
        using var context = new Context();
        using var scene = new Scene(
            context,
            new N.SceneSettings
            {
                Type = N.SceneType.Default
            });
        N.Vector3[] vertices = [new()
        {
            X = -2,
            Y = -2,
            Z = -1
        }, new()
        {
            X = 2,
            Y = -2,
            Z = -1
        }, new()
        {
            Y = 2,
            Z = -1
        }
        ];
        var triangle = new N.Triangle();
        triangle.Indices[0] = 0;
        triangle.Indices[1] = 1;
        triangle.Indices[2] = 2;
        using var mesh = new StaticMesh(
            scene,
            vertices,
            new[]
            {
                triangle
            },
            new[]
            {
                0
            },
            new N.Material[1]);
        mesh.Add();
        scene.Commit();
        var material = new N.Material
        {
            Scattering = .5f
        };
        material.Absorption[0] = .2f;
        mesh.SetMaterial(0, material);
        scene.Commit();
        var copiedMaterial = material;
        Assert.Throws<ArgumentOutOfRangeException>(() => mesh.SetMaterial(1, copiedMaterial));
        mesh.Remove();
        scene.Commit();
        triangle.Indices[2] = 99;
        var invalidTriangle = triangle;
        Assert.Throws<ArgumentException>(() => new StaticMesh(
                scene,
                vertices,
                new[]
                {
                    invalidTriangle
                },
                new[]
                {
                    0
                },
                new N.Material[1]
            ));
    }
}
