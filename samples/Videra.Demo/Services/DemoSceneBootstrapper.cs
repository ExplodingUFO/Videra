using System;
using Avalonia.Controls;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
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

        var diagnostics = view.BackendDiagnostics;
        var factory = view.GetResourceFactory();

        if (topLevel == null || factory == null)
        {
            viewModel.SetBackendStatus(
                isReady: false,
                backendDisplay: CreateBackendDisplay(diagnostics),
                statusMessage: "Waiting for the rendering backend and resource factory to become ready.");
            return false;
        }

        if (!viewModel.HasImporter)
        {
            viewModel.AttachImporter(new AvaloniaModelImporter(topLevel, view));
        }

        try
        {
            var seededDefaultScene = SeedDefaultSceneIfNeeded(viewModel, view, factory);
            var statusMessage = seededDefaultScene
                ? "Rendering backend is ready. Loaded the default demo cube and framed the scene."
                : "Rendering backend is ready. Model import is available.";
            viewModel.SetBackendStatus(true, CreateBackendDisplay(view.BackendDiagnostics), statusMessage);
        }
        catch (Exception ex)
        {
            viewModel.SetBackendStatus(
                isReady: true,
                backendDisplay: CreateBackendDisplay(view.BackendDiagnostics),
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

    private static string CreateBackendDisplay(VideraBackendDiagnostics diagnostics)
    {
        var display = $"Requested: {diagnostics.RequestedBackend} | Resolved: {diagnostics.ResolvedBackend}";

        if (diagnostics.IsUsingSoftwareFallback && !string.IsNullOrWhiteSpace(diagnostics.FallbackReason))
        {
            display = $"{display} | Fallback: {diagnostics.FallbackReason}";
        }

        return display;
    }
}
