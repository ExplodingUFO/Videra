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
    #region 网格控制

    // ==========================================
    // 网格控制 (Grid Settings)
    // ==========================================

    // 显隐
    [ObservableProperty] private bool _isGridVisible = true;

    // 高度
    [ObservableProperty] private decimal _gridHeight = 0;

    // 颜色
    [ObservableProperty] private Color _gridColor = Color.Parse("#66808080"); // 默认带一点透明的灰

    //// 当属性改变时，同步到 Engine (在 View 层通过 PropertyChanged 监听，或使用 partial method)
    //// 推荐使用 partial method 钩子
    //partial void OnIsGridVisibleChanged(bool value) => RequestGridUpdate();
    //partial void OnGridHeightChanged(decimal value) => RequestGridUpdate();
    //partial void OnGridColorChanged(Color value) => RequestGridUpdate();

    #endregion

    #region 渲染风格

    // ==========================================
    // 渲染风格 (Render Style)
    // ==========================================

    [ObservableProperty]
    private RenderStylePreset _renderStyle = RenderStylePreset.Realistic;

    // 可用预设列表 (用于 ComboBox)
    public IEnumerable<RenderStylePreset> AvailablePresets =>
        Enum.GetValues<RenderStylePreset>().Where(p => p != RenderStylePreset.Custom);

    #endregion

    #region 线框渲染

    // ==========================================
    // 线框渲染 (Wireframe)
    // ==========================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWireframeEnabled))]
    private WireframeMode _wireframeMode = WireframeMode.None;

    partial void OnWireframeModeChanged(WireframeMode value)
    {
        Console.WriteLine($"[MainWindowViewModel] WireframeMode changed to: {value}");
    }

    [ObservableProperty]
    private Color _wireframeColor = Colors.Black;

    // 可用线框模式列表
    public IEnumerable<WireframeMode> AvailableWireframeModes =>
        Enum.GetValues<WireframeMode>();

    // 是否启用线框（用于UI显隐控制）
    public bool IsWireframeEnabled => WireframeMode != WireframeMode.None;

    [RelayCommand]
    private void TestWireframe()
    {
        System.Diagnostics.Debug.WriteLine($"[TestWireframe] Current WireframeMode = {WireframeMode}");
        Console.WriteLine($"[TestWireframe] Current WireframeMode = {WireframeMode}");

        // 通过修改标题来确认按钮被点击（临时调试）
        var oldMode = WireframeMode;

        // 切换到AllEdges模式进行测试
        WireframeMode = WireframeMode == WireframeMode.None
            ? WireframeMode.AllEdges
            : WireframeMode.None;

        System.Diagnostics.Debug.WriteLine($"[TestWireframe] New WireframeMode = {WireframeMode}");
        Console.WriteLine($"[TestWireframe] New WireframeMode = {WireframeMode}");

        // 触发通知以确保UI更新
        OnPropertyChanged(nameof(WireframeMode));
        OnPropertyChanged(nameof(IsWireframeEnabled));
    }

    #endregion

    private readonly IModelImporter? _importer;
    private const float DegToRad = (float)(Math.PI / 180.0);
    private const float RadToDeg = (float)(180.0 / Math.PI);

    public MainWindowViewModel(IModelImporter importer)
    {
        _importer = importer;
    }

    public MainWindowViewModel() : this(null!) { }

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

    // =========================================================
    // 位置 (Position) - 直接映射
    // =========================================================
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

    // =========================================================
    // 旋转 (Rotation) - 【关键修改】弧度转角度
    // =========================================================
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

    // =========================================================
    // 缩放 (Scale)
    // =========================================================
    public decimal SelectedScale
    {
        get => (decimal)(SelectedObject?.Scale.X ?? 1.0f);
        set
        {
            if (SelectedObject == null) return;
            SelectedObject.Scale = new Vector3((float)value);
            OnPropertyChanged();
        }
    }

    private void UpdateTransform(Action<decimal> updateAction, decimal value)
    {
        if (SelectedObject != null)
        {
            updateAction(value);
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (_importer == null)
        {
            // 如果没有 importer，显示警告消息
            // TODO: 使用 Avalonia 的消息框或通知系统
            System.Diagnostics.Debug.WriteLine("[Videra] Import functionality not available - waiting for backend implementation");
            Console.WriteLine("[MainWindow] Import error: No importer service available");
            return;
        }
        
        Console.WriteLine("[MainWindow] Starting model import...");
        var models = await _importer.ImportModelsAsync();
        var modelList = models.ToList();
        Console.WriteLine($"[MainWindow] Imported {modelList.Count} models");
        
        foreach (var model in modelList)
        {
            Console.WriteLine($"[MainWindow] Adding model '{model.Name}' to scene");
            SceneObjects.Add(model);
            SelectedObject = model;
        }
        
        Console.WriteLine($"[MainWindow] Scene now has {SceneObjects.Count} objects");
    }
}