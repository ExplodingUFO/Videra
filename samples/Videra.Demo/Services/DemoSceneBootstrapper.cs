using System;
using Avalonia.Controls;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics.Abstractions;
using Videra.Demo.ViewModels;

namespace Videra.Demo.Services;

public sealed class DemoSceneBootstrapper
{
    private bool _defaultSceneSeeded;

    public bool TryInitialize(
        MainWindowViewModel viewModel,
        TopLevel? topLevel,
        VideraView view)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(view);

        viewModel.UpdateBackendDiagnostics(view.BackendDiagnostics);
        var factory = view.GetResourceFactory();

        if (topLevel == null || factory == null)
        {
            viewModel.SetBackendStatus(
                isReady: false,
                statusMessage: "Waiting for the rendering backend and resource factory to become ready.");
            return false;
        }

        if (!viewModel.HasImporter)
        {
            viewModel.AttachImporter(new AvaloniaModelImporter(topLevel, view));
        }

        viewModel.AttachViewportActions(new VideraViewViewportActions(view));

        try
        {
            var seededDefaultScene = SeedDefaultSceneIfNeeded(viewModel, view, factory);
            var statusMessage = seededDefaultScene
                ? "Rendering backend is ready. Loaded the default demo cube and framed the scene."
                : "Rendering backend is ready. Model import is available.";
            viewModel.UpdateBackendDiagnostics(view.BackendDiagnostics);
            viewModel.SetBackendStatus(true, statusMessage);
        }
        catch (Exception ex)
        {
            viewModel.UpdateBackendDiagnostics(view.BackendDiagnostics);
            viewModel.SetBackendStatus(
                isReady: true,
                statusMessage: "Rendering backend is ready, but the default demo model could not be created.");
            viewModel.SetStatusMessage($"Default demo model creation failed: {ex.Message}");
        }

        return true;
    }

    private bool SeedDefaultSceneIfNeeded(MainWindowViewModel viewModel, VideraView view, IResourceFactory factory)
    {
        if (_defaultSceneSeeded || viewModel.SceneObjects.Count > 0)
        {
            _defaultSceneSeeded = true;
            return false;
        }

        var cube = DemoMeshFactory.CreateCube(factory);
        view.AddObject(cube);
        view.FrameAll();
        viewModel.SelectedObject = cube;
        _defaultSceneSeeded = true;
        return true;
    }
}
