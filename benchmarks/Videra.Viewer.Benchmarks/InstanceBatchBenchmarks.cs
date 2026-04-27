using System.Numerics;
using BenchmarkDotNet.Attributes;
using Videra.Avalonia.Controls;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Videra.Core.Selection;

namespace Videra.Viewer.Benchmarks;

[MemoryDiagnoser]
public class InstanceBatchBenchmarks
{
    private readonly SceneHitTestService _hitTestService = new();

    private Object3D[] _normalObjects = Array.Empty<Object3D>();
    private InstanceBatchDescriptor _descriptor = null!;
    private SceneHitTestRequest _normalHitRequest = null!;
    private SceneHitTestRequest _batchHitRequest = null!;

    [Params(1000, 10000)]
    public int InstanceCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var material = new MaterialInstance(MaterialInstanceId.New(), "benchmark-material", RgbaFloat.White);
        var meshData = CreateTriangleMesh();
        var mesh = new MeshPrimitive(MeshPrimitiveId.New(), "benchmark-triangle", meshData, material.Id);
        var transforms = CreateTransforms(InstanceCount);
        var objectIds = Enumerable.Range(0, InstanceCount).Select(static _ => Guid.NewGuid()).ToArray();
        _descriptor = new InstanceBatchDescriptor(
            "benchmark-markers",
            mesh,
            material,
            transforms,
            objectIds: objectIds);
        var batch = SceneDocument.Empty.AddInstanceBatch(_descriptor).InstanceBatches[0];
        _normalObjects = CreateNormalObjects(InstanceCount, meshData, transforms);

        var camera = new OrbitCamera();
        camera.SetOrbit(new Vector3(0.5f, 0.5f, 0f), 10f, 0f, 0f);
        camera.UpdateProjection(800, 600);

        _normalHitRequest = new SceneHitTestRequest(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            _normalObjects);
        _batchHitRequest = new SceneHitTestRequest(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            [],
            [batch]);
    }

    [Benchmark]
    public int NormalObjects_SceneDocumentPopulation()
    {
        return new SceneDocument(_normalObjects).Entries.Count;
    }

    [Benchmark]
    public int InstanceBatch_SceneDocumentPopulation()
    {
        return SceneDocument.Empty.AddInstanceBatch(_descriptor).InstanceBatches[0].InstanceCount;
    }

    [Benchmark]
    public Guid NormalObjects_HitTestPickLatency()
    {
        return _hitTestService.HitTest(_normalHitRequest).PrimaryHit?.ObjectId ?? Guid.Empty;
    }

    [Benchmark]
    public int InstanceBatch_HitTestPickLatency()
    {
        return _hitTestService.HitTest(_batchHitRequest).PrimaryHit?.InstanceIndex ?? -1;
    }

    [Benchmark]
    public int InstanceBatch_DiagnosticsSnapshotEvidence()
    {
        var diagnostics = new VideraBackendDiagnostics
        {
            LastFrameUploadedBytes = 0,
            ResidentResourceCount = 0,
            ResidentResourceBytes = 0,
            PickableObjectCount = InstanceCount,
            InstanceBatchCount = 1,
            RetainedInstanceCount = InstanceCount
        };

        return VideraDiagnosticsSnapshotFormatter.Format(diagnostics).Length;
    }

    private static Object3D[] CreateNormalObjects(
        int count,
        MeshData meshData,
        IReadOnlyList<Matrix4x4> transforms)
    {
        var objects = new Object3D[count];
        for (var i = 0; i < count; i++)
        {
            var obj = new Object3D();
            obj.PrepareDeferredMesh(meshData);
            obj.Position = new Vector3(transforms[i].M41, transforms[i].M42, transforms[i].M43);
            objects[i] = obj;
        }

        return objects;
    }

    private static Matrix4x4[] CreateTransforms(int count)
    {
        var transforms = new Matrix4x4[count];
        transforms[0] = Matrix4x4.Identity;
        for (var i = 1; i < count; i++)
        {
            var x = 10f + ((i % 100) * 2f);
            var y = (i / 100) * 2f;
            transforms[i] = Matrix4x4.CreateTranslation(x, y, 0f);
        }

        return transforms;
    }

    private static MeshData CreateTriangleMesh()
    {
        return new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };
    }
}
