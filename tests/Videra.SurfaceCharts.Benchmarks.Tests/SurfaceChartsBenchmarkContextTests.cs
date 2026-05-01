using FluentAssertions;
using Videra.SurfaceCharts.Benchmarks;
using Videra.SurfaceCharts.Rendering;
using Videra.SurfaceCharts.Rendering.Software;
using Xunit;

namespace Videra.SurfaceCharts.Benchmarks.Tests;

public sealed class SurfaceChartsBenchmarkContextTests
{
    [Fact]
    public async Task BaseGpuRenderInputs_ShouldStayOnGpuWithoutFallback()
    {
        using var context = await SurfaceChartsBenchmarkContext.CreateAsync();
        var host = CreateRenderHost();

        var snapshot = host.UpdateInputs(context.GpuRenderInputs);

        AssertGpuContractSnapshot(snapshot);
    }

    [Fact]
    public async Task GpuRenderVariants_ShouldStayOnGpuWithoutFallback()
    {
        using var context = await SurfaceChartsBenchmarkContext.CreateAsync();
        var host = CreateRenderHost();

        AssertGpuContractSnapshot(host.UpdateInputs(context.GpuRenderInputs));
        AssertGpuContractSnapshot(host.UpdateInputs(context.ReplacementColorGpuRenderInputs));
        AssertGpuContractSnapshot(host.UpdateInputs(context.OrbitGpuRenderInputs));
        AssertGpuContractSnapshot(host.UpdateInputs(context.ResizedGpuRenderInputs));
        AssertGpuContractSnapshot(host.UpdateInputs(context.ReboundGpuRenderInputs));
    }

    private static SurfaceChartRenderHost CreateRenderHost()
    {
        return new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(new BenchmarkGraphicsBackend()));
    }

    private static void AssertGpuContractSnapshot(SurfaceChartRenderSnapshot snapshot)
    {
        snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        snapshot.IsFallback.Should().BeFalse();
        snapshot.FallbackReason.Should().BeNull();
        snapshot.IsReady.Should().BeTrue();
        snapshot.UsesNativeSurface.Should().BeTrue();
    }
}
