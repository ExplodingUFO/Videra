using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests.Workspace;

public sealed class SurfaceChartStreamingEvidenceTests
{
    [Fact]
    public void RegisterStreamingStatus_stores_status()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chart = new VideraChartView();
            workspace.Register(chart, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));

            var status = new SurfaceChartStreamingStatus
            {
                UpdateMode = "Replace",
                RetainedPointCount = 100_000,
                EvidenceOnly = true,
            };

            workspace.RegisterStreamingStatus("chart-a", status);

            var retrieved = workspace.GetStreamingStatus("chart-a");
            retrieved.Should().NotBeNull();
            retrieved!.UpdateMode.Should().Be("Replace");
            retrieved.RetainedPointCount.Should().Be(100_000);
            retrieved.EvidenceOnly.Should().BeTrue();
        });
    }

    [Fact]
    public void Unregister_removes_streaming_status()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chart = new VideraChartView();
            workspace.Register(chart, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));

            var status = new SurfaceChartStreamingStatus
            {
                UpdateMode = "Append",
                RetainedPointCount = 50_000,
                FifoCapacity = 100_000,
                EvidenceOnly = true,
            };

            workspace.RegisterStreamingStatus("chart-a", status);
            workspace.Unregister("chart-a");

            workspace.GetStreamingStatus("chart-a").Should().BeNull();
        });
    }

    [Fact]
    public void GetStreamingStatus_returns_null_for_unknown()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();

            workspace.GetStreamingStatus("nonexistent").Should().BeNull();
        });
    }

    [Fact]
    public void RegisterStreamingStatus_updates_existing()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chart = new VideraChartView();
            workspace.Register(chart, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));

            var first = new SurfaceChartStreamingStatus
            {
                UpdateMode = "Replace",
                RetainedPointCount = 50_000,
                EvidenceOnly = true,
            };
            workspace.RegisterStreamingStatus("chart-a", first);

            var second = new SurfaceChartStreamingStatus
            {
                UpdateMode = "Append",
                RetainedPointCount = 75_000,
                FifoCapacity = 100_000,
                EvidenceOnly = false,
            };
            workspace.RegisterStreamingStatus("chart-a", second);

            var retrieved = workspace.GetStreamingStatus("chart-a");
            retrieved.Should().NotBeNull();
            retrieved!.UpdateMode.Should().Be("Append");
            retrieved.RetainedPointCount.Should().Be(75_000);
            retrieved.FifoCapacity.Should().Be(100_000);
            retrieved.EvidenceOnly.Should().BeFalse();
        });
    }

    [Fact]
    public void WorkspaceEvidence_includes_streaming_section_when_statuses_exist()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chartA = new VideraChartView();
            workspace.Register(chartA, new SurfaceChartPanelInfo("chart-a", "Scatter A", Plot3DSeriesKind.Scatter));

            workspace.RegisterStreamingStatus("chart-a", new SurfaceChartStreamingStatus
            {
                UpdateMode = "Replace",
                RetainedPointCount = 100_000,
                ReplaceBatchCount = 5,
                EvidenceOnly = true,
            });

            var evidence = workspace.CreateWorkspaceEvidence();

            evidence.Should().Contain("StreamingChartCount: 1");
            evidence.Should().Contain("Streaming[chart-a]");
            evidence.Should().Contain("Mode=Replace");
            evidence.Should().Contain("Retained=100000");
            evidence.Should().Contain("StreamingBoundary:");
        });
    }

    [Fact]
    public void WorkspaceEvidence_no_streaming_section_when_no_statuses()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chartA = new VideraChartView();
            workspace.Register(chartA, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));

            var evidence = workspace.CreateWorkspaceEvidence();

            evidence.Should().NotContain("StreamingChartCount");
            evidence.Should().NotContain("StreamingBoundary");
        });
    }
}
