using System.Collections;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Graphics.Software;
using Videra.Core.Scene;
using Videra.Core.Selection.Annotations;
using Videra.Core.Styles.Presets;
using Videra.Import.Obj;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraViewSceneIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private bool _disposed;

    public VideraViewSceneIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"VideraScene_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task LoadModelAsync_WithoutConfiguredImporter_ReturnsFailureAndDoesNotMutateScene()
    {
        var path = WriteObj("missing-importer.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var view = new VideraView();
        try
        {
            var result = await view.LoadModelAsync(path);

            result.Succeeded.Should().BeFalse();
            result.Entry.Should().BeNull();
            result.Failure.Should().NotBeNull();
            result.Failure!.ErrorMessage.Should().Contain("No model importer is configured");
            GetSceneObjectCount(view).Should().Be(0);
            ReadSceneDocumentObjects(view).Should().BeEmpty();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task LoadModelAsync_ValidPath_ReturnsLoadedObjectAndAddsItToScene()
    {
        var path = WriteObj("triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var view = ConfigureObjImporter(new VideraView());
        try
        {
            var result = await view.LoadModelAsync(path);

            result.Succeeded.Should().BeTrue();
            result.Entry.Should().NotBeNull();
            result.Failure.Should().BeNull();
            GetSceneObjectCount(view).Should().Be(0);
            ReadSceneDocumentObjects(view).Should().ContainSingle().Which.Should().BeSameAs(result.Entry!.SceneObject);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task LoadModelAsync_BeforeBackendReady_CreatesDeferredSceneObjectAndRetainsImportedAsset()
    {
        var path = WriteObj("deferred-triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var view = ConfigureObjImporter(new VideraView());
        try
        {
            var result = await view.LoadModelAsync(path);

            result.Succeeded.Should().BeTrue();
            result.Entry.Should().NotBeNull();
            GetSceneObjectCount(view).Should().Be(0);
            ReadSceneDocumentObjects(view).Should().ContainSingle().Which.Should().BeSameAs(result.Entry!.SceneObject);
            ReadSceneDocumentImportedAssets(view).Should().ContainSingle(asset => asset.FilePath == path);
            ReadObjectVertexBuffer(result.Entry!.SceneObject).Should().BeNull();
            result.Entry.SceneObject.LocalBounds.Should().NotBeNull();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task LoadModelsAsync_MixedPaths_ReturnsNoSceneEntriesAndFailuresWithoutReplacingScene()
    {
        var validPath = WriteObj("triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);
        var missingPath = Path.Combine(_tempDir, "missing.obj");

        var view = ConfigureObjImporter(new VideraView());
        try
        {
            var result = await view.LoadModelsAsync(new[] { validPath, missingPath });

            result.Entries.Should().BeEmpty();
            result.Failures.Should().HaveCount(1);
            result.Failures[0].Path.Should().Be(missingPath);
            GetSceneObjectCount(view).Should().Be(0);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ReplaceScene_ReplacesEngineSceneObjects()
    {
        var view = ConfigureObjImporter(new VideraView());
        try
        {
            var factory = new SoftwareResourceFactory();
            var first = DemoMeshFactory.CreateTestCube(factory, size: 1f);
            var second = DemoMeshFactory.CreateTestCube(factory, size: 2f);

            view.AddObject(first);
            GetSceneObjectCount(view).Should().Be(1);

            view.ReplaceScene(new[] { second });

            GetSceneObjectCount(view).Should().Be(1);
            GetSceneObjects(view).Should().ContainSingle().Which.Should().BeSameAs(second);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ClearScene_RemovesAllSceneObjects()
    {
        var view = ConfigureObjImporter(new VideraView());
        try
        {
            var factory = new SoftwareResourceFactory();

            view.AddObject(DemoMeshFactory.CreateTestCube(factory, size: 1f));
            view.AddObject(DemoMeshFactory.CreateTestCube(factory, size: 2f));
            GetSceneObjectCount(view).Should().Be(2);

            view.ClearScene();

            GetSceneObjectCount(view).Should().Be(0);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void AddInstanceBatch_RetainsBatchForDiagnosticsWithoutCreatingRuntimeObjects()
    {
        var view = new VideraView(null, bitmapFactory: static (_, _) => null);
        try
        {
            var material = CreateInstanceBatchMaterial();
            var mesh = CreateInstanceBatchMesh(material.Id);
            var descriptor = new InstanceBatchDescriptor(
                "markers",
                mesh,
                material,
                new[] { Matrix4x4.Identity, Matrix4x4.CreateTranslation(2f, 0f, 0f) });

            view.AddInstanceBatch(descriptor);
            RefreshBackendDiagnostics(view);

            GetSceneObjects(view).Should().BeEmpty();
            ReadSceneDocumentInstanceBatches(view).Should().ContainSingle()
                .Which.InstanceCount.Should().Be(2);
            view.BackendDiagnostics.InstanceBatchCount.Should().Be(1);
            view.BackendDiagnostics.RetainedInstanceCount.Should().Be(2);
            view.BackendDiagnostics.PickableObjectCount.Should().Be(2);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void FrameAll_IncludesRetainedInstanceBatchBounds()
    {
        var view = new VideraView(null, bitmapFactory: static (_, _) => null);
        try
        {
            var material = CreateInstanceBatchMaterial();
            var mesh = CreateInstanceBatchMesh(material.Id);
            var descriptor = new InstanceBatchDescriptor(
                "markers",
                mesh,
                material,
                new[] { Matrix4x4.CreateTranslation(10f, 0f, 0f) });

            view.AddInstanceBatch(descriptor);

            view.FrameAll().Should().BeTrue();
            view.Engine.Camera.Target.X.Should().BeApproximately(10.5f, 0.01f);
            view.Engine.Camera.Target.Y.Should().BeApproximately(0.5f, 0.01f);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task LoadModelsAsync_WithFailures_DoesNotReplaceActiveScene()
    {
        var validPath = WriteObj("atomic-valid.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);
        var missingPath = Path.Combine(_tempDir, "atomic-missing.obj");

        var view = ConfigureObjImporter(new VideraView());
        try
        {
            var initial = DemoMeshFactory.CreateTestCube(new SoftwareResourceFactory(), size: 1f);
            view.AddObject(initial);

            var result = await view.LoadModelsAsync(new[] { validPath, missingPath });

            result.Succeeded.Should().BeFalse();
            result.Entries.Should().BeEmpty();
            GetSceneObjects(view).Should().ContainSingle().Which.Should().BeSameAs(initial);
            ReadSceneDocumentObjects(view).Should().ContainSingle().Which.Should().BeSameAs(initial);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task LoadModelsAsync_AllValid_ReplacesSceneAtomically()
    {
        var firstPath = WriteObj("replace-a.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);
        var secondPath = WriteObj("replace-b.obj", """
            v 0.0 0.0 0.0
            v 0.0 1.0 0.0
            v 0.0 0.0 1.0
            vn 1.0 0.0 0.0
            f 1//1 2//1 3//1
            """);

        var view = ConfigureObjImporter(new VideraView());
        try
        {
            var initial = DemoMeshFactory.CreateTestCube(new SoftwareResourceFactory(), size: 1f);
            view.AddObject(initial);

            var result = await view.LoadModelsAsync(new[] { firstPath, secondPath });

            result.Succeeded.Should().BeTrue();
            result.Entries.Should().HaveCount(2);
            GetSceneObjects(view).Should().BeEmpty();
            ReadSceneDocumentObjects(view).Should().HaveCount(2);
            ReadSceneDocumentObjects(view).Should().NotContain(initial);
            ReadSceneDocumentImportedAssets(view).Should().HaveCount(2);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void RuntimeSceneDocument_TracksSceneMutations()
    {
        var view = new VideraView();
        try
        {
            var factory = new SoftwareResourceFactory();
            var first = DemoMeshFactory.CreateTestCube(factory, size: 1f);
            var second = DemoMeshFactory.CreateTestCube(factory, size: 2f);

            view.AddObject(first);
            ReadSceneDocumentObjects(view).Should().ContainSingle().Which.Should().BeSameAs(first);

            view.ReplaceScene(new[] { second });
            ReadSceneDocumentObjects(view).Should().ContainSingle().Which.Should().BeSameAs(second);

            view.ClearScene();
            ReadSceneDocumentObjects(view).Should().BeEmpty();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void RuntimeSceneDocument_TracksBoundItemsCollectionMutations()
    {
        var view = new VideraView();
        try
        {
            var factory = new SoftwareResourceFactory();
            var items = new ObservableCollection<Object3D>();
            view.Items = items;

            items.Add(DemoMeshFactory.CreateTestCube(factory, size: 1f));
            ReadSceneDocumentObjects(view).Should().HaveCount(1);
            GetSceneObjects(view).Should().HaveCount(1);

            items.Add(DemoMeshFactory.CreateTestCube(factory, size: 2f));
            ReadSceneDocumentObjects(view).Should().HaveCount(2);
            GetSceneObjects(view).Should().HaveCount(2);

            items.RemoveAt(0);
            ReadSceneDocumentObjects(view).Should().HaveCount(1);
            GetSceneObjects(view).Should().HaveCount(1);

            items.Clear();
            ReadSceneDocumentObjects(view).Should().BeEmpty();
            GetSceneObjects(view).Should().BeEmpty();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task DeferredImportedSceneObject_RehydratesWhenGraphicsResourcesBecomeAvailable()
    {
        var path = WriteObj("rehydrate.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var view = ConfigureObjImporter(new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null));
        try
        {
            var result = await view.LoadModelAsync(path);
            var loadedObject = result.Entry!.SceneObject;

            loadedObject.Should().NotBeNull();
            ReadObjectVertexBuffer(loadedObject).Should().BeNull();

            var session = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            session.Attach(GraphicsBackendPreference.Software);
            session.Resize(128, 96, 1f);
            VideraViewRuntimeTestAccess.ReadRuntimeMethod(view, "ApplyViewState").Invoke(VideraViewRuntimeTestAccess.ReadRuntime(view), Array.Empty<object>());

            ReadObjectVertexBuffer(loadedObject).Should().NotBeNull();
            GetSceneObjects(view).Should().Contain(loadedObject);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task BackendDiagnostics_ShouldExposeSceneResidencyCountsBeforeAndAfterUpload()
    {
        var path = WriteObj("diagnostics-residency.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var view = ConfigureObjImporter(new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null));
        try
        {
            var result = await view.LoadModelAsync(path);
            result.Succeeded.Should().BeTrue();

            view.BackendDiagnostics.SceneDocumentVersion.Should().BeGreaterThan(0);
            view.BackendDiagnostics.PendingSceneUploads.Should().Be(1);
            view.BackendDiagnostics.PendingSceneUploadBytes.Should().BeGreaterThan(0);
            view.BackendDiagnostics.ResidentSceneObjects.Should().Be(0);
            view.BackendDiagnostics.DirtySceneObjects.Should().Be(0);
            view.BackendDiagnostics.FailedSceneUploads.Should().Be(0);

            var session = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            session.Attach(GraphicsBackendPreference.Software);
            session.Resize(128, 96, 1f);
            session.RenderOnce();
            RefreshBackendDiagnostics(view);

            var lastFlush = VideraViewRuntimeTestAccess.ReadRuntimeField<object>(view, "_lastSceneUploadFlushResult");
            var lastFlushType = lastFlush.GetType();
            var uploadedRecords = (IReadOnlyList<object>)lastFlushType.GetProperty("UploadedRecords")!.GetValue(lastFlush)!;
            uploadedRecords.Should().HaveCount(1);

            view.BackendDiagnostics.PendingSceneUploads.Should().Be(0);
            view.BackendDiagnostics.ResidentSceneObjects.Should().Be(1);
            view.BackendDiagnostics.DirtySceneObjects.Should().Be(0);
            view.BackendDiagnostics.FailedSceneUploads.Should().Be(0);
            view.BackendDiagnostics.LastFrameUploadedObjects.Should().Be(1);
            view.BackendDiagnostics.LastFrameUploadedBytes.Should().BeGreaterThan(0);
            view.BackendDiagnostics.ResolvedUploadBudgetObjects.Should().BeGreaterThan(0);
            view.BackendDiagnostics.ResolvedUploadBudgetBytes.Should().BeGreaterThan(0);
            view.BackendDiagnostics.LastFrameUploadDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task SteadyStateRender_ShouldNotReuploadResidentSceneObjectsWithoutDirtyEvent()
    {
        var path = WriteObj("steady-state.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var view = ConfigureObjImporter(new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null));
        try
        {
            var result = await view.LoadModelAsync(path);
            var loadedObject = result.Entry!.SceneObject;

            var session = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            session.Attach(GraphicsBackendPreference.Software);
            session.Resize(128, 96, 1f);
            session.RenderOnce();
            RefreshBackendDiagnostics(view);

            var firstVertexBuffer = ReadObjectVertexBuffer(loadedObject);
            firstVertexBuffer.Should().NotBeNull();

            session.RenderOnce();
            RefreshBackendDiagnostics(view);

            ReadObjectVertexBuffer(loadedObject).Should().BeSameAs(firstVertexBuffer);
            var residencyDiagnostics = ReadSceneResidencyDiagnostics(view);
            residencyDiagnostics.PendingUploads.Should().Be(0);
            residencyDiagnostics.DirtyObjects.Should().Be(0);
            residencyDiagnostics.ResidentObjects.Should().Be(1);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task BackendRebind_ShouldRequeueResidentImportedSceneObjectsForUpload()
    {
        var path = WriteObj("rebind-reupload.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var view = ConfigureObjImporter(new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null));
        try
        {
            var result = await view.LoadModelAsync(path);
            var loadedObject = result.Entry!.SceneObject;

            var session = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            session.Attach(GraphicsBackendPreference.Software);
            session.Resize(128, 96, 1f);
            session.RenderOnce();
            RefreshBackendDiagnostics(view);

            var originalVertexBuffer = ReadObjectVertexBuffer(loadedObject);
            originalVertexBuffer.Should().NotBeNull();

            VideraViewRuntimeTestAccess.ReadRuntimeMethod(view, "OnSceneBackendReady")
                .Invoke(VideraViewRuntimeTestAccess.ReadRuntime(view), Array.Empty<object>());

            session.RenderOnce();
            RefreshBackendDiagnostics(view);

            ReadObjectVertexBuffer(loadedObject).Should().NotBeNull();
            ReadObjectVertexBuffer(loadedObject).Should().NotBeSameAs(originalVertexBuffer);
            var residencyDiagnostics = ReadSceneResidencyDiagnostics(view);
            residencyDiagnostics.PendingUploads.Should().Be(0);
            residencyDiagnostics.ResidentObjects.Should().Be(1);
            residencyDiagnostics.DirtyObjects.Should().Be(0);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ModelImporter_ImportAndUpload_ProduceBackendNeutralSceneAsset()
    {
        var path = WriteObj("asset-triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var asset = ObjModelImporter.Import(path);
        var uploaded = SceneUploadCoordinator.Upload(asset, new SoftwareResourceFactory());

        asset.Name.Should().Be("asset-triangle.obj");
        uploaded.Name.Should().Be("asset-triangle.obj");
        uploaded.LocalBounds.Should().NotBeNull();
        uploaded.IndexCount.Should().BeGreaterThan(0u);
    }

    [Fact]
    public void FrameAll_EmptyScene_ReturnsFalse()
    {
        var view = new VideraView();
        try
        {
            view.FrameAll().Should().BeFalse();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void FrameAll_WithObjects_ReturnsTrueAndUpdatesCameraTarget()
    {
        var view = new VideraView();
        try
        {
            var factory = new SoftwareResourceFactory();
            var cube = DemoMeshFactory.CreateTestCube(factory, size: 4f);
            cube.Position = new System.Numerics.Vector3(5f, 0f, 0f);
            view.AddObject(cube);

            var framed = view.FrameAll();

            framed.Should().BeTrue();
            view.Engine.Camera.Target.X.Should().BeApproximately(5f, 0.001f);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ResetCamera_RestoresDefaultCameraState()
    {
        var view = new VideraView();
        try
        {
            view.Engine.Camera.Rotate(20f, 10f);
            view.Engine.Camera.Zoom(3f);

            view.ResetCamera();

            view.Engine.Camera.Yaw.Should().BeApproximately(0.5f, 0.0001f);
            view.Engine.Camera.Pitch.Should().BeApproximately(0.5f, 0.0001f);
            view.Engine.Camera.Target.Should().Be(System.Numerics.Vector3.Zero);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Theory]
    [InlineData(ViewPreset.Top)]
    [InlineData(ViewPreset.Front)]
    [InlineData(ViewPreset.Isometric)]
    public void SetViewPreset_UpdatesCameraOrientation(ViewPreset preset)
    {
        var view = new VideraView();
        try
        {
            var yawBefore = view.Engine.Camera.Yaw;
            var pitchBefore = view.Engine.Camera.Pitch;

            view.SetViewPreset(preset);

            if (preset == ViewPreset.Isometric)
            {
                view.Engine.Camera.Yaw.Should().NotBe(yawBefore);
                view.Engine.Camera.Pitch.Should().NotBe(pitchBefore);
            }
            else
            {
                view.Engine.Camera.Pitch.Should().NotBe(pitchBefore);
            }
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void RenderStyle_Wireframe_UpdatesEngineStyleServiceBeforeBackendReady()
    {
        var view = new VideraView();
        try
        {
            view.RenderStyle = RenderStylePreset.Wireframe;

            view.Engine.StyleService.CurrentPreset.Should().Be(RenderStylePreset.Wireframe);
            view.Engine.StyleService.CurrentParameters.Material.WireframeMode.Should().BeTrue();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void WireframeMode_ExplicitOverride_UpdatesEngineBeforeBackendReady()
    {
        var view = new VideraView();
        try
        {
            view.RenderStyle = RenderStylePreset.Wireframe;
            view.WireframeMode = WireframeMode.Overlay;

            view.Engine.StyleService.CurrentPreset.Should().Be(RenderStylePreset.Wireframe);
            view.Engine.StyleService.CurrentParameters.Material.WireframeMode.Should().BeTrue();
            view.Engine.Wireframe.Mode.Should().Be(WireframeMode.Overlay);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void VideraView_ShouldExposeControlledInteractionPublicSurface()
    {
        var viewType = typeof(VideraView);

        var selectionStateProperty = viewType.GetProperty("SelectionState");
        selectionStateProperty.Should().NotBeNull();
        selectionStateProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraSelectionState");
        selectionStateProperty.CanRead.Should().BeTrue();
        selectionStateProperty.CanWrite.Should().BeTrue();

        var annotationsProperty = viewType.GetProperty("Annotations");
        annotationsProperty.Should().NotBeNull();
        annotationsProperty!.CanRead.Should().BeTrue();
        annotationsProperty.CanWrite.Should().BeTrue();
        annotationsProperty.PropertyType.IsGenericType.Should().BeTrue();
        annotationsProperty.PropertyType.GetGenericTypeDefinition().Should().Be(typeof(IReadOnlyList<>));
        annotationsProperty.PropertyType.GetGenericArguments()[0].FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraAnnotation");

        var interactionModeProperty = viewType.GetProperty("InteractionMode");
        interactionModeProperty.Should().NotBeNull();
        interactionModeProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraInteractionMode");
        interactionModeProperty.CanRead.Should().BeTrue();
        interactionModeProperty.CanWrite.Should().BeTrue();

        var interactionOptionsProperty = viewType.GetProperty("InteractionOptions");
        interactionOptionsProperty.Should().NotBeNull();
        interactionOptionsProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraInteractionOptions");
        interactionOptionsProperty.CanRead.Should().BeTrue();
        interactionOptionsProperty.CanWrite.Should().BeTrue();

        var selectionRequestedEvent = viewType.GetEvent("SelectionRequested");
        selectionRequestedEvent.Should().NotBeNull();
        GetEventArgsType(selectionRequestedEvent!).FullName.Should().Be("Videra.Avalonia.Controls.Interaction.SelectionRequestedEventArgs");

        var selectionArgsType = GetEventArgsType(selectionRequestedEvent!);
        var selectionRequestProperty = selectionArgsType.GetProperty("Request");
        selectionRequestProperty.Should().NotBeNull();
        selectionRequestProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraSelectionRequest");

        var requestObjectIdsProperty = selectionArgsType.GetProperty("ObjectIds");
        requestObjectIdsProperty.Should().NotBeNull();
        requestObjectIdsProperty!.PropertyType.IsGenericType.Should().BeTrue();
        requestObjectIdsProperty.PropertyType.GetGenericTypeDefinition().Should().Be(typeof(IReadOnlyList<>));
        requestObjectIdsProperty.PropertyType.GetGenericArguments()[0].Should().Be(typeof(Guid));

        var requestPrimaryObjectIdProperty = selectionArgsType.GetProperty("PrimaryObjectId");
        requestPrimaryObjectIdProperty.Should().NotBeNull();
        requestPrimaryObjectIdProperty!.PropertyType.Should().Be(typeof(Guid?));

        var operationProperty = selectionArgsType.GetProperty("Operation");
        operationProperty.Should().NotBeNull();
        operationProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraSelectionOperation");

        var emptySpaceBehaviorProperty = selectionArgsType.GetProperty("EmptySpaceSelectionBehavior");
        emptySpaceBehaviorProperty.Should().NotBeNull();
        emptySpaceBehaviorProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraEmptySpaceSelectionBehavior");

        var objectIdsProperty = selectionRequestProperty.PropertyType.GetProperty("ObjectIds");
        objectIdsProperty.Should().NotBeNull();
        objectIdsProperty!.PropertyType.IsGenericType.Should().BeTrue();
        objectIdsProperty.PropertyType.GetGenericTypeDefinition().Should().Be(typeof(IReadOnlyList<>));
        objectIdsProperty.PropertyType.GetGenericArguments()[0].Should().Be(typeof(Guid));

        var primaryObjectIdProperty = selectionRequestProperty.PropertyType.GetProperty("PrimaryObjectId");
        primaryObjectIdProperty.Should().NotBeNull();
        primaryObjectIdProperty!.PropertyType.Should().Be(typeof(Guid?));

        var requestOperationProperty = selectionRequestProperty.PropertyType.GetProperty("Operation");
        requestOperationProperty.Should().NotBeNull();
        requestOperationProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraSelectionOperation");

        var requestEmptySpaceBehaviorProperty = selectionRequestProperty.PropertyType.GetProperty("EmptySpaceSelectionBehavior");
        requestEmptySpaceBehaviorProperty.Should().NotBeNull();
        requestEmptySpaceBehaviorProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraEmptySpaceSelectionBehavior");

        var annotationRequestedEvent = viewType.GetEvent("AnnotationRequested");
        annotationRequestedEvent.Should().NotBeNull();
        GetEventArgsType(annotationRequestedEvent!).FullName.Should().Be("Videra.Avalonia.Controls.Interaction.AnnotationRequestedEventArgs");

        var annotationArgsType = GetEventArgsType(annotationRequestedEvent!);
        var anchorProperty = annotationArgsType.GetProperty("Anchor");
        anchorProperty.Should().NotBeNull();
        anchorProperty!.PropertyType.Should().Be(typeof(AnnotationAnchorDescriptor));

        var emptySpaceSelectionBehaviorProperty = interactionOptionsProperty.PropertyType.GetProperty("EmptySpaceSelectionBehavior");
        emptySpaceSelectionBehaviorProperty.Should().NotBeNull();
        emptySpaceSelectionBehaviorProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraEmptySpaceSelectionBehavior");
    }

    [Fact]
    public void VideraInteractionDiagnostics_ShouldNotExposeInputAttachmentInternals()
    {
        var diagnosticsType = typeof(VideraView).Assembly.GetType("Videra.Avalonia.Controls.Interaction.VideraInteractionDiagnostics");
        diagnosticsType.Should().NotBeNull();

        diagnosticsType!.GetProperty("SupportsControlledSelection").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsControlledAnnotations").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsIntentEvents").Should().NotBeNull();
        diagnosticsType.GetProperty("IsInputBehaviorAttached").Should().BeNull();
    }

    [Fact]
    public void BackendDiagnostics_ShouldExposeLinuxDisplayServerResolutionFields()
    {
        var diagnosticsType = typeof(VideraBackendDiagnostics);

        diagnosticsType.GetProperty("ResolvedDisplayServer").Should().NotBeNull();
        diagnosticsType.GetProperty("DisplayServerFallbackUsed").Should().NotBeNull();
        diagnosticsType.GetProperty("DisplayServerFallbackReason").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsPassContributors").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsPassReplacement").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsFrameHooks").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsPipelineSnapshots").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsShaderCreation").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsResourceSetCreation").Should().NotBeNull();
        diagnosticsType.GetProperty("SupportsResourceSetBinding").Should().NotBeNull();

        var view = new VideraView();
        try
        {
            view.BackendDiagnostics.Should().NotBeNull();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void BackendDiagnostics_CanReflectNativeHostDisplayServerMetadata()
    {
        var view = new VideraView();
        try
        {
            var renderSession = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            renderSession.Should().NotBeNull();

            var setDisplayServerDiagnostics = renderSession!.GetType().GetMethod(
                "SetDisplayServerDiagnostics",
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(string), typeof(bool), typeof(string) },
                modifiers: null);
            setDisplayServerDiagnostics.Should().NotBeNull();

            setDisplayServerDiagnostics!.Invoke(renderSession, new object?[] { "XWayland", true, "Wayland host unavailable." });

            var refreshDiagnostics = typeof(VideraView).GetMethod(
                "RefreshBackendDiagnostics",
                BindingFlags.Instance | BindingFlags.NonPublic);
            refreshDiagnostics.Should().NotBeNull();
            refreshDiagnostics!.Invoke(view, new object?[] { null });

            view.BackendDiagnostics.ResolvedDisplayServer.Should().Be("XWayland");
            view.BackendDiagnostics.DisplayServerFallbackUsed.Should().BeTrue();
            view.BackendDiagnostics.DisplayServerFallbackReason.Should().Be("Wayland host unavailable.");
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void BackendDiagnostics_CanReflectPipelineSummaryForSoftwarePresentation()
    {
        var view = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        try
        {
            var renderSession = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            renderSession.Should().NotBeNull();

            renderSession!.Attach(GraphicsBackendPreference.Software);
            renderSession.Resize(128, 96, 1f);
            renderSession.RenderOnce();

            var refreshDiagnostics = typeof(VideraView).GetMethod(
                "RefreshBackendDiagnostics",
                BindingFlags.Instance | BindingFlags.NonPublic);
            refreshDiagnostics.Should().NotBeNull();
            refreshDiagnostics!.Invoke(view, new object?[] { null });

            view.BackendDiagnostics.RenderPipelineProfile.Should().Be("Standard");
            view.BackendDiagnostics.LastFrameStageNames.Should().NotBeNull();
            view.BackendDiagnostics.LastFrameStageNames.Should().Contain("PrepareFrame");
            view.BackendDiagnostics.LastFrameStageNames.Should().Contain("PresentFrame");
            view.BackendDiagnostics.LastFrameFeatureNames.Should().Equal("Overlay");
            view.BackendDiagnostics.SupportedRenderFeatureNames.Should().Equal("Opaque", "Transparent", "Overlay", "Picking", "Screenshot");
            view.BackendDiagnostics.TransparentFeatureStatus.Should().Be(VideraBackendDiagnostics.CurrentTransparentFeatureStatus);
            view.BackendDiagnostics.UsesSoftwarePresentationCopy.Should().BeTrue();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ReleaseNativeHost_ShouldUnbindRenderSessionBeforeRemovingNativeChild()
    {
        var nativeHostFactory = new RecordingNativeHostFactory();
        var view = new ReleaseOrderTrackingView(nativeHostFactory, bitmapFactory: static (_, _) => null);
        try
        {
            view.Measure(new Size(200, 200));
            view.Arrange(new Rect(0, 0, 200, 200));

            InvokeNonPublicMethod(view, "EnsureNativeHost");

            nativeHostFactory.LastCreatedHost.Should().NotBeNull();
            nativeHostFactory.LastCreatedHost!.RaiseHandleCreated(new IntPtr(0x1234));
            VideraViewRuntimeTestAccess.ReadRenderSession(view).HandleState.IsBound.Should().BeTrue();

            InvokeNonPublicMethod(view, "ReleaseNativeHost");

            view.WasRenderSessionHandleBoundWhenChildRemoved.Should().BeFalse();
            VideraViewRuntimeTestAccess.ReadRenderSession(view).HandleState.IsBound.Should().BeFalse();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void NativeHandleCreated_WithZeroBounds_BindsHandleWithoutPrematureNativeInitialization()
    {
        var view = new VideraView(new RecordingNativeHostFactory(), bitmapFactory: static (_, _) => null);
        NativeTrackingSessionSwap? sessionSwap = null;
        try
        {
            sessionSwap = NativeTrackingSessionSwap.Install(view);
            view.PreferredBackend = GraphicsBackendPreference.D3D11;

            var onNativeHandleCreated = VideraViewRuntimeTestAccess.ReadRuntimeMethod(view, "OnNativeHandleCreated");
            var runtime = VideraViewRuntimeTestAccess.ReadRuntime(view);
            var renderSession = VideraViewRuntimeTestAccess.ReadRenderSession(view);

            view.Bounds.Width.Should().Be(0d);
            view.Bounds.Height.Should().Be(0d);

            onNativeHandleCreated.Invoke(runtime, [new IntPtr(0x1234)]);

            renderSession.HandleState.IsBound.Should().BeTrue();
            renderSession.IsReady.Should().BeFalse("native startup should wait for a real size instead of initializing a synthetic 64x64 surface");

            var synchronizeSession = VideraViewRuntimeTestAccess.ReadRuntimeMethod(view, "SynchronizeSession");
            synchronizeSession.Invoke(runtime, [960u, 599u, 0, false]);

            renderSession.IsReady.Should().BeTrue();
        }
        finally
        {
            sessionSwap?.Dispose();
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void OverlayPresenter_ConsumesHostOwnedOverlayState_WhenOverlayContainerIsPresent()
    {
        var view = new VideraView(new RecordingNativeHostFactory(), bitmapFactory: static (_, _) => null);
        try
        {
            var renderSession = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            renderSession.Should().NotBeNull();
            renderSession!.Attach(GraphicsBackendPreference.Software);
            renderSession.Resize(200, 200, 1f);

            var sceneObject = DemoMeshFactory.CreateWhiteQuad(renderSession.ResourceFactory!);
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
                    Id = Guid.NewGuid(),
                    Text = "Selected",
                    ObjectId = sceneObject.Id
                }
            ];

            var ensureNativeHost = typeof(VideraView).GetMethod("EnsureNativeHost", BindingFlags.Instance | BindingFlags.NonPublic);
            ensureNativeHost.Should().NotBeNull();
            ensureNativeHost!.Invoke(view, Array.Empty<object>());

            var inputOverlayField = typeof(VideraView).GetField("_inputOverlay", BindingFlags.Instance | BindingFlags.NonPublic);
            inputOverlayField.Should().NotBeNull();
            var inputOverlay = (Border?)inputOverlayField!.GetValue(view);
            inputOverlay.Should().NotBeNull();
            inputOverlay!.Child.Should().NotBeNull();

            var overlayPresenterType = typeof(VideraView).Assembly.GetType("Videra.Avalonia.Controls.VideraViewOverlayPresenter");
            overlayPresenterType.Should().NotBeNull();
            inputOverlay.Child!.GetType().Should().Be(overlayPresenterType);

            renderSession.RenderOnce();

            var overlayStateField = overlayPresenterType!.GetField("_overlayState", BindingFlags.Instance | BindingFlags.NonPublic);
            overlayStateField.Should().NotBeNull();
            var overlayState = overlayStateField!.GetValue(inputOverlay.Child!);
            overlayState.Should().NotBeNull();

            var selectionOutlines = (IReadOnlyList<object>)overlayState!.GetType().GetProperty("SelectionOutlines")!.GetValue(overlayState)!;
            var labels = (IReadOnlyList<object>)overlayState.GetType().GetProperty("Labels")!.GetValue(overlayState)!;
            selectionOutlines.Should().ContainSingle();
            var screenBounds = (Rect)selectionOutlines[0].GetType().GetProperty("ScreenBounds")!.GetValue(selectionOutlines[0])!;
            screenBounds.Width.Should().BeGreaterThan(0d);
            screenBounds.Height.Should().BeGreaterThan(0d);
            labels.Should().ContainSingle();
            labels[0].GetType().GetProperty("Text")!.GetValue(labels[0]).Should().Be("Selected");
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void OverlayPresenter_RefreshesAfterNativeFrameRender_WhenNativeHandleIsBound()
    {
        var nativeHostFactory = new RecordingNativeHostFactory();
        var view = new VideraView(nativeHostFactory, bitmapFactory: static (_, _) => null);
        NativeTrackingSessionSwap? sessionSwap = null;
        try
        {
            sessionSwap = NativeTrackingSessionSwap.Install(view);
            view.PreferredBackend = GraphicsBackendPreference.D3D11;
            view.Measure(new Size(200, 200));
            view.Arrange(new Rect(0, 0, 200, 200));

            var ensureNativeHost = typeof(VideraView).GetMethod("EnsureNativeHost", BindingFlags.Instance | BindingFlags.NonPublic);
            ensureNativeHost.Should().NotBeNull();
            ensureNativeHost!.Invoke(view, Array.Empty<object>());

            nativeHostFactory.LastCreatedHost.Should().NotBeNull();
            nativeHostFactory.LastCreatedHost!.RaiseHandleCreated(new IntPtr(0x1234));

            var renderSession = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            renderSession.Should().NotBeNull();
            renderSession!.IsReady.Should().BeTrue();
            renderSession.IsSoftwareBackend.Should().BeFalse();
            renderSession.ResourceFactory.Should().NotBeNull();

            view.Engine.Camera.SetOrbit(Vector3.Zero, 10f, 0f, 0f);
            view.Engine.Camera.UpdateProjection(64, 64);

            var sceneObject = DemoMeshFactory.CreateTestCube(renderSession.ResourceFactory!, size: 0.5f);
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
                    Id = Guid.NewGuid(),
                    Text = "Selected",
                    ObjectId = sceneObject.Id
                }
            ];

            renderSession.RenderOnce();

            var initialRect = GetPresenterSelectionRect(view);
            var initialLabelPosition = GetPresenterLabelPosition(view);

            sceneObject.Position = new Vector3(1.5f, 0f, 0f);

            GetPresenterSelectionRect(view).Should().Be(initialRect);
            GetPresenterLabelPosition(view).Should().Be(initialLabelPosition);

            renderSession.RenderOnce();

            var updatedRect = GetPresenterSelectionRect(view);
            var updatedLabelPosition = GetPresenterLabelPosition(view);

            updatedRect.X.Should().NotBeApproximately(initialRect.X, 0.5d);
            updatedLabelPosition.X.Should().NotBeApproximately(initialLabelPosition.X, 0.5f);
        }
        finally
        {
            sessionSwap?.Dispose();
            view.Engine.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Best-effort temp cleanup for test artifacts.
        }
    }

    private string WriteObj(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private static VideraView ConfigureObjImporter(VideraView view)
    {
        view.Options = new VideraViewOptions
        {
            ModelImporter = static path => ObjModelImporter.Import(path)
        };
        return view;
    }

    private static int GetSceneObjectCount(VideraView view)
    {
        return GetSceneObjects(view).Count;
    }

    private static Type GetEventArgsType(EventInfo eventInfo)
    {
        eventInfo.EventHandlerType.Should().NotBeNull();
        eventInfo.EventHandlerType!.IsGenericType.Should().BeTrue();
        eventInfo.EventHandlerType.GetGenericTypeDefinition().Should().Be(typeof(EventHandler<>));
        return eventInfo.EventHandlerType.GetGenericArguments()[0];
    }

    private static IList<Object3D> GetSceneObjects(VideraView view)
    {
        return view.Engine.SceneObjects.ToList();
    }

    private static IReadOnlyList<Object3D> ReadSceneDocumentObjects(VideraView view)
    {
        var sceneDocument = VideraViewRuntimeTestAccess.ReadRuntimeField<object>(view, "_sceneDocument");
        sceneDocument.Should().BeAssignableTo<SceneDocument>();
        return ((SceneDocument)sceneDocument).Entries.Select(static entry => entry.SceneObject).ToArray();
    }

    private static IReadOnlyList<ImportedSceneAsset> ReadSceneDocumentImportedAssets(VideraView view)
    {
        var sceneDocument = VideraViewRuntimeTestAccess.ReadRuntimeField<object>(view, "_sceneDocument");
        sceneDocument.Should().BeAssignableTo<SceneDocument>();

        return ((SceneDocument)sceneDocument).Entries
            .Select(entry => entry.ImportedAsset)
            .Where(asset => asset is not null)
            .Cast<ImportedSceneAsset>()
            .ToArray();
    }

    private static IReadOnlyList<InstanceBatchEntry> ReadSceneDocumentInstanceBatches(VideraView view)
    {
        var sceneDocument = VideraViewRuntimeTestAccess.ReadRuntimeField<object>(view, "_sceneDocument");
        sceneDocument.Should().BeAssignableTo<SceneDocument>();
        return ((SceneDocument)sceneDocument).InstanceBatches;
    }

    private static MaterialInstance CreateInstanceBatchMaterial()
    {
        return new MaterialInstance(MaterialInstanceId.New(), "material", RgbaFloat.White);
    }

    private static MeshPrimitive CreateInstanceBatchMesh(MaterialInstanceId materialId)
    {
        return new MeshPrimitive(
            MeshPrimitiveId.New(),
            "triangle",
            new MeshData
            {
                Vertices =
                [
                    new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
                ],
                Indices = [0u, 1u, 2u],
                Topology = MeshTopology.Triangles
            },
            materialId);
    }

    private static object? ReadObjectVertexBuffer(Object3D sceneObject)
    {
        var property = typeof(Object3D).GetProperty("VertexBuffer", BindingFlags.Instance | BindingFlags.NonPublic);
        property.Should().NotBeNull();
        return property!.GetValue(sceneObject);
    }

    private static void RefreshBackendDiagnostics(VideraView view)
    {
        var refreshDiagnostics = typeof(VideraView).GetMethod(
            "RefreshBackendDiagnostics",
            BindingFlags.Instance | BindingFlags.NonPublic);
        refreshDiagnostics.Should().NotBeNull();
        refreshDiagnostics!.Invoke(view, new object?[] { null });
    }

    private static (int PendingUploads, int ResidentObjects, int DirtyObjects, int FailedUploads) ReadSceneResidencyDiagnostics(VideraView view)
    {
        var diagnostics = VideraViewRuntimeTestAccess.ReadRuntimeField<object>(view, "_sceneDiagnostics");
        var diagnosticsType = diagnostics.GetType();
        return (
            PendingUploads: (int)diagnosticsType.GetProperty("PendingUploads")!.GetValue(diagnostics)!,
            ResidentObjects: (int)diagnosticsType.GetProperty("ResidentObjects")!.GetValue(diagnostics)!,
            DirtyObjects: (int)diagnosticsType.GetProperty("DirtyObjects")!.GetValue(diagnostics)!,
            FailedUploads: (int)diagnosticsType.GetProperty("FailedUploads")!.GetValue(diagnostics)!);
    }

    private sealed class RecordingNativeHostFactory : INativeHostFactory
    {
        public RecordingNativeHost? LastCreatedHost { get; private set; }

        public IVideraNativeHost? CreateHost()
        {
            LastCreatedHost = new RecordingNativeHost();
            return LastCreatedHost;
        }
    }

    private sealed class RecordingNativeHost : NativeControlHost, IVideraNativeHost
    {
        public event Action<IntPtr>? HandleCreated;
        public event Action? HandleDestroyed;
        public event Action<NativePointerEvent>? NativePointer;

        public void RaiseHandleCreated(IntPtr handle)
        {
            HandleCreated?.Invoke(handle);
        }
    }

    private sealed class ReleaseOrderTrackingView : VideraView
    {
        public ReleaseOrderTrackingView(
            INativeHostFactory? nativeHostFactory,
            Func<uint, uint, global::Avalonia.Media.Imaging.WriteableBitmap?>? bitmapFactory)
            : base(nativeHostFactory, bitmapFactory)
        {
        }

        public bool? WasRenderSessionHandleBoundWhenChildRemoved { get; private set; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ChildProperty &&
                change is AvaloniaPropertyChangedEventArgs<Control?> childChange &&
                childChange.OldValue.GetValueOrDefault() is not null &&
                childChange.NewValue.GetValueOrDefault() is null)
            {
                WasRenderSessionHandleBoundWhenChildRemoved =
                    VideraViewRuntimeTestAccess.ReadRenderSession(this).HandleState.IsBound;
            }

            base.OnPropertyChanged(change);
        }
    }

    private static Rect GetPresenterSelectionRect(VideraView view)
    {
        var overlayState = GetOverlayState(view);
        var selectionOutlines = (IReadOnlyList<object>)overlayState.GetType().GetProperty("SelectionOutlines")!.GetValue(overlayState)!;
        selectionOutlines.Should().ContainSingle();
        return (Rect)selectionOutlines[0].GetType().GetProperty("ScreenBounds")!.GetValue(selectionOutlines[0])!;
    }

    private static Point GetPresenterLabelPosition(VideraView view)
    {
        var overlayState = GetOverlayState(view);
        var labels = (IReadOnlyList<object>)overlayState.GetType().GetProperty("Labels")!.GetValue(overlayState)!;
        labels.Should().ContainSingle();
        var screenPosition = (Vector2)labels[0].GetType().GetProperty("ScreenPosition")!.GetValue(labels[0])!;
        return new Point(screenPosition.X, screenPosition.Y);
    }

    private static object GetOverlayState(VideraView view)
    {
        var inputOverlayField = typeof(VideraView).GetField("_inputOverlay", BindingFlags.Instance | BindingFlags.NonPublic);
        inputOverlayField.Should().NotBeNull();
        var inputOverlay = (Border?)inputOverlayField!.GetValue(view);
        inputOverlay.Should().NotBeNull();
        inputOverlay!.Child.Should().NotBeNull();

        var overlayStateField = inputOverlay.Child!.GetType().GetField("_overlayState", BindingFlags.Instance | BindingFlags.NonPublic);
        overlayStateField.Should().NotBeNull();
        var overlayState = overlayStateField!.GetValue(inputOverlay.Child!);
        overlayState.Should().NotBeNull();
        return overlayState!;
    }

    private static void InvokeNonPublicMethod(object target, string methodName)
    {
        var type = target.GetType();
        MethodInfo? method = null;
        while (type is not null && method is null)
        {
            method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            type = type.BaseType;
        }

        method.Should().NotBeNull($"method {methodName} should exist on {target.GetType().FullName}");
        method!.Invoke(target, Array.Empty<object>());
    }

    private sealed class NativeTrackingSessionSwap : IDisposable
    {
        private static readonly MethodInfo ReleaseNativeHostMethod =
            typeof(VideraView).GetMethod("ReleaseNativeHost", BindingFlags.Instance | BindingFlags.NonPublic)!;

        private readonly VideraView _view;
        private readonly RenderSession _originalSession;
        private readonly object _originalSessionBridge;
        private readonly RenderSession _replacementSession;
        private readonly EventHandler _backendReadyHandler;
        private bool _disposed;

        private NativeTrackingSessionSwap(
            VideraView view,
            RenderSession originalSession,
            object originalSessionBridge,
            RenderSession replacementSession,
            EventHandler backendReadyHandler)
        {
            _view = view;
            _originalSession = originalSession;
            _originalSessionBridge = originalSessionBridge;
            _replacementSession = replacementSession;
            _backendReadyHandler = backendReadyHandler;
        }

        public static NativeTrackingSessionSwap Install(VideraView view)
        {
            ReleaseNativeHostMethod.Should().NotBeNull();

            var originalSession = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            var originalSessionBridge = VideraViewRuntimeTestAccess.ReadSessionBridge(view);
            var createSessionBridgeMethod = VideraViewRuntimeTestAccess.ReadRuntimeMethod(view, "CreateSessionBridge");
            var onBackendReadyMethod = VideraViewRuntimeTestAccess.ReadRuntimeMethod(view, "OnRenderSessionBackendReady");
            var pushOverlayRenderStateMethod = VideraViewRuntimeTestAccess.ReadRuntimeMethod(view, "PushOverlayRenderState");
            var onRenderSessionFrameRequestedMethod = VideraViewRuntimeTestAccess.ReadRuntimeMethod(view, "OnRenderSessionFrameRequested");

            originalSession.Should().NotBeNull();
            originalSessionBridge.Should().NotBeNull();

            var replacementSession = new RenderSession(
                view.Engine,
                backendFactory: static _ => new NativeTrackingBackend(),
                beforeRender: () => pushOverlayRenderStateMethod.Invoke(VideraViewRuntimeTestAccess.ReadRuntime(view), Array.Empty<object>()),
                requestRender: () => onRenderSessionFrameRequestedMethod.Invoke(VideraViewRuntimeTestAccess.ReadRuntime(view), Array.Empty<object>()),
                bitmapFactory: static (_, _) => null);
            EventHandler backendReadyHandler = (sender, args) => onBackendReadyMethod.Invoke(VideraViewRuntimeTestAccess.ReadRuntime(view), [sender, args]);
            replacementSession.BackendReady += backendReadyHandler;

            VideraViewRuntimeTestAccess.WriteRuntimeField(view, "_renderSession", replacementSession);
            VideraViewRuntimeTestAccess.WriteRuntimeField(view, "_sessionBridge", createSessionBridgeMethod.Invoke(VideraViewRuntimeTestAccess.ReadRuntime(view), Array.Empty<object>())!);

            return new NativeTrackingSessionSwap(
                view,
                originalSession!,
                originalSessionBridge!,
                replacementSession,
                backendReadyHandler);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ReleaseNativeHostMethod.Invoke(_view, Array.Empty<object>());
            _replacementSession.BackendReady -= _backendReadyHandler;
            _replacementSession.BindHandle(IntPtr.Zero);
            VideraViewRuntimeTestAccess.WriteRuntimeField(_view, "_renderSession", _originalSession);
            VideraViewRuntimeTestAccess.WriteRuntimeField(_view, "_sessionBridge", _originalSessionBridge);
        }
    }

    private sealed class NativeTrackingBackend : IGraphicsBackend
    {
        private readonly TrackingResourceFactory _resourceFactory = new();
        private readonly TrackingCommandExecutor _commandExecutor = new();

        public bool IsInitialized { get; private set; }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            _ = windowHandle;
            _ = width;
            _ = height;
            IsInitialized = true;
        }

        public void Resize(int width, int height)
        {
            _ = width;
            _ = height;
        }

        public void BeginFrame()
        {
        }

        public void EndFrame()
        {
        }

        public void SetClearColor(Vector4 color)
        {
            _ = color;
        }

        public IResourceFactory GetResourceFactory() => _resourceFactory;

        public ICommandExecutor GetCommandExecutor() => _commandExecutor;

        public void Dispose()
        {
            IsInitialized = false;
        }
    }

    private sealed class TrackingResourceFactory : IResourceFactory
    {
        public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices) => new TrackingBuffer((uint)(vertices.Length * sizeof(float) * 10));

        public IBuffer CreateVertexBuffer(uint sizeInBytes) => new TrackingBuffer(sizeInBytes);

        public IBuffer CreateIndexBuffer(uint[] indices) => new TrackingBuffer((uint)(indices.Length * sizeof(uint)));

        public IBuffer CreateIndexBuffer(uint sizeInBytes) => new TrackingBuffer(sizeInBytes);

        public IBuffer CreateUniformBuffer(uint sizeInBytes) => new TrackingBuffer(sizeInBytes);

        public IPipeline CreatePipeline(PipelineDescription description) => new TrackingPipeline();

        public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors) => new TrackingPipeline();

        public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint) => new TrackingShader();

        public IResourceSet CreateResourceSet(ResourceSetDescription description) => new TrackingResourceSet();
    }

    private sealed class TrackingCommandExecutor : ICommandExecutor
    {
        public void SetPipeline(IPipeline pipeline)
        {
        }

        public void SetVertexBuffer(IBuffer buffer, uint index = 0)
        {
        }

        public void SetIndexBuffer(IBuffer buffer)
        {
        }

        public void SetResourceSet(uint slot, IResourceSet resourceSet)
        {
        }

        public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
        {
        }

        public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
        {
        }

        public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
        {
        }

        public void SetViewport(float x, float y, float width, float height, float minDepth = 0f, float maxDepth = 1f)
        {
        }

        public void SetScissorRect(int x, int y, int width, int height)
        {
        }

        public void Clear(float r, float g, float b, float a)
        {
        }

        public void SetDepthState(bool testEnabled, bool writeEnabled)
        {
        }

        public void ResetDepthState()
        {
        }
    }

    private sealed class TrackingBuffer(uint sizeInBytes) : IBuffer
    {
        public uint SizeInBytes { get; } = sizeInBytes;

        public void Update<T>(T data) where T : unmanaged
        {
        }

        public void UpdateArray<T>(T[] data) where T : unmanaged
        {
        }

        public void SetData<T>(T data, uint offset) where T : unmanaged
        {
        }

        public void SetData<T>(T[] data, uint offset) where T : unmanaged
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class TrackingPipeline : IPipeline
    {
        public void Dispose()
        {
        }
    }

    private sealed class TrackingShader : IShader
    {
        public void Dispose()
        {
        }
    }

    private sealed class TrackingResourceSet : IResourceSet
    {
        public void Dispose()
        {
        }
    }
}
