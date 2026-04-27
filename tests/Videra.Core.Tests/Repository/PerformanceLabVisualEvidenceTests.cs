using System.Text.Json;
using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Videra.PerformanceLabVisualEvidence;
using Xunit;

namespace Videra.Core.Tests.Repository;

public class PerformanceLabVisualEvidenceTests
{
    [Fact]
    public void CiWorkflow_ShouldPublishPerformanceLabVisualEvidenceAsEvidenceOnlyArtifact()
    {
        var repositoryRoot = GetRepositoryRoot();
        var workflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "ci.yml"));

        workflow.Should().Contain("performance-lab-visual-evidence:");
        workflow.Should().Contain("Invoke-PerformanceLabVisualEvidence.ps1");
        workflow.Should().Contain("artifacts/performance-lab-visual-evidence");
        workflow.Should().Contain("actions/upload-artifact@v4");
        workflow.Should().Contain("FullyQualifiedName~PerformanceLabVisualEvidenceTests");
        workflow.Should().NotContain("pixel-diff");
    }

    [Fact]
    public void Documentation_ShouldDescribeVisualEvidenceBoundaries()
    {
        var repositoryRoot = GetRepositoryRoot();
        var rootReadme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var demoReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.Demo", "README.md"));
        var surfaceChartsReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));
        var qualityGates = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "maintenance-quality-gates.md"));
        var chineseReadme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var chineseDemo = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "demo.md"));

        foreach (var document in new[] { rootReadme, demoReadme, surfaceChartsReadme, qualityGates, chineseReadme, chineseDemo })
        {
            document.Should().Contain("Invoke-PerformanceLabVisualEvidence.ps1");
            document.Should().Contain("artifacts/performance-lab-visual-evidence");
        }

        rootReadme.Should().Contain("not pixel-perfect visual-regression gates");
        demoReadme.Should().Contain("does not claim real GPU instancing");
        surfaceChartsReadme.Should().Contain("not a claim that SurfaceCharts has gained GPU-driven culling or a new chart family");
        qualityGates.Should().Contain("not pixel-perfect visual-regression gates");
        chineseReadme.Should().Contain("不是 pixel-perfect visual-regression gate");
        chineseDemo.Should().Contain("不代表 real GPU instancing");
    }

    [Fact]
    public void Capture_ShouldWriteManifestSummaryDiagnosticsAndNonblankPngArtifacts()
    {
        using var workspace = TemporaryDirectory.Create();
        var result = PerformanceLabVisualEvidenceCapture.Capture(new PerformanceLabVisualEvidenceOptions(
            workspace.Path,
            320,
            180,
            ["viewer-instance-small"],
            ["scatter-replace-100k"]));

        result.Status.Should().Be("produced");
        File.Exists(result.ManifestPath).Should().BeTrue();

        using var manifest = JsonDocument.Parse(File.ReadAllText(result.ManifestPath));
        var root = manifest.RootElement;
        root.GetProperty("schemaVersion").GetInt32().Should().Be(1);
        root.GetProperty("evidenceKind").GetString().Should().Be("PerformanceLabVisualEvidence");
        root.GetProperty("evidenceOnly").GetBoolean().Should().BeTrue();
        root.GetProperty("status").GetString().Should().Be("produced");
        root.GetProperty("width").GetInt32().Should().Be(320);
        root.GetProperty("height").GetInt32().Should().Be(180);
        var summaryPath = root.GetProperty("summaryPath").GetString();
        summaryPath.Should().NotBeNullOrWhiteSpace();
        File.Exists(ToLocalPath(summaryPath!)).Should().BeTrue();

        var entries = root.GetProperty("entries").EnumerateArray().ToArray();
        entries.Should().HaveCount(2);
        entries.Select(static entry => entry.GetProperty("id").GetString()).Should().Equal(
            "viewer-instance-small",
            "scatter-replace-100k");

        foreach (var entry in entries)
        {
            entry.GetProperty("status").GetString().Should().Be("produced");
            var pngPath = entry.GetProperty("pngPath").GetString();
            pngPath.Should().NotBeNullOrWhiteSpace();
            AssertPngIsNonblank(ToLocalPath(pngPath!));

            var diagnosticsPath = entry.GetProperty("diagnosticsPath").GetString();
            diagnosticsPath.Should().NotBeNullOrWhiteSpace();
            File.ReadAllText(ToLocalPath(diagnosticsPath!)).Should().Contain("EvidenceOnly: true");
            entry.GetProperty("artifacts").EnumerateArray().Select(static artifact => artifact.GetString())
                .Should().Contain([pngPath, diagnosticsPath]);
        }

        File.ReadAllText(ToLocalPath(summaryPath!)).Should().Contain("PixelPerfectRegressionGate: false");
    }

    [Fact]
    public void Capture_ShouldRepresentUnavailableVisualHostWithoutPngArtifacts()
    {
        using var workspace = TemporaryDirectory.Create();
        var result = PerformanceLabVisualEvidenceCapture.Capture(new PerformanceLabVisualEvidenceOptions(
            workspace.Path,
            320,
            180,
            ["viewer-instance-small"],
            ["scatter-replace-100k"],
            SimulateUnavailable: true));

        result.Status.Should().Be("unavailable");

        using var manifest = JsonDocument.Parse(File.ReadAllText(result.ManifestPath));
        var root = manifest.RootElement;
        root.GetProperty("status").GetString().Should().Be("unavailable");
        root.GetProperty("notes").EnumerateArray().Select(static note => note.GetString())
            .Any(static note => note != null && note.Contains("not product rendering failures", StringComparison.Ordinal))
            .Should().BeTrue();

        foreach (var entry in root.GetProperty("entries").EnumerateArray())
        {
            entry.GetProperty("status").GetString().Should().Be("unavailable");
            entry.GetProperty("pngPath").ValueKind.Should().Be(JsonValueKind.Null);
            var diagnosticsPath = entry.GetProperty("diagnosticsPath").GetString();
            File.Exists(ToLocalPath(diagnosticsPath!)).Should().BeTrue();
            entry.GetProperty("artifacts").EnumerateArray().Select(static artifact => artifact.GetString())
                .Should().Equal(diagnosticsPath);
        }
    }

    private static void AssertPngIsNonblank(string path)
    {
        File.Exists(path).Should().BeTrue();
        using var image = Image.Load<Rgba32>(path);
        image.Width.Should().Be(320);
        image.Height.Should().Be(180);

        var first = image[0, 0];
        var differentPixelFound = false;
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height && !differentPixelFound; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    if (!row[x].Equals(first))
                    {
                        differentPixelFound = true;
                        break;
                    }
                }
            }
        });
        differentPixelFound.Should().BeTrue("visual evidence PNG should contain more than a blank solid fill");
    }

    private static string ToLocalPath(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Videra.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Unable to locate repository root.");
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryDirectory Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"videra-visual-evidence-{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            return new TemporaryDirectory(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
