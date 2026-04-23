using System.Numerics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Inspection;
using Videra.Core.Scene;
using Videra.Core.Selection.Annotations;
using Videra.Import.Obj;

namespace Videra.InteractionSample.Views;

public partial class MainWindow : Window
{
    private readonly TextBlock _contractStatusText;
    private readonly TextBlock _loadStatusText;
    private readonly TextBlock _sceneStatusText;
    private readonly TextBlock _selectionStatusText;
    private readonly TextBlock _annotationStatusText;
    private readonly TextBlock _measurementStatusText;
    private readonly TextBlock _inspectionStatusText;
    private readonly TextBlock _lastRequestText;
    private readonly Button _navigateModeButton;
    private readonly Button _selectModeButton;
    private readonly Button _annotateModeButton;
    private readonly Button _measureModeButton;
    private readonly Button _measurementSnapModeButton;
    private readonly Dictionary<Guid, string> _objectNames = new();
    private readonly List<Object3D> _sceneObjects = new();

    private VideraSelectionState _selectionState = new();
    private IReadOnlyList<VideraAnnotation> _annotations = Array.Empty<VideraAnnotation>();
    private VideraInspectionState? _savedInspectionState;
    private string? _lastInspectionBundleDirectory;

    private bool _sampleStarted;
    private bool _sectionPlaneEnabled;
    private int _annotationSequence = 1;
    private int _loadedObjectCount;
    private string _loadSummary = "Waiting for backend readiness before loading the focused interaction scene.";
    private string _inspectionSummary = "Toggle a section plane, save the current view state, export a snapshot, or export an inspection bundle after the scene loads. Check CanReplayScene and ReplayLimitation before importing a bundle.";
    private string _lastRequestSummary = "Switch modes, click objects, or click empty space to drive the public interaction flow.";

    public MainWindow()
    {
        InitializeComponent();

        _contractStatusText = this.FindControl<TextBlock>("ContractStatusText")
            ?? throw new InvalidOperationException("Contract status control is missing.");
        _loadStatusText = this.FindControl<TextBlock>("LoadStatusText")
            ?? throw new InvalidOperationException("Load status control is missing.");
        _sceneStatusText = this.FindControl<TextBlock>("SceneStatusText")
            ?? throw new InvalidOperationException("Scene status control is missing.");
        _selectionStatusText = this.FindControl<TextBlock>("SelectionStatusText")
            ?? throw new InvalidOperationException("Selection status control is missing.");
        _annotationStatusText = this.FindControl<TextBlock>("AnnotationStatusText")
            ?? throw new InvalidOperationException("Annotation status control is missing.");
        _measurementStatusText = this.FindControl<TextBlock>("MeasurementStatusText")
            ?? throw new InvalidOperationException("Measurement status control is missing.");
        _inspectionStatusText = this.FindControl<TextBlock>("InspectionStatusText")
            ?? throw new InvalidOperationException("Inspection status control is missing.");
        _lastRequestText = this.FindControl<TextBlock>("LastRequestText")
            ?? throw new InvalidOperationException("Last request control is missing.");
        _navigateModeButton = this.FindControl<Button>("NavigateModeButton")
            ?? throw new InvalidOperationException("Navigate mode button is missing.");
        _selectModeButton = this.FindControl<Button>("SelectModeButton")
            ?? throw new InvalidOperationException("Select mode button is missing.");
        _annotateModeButton = this.FindControl<Button>("AnnotateModeButton")
            ?? throw new InvalidOperationException("Annotate mode button is missing.");
        _measureModeButton = this.FindControl<Button>("MeasureModeButton")
            ?? throw new InvalidOperationException("Measure mode button is missing.");
        _measurementSnapModeButton = this.FindControl<Button>("MeasurementSnapModeButton")
            ?? throw new InvalidOperationException("Measurement snap mode button is missing.");

        View3D.InteractionOptions = new VideraInteractionOptions
        {
            EmptySpaceSelectionBehavior = VideraEmptySpaceSelectionBehavior.ClearSelection
        };
        View3D.SelectionState = _selectionState;
        View3D.Annotations = _annotations;
        View3D.Measurements = Array.Empty<VideraMeasurement>();
        View3D.InteractionOptions.MeasurementSnapMode = VideraMeasurementSnapMode.Free;
        View3D.ClippingPlanes = Array.Empty<VideraClipPlane>();
        View3D.InteractionMode = VideraInteractionMode.Navigate;
        View3D.SelectionRequested += OnSelectionRequested;
        View3D.AnnotationRequested += OnAnnotationRequested;
        View3D.BackendReady += OnSampleStartRequested;
        View3D.BackendStatusChanged += OnBackendStatusChanged;
        View3D.InitializationFailed += OnInitializationFailed;

        Opened += OnSampleStartRequested;
        Closed += OnClosed;

        UpdateModeButtons();
        UpdateStatusPanel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        Opened -= OnSampleStartRequested;
        Closed -= OnClosed;
        View3D.SelectionRequested -= OnSelectionRequested;
        View3D.AnnotationRequested -= OnAnnotationRequested;
        View3D.BackendReady -= OnSampleStartRequested;
        View3D.BackendStatusChanged -= OnBackendStatusChanged;
        View3D.InitializationFailed -= OnInitializationFailed;
    }

