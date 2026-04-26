using System.Diagnostics;
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

        script.Should().NotContain("dotnet nuget push");
        script.Should().NotContain("git push");
        script.Should().NotContain("git tag");
        script.Should().NotContain("Set-ExecutionPolicy");
        script.Should().NotContain("Remove-Item");

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

        var docsIndex = File.ReadAllText(docsIndexPath);
        docsIndex.Should().Contain("videra-doctor.md");
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
    }

    private static DoctorRunResult RunPowerShell(string scriptPath, string outputRoot, string repositoryRoot)
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

        process.Start().Should().BeTrue();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(60_000).Should().BeTrue();

        return new DoctorRunResult(process.ExitCode, stdout, stderr);
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
}
