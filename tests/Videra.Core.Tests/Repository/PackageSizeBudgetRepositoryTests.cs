using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class PackageSizeBudgetRepositoryTests
{
    [Fact]
    public void PackageSizeBudgetContract_ShouldDefineCanonicalPublicPackageBudgets()
    {
        var repositoryRoot = GetRepositoryRoot();
        var budgetPath = Path.Combine(repositoryRoot, "eng", "package-size-budgets.json");

        File.Exists(budgetPath).Should().BeTrue();

        using var budgetDocument = JsonDocument.Parse(File.ReadAllText(budgetPath));
        var root = budgetDocument.RootElement;
        root.GetProperty("schemaVersion").GetInt32().Should().Be(1);

        var packages = root.GetProperty("packages").EnumerateArray().ToDictionary(
            static package => package.GetProperty("id").GetString()!,
            static package => package);

        packages.Keys.Should().BeEquivalentTo(
            "Videra.Core",
            "Videra.Import.Gltf",
            "Videra.Import.Obj",
            "Videra.Avalonia",
            "Videra.Platform.Windows",
            "Videra.Platform.Linux",
            "Videra.Platform.macOS",
            "Videra.SurfaceCharts.Core",
            "Videra.SurfaceCharts.Rendering",
            "Videra.SurfaceCharts.Processing",
            "Videra.SurfaceCharts.Avalonia");

        AssertBudget(packages["Videra.Core"], 166_912, 70_656);
        AssertBudget(packages["Videra.Import.Gltf"], 25_600, 20_480);
        AssertBudget(packages["Videra.Import.Obj"], 18_432, 17_408);
        AssertBudget(packages["Videra.Avalonia"], 164_864, 65_536);
        AssertBudget(packages["Videra.Platform.Windows"], 31_744, 20_480);
        AssertBudget(packages["Videra.Platform.Linux"], 46_080, 24_576);
        AssertBudget(packages["Videra.Platform.macOS"], 33_792, 23_552);
        AssertBudget(packages["Videra.SurfaceCharts.Core"], 57_344, 24_576);
        AssertBudget(packages["Videra.SurfaceCharts.Rendering"], 28_672, 18_432);
        AssertBudget(packages["Videra.SurfaceCharts.Processing"], 36_864, 16_384);
        AssertBudget(packages["Videra.SurfaceCharts.Avalonia"], 66_816, 29_696);
    }

    [Fact]
    public void ValidatePackagesScript_ShouldEmitPackageSizeEvaluationArtifacts()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Validate-Packages.ps1");

        File.Exists(scriptPath).Should().BeTrue();

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("eng/package-size-budgets.json");
        script.Should().Contain("package-size-evaluation.json");
        script.Should().Contain("package-size-summary.txt");
        script.Should().Contain(".nupkg");
        script.Should().Contain(".snupkg");
        script.Should().Contain("Package size budgets failed");
    }

    private static void AssertBudget(JsonElement package, long nupkgMaxBytes, long snupkgMaxBytes)
    {
        package.GetProperty("nupkgMaxBytes").GetInt64().Should().Be(nupkgMaxBytes);
        package.GetProperty("snupkgMaxBytes").GetInt64().Should().Be(snupkgMaxBytes);
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
