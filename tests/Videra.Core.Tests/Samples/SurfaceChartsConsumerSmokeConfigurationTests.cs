using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsConsumerSmokeConfigurationTests
{
    [Fact]
    public void SurfaceChartsConsumerSmoke_ShouldExistAsPackagedFirstChartProof()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeRoot = Path.Combine(repositoryRoot, "smoke", "Videra.SurfaceCharts.ConsumerSmoke");
        var projectPath = Path.Combine(smokeRoot, "Videra.SurfaceCharts.ConsumerSmoke.csproj");
        var appXamlPath = Path.Combine(smokeRoot, "App.axaml");
        var appCodeBehindPath = Path.Combine(smokeRoot, "App.axaml.cs");
        var programPath = Path.Combine(smokeRoot, "Program.cs");
        var mainWindowXamlPath = Path.Combine(smokeRoot, "Views", "MainWindow.axaml");
        var mainWindowCodeBehindPath = Path.Combine(smokeRoot, "Views", "MainWindow.axaml.cs");

        File.Exists(projectPath).Should().BeTrue();
        File.Exists(appXamlPath).Should().BeTrue();
        File.Exists(appCodeBehindPath).Should().BeTrue();
        File.Exists(programPath).Should().BeTrue();
        File.Exists(mainWindowXamlPath).Should().BeTrue();
        File.Exists(mainWindowCodeBehindPath).Should().BeTrue();

        var project = File.ReadAllText(projectPath);
        project.Should().Contain("<PackageReference Include=\"Videra.SurfaceCharts.Avalonia\"");
        project.Should().Contain("<PackageReference Include=\"Videra.SurfaceCharts.Processing\"");
        project.Should().NotContain("<ProjectReference");

        var mainWindowXaml = File.ReadAllText(mainWindowXamlPath);
        mainWindowXaml.Should().Contain("VideraChartView");
        mainWindowXaml.Should().Contain("StatusText");
        mainWindowXaml.Should().Contain("Videra SurfaceCharts Consumer Smoke");
        mainWindowXaml.Should().NotContain("VideraView");

        var mainWindowCodeBehind = File.ReadAllText(mainWindowCodeBehindPath);
        mainWindowCodeBehind.Should().Contain("Start here: In-memory first chart");
        mainWindowCodeBehind.Should().Contain("surfacecharts-support-summary.txt");
        mainWindowCodeBehind.Should().Contain("SurfacePyramidBuilder");
        mainWindowCodeBehind.Should().Contain("SurfaceChartOverlayOptions");
        mainWindowCodeBehind.Should().Contain("InteractionQuality");
        mainWindowCodeBehind.Should().Contain("RenderingStatus");
        mainWindowCodeBehind.Should().Contain("GeneratedUtc:");
        mainWindowCodeBehind.Should().Contain("EvidenceKind: SurfaceChartsDatasetProof");
        mainWindowCodeBehind.Should().Contain("EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.");
        mainWindowCodeBehind.Should().Contain("ChartControl:");
        mainWindowCodeBehind.Should().Contain("EnvironmentRuntime:");
        mainWindowCodeBehind.Should().Contain("AssemblyIdentity:");
        mainWindowCodeBehind.Should().Contain("BackendDisplayEnvironment:");
        mainWindowCodeBehind.Should().Contain("SeriesCount:");
        mainWindowCodeBehind.Should().Contain("ActiveSeries:");
        mainWindowCodeBehind.Should().Contain("ChartKind:");
        mainWindowCodeBehind.Should().Contain("ColorMap:");
        mainWindowCodeBehind.Should().Contain("PrecisionProfile:");
        mainWindowCodeBehind.Should().Contain("OutputEvidenceKind:");
        mainWindowCodeBehind.Should().Contain("OutputCapabilityDiagnostics:");
        mainWindowCodeBehind.Should().Contain("DatasetEvidenceKind:");
        mainWindowCodeBehind.Should().Contain("DatasetSeriesCount:");
        mainWindowCodeBehind.Should().Contain("DatasetActiveSeriesIndex:");
        mainWindowCodeBehind.Should().Contain("DatasetActiveSeriesMetadata:");
        mainWindowCodeBehind.Should().Contain("FirstChartRendered");
        mainWindowCodeBehind.Should().Contain("ViewState");
        mainWindowCodeBehind.Should().Contain("ResidentTileCount");
        mainWindowCodeBehind.Should().Contain("ResolveLightingProofHoldSeconds()");
        mainWindowCodeBehind.Should().Contain("VIDERA_LIGHTING_PROOF_HOLD_SECONDS");
        mainWindowCodeBehind.Should().Contain("Lighting proof hold active for");
        mainWindowCodeBehind.Should().Contain("await Task.Delay(TimeSpan.FromSeconds(_lightingProofHoldSeconds)).ConfigureAwait(true);");
        mainWindowCodeBehind.Should().NotContain("FrameAll");
        mainWindowCodeBehind.Should().NotContain("VideraView");
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