    private void OnSampleStartRequested(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        QueueSampleStart();
    }

    private void OnBackendStatusChanged(object? sender, VideraBackendStatusChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        UpdateStatusPanel();
    }

    private void OnInitializationFailed(object? sender, VideraBackendFailureEventArgs e)
    {
        _ = sender;
        _loadSummary = $"Backend initialization failed: {e.Exception.Message}";
        UpdateStatusPanel();
    }

    private Task TryRunSampleAsync()
    {
        if (_sampleStarted || !View3D.BackendDiagnostics.IsReady)
        {
            UpdateStatusPanel();
            return Task.CompletedTask;
        }

        _sampleStarted = true;

        try
        {
            var sceneObjects = CreateInteractionSceneObjects();
            ConfigureSceneObjects(sceneObjects);
            View3D.ReplaceScene(sceneObjects);
            SeedHostOwnedState();
            _loadedObjectCount = sceneObjects.Count;

            var framed = View3D.FrameAll();
            View3D.InvalidateVisual();

            _loadSummary =
                $"Imported {sceneObjects.Count} host-owned scene objects from Assets/reference-cube.obj through ObjModelImporter.Import(...) and SceneUploadCoordinator.CreateDeferredObject(...), then replaced the scene through View3D.ReplaceScene(...). " +
                $"FrameAll() returned {framed}. The host now owns SelectionState, Annotations, and annotation state for follow-up interaction.";
        }
        catch (Exception ex)
        {
            _loadSummary = $"Interaction sample flow failed: {ex.Message}";
        }
        finally
        {
            UpdateStatusPanel();
        }

        return Task.CompletedTask;
    }

    private void ConfigureSceneObjects(IReadOnlyList<Object3D> sceneObjects)
    {
        _objectNames.Clear();
        _sceneObjects.Clear();
        _sceneObjects.AddRange(sceneObjects);
        _loadedObjectCount = sceneObjects.Count;

        for (var index = 0; index < sceneObjects.Count; index++)
        {
            var sceneObject = sceneObjects[index];
            _objectNames[sceneObject.Id] = sceneObject.Name;
        }
    }

