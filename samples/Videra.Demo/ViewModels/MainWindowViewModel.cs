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
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Styles.Presets;
using Videra.Demo.Services;

namespace Videra.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IEnumerable<RenderStylePreset> _availablePresets =
        Enum.GetValues<RenderStylePreset>().Where(p => p != RenderStylePreset.Custom).ToArray();

    private readonly IEnumerable<WireframeMode> _availableWireframeModes =
        Enum.GetValues<WireframeMode>().ToArray();

    private IModelImporter? _importer;
    private IDemoViewportActions? _viewportActions;
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

    [ObservableProperty] private string _statusMessage = "Waiting for rendering backend initialization...";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FrameAllCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetCameraCommand))]
    private bool _isBackendReady;

    [ObservableProperty] private string _backendDisplay = "Requested: Auto | Resolved: Auto";
    [ObservableProperty] private string _backendDetails = "Ready: false | Native host: false | Render loop: Dispatcher";
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

    public void SetBackendStatus(bool isReady, string statusMessage)
    {
        IsBackendReady = isReady;
        StatusMessage = statusMessage;
    }

    public void UpdateBackendDiagnostics(VideraBackendDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        BackendDisplay = $"Requested: {diagnostics.RequestedBackend} | Resolved: {diagnostics.ResolvedBackend}";

        var details = new List<string>
        {
            $"Ready: {diagnostics.IsReady}",
            $"Native host: {diagnostics.NativeHostBound}",
            $"Render loop: {diagnostics.RenderLoopMode}"
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
    }

    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
    }

    public bool HasImporter => _importer is not null;

    public void AttachImporter(IModelImporter importer)
    {
        ArgumentNullException.ThrowIfNull(importer);
        _importer = importer;
    }

    public void AttachViewportActions(IDemoViewportActions viewportActions)
    {
        _viewportActions = viewportActions ?? throw new ArgumentNullException(nameof(viewportActions));
        FrameAllCommand.NotifyCanExecuteChanged();
        ResetCameraCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void TestWireframe()
    {
        WireframeMode = WireframeMode == WireframeMode.None
            ? WireframeMode.AllEdges
            : WireframeMode.None;

        StatusMessage = WireframeMode == WireframeMode.None
            ? "Wireframe mode disabled."
            : $"Wireframe mode enabled: {WireframeMode}.";

        OnPropertyChanged(nameof(WireframeMode));
        OnPropertyChanged(nameof(IsWireframeEnabled));
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

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (_importer == null)
        {
            StatusMessage = "Import is unavailable until the rendering backend is ready.";
            return;
        }

        StatusMessage = "Importing models...";
        var loadResult = await _importer.ImportModelsAsync();

        if (loadResult.LoadedObjects.Count == 0 && loadResult.Failures.Count == 0)
        {
            StatusMessage = "No models were selected.";
            return;
        }

        if (loadResult.LoadedObjects.Count > 0)
        {
            SelectedObject = loadResult.LoadedObjects[^1];
        }

        StatusMessage = BuildImportStatus(loadResult);
    }

    private static string BuildImportStatus(ModelLoadBatchResult loadResult)
    {
        if (loadResult.LoadedObjects.Count == 0)
        {
            var firstFailure = loadResult.Failures[0];
            return $"Import failed for {loadResult.Failures.Count} file(s). First error: {firstFailure.ErrorMessage}";
        }

        if (loadResult.Failures.Count == 0)
        {
            return $"Imported {loadResult.LoadedObjects.Count} model(s).";
        }

        var lastFailure = loadResult.Failures[^1];
        return $"Imported {loadResult.LoadedObjects.Count} model(s); {loadResult.Failures.Count} failed. Last error: {lastFailure.ErrorMessage}";
    }
}
