using CommunityToolkit.Mvvm.ComponentModel; // 推荐使用这个包，或者手写 INPC
using System.Numerics;
using System.Xml.Linq;
using Videra.Core.Graphics;

namespace Videra.Demo.ViewModels;

// 包装 Object3D，提供绑定属性
public partial class ModelViewModel : ObservableObject
{
    private readonly Object3D _model;

    public ModelViewModel(Object3D model)
    {
        _model = model;
        Name = model.Name;
    }

    [ObservableProperty] private string _name;

    // 位置
    public float PosX { get => _model.Position.X; set { var p = _model.Position; p.X = value; _model.Position = p; OnPropertyChanged(); } }
    public float PosY { get => _model.Position.Y; set { var p = _model.Position; p.Y = value; _model.Position = p; OnPropertyChanged(); } }
    public float PosZ { get => _model.Position.Z; set { var p = _model.Position; p.Z = value; _model.Position = p; OnPropertyChanged(); } }

    // 缩放
    public float Scale { get => _model.Scale.X; set { _model.Scale = new Vector3(value); OnPropertyChanged(); } }

    // 旋转 (简单演示：Y轴旋转)
    public float RotY { get => _model.Rotation.Y; set { var r = _model.Rotation; r.Y = value; _model.Rotation = r; OnPropertyChanged(); } }
}