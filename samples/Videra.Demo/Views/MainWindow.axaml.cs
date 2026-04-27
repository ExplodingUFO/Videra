using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Videra.Avalonia.Controls;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Videra.Core.Selection;
using Videra.Demo.Services;
using Videra.Demo.ViewModels;

namespace Videra.Demo.Views;

public partial class MainWindow : Window
{
    private readonly DemoSceneBootstrapper _sceneBootstrapper;
    private readonly MainWindowViewModel _viewModel;
    private bool _viewModelInitialized;

    public MainWindow()
        : this(new MainWindowViewModel(), new DemoSceneBootstrapper())
    {
    }

    public MainWindow(MainWindowViewModel viewModel, DemoSceneBootstrapper sceneBootstrapper)
    {
        _viewModel = viewModel;
        _sceneBootstrapper = sceneBootstrapper;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        View3D.BackendReady -= OnBackendReady;
        View3D.BackendReady += OnBackendReady;
        View3D.BackendStatusChanged -= OnBackendStatusChanged;
        View3D.BackendStatusChanged += OnBackendStatusChanged;
        View3D.InitializationFailed -= OnInitializationFailed;
        View3D.InitializationFailed += OnInitializationFailed;
        _viewModel.UpdateBackendDiagnostics(View3D.BackendDiagnostics);
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        TryInitializeScene();
    }

    private void OnBackendReady(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(TryInitializeScene, DispatcherPriority.Background);
    }

    private void OnBackendStatusChanged(object? sender, VideraBackendStatusChangedEventArgs e)
    {
        _viewModel.UpdateBackendDiagnostics(e.Diagnostics);
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
    }

    private void OnInitializationFailed(object? sender, VideraBackendFailureEventArgs e)
    {
        _viewModel.UpdateBackendDiagnostics(e.Diagnostics);
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        _viewModel.SetBackendInitializationFailed(e.Exception.Message);
    }

    private void TryInitializeScene()
    {
        if (_viewModelInitialized)
        {
            return;
        }

        var initialized = _sceneBootstrapper.TryInitialize(
            _viewModel,
            TopLevel.GetTopLevel(this),
            View3D);

        if (!initialized)
        {
            return;
        }

        _viewModelInitialized = true;
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        View3D.BackendReady -= OnBackendReady;
    }

    private async void OnCopyDiagnosticsBundleClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        await CopySupportTextAsync(_viewModel.DiagnosticsBundle, "Copied diagnostics bundle to the clipboard.").ConfigureAwait(true);
    }

    private async void OnCopyMinimalReproClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        await CopySupportTextAsync(_viewModel.MinimalReproduction, "Copied minimal reproduction metadata to the clipboard.").ConfigureAwait(true);
    }

    private void OnGeneratePerformanceLabClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        var factory = View3D.GetResourceFactory();
        if (factory is null && _viewModel.PerformanceLabMode == PerformanceLabMode.NormalObjects)
        {
            _viewModel.SetStatusMessage("Performance Lab normal-object mode is unavailable until the resource factory is ready.");
            return;
        }

        try
        {
            var result = GeneratePerformanceLabDataset(factory);
            _viewModel.UpdateBackendDiagnostics(View3D.BackendDiagnostics);
            _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
            _viewModel.UpdatePerformanceLabReport(
                result.Diagnostics,
                result.Snapshot,
                $"Generated Performance Lab dataset: {result.Mode}, {result.ObjectCount:N0} object(s).");
        }
        catch (Exception ex)
        {
            _viewModel.SetStatusMessage($"Performance Lab generation failed: {ex.Message}");
        }
    }

    private async void OnCopyPerformanceLabSnapshotClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        await CopySupportTextAsync(_viewModel.PerformanceLabSnapshot, "Copied Performance Lab snapshot to the clipboard.").ConfigureAwait(true);
    }

    private async Task CopySupportTextAsync(string text, string successMessage)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is null)
        {
            _viewModel.SetStatusMessage("Clipboard is unavailable.");
            return;
        }

        await topLevel.Clipboard.SetTextAsync(text).ConfigureAwait(true);
        _viewModel.SetStatusMessage(successMessage);
    }

    private PerformanceLabResult GeneratePerformanceLabDataset(IResourceFactory? factory)
    {
        var scenario = _viewModel.SelectedPerformanceLabViewerScenario;
        var mode = _viewModel.PerformanceLabMode;
        var objectCount = scenario.ObjectCount;
        var pickable = _viewModel.PerformanceLabPickable;
        var meshData = CreatePerformanceLabMesh();
        var transforms = PerformanceLabViewerScenarios.CreateTransforms(scenario);
        var objectIds = PerformanceLabViewerScenarios.CreateObjectIds(scenario);
        var colors = PerformanceLabViewerScenarios.CreateColors(scenario);
        var buildStopwatch = Stopwatch.StartNew();

        View3D.ClearScene();
        _viewModel.SceneObjects.Clear();

        SceneHitTestRequest hitRequest;
        if (mode == PerformanceLabMode.NormalObjects)
        {
            var objects = CreatePerformanceLabObjects(factory!, meshData, transforms);
            foreach (var obj in objects)
            {
                _viewModel.SceneObjects.Add(obj);
            }

            _viewModel.SelectedObject = objects.FirstOrDefault();
            hitRequest = CreatePerformanceLabHitRequest(objects, []);
        }
        else
        {
            var material = new MaterialInstance(MaterialInstanceId.New(), "performance-lab-material", RgbaFloat.White);
            var mesh = new MeshPrimitive(MeshPrimitiveId.New(), "performance-lab-marker", meshData, material.Id);
            var descriptor = new InstanceBatchDescriptor(
                "performance-lab-markers",
                mesh,
                material,
                transforms,
                colors,
                objectIds: objectIds,
                pickable: pickable);
            View3D.AddInstanceBatch(descriptor);
            var batch = SceneDocument.Empty.AddInstanceBatch(descriptor).InstanceBatches[0];
            _viewModel.SelectedObject = null;
            hitRequest = CreatePerformanceLabHitRequest([], [batch]);
        }

        View3D.FrameAll();
        buildStopwatch.Stop();

        var pickStopwatch = Stopwatch.StartNew();
        var hit = pickable
            ? new SceneHitTestService().HitTest(hitRequest).PrimaryHit
            : null;
        pickStopwatch.Stop();

        var diagnostics = View3D.BackendDiagnostics;
        var diagnosticsText = BuildPerformanceLabDiagnostics(
            mode,
            scenario,
            objectCount,
            pickable,
            buildStopwatch.Elapsed,
            pickStopwatch.Elapsed,
            hit,
            diagnostics);
        var snapshot = BuildPerformanceLabSnapshot(diagnosticsText, diagnostics);
        return new PerformanceLabResult(mode, objectCount, diagnosticsText, snapshot);
    }

    private static SceneHitTestRequest CreatePerformanceLabHitRequest(
        IReadOnlyList<Object3D> objects,
        IReadOnlyList<InstanceBatchEntry> batches)
    {
        var camera = new OrbitCamera();
        camera.SetOrbit(new Vector3(0.5f, 0.5f, 0f), 10f, 0f, 0f);
        camera.UpdateProjection(800, 600);
        return new SceneHitTestRequest(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            objects,
            batches);
    }

    private static Object3D[] CreatePerformanceLabObjects(
        IResourceFactory factory,
        MeshData meshData,
        IReadOnlyList<Matrix4x4> transforms)
    {
        var objects = new Object3D[transforms.Count];
        for (var i = 0; i < transforms.Count; i++)
        {
            var obj = new Object3D { Name = $"Marker {i + 1:N0}" };
            obj.Initialize(factory, meshData);
            obj.Position = new Vector3(transforms[i].M41, transforms[i].M42, transforms[i].M43);
            objects[i] = obj;
        }

        return objects;
    }

    private static MeshData CreatePerformanceLabMesh()
    {
        const float size = 0.4f;
        return new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(size, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, size, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };
    }

    private static string BuildPerformanceLabDiagnostics(
        PerformanceLabMode mode,
        PerformanceLabViewerScenario scenario,
        int objectCount,
        bool pickable,
        TimeSpan buildDuration,
        TimeSpan pickDuration,
        SceneHitTestResult.SceneHit? hit,
        VideraBackendDiagnostics diagnostics)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"ScenarioId: {scenario.Id}");
        builder.AppendLine($"ScenarioName: {scenario.DisplayName}");
        builder.AppendLine($"ScenarioSize: {scenario.Size}");
        builder.AppendLine($"Mode: {mode}");
        builder.AppendLine($"ObjectCount: {objectCount:N0}");
        builder.AppendLine($"Pickable: {pickable}");
        builder.AppendLine($"BuildDurationMs: {buildDuration.TotalMilliseconds:N1}");
        builder.AppendLine($"PickLatencyMs: {pickDuration.TotalMilliseconds:N3}");
        builder.AppendLine($"PickHit: {hit is not null}");
        builder.AppendLine($"PickObjectId: {hit?.ObjectId.ToString() ?? "Unavailable"}");
        builder.AppendLine($"PickInstanceIndex: {hit?.InstanceIndex?.ToString() ?? "Unavailable"}");
        builder.AppendLine($"FrameTimeProxyMs: {buildDuration.TotalMilliseconds:N1}");
        builder.AppendLine($"DrawCalls: {FormatNullable(diagnostics.LastFrameDrawCallCount)}");
        builder.AppendLine($"UploadBytes: {diagnostics.LastFrameUploadBytes}");
        builder.AppendLine($"ResidentBytes: {diagnostics.ResidentResourceBytes}");
        builder.AppendLine($"SubmittedInstances: {FormatNullable(diagnostics.LastFrameInstanceCount)}");
        builder.AppendLine($"RetainedInstanceCount: {diagnostics.RetainedInstanceCount}");
        builder.AppendLine($"PickableObjectCount: {FormatNullable(diagnostics.PickableObjectCount)}");
        return builder.ToString().TrimEnd();
    }

    private static string BuildPerformanceLabSnapshot(
        string diagnosticsText,
        VideraBackendDiagnostics diagnostics)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Videra Performance Lab snapshot");
        builder.AppendLine($"GeneratedUtc: {DateTimeOffset.UtcNow:O}");
        builder.AppendLine();
        builder.AppendLine(diagnosticsText);
        builder.AppendLine();
        builder.AppendLine("Backend diagnostics");
        builder.AppendLine(VideraDiagnosticsSnapshotFormatter.Format(diagnostics));
        return builder.ToString().TrimEnd();
    }

    private static string FormatNullable(int? value)
    {
        return value?.ToString() ?? "Unavailable";
    }

    private sealed record PerformanceLabResult(
        PerformanceLabMode Mode,
        int ObjectCount,
        string Diagnostics,
        string Snapshot);
}
