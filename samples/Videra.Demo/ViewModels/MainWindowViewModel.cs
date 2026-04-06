using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform.Storage;
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

    #region 网格控制

    [ObservableProperty] private bool _isGridVisible = true;
    [ObservableProperty] private decimal _gridHeight = 0;
    [ObservableProperty] private Color _gridColor = Color.Parse("#66808080");

    #endregion

    #region 渲染风格

    [ObservableProperty]
    private RenderStylePreset _renderStyle = RenderStylePreset.Realistic;

    public IEnumerable<RenderStylePreset> AvailablePresets => _availablePresets;

    #endregion

    #region 线框渲染

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWireframeEnabled))]
    private WireframeMode _wireframeMode = WireframeMode.None;

    [ObservableProperty]
    private Color _wireframeColor = Colors.Black;

    public IEnumerable<WireframeMode> AvailableWireframeModes => _availableWireframeModes;

    public bool IsWireframeEnabled => WireframeMode != WireframeMode.None;

    #endregion

    private IModelImporter? _importer;
    private const float DegToRad = (float)(Math.PI / 180.0);
    private const float RadToDeg = (float)(180.0 / Math.PI);

    [ObservableProperty] private string _statusMessage = "等待渲染后端初始化...";
    [ObservableProperty] private bool _isBackendReady;
    [ObservableProperty] private string _backendDisplay = "Auto";

    public MainWindowViewModel()
    {
    }

    public ObservableCollection<Object3D> SceneObjects { get; } = new();

    [ObservableProperty] private Color _bgColor = Color.Parse("#1e1e1e");
    public CameraViewModel Camera { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPosX))]
    [NotifyPropertyChangedFor(nameof(SelectedPosY))]
    [NotifyPropertyChangedFor(nameof(SelectedPosZ))]
    [NotifyPropertyChangedFor(nameof(SelectedRotX))]
    [NotifyPropertyChangedFor(nameof(SelectedRotY))]
    [NotifyPropertyChangedFor(nameof(SelectedRotZ))]
    [NotifyPropertyChangedFor(nameof(SelectedScale))]
    private Object3D? _selectedObject;

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
            if (SelectedObject == null) return;
            SelectedObject.Scale = new Vector3((float)value);
            OnPropertyChanged();
            StatusMessage = $"已更新对象 {SelectedObject.Name} 的缩放";
        }
    }

    public void SetBackendStatus(bool isReady, string backendDisplay, string statusMessage)
    {
        IsBackendReady = isReady;
        BackendDisplay = backendDisplay;
        StatusMessage = statusMessage;
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

    [RelayCommand]
    private void TestWireframe()
    {
        WireframeMode = WireframeMode == WireframeMode.None
            ? WireframeMode.AllEdges
            : WireframeMode.None;

        StatusMessage = WireframeMode == WireframeMode.None
            ? "已关闭线框模式"
            : $"已切换到线框模式: {WireframeMode}";

        OnPropertyChanged(nameof(WireframeMode));
        OnPropertyChanged(nameof(IsWireframeEnabled));
    }

    private void UpdateTransform(Action<decimal> updateAction, decimal value)
    {
        if (SelectedObject != null)
        {
            updateAction(value);
            OnPropertyChanged();
            StatusMessage = $"已更新对象 {SelectedObject.Name} 的变换";
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (_importer == null)
        {
            StatusMessage = "导入功能暂不可用：渲染后端尚未准备好。";
            return;
        }

        StatusMessage = "正在导入模型...";
        var models = await _importer.ImportModelsAsync();
        var modelList = models.ToList();

        if (modelList.Count == 0)
        {
            StatusMessage = "未导入任何模型。";
            return;
        }

        foreach (var model in modelList)
        {
            SceneObjects.Add(model);
            SelectedObject = model;
        }

        StatusMessage = $"已导入 {modelList.Count} 个模型，当前场景共 {SceneObjects.Count} 个对象。";
    }
}
