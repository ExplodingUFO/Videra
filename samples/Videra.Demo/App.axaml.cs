using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Videra.Demo.Services;
using Videra.Demo.ViewModels;
using Videra.Demo.Views;

namespace Videra.Demo;

public class App : Application
{
    // 你也可以做成 public static 方便全局访问
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // ✅ 添加全局异常处理
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Debug.WriteLine($"[UnhandledException] {ex?.Message}");
                Debug.WriteLine($"[StackTrace] {ex?.StackTrace}");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Debug.WriteLine($"[UnobservedTask] {e.Exception?.Message}");
                e.SetObserved();
            };
            
            // 避免 Avalonia 和 CommunityToolkit 重复验证
            DisableAvaloniaDataAnnotationValidation();

            // 1) 构建 Host（DI 容器）
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            // 2) 启动 Host（如果你有 HostedService/后台任务会需要）
            _host.Start();

            // 3) 从容器取出 MainWindow（MainWindow 的 DataContext 也由 DI 注入）
            desktop.MainWindow = _host.Services.GetRequiredService<MainWindow>();

            // 4) 退出时优雅停止
            desktop.Exit += OnDesktopExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>(); // 会自动注入 MainWindowViewModel
        services.AddSingleton<IModelImporter, AvaloniaModelImporter>();
    }

    private async void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_host is null) return;

        try
        {
            await _host.StopAsync();
        }
        finally
        {
            _host.Dispose();
            _host = null;
        }
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}
