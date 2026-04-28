using System.Numerics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Videra.AvaloniaWorkbenchSample;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Inspection;
using Videra.Core.Scene;
using Videra.Import.Obj;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

namespace Videra.AvaloniaWorkbenchSample.Views;

public partial class MainWindow : Window
{
    private static readonly Guid MarkerAId = Guid.Parse("a8e26a98-c1f1-4f3e-99d0-db3dbf7f7d7a");
    private static readonly Guid MarkerBId = Guid.Parse("dbd36a25-e9b1-4498-8d48-7d8e063f58dc");
    private static readonly Guid MarkerCId = Guid.Parse("90399a54-33af-4e94-8b27-c8b2040311f0");

    private readonly Button _loadAuthoredSceneButton;
    private readonly Button _loadReferenceAssetButton;
    private readonly Button _frameAllButton;
    private readonly Button _resetCameraButton;
    private readonly Button _refreshSnapshotButton;
    private readonly Button _copySupportCaptureButton;
    private readonly TextBlock _sceneEvidenceText;
    private readonly TextBlock _supportStatusText;
    private readonly TextBlock _diagnosticsText;
    private readonly TextBlock _chartPrecisionText;
    private WorkbenchSceneEvidence _activeSceneEvidence = new(
        "No scene loaded.",
        "none",
        SceneVersion: null,
        NodeCount: 0,
        PrimitiveCount: 0,
        InstanceBatchCount: 0,
        InstanceCount: 0,
        SelectedMarkerId: null);
    private readonly WorkbenchChartEvidence _chartEvidence = CreateChartPrecisionEvidence();
    private string _latestDiagnosticsSnapshot = string.Empty;

    public MainWindow()
    {
        InitializeComponent();

        _loadAuthoredSceneButton = this.FindControl<Button>("LoadAuthoredSceneButton")
            ?? throw new InvalidOperationException("LoadAuthoredSceneButton is missing.");
        _loadReferenceAssetButton = this.FindControl<Button>("LoadReferenceAssetButton")
            ?? throw new InvalidOperationException("LoadReferenceAssetButton is missing.");
        _frameAllButton = this.FindControl<Button>("FrameAllButton")
            ?? throw new InvalidOperationException("FrameAllButton is missing.");
        _resetCameraButton = this.FindControl<Button>("ResetCameraButton")
            ?? throw new InvalidOperationException("ResetCameraButton is missing.");
        _refreshSnapshotButton = this.FindControl<Button>("RefreshSnapshotButton")
            ?? throw new InvalidOperationException("RefreshSnapshotButton is missing.");
        _copySupportCaptureButton = this.FindControl<Button>("CopySupportCaptureButton")
            ?? throw new InvalidOperationException("CopySupportCaptureButton is missing.");
        _sceneEvidenceText = this.FindControl<TextBlock>("SceneEvidenceText")
            ?? throw new InvalidOperationException("SceneEvidenceText is missing.");
        _supportStatusText = this.FindControl<TextBlock>("SupportStatusText")
            ?? throw new InvalidOperationException("SupportStatusText is missing.");
        _diagnosticsText = this.FindControl<TextBlock>("DiagnosticsText")
            ?? throw new InvalidOperationException("DiagnosticsText is missing.");
        _chartPrecisionText = this.FindControl<TextBlock>("ChartPrecisionText")
            ?? throw new InvalidOperationException("ChartPrecisionText is missing.");

        View3D.Options = new VideraViewOptions
        {
            Backend =
            {
                PreferredBackend = GraphicsBackendPreference.Auto
            }
        }.UseModelImporter(ObjModelImporter.Create());

        View3D.BackendReady += OnBackendReady;
        View3D.BackendStatusChanged += OnBackendStatusChanged;
        View3D.InitializationFailed += OnInitializationFailed;

        _loadAuthoredSceneButton.Click += OnLoadAuthoredSceneClicked;
        _loadReferenceAssetButton.Click += OnLoadReferenceAssetClicked;
        _frameAllButton.Click += OnFrameAllClicked;
        _resetCameraButton.Click += OnResetCameraClicked;
        _refreshSnapshotButton.Click += OnRefreshSnapshotClicked;
        _copySupportCaptureButton.Click += OnCopySupportCaptureClicked;

        Opened += OnOpened;
        Closed += OnClosed;

        _sceneEvidenceText.Text = WorkbenchSupportCapture.FormatSceneEvidence(_activeSceneEvidence);
        _supportStatusText.Text = "Diagnostics are captured on explicit refresh, backend status changes, and support-copy actions.";
        _chartPrecisionText.Text = WorkbenchSupportCapture.FormatChartEvidence(_chartEvidence);
        RefreshDiagnosticsSnapshot();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        LoadAuthoredScene();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Opened -= OnOpened;
        Closed -= OnClosed;
        View3D.BackendReady -= OnBackendReady;
        View3D.BackendStatusChanged -= OnBackendStatusChanged;
        View3D.InitializationFailed -= OnInitializationFailed;
        _loadAuthoredSceneButton.Click -= OnLoadAuthoredSceneClicked;
        _loadReferenceAssetButton.Click -= OnLoadReferenceAssetClicked;
        _frameAllButton.Click -= OnFrameAllClicked;
        _resetCameraButton.Click -= OnResetCameraClicked;
        _refreshSnapshotButton.Click -= OnRefreshSnapshotClicked;
        _copySupportCaptureButton.Click -= OnCopySupportCaptureClicked;
    }

