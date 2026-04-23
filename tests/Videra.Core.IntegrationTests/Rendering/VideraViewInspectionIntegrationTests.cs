using System.Numerics;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Inspection;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraViewInspectionIntegrationTests
{
    [Fact]
    public void ClippingPlanes_ShouldUpdateSceneTruthAndDiagnostics()
    {
        var view = new VideraView();
        try
        {
            var sceneObject = new Object3D { Name = "ClipTest" };
            sceneObject.PrepareDeferredMesh(new MeshData
            {
                Vertices =
                [
                    new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
                ],
                Indices = [0u, 1u, 2u],
                Topology = MeshTopology.Triangles
            });

            view.AddObject(sceneObject);
            sceneObject.LocalBounds.Should().NotBeNull();
            sceneObject.LocalBounds!.Value.Min.X.Should().Be(0f);

            view.ClippingPlanes =
            [
                VideraClipPlane.FromPointNormal(new Vector3(0.5f, 0f, 0f), Vector3.UnitX)
            ];

            sceneObject.LocalBounds!.Value.Min.X.Should().BeGreaterThan(0.49f);
            view.BackendDiagnostics.IsClippingActive.Should().BeTrue();
            view.BackendDiagnostics.ActiveClippingPlaneCount.Should().Be(1);

            view.ClippingPlanes = Array.Empty<VideraClipPlane>();

            sceneObject.LocalBounds!.Value.Min.X.Should().Be(0f);
            view.BackendDiagnostics.IsClippingActive.Should().BeFalse();
            view.BackendDiagnostics.ActiveClippingPlaneCount.Should().Be(0);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void CaptureAndApplyInspectionState_ShouldRoundTripCameraSelectionAndInspectionTruth()
    {
        var view = new VideraView();
        try
        {
            var sceneObject = new Object3D { Name = "InspectionState" };
            sceneObject.PrepareDeferredMesh(new MeshData
            {
                Vertices =
                [
                    new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
                ],
                Indices = [0u, 1u, 2u],
                Topology = MeshTopology.Triangles
            });

            view.AddObject(sceneObject);
            view.SelectionState = new VideraSelectionState
            {
                ObjectIds = [sceneObject.Id],
                PrimaryObjectId = sceneObject.Id
            };
            view.Annotations =
            [
                new VideraNodeAnnotation
                {
                    ObjectId = sceneObject.Id,
                    Text = "Inspection note"
                }
            ];
            view.Measurements =
            [
                new VideraMeasurement
                {
                    Label = "Edge",
                    Start = VideraMeasurementAnchor.ForObjectPoint(sceneObject.Id, new Vector3(0f, 0f, 0f)),
                    End = VideraMeasurementAnchor.ForWorldPoint(new Vector3(0f, 2f, 0f))
                }
            ];
            view.InteractionOptions = new VideraInteractionOptions
            {
                MeasurementSnapMode = VideraMeasurementSnapMode.EdgeMidpoint
            };
            view.ClippingPlanes =
            [
                VideraClipPlane.FromPointNormal(new Vector3(0.5f, 0f, 0f), Vector3.UnitX)
            ];
            view.Engine.Camera.SetOrbit(new Vector3(2f, 3f, 4f), 7f, 0.8f, 0.25f);

            var snapshot = view.CaptureInspectionState();

            view.SelectionState = new VideraSelectionState();
            view.Annotations = Array.Empty<VideraAnnotation>();
            view.Measurements = Array.Empty<VideraMeasurement>();
            view.InteractionOptions = new VideraInteractionOptions
            {
                MeasurementSnapMode = VideraMeasurementSnapMode.Free
            };
            view.ClippingPlanes = Array.Empty<VideraClipPlane>();
            view.Engine.Camera.Reset();

            view.ApplyInspectionState(snapshot);

            view.SelectionState.ObjectIds.Should().ContainSingle().Which.Should().Be(sceneObject.Id);
            view.SelectionState.PrimaryObjectId.Should().Be(sceneObject.Id);
            view.Annotations.Should().ContainSingle();
            view.Annotations.OfType<VideraNodeAnnotation>().Single().ObjectId.Should().Be(sceneObject.Id);
            view.Annotations[0].Text.Should().Be("Inspection note");
            view.Measurements.Should().ContainSingle();
            view.Measurements[0].Label.Should().Be("Edge");
            view.InteractionOptions.MeasurementSnapMode.Should().Be(VideraMeasurementSnapMode.EdgeMidpoint);
            view.ClippingPlanes.Should().ContainSingle();
            view.Engine.Camera.Target.Should().Be(new Vector3(2f, 3f, 4f));
            view.Engine.Camera.Radius.Should().BeApproximately(7f, 0.0001f);
            view.Engine.Camera.Yaw.Should().BeApproximately(0.8f, 0.0001f);
            view.Engine.Camera.Pitch.Should().BeApproximately(0.25f, 0.0001f);
            view.BackendDiagnostics.MeasurementCount.Should().Be(1);
            view.BackendDiagnostics.ActiveClippingPlaneCount.Should().Be(1);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task ExportSnapshotAsync_ShouldWriteArtifact_AndUpdateDiagnostics()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"videra-inspection-{Guid.NewGuid():N}.png");
        var view = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        try
        {
            var sceneObject = new Object3D { Name = "Snapshot" };
            sceneObject.PrepareDeferredMesh(new MeshData
            {
                Vertices =
                [
                    new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.White)
                ],
                Indices = [0u, 1u, 2u],
                Topology = MeshTopology.Triangles
            });
            view.AddObject(sceneObject);
            view.SelectionState = new VideraSelectionState
            {
                ObjectIds = [sceneObject.Id],
                PrimaryObjectId = sceneObject.Id
            };
            view.Measurements =
            [
                new VideraMeasurement
                {
                    Start = VideraMeasurementAnchor.ForObjectPoint(sceneObject.Id, new Vector3(0f, 0f, 0f)),
                    End = VideraMeasurementAnchor.ForWorldPoint(new Vector3(0f, 1f, 0f))
                }
            ];
            view.ClippingPlanes =
            [
                VideraClipPlane.FromPointNormal(Vector3.Zero, Vector3.UnitZ)
            ];
            view.Engine.Camera.SetOrbit(Vector3.Zero, 4f, 0.5f, 0.25f);

            var export = await view.ExportSnapshotAsync(outputPath);

            export.Failure.Should().BeNull(export.Failure?.ToString());
            export.Succeeded.Should().BeTrue();
            export.Path.Should().Be(outputPath);
            export.Width.Should().BeGreaterThan(0u);
            export.Height.Should().BeGreaterThan(0u);
            File.Exists(outputPath).Should().BeTrue();
            new FileInfo(outputPath).Length.Should().BeGreaterThan(0L);
            view.BackendDiagnostics.LastSnapshotExportPath.Should().Be(outputPath);
            view.BackendDiagnostics.LastSnapshotExportStatus.Should().Be("Succeeded");
            view.BackendDiagnostics.MeasurementCount.Should().Be(1);
            view.BackendDiagnostics.ActiveClippingPlaneCount.Should().Be(1);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task ExportSnapshotAsync_CanceledRequest_RecordsCancelledStatus_WithoutWritingArtifact()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"videra-inspection-{Guid.NewGuid():N}.png");
        var view = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        try
        {
            using var cancellation = new System.Threading.CancellationTokenSource();
            await cancellation.CancelAsync();

            await Assert.ThrowsAsync<OperationCanceledException>(() => view.ExportSnapshotAsync(outputPath, cancellation.Token));

            view.BackendDiagnostics.LastSnapshotExportPath.Should().Be(outputPath);
            view.BackendDiagnostics.LastSnapshotExportStatus.Should().Be("Cancelled");
            File.Exists(outputPath).Should().BeFalse();
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            view.Engine.Dispose();
        }
    }
}