    private void SeedHostOwnedState()
    {
        if (_objectNames.Count == 0)
        {
            return;
        }

        var primaryObject = _sceneObjects[0];

        _selectionState = new VideraSelectionState
        {
            ObjectIds = [primaryObject.Id],
            PrimaryObjectId = primaryObject.Id
        };

        _annotationSequence = 1;
        _annotations =
        [
            new VideraNodeAnnotation
            {
                ObjectId = primaryObject.Id,
                Text = $"Object note {NextAnnotationIndex()}: {GetObjectLabel(primaryObject.Id)}",
                Color = Color.Parse("#0EA5E9")
            },
            new VideraWorldPointAnnotation
            {
                WorldPoint = new Vector3(0f, 0.95f, 0f),
                Text = $"World note {NextAnnotationIndex()}: origin marker",
                Color = Color.Parse("#F97316")
            }
        ];

        View3D.Measurements = Array.Empty<VideraMeasurement>();
        _savedInspectionState = null;
        _lastInspectionBundleDirectory = null;
        _sectionPlaneEnabled = false;
        View3D.InteractionOptions.MeasurementSnapMode = VideraMeasurementSnapMode.Free;
        View3D.ClippingPlanes = Array.Empty<VideraClipPlane>();
        PushHostState();
        _lastRequestSummary = "Seeded host-owned object selection plus one object-anchor note and one world-point note.";
        _inspectionSummary = "The inspection workflow is ready. Toggle a section plane, switch to Measure, or export a snapshot.";
    }

    private void OnSelectionRequested(object? sender, SelectionRequestedEventArgs e)
    {
        _ = sender;
        ApplySelectionRequest(e);
        _lastRequestSummary =
            $"SelectionRequested -> Operation={e.Operation}; Primary={FormatGuid(e.PrimaryObjectId)}; " +
            $"ObjectIds={FormatObjectList(e.ObjectIds)}";
        PushHostState();
    }

    private void OnAnnotationRequested(object? sender, AnnotationRequestedEventArgs e)
    {
        _ = sender;

        VideraAnnotation annotation = e.Anchor.Kind switch
        {
            AnnotationAnchorKind.Object when e.Anchor.ObjectId is Guid objectId => new VideraNodeAnnotation
            {
                ObjectId = objectId,
                Text = $"Object note {NextAnnotationIndex()}: {GetObjectLabel(objectId)}",
                Color = Color.Parse("#0EA5E9")
            },
            AnnotationAnchorKind.WorldPoint when e.Anchor.WorldPoint is Vector3 worldPoint => new VideraWorldPointAnnotation
            {
                WorldPoint = worldPoint,
                Text = $"World note {NextAnnotationIndex()}: ({worldPoint.X:0.00}, {worldPoint.Y:0.00}, {worldPoint.Z:0.00})",
                Color = Color.Parse("#F97316")
            },
            _ => throw new InvalidOperationException("Unsupported annotation anchor.")
        };

        _annotations = _annotations.Concat([annotation]).ToArray();
        _lastRequestSummary =
            $"AnnotationRequested -> Kind={e.Anchor.Kind}; ObjectId={FormatGuid(e.Anchor.ObjectId)}; " +
            $"WorldPoint={FormatWorldPoint(e.Anchor.WorldPoint)}";
        PushHostState();
    }

    private void ApplySelectionRequest(SelectionRequestedEventArgs e)
    {
        var selected = _selectionState.ObjectIds.Distinct().ToList();

        switch (e.Operation)
        {
            case VideraSelectionOperation.Replace:
                selected = e.ObjectIds.Distinct().ToList();
                break;
            case VideraSelectionOperation.Add:
                foreach (var objectId in e.ObjectIds)
                {
                    if (!selected.Contains(objectId))
                    {
                        selected.Add(objectId);
                    }
                }

                break;
            case VideraSelectionOperation.Toggle:
                foreach (var objectId in e.ObjectIds)
                {
                    if (!selected.Remove(objectId))
                    {
                        selected.Add(objectId);
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), e.Operation, "Unknown selection operation.");
        }

        Guid? primaryObjectId = null;
        if (selected.Count > 0)
        {
            primaryObjectId = e.PrimaryObjectId is Guid requestedPrimary && selected.Contains(requestedPrimary)
                ? requestedPrimary
                : selected[0];
        }

        _selectionState = new VideraSelectionState
        {
            ObjectIds = selected,
            PrimaryObjectId = primaryObjectId
        };
    }

