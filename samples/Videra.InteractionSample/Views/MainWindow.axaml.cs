using System.Numerics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Graphics;
using Videra.Core.Selection.Annotations;

namespace Videra.InteractionSample.Views;

public partial class MainWindow : Window
{
    private readonly TextBlock _contractStatusText;
    private readonly TextBlock _loadStatusText;
    private readonly TextBlock _sceneStatusText;
    private readonly TextBlock _selectionStatusText;
    private readonly TextBlock _annotationStatusText;
    private readonly TextBlock _lastRequestText;
    private readonly Button _navigateModeButton;
    private readonly Button _selectModeButton;
    private readonly Button _annotateModeButton;
    private readonly Dictionary<Guid, string> _objectNames = new();
    private readonly List<Object3D> _sceneObjects = new();

    private VideraSelectionState _selectionState = new();
    private IReadOnlyList<VideraAnnotation> _annotations = Array.Empty<VideraAnnotation>();

    private bool _sampleStarted;
    private int _annotationSequence = 1;
    private string _loadSummary = "Waiting for backend readiness before loading the focused interaction scene.";
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
        _lastRequestText = this.FindControl<TextBlock>("LastRequestText")
            ?? throw new InvalidOperationException("Last request control is missing.");
        _navigateModeButton = this.FindControl<Button>("NavigateModeButton")
            ?? throw new InvalidOperationException("Navigate mode button is missing.");
        _selectModeButton = this.FindControl<Button>("SelectModeButton")
            ?? throw new InvalidOperationException("Select mode button is missing.");
        _annotateModeButton = this.FindControl<Button>("AnnotateModeButton")
            ?? throw new InvalidOperationException("Annotate mode button is missing.");

        View3D.InteractionOptions = new VideraInteractionOptions
        {
            EmptySpaceSelectionBehavior = VideraEmptySpaceSelectionBehavior.ClearSelection
        };
        View3D.SelectionState = _selectionState;
        View3D.Annotations = _annotations;
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

    private async Task TryRunSampleAsync()
    {
        if (_sampleStarted || !View3D.BackendDiagnostics.IsReady)
        {
            UpdateStatusPanel();
            return;
        }

        _sampleStarted = true;

        try
        {
            var result = await View3D.LoadModelsAsync(
                [
                    "Assets/reference-cube.obj",
                    "Assets/reference-cube.obj"
                ]).ConfigureAwait(true);

            if (!result.Succeeded || result.LoadedObjects.Count < 2)
            {
                var failureSummary = result.Failures.Count == 0
                    ? "Expected two cubes, but one or more scene objects did not load."
                    : string.Join(" | ", result.Failures.Select(failure => $"{failure.Path}: {failure.ErrorMessage}"));
                _loadSummary = $"LoadModelsAsync(...) did not prepare the interaction scene: {failureSummary}";
                UpdateStatusPanel();
                return;
            }

            ConfigureLoadedObjects(result.LoadedObjects);
            SeedHostOwnedState();

            var framed = View3D.FrameAll();
            View3D.InvalidateVisual();

            _loadSummary =
                $"LoadModelsAsync(...) loaded {result.LoadedObjects.Count} cubes in {result.Duration.TotalMilliseconds:N0} ms. " +
                $"FrameAll() returned {framed}. The host now owns SelectionState and annotation state for follow-up interaction.";
        }
        catch (Exception ex)
        {
            _loadSummary = $"Interaction sample flow failed: {ex.Message}";
        }
        finally
        {
            UpdateStatusPanel();
        }
    }

    private void ConfigureLoadedObjects(IReadOnlyList<Object3D> loadedObjects)
    {
        _objectNames.Clear();
        _sceneObjects.Clear();
        _sceneObjects.AddRange(loadedObjects);

        var layout = new[]
        {
            ("Selection Cube A", new Vector3(-1.35f, 0f, 0f)),
            ("Selection Cube B", new Vector3(1.35f, 0f, 0f))
        };

        for (var index = 0; index < loadedObjects.Count; index++)
        {
            var sceneObject = loadedObjects[index];
            var target = layout[Math.Min(index, layout.Length - 1)];
            sceneObject.Name = target.Item1;
            sceneObject.Position = target.Item2;
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

        PushHostState();
        _lastRequestSummary = "Seeded host-owned object selection plus one object/node note and one world-point note.";
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
        _lastRequestSummary = "InteractionMode switched to Annotate. Click an object for a node anchor or empty space for a world-point anchor.";
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

    private void UpdateStatusPanel()
    {
        _contractStatusText.Text =
            $"Current mode: {View3D.InteractionMode}. The host owns SelectionState and annotation state, " +
            "while VideraView translates that state into 3D highlight/render state and 2D label/feedback rendering.";
        _loadStatusText.Text = _loadSummary;
        _sceneStatusText.Text = FormatSceneSummary();
        _selectionStatusText.Text = FormatSelectionSummary();
        _annotationStatusText.Text = FormatAnnotationSummary();
        _lastRequestText.Text = _lastRequestSummary;
    }

    private void UpdateModeButtons()
    {
        UpdateModeButton(_navigateModeButton, View3D.InteractionMode == VideraInteractionMode.Navigate);
        UpdateModeButton(_selectModeButton, View3D.InteractionMode == VideraInteractionMode.Select);
        UpdateModeButton(_annotateModeButton, View3D.InteractionMode == VideraInteractionMode.Annotate);
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
            return "No interaction scene is loaded yet.";
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
            return "No annotations. Click in Annotate mode to add object/node or world-point notes.";
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
}
