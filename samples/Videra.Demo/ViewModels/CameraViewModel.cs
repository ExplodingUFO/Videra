using CommunityToolkit.Mvvm.ComponentModel;

// 如果需要直接引用 Core 类型

namespace Videra.Demo.ViewModels;

public partial class CameraViewModel : ObservableObject
{
    // 控制反转 (绑定到 VideraView.CameraInvertX)
    [ObservableProperty] private bool _invertX;
    [ObservableProperty] private bool _invertY;

    // 控制速度
    [ObservableProperty] private float _moveSpeed = 1.0f;

    // 你甚至可以在这里显示相机位置 (需要从 Engine 反向同步，或者单向绑定)
    [ObservableProperty] private string _positionInfo = "0, 0, 0";

    // 重置相机命令 (略)
}