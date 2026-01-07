using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Videra.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // 背景颜色 (Avalonia Color)
    [ObservableProperty] private Color _bgColor = Colors.Black;

    // 当前选中的模型
    [ObservableProperty] private ModelViewModel? _selectedModel;

    // 模型列表
    public ObservableCollection<ModelViewModel> Models { get; } = new();

    // 当 UI 颜色改变时，转换并同步给 Veldrid
    partial void OnBgColorChanged(Color value)
    {
        // 这里只是数据，View 层会监听并传给 Engine
    }
}