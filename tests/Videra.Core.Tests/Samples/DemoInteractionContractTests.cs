using System;
using System.Threading.Tasks;
using FluentAssertions;
using Videra.Core.Graphics.Wireframe;
using Videra.Demo.Services;
using Videra.Demo.ViewModels;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class DemoInteractionContractTests
{
    [Fact]
    public void ImportCommand_ShouldRequireReadyBackendAndAttachedImporter()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.ImportCommand.CanExecute(null).Should().BeFalse();

        viewModel.SetBackendReadyWithoutDefaultScene();
        viewModel.ImportCommand.CanExecute(null).Should().BeFalse();

        viewModel.AttachImporter(new StubModelImporter());
        viewModel.ImportCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void CameraCommands_ShouldRequireReadyBackendAndViewportActions()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.FrameAllCommand.CanExecute(null).Should().BeFalse();
        viewModel.ResetCameraCommand.CanExecute(null).Should().BeFalse();

        viewModel.SetBackendReadyWithoutDefaultScene();
        viewModel.FrameAllCommand.CanExecute(null).Should().BeFalse();
        viewModel.ResetCameraCommand.CanExecute(null).Should().BeFalse();

        viewModel.AttachViewportActions(new StubViewportActions());
        viewModel.FrameAllCommand.CanExecute(null).Should().BeTrue();
        viewModel.ResetCameraCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void WireframeMode_ShouldRemainTheSingleSourceOfWireframeEnablement()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.IsWireframeEnabled.Should().BeFalse();

        viewModel.WireframeMode = WireframeMode.AllEdges;

        viewModel.IsWireframeEnabled.Should().BeTrue();
    }

    private sealed class StubModelImporter : IModelImporter
    {
        public Task<Videra.Avalonia.Controls.ModelLoadBatchResult> ImportModelsAsync()
        {
            throw new NotSupportedException("Import is not used in CanExecute tests.");
        }
    }

    private sealed class StubViewportActions : IDemoViewportActions
    {
        public bool FrameAll() => true;

        public void ResetCamera()
        {
        }
    }
}
