using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Runtime;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Inspection;
using Videra.Core.Selection;

namespace Videra.Viewer.Benchmarks;

[MemoryDiagnoser]
public class InspectionBenchmarks
{
    private readonly SceneHitTestService _hitTestService = new();
    private readonly VideraClipPayloadService _clipPayloadService = new();
    private readonly VideraSnapshotExportService _snapshotExportService = new();

    private SceneHitTestRequest _hitRequest = null!;
    private MeshPayload _clipSourcePayload = null!;
    private VideraClipPlane[] _clippingPlanes = null!;
    private string _workspace = string.Empty;
    private string _snapshotPath = string.Empty;
    private TrackingSoftwareBackend _snapshotBackend = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var sceneObject = new Object3D();
        sceneObject.PrepareDeferredMesh(CreateSlantedTriangleMesh());

        var camera = new OrbitCamera();
        camera.SetOrbit(Vector3.Zero, 10f, 0f, 0f);
        camera.UpdateProjection(800, 600);

        _hitRequest = new SceneHitTestRequest(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            [sceneObject]);

        _clipSourcePayload = MeshPayload.FromMesh(CreateDenseGridMesh(cellCount: 32), cloneArrays: true);
        _clippingPlanes =
        [
            VideraClipPlane.FromPointNormal(new Vector3(0.5f, 0f, 0.5f), Vector3.UnitX),
            VideraClipPlane.FromPointNormal(new Vector3(0f, 0f, 0.5f), Vector3.UnitZ)
        ];
        _ = _clipPayloadService.Clip(_clipSourcePayload, _clippingPlanes);

        _workspace = Path.Combine(Path.GetTempPath(), "Videra.Viewer.Benchmarks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workspace);
        _snapshotPath = Path.Combine(_workspace, "inspection-fast-path.png");
        _snapshotBackend = new TrackingSoftwareBackend(width: 256, height: 256);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (!string.IsNullOrWhiteSpace(_workspace) && Directory.Exists(_workspace))
        {
            Directory.Delete(_workspace, recursive: true);
        }
    }

    [Benchmark]
    public float SceneHitTest_MeshAccurateDistance()
    {
        return _hitTestService.HitTest(_hitRequest).PrimaryHit!.Distance;
    }

    [Benchmark]
    public int ClipPayload_CachedPlaneSignature()
    {
        return _clipPayloadService.Clip(_clipSourcePayload, _clippingPlanes)?.Indices.Length ?? 0;
    }

    [Benchmark]
    public async Task<long> SnapshotExport_LiveReadbackFastPath()
    {
        using var engine = new VideraEngine();
        await _snapshotExportService.ExportAsync(
            _snapshotPath,
            width: 256,
            height: 256,
            engine,
            sceneObjects: Array.Empty<Object3D>(),
            selectionState: new VideraSelectionState(),
            annotations: Array.Empty<VideraAnnotation>(),
            measurements: Array.Empty<VideraMeasurement>(),
            overlayState: VideraViewOverlayState.Empty,
            preferredReadbackBackend: _snapshotBackend,
            logger: NullLogger.Instance,
            cancellationToken: CancellationToken.None).ConfigureAwait(false);

        return new FileInfo(_snapshotPath).Length;
    }

    private static MeshData CreateSlantedTriangleMesh()
    {
        return new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-1f, -1f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(1f, -1f, 2f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 1f, 2f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };
    }

    private static MeshData CreateDenseGridMesh(int cellCount)
    {
        var vertices = new List<VertexPositionNormalColor>();
        var indices = new List<uint>();

        for (var z = 0; z <= cellCount; z++)
        {
            for (var x = 0; x <= cellCount; x++)
            {
                vertices.Add(new VertexPositionNormalColor(
                    new Vector3(x / (float)cellCount, 0f, z / (float)cellCount),
                    Vector3.UnitY,
                    RgbaFloat.White));
            }
        }

        var stride = cellCount + 1;
        for (var z = 0; z < cellCount; z++)
        {
            for (var x = 0; x < cellCount; x++)
            {
                var topLeft = (uint)((z * stride) + x);
                var topRight = topLeft + 1u;
                var bottomLeft = (uint)(((z + 1) * stride) + x);
                var bottomRight = bottomLeft + 1u;

                indices.Add(topLeft);
                indices.Add(bottomLeft);
                indices.Add(topRight);
                indices.Add(topRight);
                indices.Add(bottomLeft);
                indices.Add(bottomRight);
            }
        }

        return new MeshData
        {
            Vertices = vertices.ToArray(),
            Indices = indices.ToArray(),
            Topology = MeshTopology.Triangles
        };
    }

    private sealed class TrackingSoftwareBackend(int width, int height) : ISoftwareBackend
    {
        public int Width { get; } = width;

        public int Height { get; } = height;

        public void CopyFrameTo(IntPtr destination, int destinationStride)
        {
            for (var y = 0; y < Height; y++)
            {
                var row = new byte[destinationStride];
                for (var x = 0; x < Width; x++)
                {
                    var offset = x * 4;
                    row[offset + 0] = 0x22;
                    row[offset + 1] = 0x44;
                    row[offset + 2] = 0x88;
                    row[offset + 3] = 0xFF;
                }

                Marshal.Copy(row, 0, destination + (y * destinationStride), destinationStride);
            }
        }
    }
}
