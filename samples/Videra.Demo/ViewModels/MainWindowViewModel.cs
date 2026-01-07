using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia.Media;
using Videra.Core.Graphics;
using Videra.Demo.Services;
using System;

namespace Videra.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IModelImporter _importer;
    private const float DegToRad = (float)(Math.PI / 180.0);
    private const float RadToDeg = (float)(180.0 / Math.PI);

    public MainWindowViewModel(IModelImporter importer)
    {
        _importer = importer;
    }

    public MainWindowViewModel() { }

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
        var models = await _importer.ImportModelsAsync();
        foreach (var model in models)
        {
            SceneObjects.Add(model);
            SelectedObject = model;
        }
    }
}