using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Styles.Presets;
using Videra.Demo.Services;

namespace Videra.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const string WaitingForBackendStatusMessage =
        "Waiting for the rendering backend and resource factory to become ready.";
    private const string BackendReadyWithDefaultSceneStatusMessage =
        "Rendering backend is ready. Loaded the default demo cube and framed the scene.";
    private const string BackendReadyWithoutDefaultSceneStatusMessage =
        "Rendering backend is ready. Model import is available.";
    private const string DefaultSceneFailureStatusPrefix =
        "Rendering backend is ready, but the default demo cube could not be created. Model import remains available. Last error: ";
    private const string BackendInitializationFailureStatusPrefix =
        "Rendering backend initialization failed: ";

    private readonly IEnumerable<RenderStylePreset> _availablePresets =
        Enum.GetValues<RenderStylePreset>().Where(p => p != RenderStylePreset.Custom).ToArray();

    private readonly IEnumerable<WireframeMode> _availableWireframeModes =
        Enum.GetValues<WireframeMode>().ToArray();

    private readonly IEnumerable<PerformanceLabMode> _availablePerformanceLabModes =
        Enum.GetValues<PerformanceLabMode>().ToArray();

    private readonly IReadOnlyList<int> _availablePerformanceLabObjectCounts = [1000, 5000, 10000];

    private IModelImporter? _importer;
    private IDemoViewportActions? _viewportActions;
    private VideraBackendDiagnostics? _lastBackendDiagnostics;
    private RenderCapabilitySnapshot? _lastRenderCapabilities;
    private ModelLoadBatchResult? _lastLoadResult;
    private const float DegToRad = (float)(Math.PI / 180.0);
    private const float RadToDeg = (float)(180.0 / Math.PI);

    [ObservableProperty] private bool _isGridVisible = true;
    [ObservableProperty] private decimal _gridHeight;
    [ObservableProperty] private Color _gridColor = Color.Parse("#66808080");

    [ObservableProperty]
    private RenderStylePreset _renderStyle = RenderStylePreset.Realistic;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWireframeEnabled))]
    private WireframeMode _wireframeMode = WireframeMode.None;

    [ObservableProperty]
    private Color _wireframeColor = Colors.Black;

    [ObservableProperty] private string _statusMessage = WaitingForBackendStatusMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    [NotifyCanExecuteChangedFor(nameof(FrameAllCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetCameraCommand))]
    private bool _isBackendReady;

    [ObservableProperty] private string _backendDisplay = "Requested: Auto | Resolved: Auto";
    [ObservableProperty] private string _backendDetails = "Ready: false | Native host: false | Render loop: Dispatcher";
    [ObservableProperty] private string _scenePipelineMetrics = "Document v0 | Pending 0 | Resident 0 | Dirty 0 | Failed 0";
    [ObservableProperty] private string _importReport = DemoSupportReportBuilder.FormatImportReport(null);
    [ObservableProperty] private string _diagnosticsBundle = string.Empty;
    [ObservableProperty] private string _minimalReproduction = string.Empty;
    [ObservableProperty] private int _performanceLabObjectCount = 5000;
    [ObservableProperty] private PerformanceLabMode _performanceLabMode = PerformanceLabMode.InstanceBatch;
    [ObservableProperty] private bool _performanceLabPickable = true;
    [ObservableProperty] private string _performanceLabDiagnostics = "No performance lab dataset generated.";
    [ObservableProperty] private string _performanceLabSnapshot = "No performance lab snapshot generated.";
    [ObservableProperty] private Color _bgColor = Color.Parse("#1e1e1e");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPosX))]
    [NotifyPropertyChangedFor(nameof(SelectedPosY))]
    [NotifyPropertyChangedFor(nameof(SelectedPosZ))]
    [NotifyPropertyChangedFor(nameof(SelectedRotX))]
    [NotifyPropertyChangedFor(nameof(SelectedRotY))]
    [NotifyPropertyChangedFor(nameof(SelectedRotZ))]
    [NotifyPropertyChangedFor(nameof(SelectedScale))]
    private Object3D? _selectedObject;

    public IEnumerable<RenderStylePreset> AvailablePresets => _availablePresets;

    public IEnumerable<WireframeMode> AvailableWireframeModes => _availableWireframeModes;

    public IEnumerable<PerformanceLabMode> AvailablePerformanceLabModes => _availablePerformanceLabModes;

    public IReadOnlyList<int> AvailablePerformanceLabObjectCounts => _availablePerformanceLabObjectCounts;

    public bool IsWireframeEnabled => WireframeMode != WireframeMode.None;

    public ObservableCollection<Object3D> SceneObjects { get; } = new();

    public CameraViewModel Camera { get; } = new();

    public decimal SelectedPosX
    {
        get => (decimal)(SelectedObject?.Position.X ?? 0f);
        set => UpdateTransform(v => { var p = SelectedObject!.Position; p.X = (float)v; SelectedObject.Position = p; }, value);
    }

    public decimal SelectedPosY
    {
        get => (decimal)(SelectedObject?.Position.Y ?? 0f);
        set => UpdateTransform(v => { var p = SelectedObject!.Position; p.Y = (float)v; SelectedObject.Position = p; }, value);
    }

    public decimal SelectedPosZ
    {
        get => (decimal)(SelectedObject?.Position.Z ?? 0f);
        set => UpdateTransform(v => { var p = SelectedObject!.Position; p.Z = (float)v; SelectedObject.Position = p; }, value);
    }

    public decimal SelectedRotX
    {
        get => (decimal)((SelectedObject?.Rotation.X ?? 0f) * RadToDeg);
        set => UpdateTransform(v => { var r = SelectedObject!.Rotation; r.X = (float)v * DegToRad; SelectedObject.Rotation = r; }, value);
    }

    public decimal SelectedRotY
    {
        get => (decimal)((SelectedObject?.Rotation.Y ?? 0f) * RadToDeg);
        set => UpdateTransform(v => { var r = SelectedObject!.Rotation; r.Y = (float)v * DegToRad; SelectedObject.Rotation = r; }, value);
    }

    public decimal SelectedRotZ
    {
        get => (decimal)((SelectedObject?.Rotation.Z ?? 0f) * RadToDeg);
        set => UpdateTransform(v => { var r = SelectedObject!.Rotation; r.Z = (float)v * DegToRad; SelectedObject.Rotation = r; }, value);
    }

    public decimal SelectedScale
    {
        get => (decimal)(SelectedObject?.Scale.X ?? 1.0f);
        set
        {
            if (SelectedObject == null)
            {
                return;
            }

            SelectedObject.Scale = new Vector3((float)value);
            OnPropertyChanged();
            StatusMessage = $"Updated scale for {SelectedObject.Name}.";
        }
    }

    public void SetWaitingForBackend()
    {
        ApplyBackendStatus(isReady: false, WaitingForBackendStatusMessage);
    }

    public void SetBackendReadyWithDefaultScene()
    {
        ApplyBackendStatus(isReady: true, BackendReadyWithDefaultSceneStatusMessage);
    }

    public void SetBackendReadyWithoutDefaultScene()
    {
        ApplyBackendStatus(isReady: true, BackendReadyWithoutDefaultSceneStatusMessage);
    }

    public void SetBackendReadyWithDefaultSceneFailure(string errorMessage)
    {
        ApplyBackendStatus(
            isReady: true,
            $"{DefaultSceneFailureStatusPrefix}{NormalizeStatusError(errorMessage)}");
    }

    public void SetBackendInitializationFailed(string errorMessage)
    {
        ApplyBackendStatus(
            isReady: false,
            $"{BackendInitializationFailureStatusPrefix}{NormalizeStatusError(errorMessage)}");
    }

    public void UpdateBackendDiagnostics(VideraBackendDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        _lastBackendDiagnostics = diagnostics;

        BackendDisplay = $"Requested: {diagnostics.RequestedBackend} | Resolved: {diagnostics.ResolvedBackend}";
        var lastFrameFeatures = FormatFeatureList(diagnostics.LastFrameFeatureNames);
        var supportedFeatures = FormatFeatureList(diagnostics.SupportedRenderFeatureNames);

        var details = new List<string>
        {
            $"Ready: {diagnostics.IsReady}",
            $"Native host: {diagnostics.NativeHostBound}",
            $"Render loop: {diagnostics.RenderLoopMode}",
            $"Last frame features: {lastFrameFeatures}",
            $"Supported features: {supportedFeatures}"
        };

        if (diagnostics.IsUsingSoftwareFallback)
        {
            details.Add($"Fallback: {diagnostics.FallbackReason ?? "Software"}");
        }

        if (diagnostics.EnvironmentOverrideApplied)
        {
            details.Add("Environment override applied");
        }

        if (!string.IsNullOrWhiteSpace(diagnostics.LastInitializationError))
        {
            details.Add($"Last error: {diagnostics.LastInitializationError}");
        }

        BackendDetails = string.Join(" | ", details);
        ScenePipelineMetrics =
            $"Document v{diagnostics.SceneDocumentVersion} | Pending {diagnostics.PendingSceneUploads} ({diagnostics.PendingSceneUploadBytes / 1024d:N1} KB) | Resident {diagnostics.ResidentSceneObjects} | Dirty {diagnostics.DirtySceneObjects} | Failed {diagnostics.FailedSceneUploads} | Last upload {diagnostics.LastFrameUploadedObjects} obj / {diagnostics.LastFrameUploadedBytes / 1024d:N1} KB in {diagnostics.LastFrameUploadDuration.TotalMilliseconds:N1} ms | Budget {diagnostics.ResolvedUploadBudgetObjects} obj / {diagnostics.ResolvedUploadBudgetBytes / 1024d:N1} KB | LastFrameObjectCount: {diagnostics.LastFrameObjectCount} | LastFrameOpaqueObjectCount: {diagnostics.LastFrameOpaqueObjectCount} | LastFrameTransparentObjectCount: {diagnostics.LastFrameTransparentObjectCount}";
        RefreshSupportReports();
    }

    public void UpdateRenderCapabilities(RenderCapabilitySnapshot renderCapabilities)
    {
        _lastRenderCapabilities = renderCapabilities ?? throw new ArgumentNullException(nameof(renderCapabilities));
        RefreshSupportReports();
    }

    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
    }

    public void UpdatePerformanceLabReport(string diagnostics, string snapshot, string statusMessage)
    {
        PerformanceLabDiagnostics = diagnostics;
        PerformanceLabSnapshot = snapshot;
        StatusMessage = statusMessage;
    }

    public bool HasImporter => _importer is not null;

    public void AttachImporter(IModelImporter importer)
    {
        ArgumentNullException.ThrowIfNull(importer);
        _importer = importer;
        ImportCommand.NotifyCanExecuteChanged();
    }

    public void AttachViewportActions(IDemoViewportActions viewportActions)
    {
        _viewportActions = viewportActions ?? throw new ArgumentNullException(nameof(viewportActions));
        FrameAllCommand.NotifyCanExecuteChanged();
        ResetCameraCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanUseViewportActions))]
    private void FrameAll()
    {
        if (_viewportActions is null)
        {
            return;
        }

        var framed = _viewportActions.FrameAll();
        StatusMessage = framed
            ? "Framed all scene objects."
            : "No scene objects are available to frame.";
    }

    [RelayCommand(CanExecute = nameof(CanUseViewportActions))]
    private void ResetCamera()
    {
        if (_viewportActions is null)
        {
            return;
        }

        _viewportActions.ResetCamera();
        StatusMessage = "Camera reset to the default view.";
    }

    private bool CanUseViewportActions()
    {
        return IsBackendReady && _viewportActions is not null;
    }

    private void UpdateTransform(Action<decimal> updateAction, decimal value)
    {
        if (SelectedObject != null)
        {
            updateAction(value);
            OnPropertyChanged();
            StatusMessage = $"Updated transform for {SelectedObject.Name}.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanImportModels))]
    private async Task ImportAsync()
    {
        if (_importer == null)
        {
            StatusMessage = "Import is unavailable until the rendering backend is ready.";
            return;
        }

        StatusMessage = "Importing models...";
        var loadResult = await _importer.ImportModelsAsync();
        _lastLoadResult = loadResult;

        if (loadResult.Entries.Count == 0 && loadResult.Failures.Count == 0)
        {
            RefreshSupportReports();
            StatusMessage = "No models were selected.";
            return;
        }

        if (loadResult.Entries.Count > 0)
        {
            SelectedObject = SceneObjects.LastOrDefault();
        }

        StatusMessage = BuildImportStatus(loadResult);
        RefreshSupportReports();
    }

    private bool CanImportModels()
    {
        return IsBackendReady && _importer is not null;
    }

    private static string BuildImportStatus(ModelLoadBatchResult loadResult)
    {
        var importedCount = loadResult.Results.Count(static result => result.Imported);
        if (loadResult.Entries.Count == 0 && importedCount == 0)
        {
            var firstFailure = loadResult.Failures[0];
            return $"Import failed for {loadResult.Failures.Count} file(s). First error: {firstFailure.ErrorMessage}";
        }

        if (loadResult.Failures.Count == 0)
        {
            return $"Imported {loadResult.Entries.Count} model(s).";
        }

        var lastFailure = loadResult.Failures[^1];
        return $"{importedCount} file(s) imported, but the active scene was not replaced because {loadResult.Failures.Count} import(s) failed. Last error: {lastFailure.ErrorMessage}";
    }

    private void RefreshSupportReports()
    {
        var settings = CreateSupportSettings();
        ImportReport = DemoSupportReportBuilder.FormatImportReport(_lastLoadResult);
        DiagnosticsBundle = DemoSupportReportBuilder.BuildDiagnosticsBundle(
            _lastBackendDiagnostics,
            _lastRenderCapabilities,
            SceneObjects.Count,
            _lastLoadResult,
            settings);
        MinimalReproduction = DemoSupportReportBuilder.BuildMinimalReproduction(
            _lastBackendDiagnostics,
            _lastLoadResult,
            settings);
    }

    private DemoSupportSettings CreateSupportSettings()
    {
        return new DemoSupportSettings(
            RenderStyle,
            WireframeMode,
            IsGridVisible,
            GridHeight,
            GridColor.ToString(),
            BgColor.ToString(),
            Camera.InvertX,
            Camera.InvertY,
            SelectedObject?.Name);
    }

    private void ApplyBackendStatus(bool isReady, string statusMessage)
    {
        IsBackendReady = isReady;
        StatusMessage = statusMessage;
    }

    private static string NormalizeStatusError(string errorMessage)
    {
        return string.IsNullOrWhiteSpace(errorMessage)
            ? "unknown error"
            : errorMessage;
    }

    private static string FormatFeatureList(IReadOnlyList<string>? features)
    {
        return features is { Count: > 0 }
            ? string.Join(", ", features)
            : "None";
    }
}

public enum PerformanceLabMode
{
    NormalObjects,
    InstanceBatch
}
