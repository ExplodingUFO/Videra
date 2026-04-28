using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class VideraDoctorRepositoryTests
{
    [Fact]
    public void VideraDoctor_ShouldRemainRepoOnlyAndNonMutating()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-VideraDoctor.ps1");
        var docsPath = Path.Combine(repositoryRoot, "docs", "videra-doctor.md");
        var docsIndexPath = Path.Combine(repositoryRoot, "docs", "index.md");

        File.Exists(scriptPath).Should().BeTrue();
        File.Exists(docsPath).Should().BeTrue();

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("repo-only");
        script.Should().Contain("non-mutating");
        script.Should().Contain("artifacts/doctor");
        script.Should().Contain("doctor-report.json");
        script.Should().Contain("doctor-summary.txt");
        script.Should().Contain("RunPackageValidation");
        script.Should().Contain("RunBenchmarkThresholds");
        script.Should().Contain("RunConsumerSmoke");
        script.Should().Contain("RunNativeValidation");
        script.Should().Contain("Get-PerformanceLabVisualEvidence");
        script.Should().Contain("performanceLabVisualEvidence");
        script.Should().Contain("Get-SurfaceChartsSupportReport");
        script.Should().Contain("surfaceChartsSupportReport");
        script.Should().Contain("artifacts/performance-lab-visual-evidence");
        script.Should().Contain("artifacts/consumer-smoke/surfacecharts-support-summary.txt");

        script.Should().NotContain("dotnet nuget push");
        script.Should().NotContain("git push");
        script.Should().NotContain("git tag");
        script.Should().NotContain("Set-ExecutionPolicy");
        script.Should().NotContain("Remove-Item");
        script.Should().NotContain("RunPerformanceLabVisualEvidence");
        script.Should().NotContain("SimulateUnavailable:");

        var projectNames = Directory.EnumerateFiles(repositoryRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();

        projectNames.Should().NotContain("Videra.Doctor");

        var docs = File.ReadAllText(docsPath);
        docs.Should().Contain("repo-only");
        docs.Should().Contain("non-mutating");
        docs.Should().Contain("scripts/Invoke-VideraDoctor.ps1");
        docs.Should().Contain("artifacts/doctor");
        docs.Should().Contain("not a public package");
        docs.Should().Contain("RunPackageValidation");
        docs.Should().Contain("RunBenchmarkThresholds");
        docs.Should().Contain("RunConsumerSmoke");
        docs.Should().Contain("RunNativeValidation");
        docs.Should().Contain("performanceLabVisualEvidence");
        docs.Should().Contain("surfaceChartsSupportReport");
        docs.Should().Contain("Doctor does not generate screenshots by default");
        docs.Should().Contain("Doctor does not run the SurfaceCharts demo or consumer smoke by default");
        docs.Should().Contain("pass");
        docs.Should().Contain("fail");
        docs.Should().Contain("skip");
        docs.Should().Contain("unavailable");

        var docsIndex = File.ReadAllText(docsIndexPath);
        docsIndex.Should().Contain("videra-doctor.md");
    }

    [Fact]
    public void VideraDoctorDocs_ShouldReferenceActualValidationScriptsContractsAndArtifacts()
    {
        var repositoryRoot = GetRepositoryRoot();
        var docs = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "videra-doctor.md"));
        var releasing = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "releasing.md"));

        foreach (var relativePath in new[]
                 {
                     "scripts/Invoke-VideraDoctor.ps1",
                     "scripts/Validate-Packages.ps1",
                     "scripts/Run-Benchmarks.ps1",
                     "scripts/Test-BenchmarkThresholds.ps1",
                     "scripts/Invoke-ConsumerSmoke.ps1",
                     "scripts/run-native-validation.ps1",
                     "scripts/Invoke-PublicReleasePreflight.ps1",
                     "benchmarks/benchmark-contract.json",
                     "benchmarks/benchmark-thresholds.json",
                     "eng/public-api-contract.json"
                 })
        {
            File.Exists(Path.Combine(repositoryRoot, relativePath)).Should().BeTrue();
            docs.Should().Contain(relativePath);
            releasing.Should().Contain(relativePath);
        }

        docs.Should().Contain("artifacts/doctor/doctor-report.json");
        docs.Should().Contain("artifacts/doctor/doctor-summary.txt");
        docs.Should().Contain("release-dry-run-evidence");
        docs.Should().Contain("does not publish");
        docs.Should().Contain("does not push");
        docs.Should().Contain("does not create tags");
    }

    [Fact]
    public void VideraDoctor_ShouldEmitHumanAndStructuredReports()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-VideraDoctor.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraDoctorTests", Guid.NewGuid().ToString("N"));

        var result = RunPowerShell(scriptPath, outputRoot, repositoryRoot);

        result.ExitCode.Should().Be(0, $"stdout: {result.Stdout}\nstderr: {result.Stderr}");

        var summaryPath = Path.Combine(outputRoot, "doctor-summary.txt");
        var reportPath = Path.Combine(outputRoot, "doctor-report.json");
        File.Exists(summaryPath).Should().BeTrue();
        File.Exists(reportPath).Should().BeTrue();

        var summary = File.ReadAllText(summaryPath);
        summary.Should().Contain("Videra Doctor");
        summary.Should().Contain("repo-only");
        summary.Should().Contain("non-mutating");
        summary.Should().Contain("Evidence packet:");

        using var reportDocument = JsonDocument.Parse(File.ReadAllText(reportPath));
        var report = reportDocument.RootElement;
        report.GetProperty("schemaVersion").GetInt32().Should().Be(1);
        report.GetProperty("name").GetString().Should().Be("Videra Doctor");
        report.GetProperty("repoOnly").GetBoolean().Should().BeTrue();
        report.GetProperty("mutatesConfiguration").GetBoolean().Should().BeFalse();
        report.GetProperty("outputRoot").GetString().Should().Be(outputRoot);

        var checks = report.GetProperty("checks")
            .EnumerateArray()
            .ToDictionary(
                static check => check.GetProperty("id").GetString()!,
                static check => check.GetProperty("status").GetString()!);

        checks.Keys.Should().Contain([
            "dotnet-sdk",
            "git-repository",
            "public-api-contract",
            "package-size-budgets",
            "benchmark-contract",
            "benchmark-thresholds",
            "script:Validate-Packages.ps1",
            "script:Run-Benchmarks.ps1",
            "script:Test-BenchmarkThresholds.ps1",
            "script:Invoke-ConsumerSmoke.ps1",
            "script:run-native-validation.ps1"
        ]);
        var validStatuses = new[] { "pass", "warn", "skip", "unavailable" };
        checks.Values.Should().OnlyContain(status => validStatuses.Contains(status));

        var supportArtifacts = report.GetProperty("supportArtifactPaths")
            .EnumerateArray()
            .Select(static path => path.GetString())
            .ToArray();

        supportArtifacts.Should().Contain("artifacts/doctor");
        supportArtifacts.Should().Contain("artifacts/benchmarks");
        supportArtifacts.Should().Contain("artifacts/release-dry-run");
        supportArtifacts.Should().Contain("artifacts/performance-lab-visual-evidence");

        var evidencePacket = report.GetProperty("evidencePacket");
        evidencePacket.GetProperty("repository").GetProperty("root").GetString().Should().Be(repositoryRoot);
        evidencePacket.GetProperty("repository").GetProperty("status").GetString().Should().NotBeNullOrWhiteSpace();
        evidencePacket.GetProperty("machine").GetProperty("dotnetStatus").GetString().Should().NotBeNullOrWhiteSpace();

        var packageContractPaths = evidencePacket.GetProperty("packageContracts")
            .EnumerateArray()
            .Select(static contract => contract.GetProperty("path").GetString())
            .ToArray();

        packageContractPaths.Should().Contain([
            "eng/public-api-contract.json",
            "eng/package-size-budgets.json",
            "benchmarks/benchmark-contract.json",
            "benchmarks/benchmark-thresholds.json"
        ]);

        var validationScriptPaths = evidencePacket.GetProperty("validationScripts")
            .EnumerateArray()
            .Select(static script => script.GetProperty("path").GetString())
            .ToArray();

        validationScriptPaths.Should().Contain([
            "scripts/Validate-Packages.ps1",
            "scripts/Run-Benchmarks.ps1",
            "scripts/Test-BenchmarkThresholds.ps1",
            "scripts/Invoke-ConsumerSmoke.ps1",
            "scripts/run-native-validation.ps1",
            "scripts/Invoke-ReleaseDryRun.ps1",
            "scripts/Invoke-PublicReleasePreflight.ps1"
        ]);

        var artifactReferences = evidencePacket.GetProperty("artifactReferences")
            .EnumerateArray()
            .ToArray();

        artifactReferences.Select(static artifact => artifact.GetProperty("category").GetString()).Should().Contain([
            "release-dry-run",
            "package-validation",
            "benchmark",
            "consumer-smoke",
            "native-validation",
            "public-release-preflight",
            "demo-support",
            "performance-lab-visual-evidence"
        ]);

        artifactReferences.Should().OnlyContain(static artifact =>
            !string.IsNullOrWhiteSpace(artifact.GetProperty("id").GetString()) &&
            !string.IsNullOrWhiteSpace(artifact.GetProperty("path").GetString()) &&
            !string.IsNullOrWhiteSpace(artifact.GetProperty("producedBy").GetString()) &&
            new[] { "present", "missing" }.Contains(artifact.GetProperty("status").GetString()));

        artifactReferences.Select(static artifact => artifact.GetProperty("path").GetString()).Should().Contain([
            "artifacts/release-dry-run/release-dry-run-summary.json",
            "artifacts/release-dry-run/release-candidate-evidence-index.json",
            "artifacts/release-dry-run/packages/.validation/package-size-evaluation.json",
            "artifacts/benchmarks/viewer/benchmark-manifest.json",
            "artifacts/benchmarks/surfacecharts/benchmark-manifest.json",
            "artifacts/consumer-smoke/consumer-smoke-result.json",
            "artifacts/native-validation",
            "artifacts/public-release-preflight/public-release-preflight-summary.json",
            "artifacts/consumer-smoke/surfacecharts-support-summary.txt",
            "artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json",
            "artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-summary.txt"
        ]);

        var visualEvidence = evidencePacket.GetProperty("performanceLabVisualEvidence");
        visualEvidence.GetProperty("status").GetString().Should().BeOneOf("present", "missing", "unavailable");
        visualEvidence.GetProperty("manifestPath").GetString()
            .Should().Be("artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json");
        visualEvidence.GetProperty("evidenceKind").GetString().Should().Be("PerformanceLabVisualEvidence");
        visualEvidence.GetProperty("screenshotPaths").ValueKind.Should().Be(JsonValueKind.Array);
        visualEvidence.GetProperty("diagnosticsPaths").ValueKind.Should().Be(JsonValueKind.Array);
        visualEvidence.GetProperty("entries").ValueKind.Should().Be(JsonValueKind.Array);

        var surfaceChartsSupportReport = evidencePacket.GetProperty("surfaceChartsSupportReport");
        surfaceChartsSupportReport.GetProperty("status").GetString().Should().BeOneOf("present", "missing", "unavailable");
        surfaceChartsSupportReport.GetProperty("supportSummaryPath").GetString()
            .Should().Be("artifacts/consumer-smoke/surfacecharts-support-summary.txt");
        surfaceChartsSupportReport.GetProperty("renderingStatusPresent").ValueKind.Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);

        var validations = report.GetProperty("validations")
            .EnumerateArray()
            .ToDictionary(
                static validation => validation.GetProperty("id").GetString()!,
                static validation => validation);

        validations.Keys.Should().Contain([
            "package-validation",
            "benchmark-thresholds:Viewer",
            "benchmark-thresholds:SurfaceCharts",
            "consumer-smoke:ViewerObj",
            "native-validation",
            "demo-diagnostics"
        ]);

        validations["package-validation"].GetProperty("status").GetString().Should().Be("skip");
        validations["package-validation"].GetProperty("script").GetString().Should().Be("scripts/Validate-Packages.ps1");
        validations["benchmark-thresholds:Viewer"].GetProperty("script").GetString().Should().Be("scripts/Test-BenchmarkThresholds.ps1");
        validations["consumer-smoke:ViewerObj"].GetProperty("script").GetString().Should().Be("scripts/Invoke-ConsumerSmoke.ps1");
        validations["native-validation"].GetProperty("script").GetString().Should().Be("scripts/run-native-validation.ps1");
        validations["demo-diagnostics"].GetProperty("artifacts").EnumerateArray().Select(static value => value.GetString())
            .Should().Contain("artifacts/doctor");

        var validValidationStatuses = new[] { "pass", "fail", "skip", "unavailable" };
        validations.Values.Should().OnlyContain(validation =>
            validValidationStatuses.Contains(validation.GetProperty("status").GetString()));

        foreach (var validation in validations.Values)
        {
            validation.TryGetProperty("script", out _).Should().BeTrue();
            validation.TryGetProperty("prerequisites", out var prerequisites).Should().BeTrue();
            prerequisites.ValueKind.Should().Be(JsonValueKind.Array);
            validation.TryGetProperty("artifacts", out var artifacts).Should().BeTrue();
            artifacts.ValueKind.Should().Be(JsonValueKind.Array);
            validation.TryGetProperty("invoked", out _).Should().BeTrue();
            validation.TryGetProperty("exitCode", out _).Should().BeTrue();
            validation.TryGetProperty("logPath", out _).Should().BeTrue();
        }
    }

    [Fact]
    public void VideraDoctor_ShouldReportUnavailableValidationPrerequisites()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-VideraDoctor.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraDoctorTests", Guid.NewGuid().ToString("N"));
        var missingBenchmarkRoot = $"artifacts/missing-benchmarks-{Guid.NewGuid():N}";

        var result = RunPowerShell(
            scriptPath,
            outputRoot,
            repositoryRoot,
            "-RunBenchmarkThresholds",
            "-BenchmarkOutputRoot",
            missingBenchmarkRoot);

        result.ExitCode.Should().Be(0, $"stdout: {result.Stdout}\nstderr: {result.Stderr}");

        using var reportDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(outputRoot, "doctor-report.json")));
        var validations = reportDocument.RootElement.GetProperty("validations")
            .EnumerateArray()
            .ToDictionary(
                static validation => validation.GetProperty("id").GetString()!,
                static validation => validation);

        validations["benchmark-thresholds:Viewer"].GetProperty("status").GetString().Should().Be("unavailable");
        validations["benchmark-thresholds:Viewer"].GetProperty("message").GetString().Should().Contain("Run-Benchmarks.ps1");
        validations["benchmark-thresholds:SurfaceCharts"].GetProperty("status").GetString().Should().Be("unavailable");
        validations["benchmark-thresholds:Viewer"].GetProperty("artifacts")
            .EnumerateArray()
            .Select(static artifact => artifact.GetString())
            .Should().Contain($"{missingBenchmarkRoot.Replace('/', Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}viewer{Path.DirectorySeparatorChar}benchmark-manifest.json");
    }

    [Fact]
    public void VideraDoctor_ShouldReportMissingVisualEvidenceAsOptionalEvidence()
    {
        var repositoryRoot = GetRepositoryRoot();
        using var visualEvidenceScope = DefaultVisualEvidenceDirectoryScope.Create(repositoryRoot);
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-VideraDoctor.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraDoctorTests", Guid.NewGuid().ToString("N"));

        var result = RunPowerShell(scriptPath, outputRoot, repositoryRoot);

        result.ExitCode.Should().Be(0, $"stdout: {result.Stdout}\nstderr: {result.Stderr}");

        using var reportDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(outputRoot, "doctor-report.json")));
        var visualEvidence = reportDocument.RootElement.GetProperty("evidencePacket").GetProperty("performanceLabVisualEvidence");
        visualEvidence.GetProperty("status").GetString().Should().Be("missing");
        visualEvidence.GetProperty("summaryPath").GetString().Should().BeEmpty();
        visualEvidence.GetProperty("screenshotPaths").EnumerateArray().Should().BeEmpty();
        visualEvidence.GetProperty("diagnosticsPaths").EnumerateArray().Should().BeEmpty();

        var summary = File.ReadAllText(Path.Combine(outputRoot, "doctor-summary.txt"));
        summary.Should().Contain("Performance Lab visual evidence:");
        summary.Should().Contain("status: missing");
        summary.Should().Contain("artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json");
    }

    [Fact]
    public void VideraDoctor_ShouldReportMissingSurfaceChartsSupportReportAsOptionalEvidence()
    {
        var repositoryRoot = GetRepositoryRoot();
        using var supportScope = DefaultSurfaceChartsSupportDirectoryScope.Create(repositoryRoot);
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-VideraDoctor.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraDoctorTests", Guid.NewGuid().ToString("N"));

        var result = RunPowerShell(scriptPath, outputRoot, repositoryRoot);

        result.ExitCode.Should().Be(0, $"stdout: {result.Stdout}\nstderr: {result.Stderr}");

        using var reportDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(outputRoot, "doctor-report.json")));
        var supportReport = reportDocument.RootElement.GetProperty("evidencePacket").GetProperty("surfaceChartsSupportReport");
        supportReport.GetProperty("status").GetString().Should().Be("missing");
        supportReport.GetProperty("supportSummaryPath").GetString().Should().Be("artifacts/consumer-smoke/surfacecharts-support-summary.txt");
        supportReport.GetProperty("renderingStatusPresent").GetBoolean().Should().BeFalse();

        var summary = File.ReadAllText(Path.Combine(outputRoot, "doctor-summary.txt"));
        summary.Should().Contain("SurfaceCharts support report:");
        summary.Should().Contain("status: missing");
    }

    [Fact]
    public void VideraDoctor_ShouldReportPresentSurfaceChartsSupportReport()
    {
        var repositoryRoot = GetRepositoryRoot();
        using var supportScope = DefaultSurfaceChartsSupportDirectoryScope.Create(repositoryRoot);
        supportScope.WriteSupportSummary(
            "SurfaceCharts support summary",
            "GeneratedUtc: 2026-04-28T02:00:00.0000000+00:00",
            "EvidenceKind: SurfaceChartsDatasetProof",
            "EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.",
            "ChartControl: SurfaceChartView (Videra.SurfaceCharts.Avalonia.Controls.SurfaceChartView)",
            "EnvironmentRuntime: .NET 8.0.0; OS Windows; ProcessArchitecture X64; OSArchitecture X64",
            "RenderingStatus:",
            "ActiveBackend: Software");

        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-VideraDoctor.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraDoctorTests", Guid.NewGuid().ToString("N"));

        var result = RunPowerShell(scriptPath, outputRoot, repositoryRoot);

        result.ExitCode.Should().Be(0, $"stdout: {result.Stdout}\nstderr: {result.Stderr}");

        using var reportDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(outputRoot, "doctor-report.json")));
        var supportReport = reportDocument.RootElement.GetProperty("evidencePacket").GetProperty("surfaceChartsSupportReport");
        supportReport.GetProperty("status").GetString().Should().Be("present");
        supportReport.GetProperty("generatedAtUtc").GetString().Should().Be("2026-04-28T02:00:00.0000000+00:00");
        supportReport.GetProperty("evidenceKind").GetString().Should().Be("SurfaceChartsDatasetProof");
        supportReport.GetProperty("evidenceOnly").GetBoolean().Should().BeTrue();
        supportReport.GetProperty("chartControl").GetString().Should().Contain("SurfaceChartView");
        supportReport.GetProperty("environmentRuntime").GetString().Should().Contain(".NET");
        supportReport.GetProperty("renderingStatusPresent").GetBoolean().Should().BeTrue();

        var summary = File.ReadAllText(Path.Combine(outputRoot, "doctor-summary.txt"));
        summary.Should().Contain("SurfaceCharts support report:");
        summary.Should().Contain("status: present");
        summary.Should().Contain("evidence kind: SurfaceChartsDatasetProof");
        summary.Should().Contain("chart control: SurfaceChartView");
    }

    [Fact]
    public void VideraDoctor_ShouldReportPresentVisualEvidenceArtifacts()
    {
        var repositoryRoot = GetRepositoryRoot();
        using var visualEvidenceScope = DefaultVisualEvidenceDirectoryScope.Create(repositoryRoot);
        var captureScript = Path.Combine(repositoryRoot, "scripts", "Invoke-PerformanceLabVisualEvidence.ps1");
        var doctorScript = Path.Combine(repositoryRoot, "scripts", "Invoke-VideraDoctor.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraDoctorTests", Guid.NewGuid().ToString("N"));

        var captureResult = RunPowerShellScript(
            captureScript,
            repositoryRoot,
            "-OutputRoot",
            "artifacts/performance-lab-visual-evidence",
            "-ViewerScenarios",
            "viewer-instance-small",
            "-ScatterScenarios",
            "scatter-replace-100k",
            "-Width",
            "320",
            "-Height",
            "180");

        captureResult.ExitCode.Should().Be(0, $"stdout: {captureResult.Stdout}\nstderr: {captureResult.Stderr}");

        var doctorResult = RunPowerShell(doctorScript, outputRoot, repositoryRoot);

        doctorResult.ExitCode.Should().Be(0, $"stdout: {doctorResult.Stdout}\nstderr: {doctorResult.Stderr}");

        using var reportDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(outputRoot, "doctor-report.json")));
        var visualEvidence = reportDocument.RootElement.GetProperty("evidencePacket").GetProperty("performanceLabVisualEvidence");
        visualEvidence.GetProperty("status").GetString().Should().Be("present");
        visualEvidence.GetProperty("captureStatus").GetString().Should().Be("produced");
        visualEvidence.GetProperty("summaryPath").GetString().Should().Contain("performance-lab-visual-evidence-summary.txt");
        visualEvidence.GetProperty("generatedAtUtc").GetString().Should().NotBeNullOrWhiteSpace();
        visualEvidence.GetProperty("schemaVersion").GetInt32().Should().Be(1);

        visualEvidence.GetProperty("screenshotPaths").EnumerateArray().Select(static path => path.GetString()).Should().Contain([
            "artifacts/performance-lab-visual-evidence/viewer-instance-small.png",
            "artifacts/performance-lab-visual-evidence/scatter-replace-100k.png"
        ]);
        visualEvidence.GetProperty("diagnosticsPaths").EnumerateArray().Select(static path => path.GetString()).Should().Contain([
            "artifacts/performance-lab-visual-evidence/viewer-instance-small-diagnostics.txt",
            "artifacts/performance-lab-visual-evidence/scatter-replace-100k-diagnostics.txt"
        ]);
        visualEvidence.GetProperty("entries").EnumerateArray().Should().HaveCount(2);

        var summary = File.ReadAllText(Path.Combine(outputRoot, "doctor-summary.txt"));
        summary.Should().Contain("status: present");
        summary.Should().Contain("screenshot: artifacts/performance-lab-visual-evidence/viewer-instance-small.png");
        summary.Should().Contain("diagnostics: artifacts/performance-lab-visual-evidence/scatter-replace-100k-diagnostics.txt");
    }

    [Fact]
    public void VideraDoctor_ShouldReportUnavailableVisualEvidenceStateDistinctFromMissing()
    {
        var repositoryRoot = GetRepositoryRoot();
        using var visualEvidenceScope = DefaultVisualEvidenceDirectoryScope.Create(repositoryRoot);
        var captureScript = Path.Combine(repositoryRoot, "scripts", "Invoke-PerformanceLabVisualEvidence.ps1");
        var doctorScript = Path.Combine(repositoryRoot, "scripts", "Invoke-VideraDoctor.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraDoctorTests", Guid.NewGuid().ToString("N"));

        var captureResult = RunPowerShellScript(
            captureScript,
            repositoryRoot,
            "-OutputRoot",
            "artifacts/performance-lab-visual-evidence",
            "-ViewerScenarios",
            "viewer-instance-small",
            "-ScatterScenarios",
            "scatter-replace-100k",
            "-Width",
            "320",
            "-Height",
            "180",
            "-SimulateUnavailable");

        captureResult.ExitCode.Should().Be(0, $"stdout: {captureResult.Stdout}\nstderr: {captureResult.Stderr}");

        var doctorResult = RunPowerShell(doctorScript, outputRoot, repositoryRoot);

        doctorResult.ExitCode.Should().Be(0, $"stdout: {doctorResult.Stdout}\nstderr: {doctorResult.Stderr}");

        using var reportDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(outputRoot, "doctor-report.json")));
        var visualEvidence = reportDocument.RootElement.GetProperty("evidencePacket").GetProperty("performanceLabVisualEvidence");
        visualEvidence.GetProperty("status").GetString().Should().Be("unavailable");
        visualEvidence.GetProperty("captureStatus").GetString().Should().Be("unavailable");
        visualEvidence.GetProperty("screenshotPaths").EnumerateArray().Should().BeEmpty();
        visualEvidence.GetProperty("diagnosticsPaths").EnumerateArray().Select(static path => path.GetString()).Should().Contain([
            "artifacts/performance-lab-visual-evidence/viewer-instance-small-diagnostics.txt",
            "artifacts/performance-lab-visual-evidence/scatter-replace-100k-diagnostics.txt"
        ]);

        var summary = File.ReadAllText(Path.Combine(outputRoot, "doctor-summary.txt"));
        summary.Should().Contain("status: unavailable");
        summary.Should().Contain("records unavailable capture state");
    }

    private static DoctorRunResult RunPowerShell(string scriptPath, string outputRoot, string repositoryRoot, params string[] additionalArguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        process.StartInfo.ArgumentList.Add("-NoProfile");
        process.StartInfo.ArgumentList.Add("-ExecutionPolicy");
        process.StartInfo.ArgumentList.Add("Bypass");
        process.StartInfo.ArgumentList.Add("-File");
        process.StartInfo.ArgumentList.Add(scriptPath);
        process.StartInfo.ArgumentList.Add("-OutputRoot");
        process.StartInfo.ArgumentList.Add(outputRoot);
        foreach (var argument in additionalArguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                stdout.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                stderr.AppendLine(args.Data);
            }
        };

        process.Start().Should().BeTrue();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        var exited = process.WaitForExit(60_000);
        if (!exited)
        {
            process.Kill(entireProcessTree: true);
        }

        exited.Should().BeTrue();

        return new DoctorRunResult(process.ExitCode, stdout.ToString(), stderr.ToString());
    }

    private static DoctorRunResult RunPowerShellScript(string scriptPath, string repositoryRoot, params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        process.StartInfo.ArgumentList.Add("-NoProfile");
        process.StartInfo.ArgumentList.Add("-ExecutionPolicy");
        process.StartInfo.ArgumentList.Add("Bypass");
        process.StartInfo.ArgumentList.Add("-File");
        process.StartInfo.ArgumentList.Add(scriptPath);
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                stdout.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                stderr.AppendLine(args.Data);
            }
        };

        process.Start().Should().BeTrue();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        var exited = process.WaitForExit(120_000);
        if (!exited)
        {
            process.Kill(entireProcessTree: true);
        }

        exited.Should().BeTrue();

        return new DoctorRunResult(process.ExitCode, stdout.ToString(), stderr.ToString());
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

    private sealed record DoctorRunResult(int ExitCode, string Stdout, string Stderr);

    private sealed class DefaultVisualEvidenceDirectoryScope : IDisposable
    {
        private readonly string _directoryPath;
        private readonly string _backupPath;
        private readonly bool _hadExistingDirectory;

        private DefaultVisualEvidenceDirectoryScope(string repositoryRoot)
        {
            _directoryPath = Path.Combine(repositoryRoot, "artifacts", "performance-lab-visual-evidence");
            _backupPath = Path.Combine(Path.GetTempPath(), "VideraDoctorTests", $"visual-evidence-backup-{Guid.NewGuid():N}");
            _hadExistingDirectory = Directory.Exists(_directoryPath);

            if (_hadExistingDirectory)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_backupPath)!);
                Directory.Move(_directoryPath, _backupPath);
            }
        }

        public static DefaultVisualEvidenceDirectoryScope Create(string repositoryRoot)
        {
            return new DefaultVisualEvidenceDirectoryScope(repositoryRoot);
        }

        public void Dispose()
        {
            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(_directoryPath, recursive: true);
            }

            if (_hadExistingDirectory && Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_directoryPath)!);
                Directory.Move(_backupPath, _directoryPath);
            }
        }
    }

    private sealed class DefaultSurfaceChartsSupportDirectoryScope : IDisposable
    {
        private readonly string _directoryPath;
        private readonly string _summaryPath;
        private readonly string _backupPath;
        private readonly bool _hadExistingDirectory;

        private DefaultSurfaceChartsSupportDirectoryScope(string repositoryRoot)
        {
            _directoryPath = Path.Combine(repositoryRoot, "artifacts", "consumer-smoke");
            _summaryPath = Path.Combine(_directoryPath, "surfacecharts-support-summary.txt");
            _backupPath = Path.Combine(repositoryRoot, "artifacts", $"surfacecharts-support-backup-{Guid.NewGuid():N}");
            _hadExistingDirectory = Directory.Exists(_directoryPath);

            if (_hadExistingDirectory)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_backupPath)!);
                Directory.Move(_directoryPath, _backupPath);
            }
        }

        public static DefaultSurfaceChartsSupportDirectoryScope Create(string repositoryRoot)
        {
            return new DefaultSurfaceChartsSupportDirectoryScope(repositoryRoot);
        }

        public void WriteSupportSummary(params string[] lines)
        {
            Directory.CreateDirectory(_directoryPath);
            File.WriteAllLines(_summaryPath, lines);
        }

        public void Dispose()
        {
            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(_directoryPath, recursive: true);
            }

            if (_hadExistingDirectory && Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_directoryPath)!);
                Directory.Move(_backupPath, _directoryPath);
            }
        }
    }
}
