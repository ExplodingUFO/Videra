using System.Collections;
using System.Reflection;
using Avalonia.Controls;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Graphics.Software;
using Videra.Core.Selection.Annotations;
using Videra.Core.Styles.Presets;
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
    public async Task LoadModelAsync_ValidPath_ReturnsLoadedObjectAndAddsItToScene()
    {
        var path = WriteObj("triangle.obj", """
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

            result.Succeeded.Should().BeTrue();
            result.LoadedObject.Should().NotBeNull();
            result.Failure.Should().BeNull();
            GetSceneObjectCount(view).Should().Be(1);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task LoadModelsAsync_MixedPaths_ReturnsLoadedObjectsAndFailures()
    {
        var validPath = WriteObj("triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);
        var missingPath = Path.Combine(_tempDir, "missing.obj");

        var view = new VideraView();
        try
        {
            var result = await view.LoadModelsAsync(new[] { validPath, missingPath });

            result.LoadedObjects.Should().HaveCount(1);
            result.Failures.Should().HaveCount(1);
            result.Failures[0].Path.Should().Be(missingPath);
            GetSceneObjectCount(view).Should().Be(1);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ReplaceScene_ReplacesEngineSceneObjects()
    {
        var view = new VideraView();
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
        var view = new VideraView();
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
        var selectionProperty = selectionArgsType.GetProperty("Selection");
        selectionProperty.Should().NotBeNull();
        selectionProperty!.PropertyType.FullName.Should().Be("Videra.Avalonia.Controls.Interaction.VideraSelectionState");

        var requestObjectIdsProperty = selectionArgsType.GetProperty("ObjectIds");
        requestObjectIdsProperty.Should().NotBeNull();
        requestObjectIdsProperty!.PropertyType.IsGenericType.Should().BeTrue();
        requestObjectIdsProperty.PropertyType.GetGenericTypeDefinition().Should().Be(typeof(IReadOnlyList<>));
        requestObjectIdsProperty.PropertyType.GetGenericArguments()[0].Should().Be(typeof(Guid));

        var requestPrimaryObjectIdProperty = selectionArgsType.GetProperty("PrimaryObjectId");
        requestPrimaryObjectIdProperty.Should().NotBeNull();
        requestPrimaryObjectIdProperty!.PropertyType.Should().Be(typeof(Guid?));

        var objectIdsProperty = selectionProperty.PropertyType.GetProperty("ObjectIds");
        objectIdsProperty.Should().NotBeNull();
        objectIdsProperty!.PropertyType.IsGenericType.Should().BeTrue();
        objectIdsProperty.PropertyType.GetGenericTypeDefinition().Should().Be(typeof(IReadOnlyList<>));
        objectIdsProperty.PropertyType.GetGenericArguments()[0].Should().Be(typeof(Guid));

        var primaryObjectIdProperty = selectionProperty.PropertyType.GetProperty("PrimaryObjectId");
        primaryObjectIdProperty.Should().NotBeNull();
        primaryObjectIdProperty!.PropertyType.Should().Be(typeof(Guid?));

        var annotationRequestedEvent = viewType.GetEvent("AnnotationRequested");
        annotationRequestedEvent.Should().NotBeNull();
        GetEventArgsType(annotationRequestedEvent!).FullName.Should().Be("Videra.Avalonia.Controls.Interaction.AnnotationRequestedEventArgs");

        var annotationArgsType = GetEventArgsType(annotationRequestedEvent!);
        var anchorProperty = annotationArgsType.GetProperty("Anchor");
        anchorProperty.Should().NotBeNull();
        anchorProperty!.PropertyType.Should().Be(typeof(AnnotationAnchorDescriptor));
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
            var renderSessionField = typeof(VideraView).GetField("_renderSession", BindingFlags.Instance | BindingFlags.NonPublic);
            renderSessionField.Should().NotBeNull();

            var renderSession = renderSessionField!.GetValue(view);
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
            var renderSessionField = typeof(VideraView).GetField("_renderSession", BindingFlags.Instance | BindingFlags.NonPublic);
            renderSessionField.Should().NotBeNull();

            var renderSession = (RenderSession?)renderSessionField!.GetValue(view);
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
            view.BackendDiagnostics.UsesSoftwarePresentationCopy.Should().BeTrue();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void NativeOverlayAdapter_ConsumesHostOwnedOverlayState()
    {
        var view = new VideraView(new RecordingNativeHostFactory(), bitmapFactory: static (_, _) => null);
        try
        {
            var renderSessionField = typeof(VideraView).GetField("_renderSession", BindingFlags.Instance | BindingFlags.NonPublic);
            renderSessionField.Should().NotBeNull();

            var renderSession = (RenderSession?)renderSessionField!.GetValue(view);
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
            labels.Should().ContainSingle();
            labels[0].GetType().GetProperty("Text")!.GetValue(labels[0]).Should().Be("Selected");
        }
        finally
        {
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
        var field = typeof(VideraEngine).GetField("_sceneObjects", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();

        var value = field!.GetValue(view.Engine);
        value.Should().BeAssignableTo<IList<Object3D>>();
        return (IList<Object3D>)value!;
    }

    private sealed class RecordingNativeHostFactory : INativeHostFactory
    {
        public IVideraNativeHost? CreateHost()
        {
            return new RecordingNativeHost();
        }
    }

    private sealed class RecordingNativeHost : NativeControlHost, IVideraNativeHost
    {
        public event Action<IntPtr>? HandleCreated;
        public event Action? HandleDestroyed;
        public event Action<NativePointerEvent>? NativePointer;

        public void RaiseHandleCreated(IntPtr handle) => HandleCreated?.Invoke(handle);

        public void RaiseHandleDestroyed() => HandleDestroyed?.Invoke();

        public void RaiseNativePointer(NativePointerEvent e) => NativePointer?.Invoke(e);
    }
}
