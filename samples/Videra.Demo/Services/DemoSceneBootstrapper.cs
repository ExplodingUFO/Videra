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
            viewModel.SetWaitingForBackend();
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
            viewModel.UpdateBackendDiagnostics(view.BackendDiagnostics);

            if (seededDefaultScene)
            {
                viewModel.SetBackendReadyWithDefaultScene();
            }
            else
            {
                viewModel.SetBackendReadyWithoutDefaultScene();
            }
        }
        catch (Exception ex)
        {
            viewModel.UpdateBackendDiagnostics(view.BackendDiagnostics);
            viewModel.SetBackendReadyWithDefaultSceneFailure(ex.Message);
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