    private void PushHostState()
    {
        View3D.SelectionState = _selectionState;
        View3D.Annotations = _annotations;
        UpdateStatusPanel();
    }

    private void PullHostOwnedStateFromView()
    {
        _selectionState = new VideraSelectionState
        {
            ObjectIds = View3D.SelectionState.ObjectIds.ToArray(),
            PrimaryObjectId = View3D.SelectionState.PrimaryObjectId
        };
        _annotations = View3D.Annotations.Select(static annotation => annotation switch
        {
            VideraNodeAnnotation node => (VideraAnnotation)new VideraNodeAnnotation
            {
                Id = node.Id,
                Text = node.Text,
                Color = node.Color,
                IsVisible = node.IsVisible,
                ObjectId = node.ObjectId
            },
            VideraWorldPointAnnotation world => new VideraWorldPointAnnotation
            {
                Id = world.Id,
                Text = world.Text,
                Color = world.Color,
                IsVisible = world.IsVisible,
                WorldPoint = world.WorldPoint
            },
            _ => throw new InvalidOperationException("Unsupported annotation type.")
        }).ToArray();
    }

    private void OnNavigateModeClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        View3D.InteractionMode = VideraInteractionMode.Navigate;
        _lastRequestSummary = "InteractionMode switched to Navigate. Drag and wheel input now stay on camera navigation.";
        UpdateModeButtons();
        UpdateStatusPanel();
    }

    private void OnSelectModeClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        View3D.InteractionMode = VideraInteractionMode.Select;
        _lastRequestSummary = "InteractionMode switched to Select. Click or drag the scene to receive SelectionRequested and update host-owned SelectionState.";
        UpdateModeButtons();
        UpdateStatusPanel();
    }

    private void OnAnnotateModeClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        View3D.InteractionMode = VideraInteractionMode.Annotate;
        _lastRequestSummary = "InteractionMode switched to Annotate. Click an object for an object anchor or empty space for a world-point anchor.";
        UpdateModeButtons();
        UpdateStatusPanel();
    }

    private void OnMeasureModeClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        View3D.InteractionMode = VideraInteractionMode.Measure;
        _lastRequestSummary = $"InteractionMode switched to Measure. Click two points to create a viewer-first distance measurement using {View3D.InteractionOptions.MeasurementSnapMode} snapping.";
        UpdateModeButtons();
        UpdateStatusPanel();
    }

    private void OnMeasurementSnapModeClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        var modes = Enum.GetValues<VideraMeasurementSnapMode>();
        var currentIndex = Array.IndexOf(modes, View3D.InteractionOptions.MeasurementSnapMode);
        var nextIndex = (currentIndex + 1) % modes.Length;
        View3D.InteractionOptions.MeasurementSnapMode = modes[nextIndex];
        _lastRequestSummary = $"MeasurementSnapMode switched to {View3D.InteractionOptions.MeasurementSnapMode}. Measure mode now snaps new anchors with that strategy.";
        UpdateModeButtons();
        UpdateStatusPanel();
    }

    private void OnClearSelectionClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _selectionState = new VideraSelectionState();
        _lastRequestSummary = "The host cleared SelectionState and pushed the empty state back into the control.";
        PushHostState();
    }

    private void OnClearAnnotationsClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _annotations = Array.Empty<VideraAnnotation>();
        _lastRequestSummary = "The host cleared the annotation state and pushed an empty annotation list back into the control.";
        PushHostState();
    }

    private void OnClearMeasurementsClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        View3D.Measurements = Array.Empty<VideraMeasurement>();
        _lastRequestSummary = "The host cleared the current measurement set from the public inspection surface.";
        UpdateStatusPanel();
    }

    private void OnReseedAnnotationsClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        if (_objectNames.Count == 0)
        {
            _lastRequestSummary = "Annotations cannot be reseeded until the sample scene is loaded.";
            UpdateStatusPanel();
            return;
        }

        SeedHostOwnedState();
    }

    private void OnToggleSectionPlaneClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _sectionPlaneEnabled = !_sectionPlaneEnabled;
        View3D.ClippingPlanes = _sectionPlaneEnabled
            ? [VideraClipPlane.FromPointNormal(Vector3.Zero, Vector3.UnitZ)]
            : Array.Empty<VideraClipPlane>();
        _inspectionSummary = _sectionPlaneEnabled
            ? "Section plane enabled. The active scene now clips against a world-space Z plane through the origin."
            : "Section plane disabled. The full scene is visible again.";
        UpdateStatusPanel();
    }

    private void OnCaptureInspectionStateClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _savedInspectionState = View3D.CaptureInspectionState();
        _inspectionSummary = "Captured the current inspection session, including camera, selection, annotations, clipping, and measurements.";
        UpdateStatusPanel();
    }

    private void OnRestoreInspectionStateClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        if (_savedInspectionState is null)
        {
            _inspectionSummary = "No saved inspection state is available yet.";
            UpdateStatusPanel();
            return;
        }

        View3D.ApplyInspectionState(_savedInspectionState);
        PullHostOwnedStateFromView();
        _sectionPlaneEnabled = View3D.ClippingPlanes.Count > 0;
        _inspectionSummary = "Restored the previously captured inspection state.";
        UpdateModeButtons();
        UpdateStatusPanel();
    }

    private async void OnExportSnapshotClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"videra-interaction-snapshot-{DateTime.UtcNow:yyyyMMddHHmmss}.png");
        var result = await View3D.ExportSnapshotAsync(outputPath).ConfigureAwait(true);
        _inspectionSummary = result.Succeeded
            ? $"ExportSnapshotAsync wrote {result.Path} at {result.Width}x{result.Height} in {result.Duration.TotalMilliseconds:N0} ms."
            : $"ExportSnapshotAsync failed: {result.Failure?.Message ?? "Unknown error."}";
        UpdateStatusPanel();
    }

    private async void OnExportInspectionBundleClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"videra-inspection-bundle-{DateTime.UtcNow:yyyyMMddHHmmss}");
        var result = await VideraInspectionBundleService.ExportAsync(View3D, outputDirectory).ConfigureAwait(true);
        _lastInspectionBundleDirectory = result.Succeeded ? result.DirectoryPath : _lastInspectionBundleDirectory;
        _inspectionSummary = result.Succeeded
            ? FormatInspectionBundleExportSummary(result)
            : $"VideraInspectionBundleService.ExportAsync failed: {result.FailureMessage ?? "Unknown error."}";
        UpdateStatusPanel();
    }

    private async void OnImportInspectionBundleClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        if (string.IsNullOrWhiteSpace(_lastInspectionBundleDirectory))
        {
            _inspectionSummary = "No exported inspection bundle is available yet.";
            UpdateStatusPanel();
            return;
        }

        var result = await VideraInspectionBundleService.ImportAsync(View3D, _lastInspectionBundleDirectory).ConfigureAwait(true);
        if (result.Succeeded)
        {
            PullHostOwnedStateFromView();
            _sectionPlaneEnabled = View3D.ClippingPlanes.Count > 0;
            _savedInspectionState = View3D.CaptureInspectionState();
            _inspectionSummary = $"VideraInspectionBundleService.ImportAsync restored the bundle from {_lastInspectionBundleDirectory}. Scene reloaded: {result.SceneReloaded}.";
            UpdateModeButtons();
        }
        else
        {
            _inspectionSummary = $"VideraInspectionBundleService.ImportAsync rejected the bundle from {_lastInspectionBundleDirectory}; current view state was not applied. Reason: {result.FailureMessage ?? "Unknown error."}";
        }

        UpdateStatusPanel();
    }

    private void UpdateStatusPanel()
    {
        _contractStatusText.Text =
            $"Current mode: {View3D.InteractionMode}. The host owns SelectionState, Annotations, and annotation state, " +
            "while VideraView translates that state into 3D highlight/render state, 2D label/feedback rendering, and viewer-first inspection workflows.";
        _loadStatusText.Text = _loadSummary;
        _sceneStatusText.Text = FormatSceneSummary();
        _selectionStatusText.Text = FormatSelectionSummary();
        _annotationStatusText.Text = FormatAnnotationSummary();
        _measurementStatusText.Text = FormatMeasurementSummary();
        _inspectionStatusText.Text = FormatInspectionSummary();
        _lastRequestText.Text = _lastRequestSummary;
    }

    private void UpdateModeButtons()
    {
        UpdateModeButton(_navigateModeButton, View3D.InteractionMode == VideraInteractionMode.Navigate);
        UpdateModeButton(_selectModeButton, View3D.InteractionMode == VideraInteractionMode.Select);
        UpdateModeButton(_annotateModeButton, View3D.InteractionMode == VideraInteractionMode.Annotate);
        UpdateModeButton(_measureModeButton, View3D.InteractionMode == VideraInteractionMode.Measure);
        _measurementSnapModeButton.Content = $"Snap: {View3D.InteractionOptions.MeasurementSnapMode}";
    }

    private static void UpdateModeButton(Button button, bool isActive)
    {
        button.Background = isActive
            ? new SolidColorBrush(Color.Parse("#A46C3B"))
            : new SolidColorBrush(Color.Parse("#F2DFC7"));
        button.Foreground = isActive
            ? Brushes.White
            : new SolidColorBrush(Color.Parse("#5A3B20"));
        button.BorderBrush = new SolidColorBrush(Color.Parse("#A46C3B"));
    }

    private string FormatSceneSummary()
    {
        if (_objectNames.Count == 0)
        {
            return _loadedObjectCount == 0
                ? "No interaction scene is loaded yet."
                : $"Loaded scene objects: {_loadedObjectCount}. Public object state is available for selection and annotation.";
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Backend ready: {View3D.BackendDiagnostics.IsReady}");
        builder.AppendLine($"Resolved backend: {View3D.BackendDiagnostics.ResolvedBackend}");
        builder.AppendLine("Loaded objects:");
        foreach (var sceneObject in _sceneObjects)
        {
            builder.AppendLine($"- {GetObjectLabel(sceneObject.Id)} at ({sceneObject.Position.X:0.00}, {sceneObject.Position.Y:0.00}, {sceneObject.Position.Z:0.00})");
        }

        return builder.ToString().TrimEnd();
    }

    private string FormatSelectionSummary()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"PrimaryObjectId: {FormatGuid(_selectionState.PrimaryObjectId)}");
        builder.AppendLine(_selectionState.ObjectIds.Count == 0
            ? "ObjectIds: <empty>"
            : $"ObjectIds: {FormatObjectList(_selectionState.ObjectIds)}");
        builder.Append("Selection is object-level and only changes when the host applies SelectionRequested.");
        return builder.ToString();
    }

    private string FormatAnnotationSummary()
    {
        if (_annotations.Count == 0)
        {
            return "No annotations. Click in Annotate mode to add object-anchor or world-point notes.";
        }

        var builder = new StringBuilder();
        foreach (var annotation in _annotations)
        {
            switch (annotation)
            {
                case VideraNodeAnnotation nodeAnnotation:
                    builder.AppendLine($"- {annotation.Text} -> VideraNodeAnnotation({GetObjectLabel(nodeAnnotation.ObjectId)})");
                    break;
                case VideraWorldPointAnnotation worldPointAnnotation:
                    builder.AppendLine(
                        $"- {annotation.Text} -> VideraWorldPointAnnotation({worldPointAnnotation.WorldPoint.X:0.00}, {worldPointAnnotation.WorldPoint.Y:0.00}, {worldPointAnnotation.WorldPoint.Z:0.00})");
                    break;
                default:
                    builder.AppendLine($"- {annotation.Text}");
                    break;
            }
        }

        builder.Append("The host owns the annotation list; overlay labels and markers reflect this state.");
        return builder.ToString();
    }

    private string FormatMeasurementSummary()
    {
        var measurements = View3D.Measurements;
        if (measurements.Count == 0)
        {
            return $"No measurements yet. Switch to Measure and click two points to create one. Current snap mode: {View3D.InteractionOptions.MeasurementSnapMode}.";
        }

        var builder = new StringBuilder();
        foreach (var measurement in measurements)
        {
            builder.AppendLine(
                $"- {measurement.Label ?? "Measurement"}: distance={measurement.Distance:0.00}, " +
                $"horizontal={measurement.HorizontalDistance:0.00}, height={measurement.HeightDelta:0.00}");
        }

        builder.Append($"Measurements stay on the public inspection surface, use {View3D.InteractionOptions.MeasurementSnapMode} for new anchors, and are included in captured view state.");
        return builder.ToString();
    }

    private string FormatInspectionSummary()
    {
        var diagnostics = View3D.BackendDiagnostics;
        var builder = new StringBuilder();
        builder.AppendLine(_inspectionSummary);
        builder.AppendLine($"Clipping active: {diagnostics.IsClippingActive} ({diagnostics.ActiveClippingPlaneCount} plane(s))");
        builder.AppendLine($"Measurements tracked: {diagnostics.MeasurementCount}");
        builder.AppendLine($"Saved view state: {(_savedInspectionState is null ? "Unavailable" : "Available")}");
        builder.AppendLine($"Last inspection bundle: {_lastInspectionBundleDirectory ?? "Unavailable"}");
        builder.Append($"Last snapshot export: {diagnostics.LastSnapshotExportStatus ?? "Unavailable"}");
        if (!string.IsNullOrWhiteSpace(diagnostics.LastSnapshotExportPath))
        {
            builder.Append($" @ {diagnostics.LastSnapshotExportPath}");
        }

        return builder.ToString();
    }

    private int NextAnnotationIndex()
    {
        return _annotationSequence++;
    }

    private void QueueSampleStart()
    {
        _ = TryRunSampleAsync();
    }

    private string GetObjectLabel(Guid objectId)
    {
        return _objectNames.TryGetValue(objectId, out var name)
            ? $"{name} ({objectId.ToString()[..8]})"
            : objectId.ToString()[..8];
    }

    private static string FormatObjectList(IEnumerable<Guid> objectIds)
    {
        return string.Join(", ", objectIds.Select(objectId => objectId.ToString()[..8]));
    }

    private static string FormatGuid(Guid? objectId)
    {
        return objectId.HasValue ? objectId.Value.ToString()[..8] : "<none>";
    }

    private static string FormatWorldPoint(Vector3? worldPoint)
    {
        return worldPoint is Vector3 point
            ? $"({point.X:0.00}, {point.Y:0.00}, {point.Z:0.00})"
            : "<none>";
    }

    private static string FormatInspectionBundleExportSummary(VideraInspectionBundleExportResult result)
    {
        var summary =
            $"VideraInspectionBundleService.ExportAsync wrote a bundle at {result.DirectoryPath}. CanReplayScene: {result.CanReplayScene}. Check replay status before import.";
        return string.IsNullOrWhiteSpace(result.ReplayLimitation)
            ? summary
            : $"{summary} Replay limitation: {result.ReplayLimitation}";
    }

    private static List<Object3D> CreateInteractionSceneObjects()
    {
        var firstObject = SceneUploadCoordinator.CreateDeferredObject(ObjModelImporter.Import("Assets/reference-cube.obj"));
        firstObject.Name = "Selection Cube A";
        firstObject.Position = new Vector3(-1.35f, 0f, 0f);

        var secondObject = SceneUploadCoordinator.CreateDeferredObject(ObjModelImporter.Import("Assets/reference-cube.obj"));
        secondObject.Name = "Selection Cube B";
        secondObject.Position = new Vector3(1.35f, 0f, 0f);

        return [firstObject, secondObject];
    }
}
