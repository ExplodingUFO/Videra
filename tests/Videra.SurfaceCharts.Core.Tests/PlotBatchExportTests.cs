using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class PlotBatchExportTests
{
    // ── Manifest construction ──────────────────────────────────────

    [Fact]
    public void Manifest_AllSucceeded_ReturnsTrue()
    {
        var manifest = CreateManifest(succeeded: [true, true, true]);

        manifest.AllSucceeded.Should().BeTrue();
        manifest.SuccessCount.Should().Be(3);
        manifest.FailureCount.Should().Be(0);
        manifest.TotalCount.Should().Be(3);
    }

    [Fact]
    public void Manifest_HasFailures_ReturnsFalse()
    {
        var manifest = CreateManifest(succeeded: [true, false, true]);

        manifest.AllSucceeded.Should().BeFalse();
        manifest.SuccessCount.Should().Be(2);
        manifest.FailureCount.Should().Be(1);
    }

    [Fact]
    public void Manifest_TimestampsAreUtc()
    {
        var manifest = CreateManifest(succeeded: [true]);

        manifest.StartedUtc.Kind.Should().Be(DateTimeKind.Utc);
        manifest.CompletedUtc.Kind.Should().Be(DateTimeKind.Utc);
        manifest.CompletedUtc.Should().BeOnOrAfter(manifest.StartedUtc);
    }

    [Fact]
    public void Manifest_EntriesPreserveNames()
    {
        var manifest = CreateManifest(succeeded: [true, false], names: ["chart-a", "chart-b"]);

        manifest.Entries[0].Name.Should().Be("chart-a");
        manifest.Entries[1].Name.Should().Be("chart-b");
    }

    // ── Entry construction ─────────────────────────────────────────

    [Fact]
    public void Entry_Successful_HasOutputPath()
    {
        var entry = new PlotBatchManifestEntry(
            "test", "/tmp/out.png", "Png", 800, 600, true, null, "Scatter:test:0", TimeSpan.FromMilliseconds(50));

        entry.Succeeded.Should().BeTrue();
        entry.OutputPath.Should().Be("/tmp/out.png");
        entry.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Entry_Failed_HasErrorMessage()
    {
        var entry = new PlotBatchManifestEntry(
            "test", null, "Png", 800, 600, false, "No active series", null, TimeSpan.FromMilliseconds(5));

        entry.Succeeded.Should().BeFalse();
        entry.OutputPath.Should().BeNull();
        entry.ErrorMessage.Should().Be("No active series");
    }

    // ── JSON round-trip ────────────────────────────────────────────

    [Fact]
    public void Manifest_RoundTripsThroughJson()
    {
        var original = CreateManifest(succeeded: [true, false], names: ["chart-a", "chart-b"]);
        var json = original.ToJson();
        var deserialized = PlotBatchManifest.FromJson(json);

        deserialized.Should().NotBeNull();
        deserialized!.TotalCount.Should().Be(original.TotalCount);
        deserialized.SuccessCount.Should().Be(original.SuccessCount);
        deserialized.FailureCount.Should().Be(original.FailureCount);
        deserialized.Entries.Count.Should().Be(original.Entries.Count);
        deserialized.Entries[0].Name.Should().Be("chart-a");
        deserialized.Entries[1].Name.Should().Be("chart-b");
        deserialized.Entries[0].Succeeded.Should().BeTrue();
        deserialized.Entries[1].Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Manifest_JsonContainsRequiredFields()
    {
        var manifest = CreateManifest(succeeded: [true]);
        var json = manifest.ToJson();

        json.Should().Contain("Entries");
        json.Should().Contain("StartedUtc");
        json.Should().Contain("CompletedUtc");
        json.Should().Contain("TotalCount");
        json.Should().Contain("SuccessCount");
        json.Should().Contain("FailureCount");
        json.Should().Contain("AllSucceeded");
    }

    [Fact]
    public void Manifest_EntryJsonContainsAllFields()
    {
        var entry = new PlotBatchManifestEntry(
            "scatter-1", "/tmp/s.svg", "Svg", 1024, 768, true, null, "Scatter:demo:0", TimeSpan.FromMilliseconds(120));
        var manifest = new PlotBatchManifest([entry], DateTime.UtcNow, DateTime.UtcNow);
        var json = manifest.ToJson();

        json.Should().Contain("scatter-1");
        json.Should().Contain("/tmp/s.svg");
        json.Should().Contain("Svg");
        json.Should().Contain("1024");
        json.Should().Contain("768");
    }

    // ── SaveAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_CreatesFileAndDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"batch-test-{Guid.NewGuid():N}");
        var manifestPath = Path.Combine(tempDir, "manifest.json");

        try
        {
            var manifest = CreateManifest(succeeded: [true]);
            await manifest.SaveAsync(manifestPath);

            File.Exists(manifestPath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(manifestPath);
            content.Should().Contain("TotalCount");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    // ── Batch item ─────────────────────────────────────────────────

    [Fact]
    public void BatchItem_CreatesWithPlotAndName()
    {
        var plot = new Plot3D(() => { });
        var item = new PlotBatchItem(plot, "my-chart");

        item.Plot.Should().BeSameAs(plot);
        item.Name.Should().Be("my-chart");
    }

    // ── Naming pattern ─────────────────────────────────────────────

    [Fact]
    public async Task ExportAsync_AppliesNamingPattern()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"batch-naming-{Guid.NewGuid():N}");

        try
        {
            var plot = CreatePlotWithSeries();
            var items = new[] { new PlotBatchItem(plot, "test-chart") };
            var request = new PlotSnapshotRequest(400, 300, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Svg);

            var manifest = await PlotBatchExporter.ExportAsync(items, tempDir, request, "{name}_v1.{format}");

            manifest.Entries[0].OutputPath.Should().Contain("test-chart_v1.svg");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_SvgFormat_WritesManifest()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"batch-manifest-{Guid.NewGuid():N}");

        try
        {
            var plot = CreatePlotWithSeries();
            var items = new[] { new PlotBatchItem(plot, "scatter-1") };
            var request = new PlotSnapshotRequest(400, 300, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Svg);

            var manifest = await PlotBatchExporter.ExportAsync(items, tempDir, request);

            manifest.AllSucceeded.Should().BeTrue();
            manifest.Entries[0].Format.Should().Be("Svg");

            var manifestPath = Path.Combine(tempDir, "manifest.json");
            File.Exists(manifestPath).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static PlotBatchManifest CreateManifest(bool[] succeeded, string[]? names = null)
    {
        var entries = new List<PlotBatchManifestEntry>();
        for (var i = 0; i < succeeded.Length; i++)
        {
            var name = names is not null && i < names.Length ? names[i] : $"chart-{i}";
            entries.Add(new PlotBatchManifestEntry(
                name,
                succeeded[i] ? $"/tmp/{name}.png" : null,
                "Png",
                800, 600,
                succeeded[i],
                succeeded[i] ? null : "Export failed",
                succeeded[i] ? $"Scatter:{name}:0" : null,
                TimeSpan.FromMilliseconds(50)));
        }

        return new PlotBatchManifest(entries, DateTime.UtcNow, DateTime.UtcNow.AddMilliseconds(100));
    }

    private static Plot3D CreatePlotWithSeries()
    {
        var plot = new Plot3D(() => { });
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, 0, 10),
            new SurfaceAxisDescriptor("Z", null, 0, 10),
            new SurfaceValueRange(0, 10));
        var series = new ScatterSeries([new ScatterPoint(1, 1, 1)], 0xFF0000);
        var scatterData = new ScatterChartData(metadata, [series]);
        plot.Add.Scatter(scatterData, "test-series");
        return plot;
    }
}
