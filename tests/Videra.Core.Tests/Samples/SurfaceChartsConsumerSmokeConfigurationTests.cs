using System.Diagnostics;
using System.Text.Json;
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
        mainWindowCodeBehind.Should().Contain("chart-snapshot.png");
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
        mainWindowCodeBehind.Should().Contain("PlotSnapshotRequest");
        mainWindowCodeBehind.Should().Contain("CaptureSnapshotAsync");
        mainWindowCodeBehind.Should().Contain("SnapshotStatus:");
        mainWindowCodeBehind.Should().Contain("SnapshotPath:");
        mainWindowCodeBehind.Should().Contain("ChartSnapshotPath");
        mainWindowCodeBehind.Should().Contain("CreateSnapshotPathSummary");
        mainWindowCodeBehind.Should().Contain("CreateReportChartSnapshotPath");
        mainWindowCodeBehind.Should().Contain("HasPersistedChartSnapshot");
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
        mainWindowCodeBehind.Should().Contain("InteractivityCrosshairEnabled:");
        mainWindowCodeBehind.Should().Contain("InteractivityTooltipOffset:");
        mainWindowCodeBehind.Should().Contain("InteractivityProbeStrategies: Surface, Bar, Contour");
        mainWindowCodeBehind.Should().NotContain("InteractivityProbeStrategies: Surface, Scatter, Bar, Contour");
        mainWindowCodeBehind.Should().Contain("InteractivityKeyboardShortcuts:");
        mainWindowCodeBehind.Should().Contain("InteractivityToolbarButtons: not included in packaged consumer smoke");
        mainWindowCodeBehind.Should().NotContain("InteractivityToolbarButtons: enabled");
        mainWindowCodeBehind.Should().Contain("SmokeCoverage: packaged first-chart readiness plus PNG snapshot evidence; repository demo UX and scatter/live-data scenarios are out of scope");
        mainWindowCodeBehind.Should().NotContain("FrameAll");
        mainWindowCodeBehind.Should().NotContain("VideraView");
    }

    [Fact]
    public void ConsumerSmokeScript_ShouldOnlyListExistingScenarioSupportArtifacts()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1");
        var helperPath = Path.Combine(repositoryRoot, "scripts", "ConsumerSmokeSupportArtifacts.ps1");
        var script = File.ReadAllText(scriptPath);

        File.Exists(helperPath).Should().BeTrue();
        script.Should().Contain("ConsumerSmokeSupportArtifacts.ps1");
        script.Should().Contain("Get-ConsumerSmokeSupportArtifactPaths");
        script.Should().Contain("SupportArtifactPaths");
        script.Should().Contain("SnapshotStatus '$snapshotStatusValue'; packaged smoke success requires a present PNG snapshot.");
        script.Should().Contain("SnapshotFormat '$($valuesByPrefix[\"SnapshotFormat:\"])'; packaged smoke success requires PNG snapshot evidence.");
        script.Should().Contain("packaged smoke success must not count unavailable chart paths as covered.");
        script.Should().Contain("claims Scatter probing, but packaged consumer smoke does not construct a scatter series.");
        script.Should().Contain("claims toolbar buttons, but packaged consumer smoke has no demo toolbar.");
    }

    [Fact]
    public void ConsumerSmokeSupportArtifactHelper_ShouldReturnOnlyExistingScenarioArtifacts()
    {
        var repositoryRoot = GetRepositoryRoot();
        var helperPath = Path.Combine(repositoryRoot, "scripts", "ConsumerSmokeSupportArtifacts.ps1");
        var tempRoot = Path.Combine(Path.GetTempPath(), "videra-consumer-smoke-artifacts-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            var diagnostics = Touch(Path.Combine(tempRoot, "diagnostics-snapshot.txt"));
            var trace = Touch(Path.Combine(tempRoot, "consumer-smoke-trace.log"));
            var stdout = Touch(Path.Combine(tempRoot, "consumer-smoke-stdout.log"));
            var stderr = Touch(Path.Combine(tempRoot, "consumer-smoke-stderr.log"));
            var environment = Touch(Path.Combine(tempRoot, "consumer-smoke-environment.txt"));
            var surfaceSummary = Touch(Path.Combine(tempRoot, "surfacecharts-support-summary.txt"));
            var chartSnapshot = Touch(Path.Combine(tempRoot, "chart-snapshot.png"));
            var inspectionSnapshot = Touch(Path.Combine(tempRoot, "inspection-snapshot.png"));
            var inspectionBundle = Directory.CreateDirectory(Path.Combine(tempRoot, "inspection-bundle")).FullName;

            var surfaceChartsPaths = InvokeSupportArtifactHelper(
                helperPath,
                "SurfaceCharts",
                diagnostics,
                inspectionSnapshot,
                inspectionBundle,
                surfaceSummary,
                chartSnapshot,
                trace,
                stdout,
                stderr,
                environment);

            surfaceChartsPaths.Should().Contain(surfaceSummary);
            surfaceChartsPaths.Should().Contain(chartSnapshot);
            surfaceChartsPaths.Should().NotContain(inspectionSnapshot);
            surfaceChartsPaths.Should().NotContain(inspectionBundle);
            surfaceChartsPaths.Should().OnlyContain(path => File.Exists(path));

            var viewerPaths = InvokeSupportArtifactHelper(
                helperPath,
                "ViewerObj",
                diagnostics,
                inspectionSnapshot,
                inspectionBundle,
                surfaceSummary,
                chartSnapshot,
                trace,
                stdout,
                stderr,
                environment);

            viewerPaths.Should().Contain(inspectionSnapshot);
            viewerPaths.Should().Contain(inspectionBundle);
            viewerPaths.Should().NotContain(surfaceSummary);
            viewerPaths.Should().NotContain(chartSnapshot);
            viewerPaths.Should().OnlyContain(path => File.Exists(path) || Directory.Exists(path));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static string Touch(string path)
    {
        File.WriteAllText(path, "artifact");
        return path;
    }

    private static string[] InvokeSupportArtifactHelper(
        string helperPath,
        string scenario,
        string diagnostics,
        string inspectionSnapshot,
        string inspectionBundle,
        string surfaceSummary,
        string chartSnapshot,
        string trace,
        string stdout,
        string stderr,
        string environment)
    {
        var script = string.Join(
            Environment.NewLine,
            $". '{EscapePowerShellSingleQuotedString(helperPath)}'",
            "$paths = Get-ConsumerSmokeSupportArtifactPaths " +
            $"-Scenario '{scenario}' " +
            $"-DiagnosticsSnapshotPath '{EscapePowerShellSingleQuotedString(diagnostics)}' " +
            $"-InspectionSnapshotPath '{EscapePowerShellSingleQuotedString(inspectionSnapshot)}' " +
            $"-InspectionBundlePath '{EscapePowerShellSingleQuotedString(inspectionBundle)}' " +
            $"-SurfaceChartsSupportSummaryPath '{EscapePowerShellSingleQuotedString(surfaceSummary)}' " +
            $"-SurfaceChartsSnapshotPath '{EscapePowerShellSingleQuotedString(chartSnapshot)}' " +
            $"-TracePath '{EscapePowerShellSingleQuotedString(trace)}' " +
            $"-StdoutPath '{EscapePowerShellSingleQuotedString(stdout)}' " +
            $"-StderrPath '{EscapePowerShellSingleQuotedString(stderr)}' " +
            $"-EnvironmentPath '{EscapePowerShellSingleQuotedString(environment)}'",
            "$paths | ConvertTo-Json -Compress");

        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add(script);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start pwsh.");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        process.ExitCode.Should().Be(0, output + error);
        var paths = JsonSerializer.Deserialize<string[]>(output.Trim());
        paths.Should().NotBeNull();
        return paths!;
    }

    private static string EscapePowerShellSingleQuotedString(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
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
