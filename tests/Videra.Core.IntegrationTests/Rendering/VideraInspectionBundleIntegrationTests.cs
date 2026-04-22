using System.Numerics;
using System.Text.Json;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Software;
using Videra.Core.Inspection;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraInspectionBundleIntegrationTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(
        Path.GetTempPath(),
        "VideraInspectionBundleTests",
        Guid.NewGuid().ToString("N"));

    public VideraInspectionBundleIntegrationTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public async Task ExportAsync_ShouldWriteInspectionBundleArtifacts()
    {
        var modelPath = WriteTriangleObj("bundle-export.obj");
        var bundlePath = Path.Combine(_tempDirectory, "bundle-export");
        var view = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        try
        {
            var loadResult = await view.LoadModelAsync(modelPath);
            loadResult.Succeeded.Should().BeTrue();

            var sceneObject = loadResult.Entry!.SceneObject;
            sceneObject.Name = "Bundle Export";
            sceneObject.Position = new Vector3(1.5f, 0f, 0f);

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
                    Text = "Exported object note"
                }
            ];
            view.Measurements =
            [
                new VideraMeasurement
                {
                    Label = "Bundle distance",
                    Start = VideraMeasurementAnchor.ForObjectPoint(sceneObject.Id, new Vector3(1.5f, 0f, 0f)),
                    End = VideraMeasurementAnchor.ForWorldPoint(new Vector3(1.5f, 1f, 0f))
                }
            ];
            view.InteractionOptions = new VideraInteractionOptions
            {
                MeasurementSnapMode = VideraMeasurementSnapMode.Face
            };
            view.ClippingPlanes =
            [
                VideraClipPlane.FromPointNormal(Vector3.Zero, Vector3.UnitZ)
            ];
            view.Engine.Camera.SetOrbit(new Vector3(2f, 3f, 4f), 7f, 0.8f, 0.25f);

            var export = await VideraInspectionBundleService.ExportAsync(view, bundlePath);

            export.Succeeded.Should().BeTrue(export.FailureMessage);
            export.CanReplayScene.Should().BeTrue();
            export.AssetCount.Should().Be(1);
            export.AnnotationCount.Should().Be(1);
            File.Exists(Path.Combine(bundlePath, VideraInspectionBundleService.InspectionStateFileName)).Should().BeTrue();
            File.Exists(Path.Combine(bundlePath, VideraInspectionBundleService.DiagnosticsFileName)).Should().BeTrue();
            File.Exists(Path.Combine(bundlePath, VideraInspectionBundleService.SnapshotFileName)).Should().BeTrue();
            var assetManifestPath = Path.Combine(bundlePath, VideraInspectionBundleService.AssetManifestFileName);
            File.Exists(assetManifestPath).Should().BeTrue();
            using var inspectionState = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(bundlePath, VideraInspectionBundleService.InspectionStateFileName)));
            inspectionState.RootElement.GetProperty("Annotations").GetArrayLength().Should().Be(1);

            using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(assetManifestPath));
            var bundledAssetPath = manifest.RootElement
                .GetProperty("Entries")[0]
                .GetProperty("FilePath")
                .GetString();
            bundledAssetPath.Should().NotBeNullOrWhiteSpace();
            Path.IsPathRooted(bundledAssetPath!).Should().BeFalse();
            File.Exists(Path.Combine(bundlePath, bundledAssetPath!)).Should().BeTrue();
            (await File.ReadAllTextAsync(assetManifestPath)).Should().NotContain(modelPath);
            File.Exists(Path.Combine(bundlePath, "annotations.json")).Should().BeFalse();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task ImportAsync_ShouldRejectBundlesThatCannotReplayHostOwnedSceneState()
    {
        var bundlePath = Path.Combine(_tempDirectory, "bundle-host-owned");
        var sourceView = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        var replayView = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        var replaySelectionId = Guid.NewGuid();
        var replayAnnotationId = Guid.NewGuid();
        try
        {
            var sourceObject = DemoMeshFactory.CreateTestCube(new SoftwareResourceFactory(), size: 1f);
            sourceObject.Name = "Host Owned Cube";
            sourceObject.Position = new Vector3(1f, 0f, 0f);
            sourceView.AddObject(sourceObject);
            sourceView.SelectionState = new VideraSelectionState
            {
                ObjectIds = [sourceObject.Id],
                PrimaryObjectId = sourceObject.Id
            };
            sourceView.Annotations =
            [
                new VideraNodeAnnotation
                {
                    ObjectId = sourceObject.Id,
                    Text = "Host-owned note"
                }
            ];

            replayView.SelectionState = new VideraSelectionState
            {
                ObjectIds = [replaySelectionId],
                PrimaryObjectId = replaySelectionId
            };
            replayView.Annotations =
            [
                new VideraWorldPointAnnotation
                {
                    Id = replayAnnotationId,
                    Text = "Existing replay note",
                    WorldPoint = new Vector3(0f, 2f, 0f)
                }
            ];

            var export = await VideraInspectionBundleService.ExportAsync(sourceView, bundlePath);

            export.Succeeded.Should().BeTrue(export.FailureMessage);
            export.CanReplayScene.Should().BeFalse();
            export.ReplayLimitation.Should().Contain("host-owned objects");
            export.AssetCount.Should().Be(0);
            export.AnnotationCount.Should().Be(1);

            var import = await VideraInspectionBundleService.ImportAsync(replayView, bundlePath);

            import.Succeeded.Should().BeFalse();
            import.FailureMessage.Should().Contain("host-owned objects");
            replayView.SelectionState.PrimaryObjectId.Should().Be(replaySelectionId);
            replayView.SelectionState.ObjectIds.Should().ContainSingle().Which.Should().Be(replaySelectionId);
            replayView.Annotations.Should().ContainSingle();
            replayView.Annotations.OfType<VideraWorldPointAnnotation>().Single().Id.Should().Be(replayAnnotationId);
        }
        finally
        {
            sourceView.Engine.Dispose();
            replayView.Engine.Dispose();
        }
    }

    [Fact]
    public async Task ImportAsync_ShouldReloadSceneAndRemapInspectionTruth()
    {
        var modelPath = WriteTriangleObj("bundle-import.obj");
        var bundlePath = Path.Combine(_tempDirectory, "bundle-import");
        var sourceView = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        var replayView = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        try
        {
            var loadResult = await sourceView.LoadModelsAsync([modelPath, modelPath]);
            loadResult.Succeeded.Should().BeTrue();
            loadResult.Entries.Should().HaveCount(2);

            var first = loadResult.Entries[0].SceneObject;
            var second = loadResult.Entries[1].SceneObject;
            first.Name = "Replay A";
            first.Position = new Vector3(-1.25f, 0f, 0f);
            second.Name = "Replay B";
            second.Position = new Vector3(1.25f, 0f, 0f);

            sourceView.SelectionState = new VideraSelectionState
            {
                ObjectIds = [first.Id],
                PrimaryObjectId = first.Id
            };
            sourceView.Annotations =
            [
                new VideraNodeAnnotation
                {
                    ObjectId = second.Id,
                    Text = "Replay object note"
                },
                new VideraWorldPointAnnotation
                {
                    WorldPoint = new Vector3(0f, 1.5f, 0f),
                    Text = "Replay world note"
                }
            ];
            sourceView.Measurements =
            [
                new VideraMeasurement
                {
                    Label = "Replay measurement",
                    Start = VideraMeasurementAnchor.ForObjectPoint(first.Id, new Vector3(-1.25f, 0f, 0f)),
                    End = VideraMeasurementAnchor.ForObjectPoint(second.Id, new Vector3(1.25f, 0f, 0f))
                }
            ];
            sourceView.InteractionOptions = new VideraInteractionOptions
            {
                MeasurementSnapMode = VideraMeasurementSnapMode.EdgeMidpoint
            };
            sourceView.ClippingPlanes =
            [
                VideraClipPlane.FromPointNormal(new Vector3(0f, 0f, 0.25f), Vector3.UnitZ)
            ];
            sourceView.Engine.Camera.SetOrbit(new Vector3(0f, 1f, 2f), 6f, 0.4f, 0.2f);

            var export = await VideraInspectionBundleService.ExportAsync(sourceView, bundlePath);
            export.Succeeded.Should().BeTrue(export.FailureMessage);
            File.Delete(modelPath);

            var import = await VideraInspectionBundleService.ImportAsync(replayView, bundlePath);

            import.Succeeded.Should().BeTrue(import.FailureMessage);
            import.SceneReloaded.Should().BeTrue();
            var replayedSceneObjects = ReadSceneDocumentObjects(replayView);
            replayedSceneObjects.Should().HaveCount(2);
            replayedSceneObjects.Select(static obj => obj.Name).Should().Contain("Replay A").And.Contain("Replay B");
            replayView.InteractionOptions.MeasurementSnapMode.Should().Be(VideraMeasurementSnapMode.EdgeMidpoint);
            replayView.ClippingPlanes.Should().ContainSingle();
            replayView.SelectionState.ObjectIds.Should().ContainSingle();
            replayView.SelectionState.ObjectIds.Should().NotContain(first.Id);
            replayView.Measurements.Should().ContainSingle();
            replayView.Measurements[0].Start.ObjectId.Should().HaveValue();
            replayView.Measurements[0].End.ObjectId.Should().HaveValue();
            replayView.Measurements[0].Start.ObjectId.Should().NotBe(first.Id);
            replayView.Measurements[0].End.ObjectId.Should().NotBe(second.Id);
            replayView.Annotations.Should().HaveCount(2);
            replayView.Annotations.OfType<VideraNodeAnnotation>().Single().ObjectId.Should().NotBe(second.Id);
            replayView.Annotations.OfType<VideraWorldPointAnnotation>().Single().WorldPoint.Should().Be(new Vector3(0f, 1.5f, 0f));
            replayView.Engine.Camera.Target.Should().Be(new Vector3(0f, 1f, 2f));
            replayView.Engine.Camera.Radius.Should().BeApproximately(6f, 0.0001f);
            replayView.Engine.Camera.Yaw.Should().BeApproximately(0.4f, 0.0001f);
            replayView.Engine.Camera.Pitch.Should().BeApproximately(0.2f, 0.0001f);
        }
        finally
        {
            sourceView.Engine.Dispose();
            replayView.Engine.Dispose();
        }
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup for temporary inspection bundle artifacts.
        }
    }

    private string WriteTriangleObj(string fileName)
    {
        var path = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(path, """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);
        return path;
    }

    private static IReadOnlyList<Object3D> ReadSceneDocumentObjects(VideraView view)
    {
        var sceneDocument = VideraViewRuntimeTestAccess.ReadRuntimeField<object>(view, "_sceneDocument");
        sceneDocument.Should().BeAssignableTo<Videra.Core.Scene.SceneDocument>();
        return ((Videra.Core.Scene.SceneDocument)sceneDocument).Entries.Select(static entry => entry.SceneObject).ToArray();
    }
}
