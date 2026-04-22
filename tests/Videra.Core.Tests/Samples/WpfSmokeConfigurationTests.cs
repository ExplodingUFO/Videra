using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class WpfSmokeConfigurationTests
{
    [Fact]
    public void WpfSmoke_ShouldExistAsWindowsOnlyProofHost_WithDpiAwareSizing()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeRoot = Path.Combine(repositoryRoot, "smoke", "Videra.WpfSmoke");
        var solution = File.ReadAllText(Path.Combine(repositoryRoot, "Videra.slnx"));
        var packageMatrix = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "package-matrix.md"));
        var releasePolicy = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "release-policy.md"));
        var consumerSmokeWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "consumer-smoke.yml"));
        var publicWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-public.yml"));
        var previewWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-github-packages.yml"));
        var existingReleaseWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-existing-public-release.yml"));
        var project = File.ReadAllText(Path.Combine(smokeRoot, "Videra.WpfSmoke.csproj"));
        var mainWindowCodeBehind = File.ReadAllText(Path.Combine(smokeRoot, "MainWindow.xaml.cs"));
        var hostCode = File.ReadAllText(Path.Combine(smokeRoot, "ViewerHwndHost.cs"));

        Directory.Exists(smokeRoot).Should().BeTrue();
        File.Exists(Path.Combine(smokeRoot, "App.xaml")).Should().BeTrue();
        File.Exists(Path.Combine(smokeRoot, "App.xaml.cs")).Should().BeTrue();
        File.Exists(Path.Combine(smokeRoot, "MainWindow.xaml")).Should().BeTrue();
        File.Exists(Path.Combine(smokeRoot, "MainWindow.xaml.cs")).Should().BeTrue();
        File.Exists(Path.Combine(smokeRoot, "ViewerHwndHost.cs")).Should().BeTrue();

        solution.Should().Contain("smoke/Videra.WpfSmoke/Videra.WpfSmoke.csproj");

        project.Should().Contain("<TargetFramework>net8.0-windows</TargetFramework>");
        project.Should().Contain("<UseWPF>true</UseWPF>");
        project.Should().Contain(@"..\..\src\Videra.Core\Videra.Core.csproj");
        project.Should().Contain(@"..\..\src\Videra.Platform.Windows\Videra.Platform.Windows.csproj");
        project.Should().NotContain("Avalonia");
        packageMatrix.Should().Contain("smoke/Videra.WpfSmoke");
        releasePolicy.Should().NotContain("Videra.WpfSmoke");
        consumerSmokeWorkflow.Should().NotContain("Videra.WpfSmoke");
        publicWorkflow.Should().NotContain("Videra.WpfSmoke");
        previewWorkflow.Should().NotContain("Videra.WpfSmoke");
        existingReleaseWorkflow.Should().NotContain("Videra.WpfSmoke");

        mainWindowCodeBehind.Should().Contain("RenderSessionOrchestrator");
        mainWindowCodeBehind.Should().Contain("D3D11Backend");
        mainWindowCodeBehind.Should().Contain("VIDERA_WPF_SMOKE_OUTPUT");
        mainWindowCodeBehind.Should().Contain("VisualTreeHelper.GetDpi(ViewerHost)");
        mainWindowCodeBehind.Should().Contain("GetRenderMetrics()");
        mainWindowCodeBehind.Should().Contain("RenderScale:");
        mainWindowCodeBehind.Should().NotContain("_orchestrator.Resize(width, height, 1f);");

        hostCode.Should().Contain("HwndHost");
        hostCode.Should().Contain("CreateWindowExW");
        hostCode.Should().Contain("VisualTreeHelper.GetDpi(this)");
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Videra.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing Videra.slnx.");
    }
}
