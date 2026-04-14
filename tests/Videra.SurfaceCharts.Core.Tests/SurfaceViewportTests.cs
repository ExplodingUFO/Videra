using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceViewportTests
{
    [Fact]
    public void ClampTo_ClampsViewportIntoMetadataBounds()
    {
        var metadata = CreateMetadata(100, 80);
        var viewport = new SurfaceViewport(-15.0, -10.0, 140.0, 120.0);

        var clamped = viewport.ClampTo(metadata);

        clamped.StartX.Should().Be(0.0);
        clamped.StartY.Should().Be(0.0);
        clamped.Width.Should().Be(100.0);
        clamped.Height.Should().Be(80.0);
    }

    [Fact]
    public void ClampTo_ShiftsViewportToPreserveSpanWhenPossible()
    {
        var metadata = CreateMetadata(100, 80);
        var viewport = new SurfaceViewport(85.0, 70.0, 20.0, 15.0);

        var clamped = viewport.ClampTo(metadata);

        clamped.StartX.Should().Be(80.0);
        clamped.StartY.Should().Be(65.0);
        clamped.Width.Should().Be(20.0);
        clamped.Height.Should().Be(15.0);
    }

    [Fact]
    public void Normalize_ConvertsClampedViewportToUnitSpace()
    {
        var metadata = CreateMetadata(100, 50);
        var viewport = new SurfaceViewport(10.0, 5.0, 40.0, 20.0);

        SurfaceNormalizedViewport normalized = viewport.Normalize(metadata);

        normalized.StartX.Should().BeApproximately(0.10, 0.000001);
        normalized.StartY.Should().BeApproximately(0.10, 0.000001);
        normalized.Width.Should().BeApproximately(0.40, 0.000001);
        normalized.Height.Should().BeApproximately(0.40, 0.000001);
    }

    [Fact]
    public void Request_ExposesClampedAndNormalizedViewport()
    {
        var metadata = CreateMetadata(200, 100);
        var request = new SurfaceViewportRequest(
            metadata,
            new SurfaceViewport(-20.0, 25.0, 80.0, 40.0),
            40,
            20);

        request.ClampedViewport.StartX.Should().Be(0.0);
        request.ClampedViewport.StartY.Should().Be(25.0);
        request.ClampedViewport.Width.Should().Be(80.0);
        request.ClampedViewport.Height.Should().Be(40.0);

        request.NormalizedViewport.StartX.Should().BeApproximately(0.0, 0.000001);
        request.NormalizedViewport.StartY.Should().BeApproximately(0.25, 0.000001);
        request.NormalizedViewport.Width.Should().BeApproximately(0.40, 0.000001);
        request.NormalizedViewport.Height.Should().BeApproximately(0.40, 0.000001);
    }

    [Fact]
    public void Request_NormalizedViewport_UsesDedicatedUnitSpaceType()
    {
        var metadata = CreateMetadata(200, 100);
        var request = new SurfaceViewportRequest(
            metadata,
            new SurfaceViewport(10.0, 15.0, 80.0, 40.0),
            40,
            20);

        request.NormalizedViewport.Should().BeOfType<SurfaceNormalizedViewport>();
    }

    [Fact]
    public void SurfaceViewport_ToDataWindow_PreservesSampleSpaceBounds()
    {
        var viewport = new SurfaceViewport(10.0, 15.0, 80.0, 40.0);

        var dataWindow = viewport.ToDataWindow();

        dataWindow.XMin.Should().Be(10.0);
        dataWindow.XMax.Should().Be(90.0);
        dataWindow.YMin.Should().Be(15.0);
        dataWindow.YMax.Should().Be(55.0);
    }

    [Fact]
    public void SurfaceViewport_FromDataWindow_RestoresViewportSpan()
    {
        var dataWindow = new SurfaceDataWindow(5.0, 25.0, 30.0, 45.0);

        var viewport = SurfaceViewport.FromDataWindow(dataWindow);

        viewport.StartX.Should().Be(5.0);
        viewport.StartY.Should().Be(30.0);
        viewport.Width.Should().Be(20.0);
        viewport.Height.Should().Be(15.0);
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0.0, 12.0),
            new SurfaceAxisDescriptor("Frequency", "Hz", 10.0, 20_000.0),
            new SurfaceValueRange(-3.5, 9.25));
    }
}
