using System;
using Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Demo.ViewModels;

namespace Videra.Demo.Services;

public sealed class DemoSceneBootstrapper
{
    private const string AutoBackendLabel = "Auto (native backend preferred)";
    private bool _defaultSceneSeeded;

    public bool TryInitialize(
        MainWindowViewModel viewModel,
        TopLevel? topLevel,
        IResourceFactory? factory,
        GraphicsBackendPreference preferredBackend)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var backendLabel = preferredBackend == GraphicsBackendPreference.Auto
            ? AutoBackendLabel
            : preferredBackend.ToString();

        if (topLevel == null || factory == null)
        {
            viewModel.SetBackendStatus(false, backendLabel, "等待渲染后端和资源工厂准备完成...");
            return false;
        }

        if (!viewModel.HasImporter)
        {
            viewModel.AttachImporter(new AvaloniaModelImporter(topLevel, factory));
        }

        try
        {
            var seededDefaultScene = SeedDefaultSceneIfNeeded(viewModel, factory);
            var statusMessage = seededDefaultScene
                ? "渲染后端已就绪，已加载默认演示立方体。Auto 模式会按当前平台优先选择原生后端。"
                : "渲染后端已就绪，导入功能可用。Auto 模式会按当前平台优先选择原生后端。";
            viewModel.SetBackendStatus(true, backendLabel, statusMessage);
        }
        catch (Exception ex)
        {
            viewModel.SetBackendStatus(true, backendLabel, "渲染后端已就绪，但默认演示模型创建失败。");
            viewModel.SetStatusMessage($"默认演示模型创建失败：{ex.Message}");
        }

        return true;
    }

    private bool SeedDefaultSceneIfNeeded(MainWindowViewModel viewModel, IResourceFactory factory)
    {
        if (_defaultSceneSeeded || viewModel.SceneObjects.Count > 0)
        {
            _defaultSceneSeeded = true;
            return false;
        }

        var cube = DemoMeshFactory.CreateCube(factory);
        viewModel.SceneObjects.Add(cube);
        viewModel.SelectedObject = cube;
        _defaultSceneSeeded = true;
        return true;
    }
}
