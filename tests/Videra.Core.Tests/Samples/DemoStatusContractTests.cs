using FluentAssertions;
using Videra.Demo.ViewModels;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class DemoStatusContractTests
{
    [Fact]
    public void SetWaitingForBackend_ShouldKeepBackendNotReady_AndExposeWaitingStatus()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.SetWaitingForBackend();

        viewModel.IsBackendReady.Should().BeFalse();
        viewModel.StatusMessage.Should().Be("Waiting for the rendering backend and resource factory to become ready.");
    }

    [Fact]
    public void SetBackendReadyWithDefaultScene_ShouldExposeDefaultSceneStatus()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.SetBackendReadyWithDefaultScene();

        viewModel.IsBackendReady.Should().BeTrue();
        viewModel.StatusMessage.Should().Be("Rendering backend is ready. Loaded the default demo cube and framed the scene.");
    }

    [Fact]
    public void SetBackendReadyWithoutDefaultScene_ShouldExposeImportAvailableStatus()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.SetBackendReadyWithoutDefaultScene();

        viewModel.IsBackendReady.Should().BeTrue();
        viewModel.StatusMessage.Should().Be("Rendering backend is ready. Model import is available.");
    }

    [Fact]
    public void SetBackendReadyWithDefaultSceneFailure_ShouldStayReady_AndExposeDegradedStatus()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.SetBackendReadyWithDefaultSceneFailure("simulated failure");

        viewModel.IsBackendReady.Should().BeTrue();
        viewModel.StatusMessage.Should().Be("Rendering backend is ready, but the default demo cube could not be created. Model import remains available. Last error: simulated failure");
    }

    [Fact]
    public void SetBackendInitializationFailed_ShouldKeepBackendNotReady_AndExposeFailureStatus()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.SetBackendInitializationFailed("simulated failure");

        viewModel.IsBackendReady.Should().BeFalse();
        viewModel.StatusMessage.Should().Be("Rendering backend initialization failed: simulated failure");
    }
}