    private void OnBackendReady(object? sender, EventArgs e)
    {
        RefreshDiagnosticsSnapshot();
    }

    private void OnBackendStatusChanged(object? sender, VideraBackendStatusChangedEventArgs e)
    {
        RefreshDiagnosticsSnapshot(e.Diagnostics);
    }

    private void OnInitializationFailed(object? sender, VideraBackendFailureEventArgs e)
    {
        _supportStatusText.Text = $"Backend initialization failed: {e.Exception.Message}";
        RefreshDiagnosticsSnapshot(e.Diagnostics);
    }

    private void OnLoadAuthoredSceneClicked(object? sender, EventArgs e)
    {
        LoadAuthoredScene();
    }

    private async void OnLoadReferenceAssetClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await View3D.LoadModelAsync("Assets/reference-cube.obj").ConfigureAwait(true);
            var importedAsset = result.Entry?.ImportedAsset;
            _activeSceneEvidence = result.Succeeded
                ? new WorkbenchSceneEvidence(
                    $"OBJ evidence loaded in {result.Duration.TotalMilliseconds:N0} ms.",
                    importedAsset?.Name ?? result.Entry?.Name ?? "reference-cube.obj",
                    SceneVersion: null,
                    NodeCount: importedAsset?.Nodes.Count ?? 0,
                    PrimitiveCount: importedAsset?.Primitives.Count ?? 0,
                    InstanceBatchCount: 0,
                    InstanceCount: 0,
                    SelectedMarkerId: null)
                : new WorkbenchSceneEvidence(
                    $"OBJ evidence failed: {result.Failure?.ErrorMessage ?? "Unknown load failure."}",
                    "reference-cube.obj",
                    SceneVersion: null,
                    NodeCount: 0,
                    PrimitiveCount: 0,
                    InstanceBatchCount: 0,
                    InstanceCount: 0,
                    SelectedMarkerId: null);
        }
        catch (Exception ex)
        {
            _activeSceneEvidence = new WorkbenchSceneEvidence(
                $"OBJ evidence failed: {ex.Message}",
                "reference-cube.obj",
                SceneVersion: null,
                NodeCount: 0,
                PrimitiveCount: 0,
                InstanceBatchCount: 0,
                InstanceCount: 0,
                SelectedMarkerId: null);
        }

        _sceneEvidenceText.Text = WorkbenchSupportCapture.FormatSceneEvidence(_activeSceneEvidence);
        View3D.FrameAll();
        RefreshDiagnosticsSnapshot();
    }

    private void OnFrameAllClicked(object? sender, EventArgs e)
    {
        var framed = View3D.FrameAll();
        _supportStatusText.Text = $"FrameAll() returned {framed}.";
        RefreshDiagnosticsSnapshot();
    }

    private void OnResetCameraClicked(object? sender, EventArgs e)
    {
        View3D.ResetCamera();
        _supportStatusText.Text = "ResetCamera() restored the default camera.";
        RefreshDiagnosticsSnapshot();
    }

    private void OnRefreshSnapshotClicked(object? sender, EventArgs e)
    {
        RefreshDiagnosticsSnapshot();
        _supportStatusText.Text = "Diagnostics snapshot refreshed.";
    }

    private async void OnCopySupportCaptureClicked(object? sender, EventArgs e)
    {
        RefreshDiagnosticsSnapshot();
        var supportCapture = CreateSupportCapture();
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(supportCapture).ConfigureAwait(true);
            _supportStatusText.Text = "Copied support capture to the clipboard.";
            return;
        }

        _supportStatusText.Text = "Clipboard is unavailable. The support capture is visible in diagnostics.";
        _diagnosticsText.Text = supportCapture;
    }

    private void LoadAuthoredScene()
    {
        var document = CreateAuthoredSceneDocument();
        var importedAsset = document.Entries.Single().ImportedAsset
            ?? throw new InvalidOperationException("Authored workbench scene did not produce imported asset evidence.");

        View3D.ClearScene();
        View3D.AddObject(SceneUploadCoordinator.CreateDeferredObject(importedAsset));
        foreach (var batch in document.InstanceBatches)
        {
            View3D.AddInstanceBatch(new InstanceBatchDescriptor(
                batch.Name,
                batch.Mesh,
                batch.Material,
                batch.Transforms,
                batch.Colors,
                batch.ObjectIds,
                batch.Pickable));
        }

        View3D.SelectionState = new VideraSelectionState
        {
            ObjectIds = [MarkerAId],
            PrimaryObjectId = MarkerAId
        };
        View3D.Annotations =
        [
            new VideraWorldPointAnnotation
            {
                Text = "Authored focus",
                WorldPoint = new Vector3(0f, 0.62f, 0f),
                Color = Colors.White
            }
        ];
        View3D.Measurements =
        [
            new VideraMeasurement
            {
                Label = "Marker span",
                Start = VideraMeasurementAnchor.ForObjectPoint(MarkerAId, new Vector3(-1.2f, 0.2f, -0.6f)),
                End = VideraMeasurementAnchor.ForObjectPoint(MarkerCId, new Vector3(1.2f, 0.2f, -0.6f))
            }
        ];

        _activeSceneEvidence = new WorkbenchSceneEvidence(
            "Authored evidence loaded.",
            importedAsset.Name,
            document.Version,
            importedAsset.Nodes.Count,
            importedAsset.Primitives.Count,
            document.InstanceBatches.Count,
            document.InstanceBatches.Sum(static batch => batch.InstanceCount),
            MarkerAId);
        _sceneEvidenceText.Text = WorkbenchSupportCapture.FormatSceneEvidence(_activeSceneEvidence);
        View3D.FrameAll();
        RefreshDiagnosticsSnapshot();
    }

    private void RefreshDiagnosticsSnapshot()
    {
        RefreshDiagnosticsSnapshot(View3D.BackendDiagnostics);
    }

    private void RefreshDiagnosticsSnapshot(VideraBackendDiagnostics diagnostics)
    {
        _latestDiagnosticsSnapshot = VideraDiagnosticsSnapshotFormatter.Format(diagnostics);
        _diagnosticsText.Text = _latestDiagnosticsSnapshot;
    }

    private string CreateSupportCapture()
    {
        return WorkbenchSupportCapture.FormatSupportCapture(
            DateTimeOffset.UtcNow,
            _activeSceneEvidence,
            _chartEvidence,
            _latestDiagnosticsSnapshot);
    }

    private static SceneDocument CreateAuthoredSceneDocument()
    {
        var ground = SceneMaterials.Matte("workbench-ground", RgbaFloat.LightGrey);
        var focus = SceneMaterials.Metal("workbench-focus", new RgbaFloat(0.2f, 0.45f, 0.9f, 1f));
        var marker = SceneMaterials.Matte("workbench-marker", RgbaFloat.White);
        var markerTransforms = new[]
        {
            Matrix4x4.CreateTranslation(-1.2f, 0.2f, -0.6f),
            Matrix4x4.CreateTranslation(0f, 0.2f, 0.6f),
            Matrix4x4.CreateTranslation(1.2f, 0.2f, -0.6f)
        };
        var markerColors = new[]
        {
            RgbaFloat.Red,
            RgbaFloat.Green,
            RgbaFloat.Blue
        };
        var markerObjectIds = new[]
        {
            MarkerAId,
            MarkerBId,
            MarkerCId
        };

        return SceneAuthoring.Create("avalonia-workbench")
            .AddPlane("ground", ground, width: 4f, depth: 3f)
            .AddGrid("grid", SceneMaterials.Matte("grid", RgbaFloat.DarkGrey), width: 4f, depth: 3f, divisions: 4)
            .AddAxisTriad("origin-axis", length: 0.9f)
            .AddSphere("focus", focus, radius: 0.35f, transform: Matrix4x4.CreateTranslation(0f, 0.35f, 0f))
            .AddInstances(
                "marker-spheres",
                SceneGeometry.Sphere(radius: 0.12f, segments: 12, rings: 6, color: marker.BaseColorFactor),
                marker,
                markerTransforms,
                markerColors,
                markerObjectIds,
                pickable: true)
            .Build();
    }

    private static WorkbenchChartEvidence CreateChartPrecisionEvidence()
    {
        var engineering = SurfaceChartNumericLabelPresets.Engineering(precision: 2);
        var scientific = SurfaceChartNumericLabelPresets.Scientific(precision: 3);
        var fixedPoint = SurfaceChartNumericLabelPresets.Fixed(precision: 4);
        var palette = SurfaceColorMapPresets.CreateProfessional();

        return new WorkbenchChartEvidence(
            "Engineering precision 2; Scientific precision 3; Fixed precision 4",
            "SurfaceColorMapPresets.CreateProfessional()",
            Enumerable.Range(0, palette.Count)
                .Select(index => WorkbenchSupportCapture.FormatPaletteColor(palette[index]))
                .ToArray(),
            [
                $"Engineering X 12345.6789 -> {engineering.FormatLabel("X", 12345.6789)}",
                $"Scientific Legend 0.000456789 -> {scientific.FormatLabel("Legend", 0.000456789)}",
                $"Fixed Value 12.3456789 -> {fixedPoint.FormatLabel("Y", 12.3456789)}"
            ]);
    }
}
