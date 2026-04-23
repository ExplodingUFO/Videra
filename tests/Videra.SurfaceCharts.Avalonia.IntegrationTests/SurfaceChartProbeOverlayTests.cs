using System.Linq;
using System.Reflection;
using System.Numerics;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartProbeOverlayTests
{
    [Fact]
    public Task ProbeUpdate_WithLoadedTiles_ProducesReadoutState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 7f);
            var view = new SurfaceChartView();

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));
            view.Viewport = new SurfaceViewport(128, 64, 256, 128);
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            UpdateProbeScreenPosition(view, new Point(64, 32));

            var overlayState = GetOverlayState(view);

            GetBooleanProperty(overlayState, "HasNoData").Should().BeFalse();
            GetStringProperty(overlayState, "NoDataText").Should().BeNull();
            GetStringProperty(overlayState, "ReadoutText").Should().Contain("7");

            var probeResult = GetPropertyValue(overlayState, "ProbeResult");
            probeResult.Should().NotBeNull();
            GetDoubleProperty(probeResult!, "SampleX").Should().BeApproximately(192d, 0.0001d);
            GetDoubleProperty(probeResult!, "SampleY").Should().BeApproximately(96d, 0.0001d);
            GetDoubleProperty(probeResult!, "Value").Should().Be(7d);
        });
    }

    [Fact]
    public Task ProbeUpdate_RepeatedSameScreenPosition_DoesNotReplaceOverlayState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 7f);
            var view = new SurfaceChartView();

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));
            view.Viewport = new SurfaceViewport(128, 64, 256, 128);
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            UpdateProbeScreenPosition(view, new Point(64, 32));

            var initialState = GetOverlayState(view);
            UpdateProbeScreenPosition(view, new Point(64, 32)).Should().BeFalse();

            var repeatedState = GetOverlayState(view);
            repeatedState.Should().BeSameAs(initialState);
        });
    }

    [Fact]
    public Task SourceWithoutCommittedTiles_PresentsNoDataOverlayState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var completion = new TaskCompletionSource<SurfaceTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 9f);
            var view = new SurfaceChartView();

            source.EnqueuePendingResponse(started, completion, observeCancellation: true);

            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));
            view.Source = source;

            await started.Task;

            var overlayState = GetOverlayState(view);

            GetBooleanProperty(overlayState, "HasNoData").Should().BeTrue();
            GetStringProperty(overlayState, "NoDataText").Should().Be("No data");
            GetStringProperty(overlayState, "ReadoutText").Should().BeNull();
            GetPropertyValue(overlayState, "ProbeResult").Should().BeNull();

            view.Source = null;
        });
    }

    [Fact]
    public Task ViewportAndSourceChanges_RecomputeAndClearOverlayState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 5f);
            var view = new SurfaceChartView();

            view.Measure(new Size(200, 100));
            view.Arrange(new Rect(0, 0, 200, 100));
            view.Viewport = new SurfaceViewport(100, 50, 200, 100);
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            UpdateProbeScreenPosition(view, new Point(100, 50));

            var initialState = GetOverlayState(view);
            GetDoubleProperty(GetPropertyValue(initialState, "ProbeResult")!, "SampleX").Should().BeApproximately(200d, 0.0001d);
            GetDoubleProperty(GetPropertyValue(initialState, "ProbeResult")!, "SampleY").Should().BeApproximately(100d, 0.0001d);

            view.Viewport = new SurfaceViewport(300, 250, 200, 100);

            var updatedState = GetOverlayState(view);
            GetBooleanProperty(updatedState, "HasNoData").Should().BeFalse();
            GetDoubleProperty(GetPropertyValue(updatedState, "ProbeResult")!, "SampleX").Should().BeApproximately(400d, 0.0001d);
            GetDoubleProperty(GetPropertyValue(updatedState, "ProbeResult")!, "SampleY").Should().BeApproximately(300d, 0.0001d);

            view.Source = null;

            var clearedState = GetOverlayState(view);
            GetBooleanProperty(clearedState, "HasNoData").Should().BeTrue();
            GetStringProperty(clearedState, "ReadoutText").Should().BeNull();
            GetPropertyValue(clearedState, "ProbeResult").Should().BeNull();
        });
    }

    [Fact]
    public void CoarseTileProbe_MapsSampleCoordinatesThroughClampedViewportAndTileBoundsSpan()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var coarseTile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 1024, 1024),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(-50, -40, 200, 200),
            new Size(100, 100),
            [coarseTile],
            new Point(50, 50));

        state.HasNoData.Should().BeFalse();
        state.ProbeResult.Should().NotBeNull();
        state.ProbeResult!.Value.SampleX.Should().BeApproximately(100d, 0.0001d);
        state.ProbeResult.Value.SampleY.Should().BeApproximately(100d, 0.0001d);
        var hoveredProbe = GetHoveredProbe(state);
        GetDoubleProperty(hoveredProbe, "AxisX").Should().BeApproximately(100d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisY").Should().BeApproximately(100d, 0.0001d);
        state.ProbeResult.Value.Value.Should().Be(10d);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeTrue();
    }

    [Fact]
    public void CoarseTileProbe_FallsBackToDiscreteReadAndRemainsApproximate()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var coarseTile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 1024, 1024),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(-50, -40, 200, 200),
            new Size(100, 100),
            [coarseTile],
            new Point(50, 50));

        var hoveredProbe = GetHoveredProbe(state);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeTrue();
        state.ReadoutText.Should().Contain("Approx");
        state.ProbeResult.Should().NotBeNull();
        state.ProbeResult!.Value.Value.Should().Be(10d);
    }

    [Fact]
    public void ExactTileProbe_InterpolatesBetweenNeighboringSamples()
    {
        var metadata = new SurfaceMetadata(
            width: 2,
            height: 2,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(-1d, 1d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 2, 2),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(0, 0, 1, 1),
            new Size(2, 2),
            [tile],
            new Point(1, 1));

        var hoveredProbe = GetHoveredProbe(state);
        GetDoubleProperty(hoveredProbe, "SampleX").Should().BeApproximately(0.5d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "SampleY").Should().BeApproximately(0.5d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisX").Should().BeApproximately(15d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisY").Should().BeApproximately(150d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "Value").Should().BeApproximately(25d, 0.0001d);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeFalse();
    }

    [Fact]
    public void ExactTileProbe_ResolvesAtRightBottomEdge()
    {
        var metadata = new SurfaceMetadata(
            width: 2,
            height: 2,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(-1d, 1d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 2, 2),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(0, 0, 1, 1),
            new Size(2, 2),
            [tile],
            new Point(2, 2));

        var hoveredProbe = GetHoveredProbe(state);
        GetDoubleProperty(hoveredProbe, "SampleX").Should().BeApproximately(1d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "SampleY").Should().BeApproximately(1d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisX").Should().BeApproximately(20d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisY").Should().BeApproximately(200d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "Value").Should().BeApproximately(40d, 0.0001d);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeFalse();
    }

    [Fact]
    public void ExactTileProbe_ResolvesExactSampleNextToMaskedNeighbor()
    {
        var metadata = new SurfaceMetadata(
            width: 2,
            height: 2,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(-1d, 1d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 2, 2),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[]
                {
                    10f, 20f,
                    30f, 40f
                },
                range: metadata.ValueRange),
            colorField: null,
            mask: new SurfaceMask(
                width: 2,
                height: 2,
                values: new bool[]
                {
                    true, false,
                    false, false
                }));

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(0, 0, 1, 1),
            new Size(2, 2),
            [tile],
            new Point(0, 0));

        var hoveredProbe = GetHoveredProbe(state);
        GetDoubleProperty(hoveredProbe, "SampleX").Should().BeApproximately(0d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "SampleY").Should().BeApproximately(0d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "Value").Should().BeApproximately(10d, 0.0001d);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeFalse();
    }

    [Fact]
    public void ProbeOverlay_ClampsViewportAndReportsAxisTruth()
    {
        var metadata = new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(-1d, 1d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 5,
            height: 5,
            new SurfaceTileBounds(0, 0, 5, 5),
            Enumerable.Repeat(3f, 25).ToArray(),
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(-2, -2, 10, 10),
            new Size(100, 100),
            [tile],
            new Point(50, 50));

        var hoveredProbe = GetHoveredProbe(state);
        GetDoubleProperty(hoveredProbe, "SampleX").Should().BeApproximately(2.5d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "SampleY").Should().BeApproximately(2.5d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisX").Should().BeApproximately(16.25d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisY").Should().BeApproximately(162.5d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "Value").Should().Be(3d);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeFalse();
    }

    [Fact]
    public void HoveredReadout_IncludesDeltaAgainstFirstPinnedProbe()
    {
        var metadata = new SurfaceMetadata(
            width: 2,
            height: 2,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(-1d, 1d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 2, 2),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(0, 0, 1, 1),
            new Size(2, 2),
            [tile],
            new Point(1, 1),
            [new SurfaceProbeRequest(0d, 0d)]);

        state.PinnedProbes.Should().ContainSingle();
        state.ReadoutText.Should().Contain("Delta vs Pin 1");
        state.ReadoutText.Should().Contain("X +5");
        state.ReadoutText.Should().Contain("Y +50");
        state.ReadoutText.Should().Contain("Value +15");
    }

    [Fact]
    public Task ProbeUpdate_ViewportFocusChange_RecomputesAxisValues()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 5,
                height: 5,
                new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
                new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
                new SurfaceValueRange(-2d, 2d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 5f);
            var view = new SurfaceChartView();

            view.Measure(new Size(200, 200));
            view.Arrange(new Rect(0, 0, 200, 200));
            view.Viewport = new SurfaceViewport(0, 0, 4, 4);
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            UpdateProbeScreenPosition(view, new Point(100, 100));

            var initialProbe = GetHoveredProbe(GetOverlayState(view));
            GetDoubleProperty(initialProbe, "SampleX").Should().BeApproximately(2d, 0.0001d);
            GetDoubleProperty(initialProbe, "SampleY").Should().BeApproximately(2d, 0.0001d);
            GetDoubleProperty(initialProbe, "AxisX").Should().BeApproximately(15d, 0.0001d);
            GetDoubleProperty(initialProbe, "AxisY").Should().BeApproximately(150d, 0.0001d);

            view.Viewport = new SurfaceViewport(2d, 1d, 1d, 2d);

            await SurfaceChartTestHelpers.AssertLoadedTileValuesStayAsync(view, [5f]);

            var focusedProbe = GetHoveredProbe(GetOverlayState(view));
            GetDoubleProperty(focusedProbe, "SampleX").Should().BeApproximately(2.5d, 0.0001d);
            GetDoubleProperty(focusedProbe, "SampleY").Should().BeApproximately(2d, 0.0001d);
            GetDoubleProperty(focusedProbe, "AxisX").Should().BeApproximately(16.25d, 0.0001d);
            GetDoubleProperty(focusedProbe, "AxisY").Should().BeApproximately(150d, 0.0001d);
            GetBooleanProperty(focusedProbe, "IsApproximate").Should().BeFalse();
        });
    }

    [Fact]
    public Task SurfaceChartRuntime_ViewStateChangesRecomputeOverlayFromCompatibilityViewport()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 5,
                height: 5,
                new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
                new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
                new SurfaceValueRange(-2d, 2d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 5f);
            var view = new SurfaceChartView();

            view.Measure(new Size(200, 200));
            view.Arrange(new Rect(0, 0, 200, 200));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            var runtime = SurfaceChartTestHelpers.GetRuntime(view);
            runtime.UpdateViewState(SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, 4d, 4d)));

            UpdateProbeScreenPosition(view, new Point(100, 100));

            var initialProbe = GetHoveredProbe(GetOverlayState(view));
            GetDoubleProperty(initialProbe, "SampleX").Should().BeApproximately(2d, 0.0001d);
            GetDoubleProperty(initialProbe, "SampleY").Should().BeApproximately(2d, 0.0001d);

            runtime.UpdateViewState(SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(2d, 1d, 1d, 2d)));

            var focusedProbe = GetHoveredProbe(GetOverlayState(view));
            GetDoubleProperty(focusedProbe, "SampleX").Should().BeApproximately(2.5d, 0.0001d);
            GetDoubleProperty(focusedProbe, "SampleY").Should().BeApproximately(2d, 0.0001d);
            GetDoubleProperty(focusedProbe, "AxisX").Should().BeApproximately(16.25d, 0.0001d);
            GetDoubleProperty(focusedProbe, "AxisY").Should().BeApproximately(150d, 0.0001d);
            GetBooleanProperty(focusedProbe, "IsApproximate").Should().BeFalse();
        });
    }

    [Fact]
    public void ProbeOverlay_FlagsApproximateWhenTileDensityIsCoarse()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var coarseTile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 1024, 1024),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(-50, -40, 200, 200),
            new Size(100, 100),
            [coarseTile],
            new Point(50, 50));

        var hoveredProbe = GetHoveredProbe(state);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeTrue();
        GetDoubleProperty(hoveredProbe, "Value").Should().Be(10d);
    }

    [Fact]
    public void ProbeOverlay_MaskedViewportSample_DoesNotResolveProbe()
    {
        var metadata = new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(-1d, 1d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 5, 5),
            new SurfaceScalarField(
                width: 5,
                height: 5,
                values: Enumerable.Repeat(3f, 25).ToArray(),
                range: metadata.ValueRange),
            colorField: null,
            mask: new SurfaceMask(
                width: 5,
                height: 5,
                values: new bool[]
                {
                    true, true, true, true, true,
                    true, true, true, true, true,
                    true, true, false, true, true,
                    true, true, true, true, true,
                    true, true, true, true, true,
                }));

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(0, 0, 4, 4),
            new Size(200, 200),
            [tile],
            new Point(100, 100));

        state.ProbeResult.Should().BeNull();
        GetPropertyValue(state, "HoveredProbe").Should().BeNull();
        GetPropertyValue(state, "HoveredProbeScreenPosition").Should().BeNull();
    }

    [Fact]
    public void ProbeOverlay_CameraFramePicking_ResolvesProjectedPeakAndWorldTruth()
    {
        var metadata = new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(0d, 10d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(2, 2, 0, 0),
            width: 5,
            height: 5,
            new SurfaceTileBounds(0, 0, 5, 5),
            new float[]
            {
                0f, 1f, 2f, 1f, 0f,
                1f, 3f, 5f, 3f, 1f,
                2f, 5f, 10f, 5f, 2f,
                1f, 3f, 5f, 3f, 1f,
                0f, 1f, 2f, 1f, 0f,
            },
            metadata.ValueRange);
        var camera = new SurfaceCameraPose(
            target: new Vector3(15f, 5f, 150f),
            yawDegrees: 210d,
            pitchDegrees: 15d,
            distance: 40d,
            fieldOfViewDegrees: 45d);
        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(new SurfaceDataWindow(0d, 0d, 5d, 5d), camera),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);
        var peakWorldPosition = new Vector3(15f, 10f, 150f);
        var screenPoint = SurfaceProjectionMath.ProjectToScreen(peakWorldPosition, cameraFrame);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            cameraFrame,
            [tile],
            new Point(screenPoint.X, screenPoint.Y));

        var hoveredProbe = GetHoveredProbe(state);
        GetDoubleProperty(hoveredProbe, "SampleX").Should().BeApproximately(2d, 0.01d);
        GetDoubleProperty(hoveredProbe, "SampleY").Should().BeApproximately(2d, 0.01d);
        GetDoubleProperty(hoveredProbe, "AxisX").Should().BeApproximately(15d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisY").Should().BeApproximately(150d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "Value").Should().BeApproximately(10d, 0.01d);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeFalse();
        GetPropertyValue(hoveredProbe, "TileKey").Should().Be(tile.Key);
        GetVector3Property(hoveredProbe, "WorldPosition").Should().Be(peakWorldPosition);
    }

    [Fact]
    public void ProbeOverlay_CameraFramePicking_SkipsMaskedPeak()
    {
        var metadata = new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(0d, 10d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(2, 2, 0, 0),
            new SurfaceTileBounds(0, 0, 5, 5),
            new SurfaceScalarField(
                width: 5,
                height: 5,
                values: new float[]
                {
                    0f, 1f, 2f, 1f, 0f,
                    1f, 3f, 5f, 3f, 1f,
                    2f, 5f, 10f, 5f, 2f,
                    1f, 3f, 5f, 3f, 1f,
                    0f, 1f, 2f, 1f, 0f,
                },
                range: metadata.ValueRange),
            colorField: null,
            mask: new SurfaceMask(
                width: 5,
                height: 5,
                values: new bool[]
                {
                    true, true, true, true, true,
                    true, true, true, true, true,
                    true, true, false, true, true,
                    true, true, true, true, true,
                    true, true, true, true, true,
                }));
        var camera = new SurfaceCameraPose(
            target: new Vector3(15f, 5f, 150f),
            yawDegrees: 210d,
            pitchDegrees: 15d,
            distance: 40d,
            fieldOfViewDegrees: 45d);
        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(new SurfaceDataWindow(0d, 0d, 5d, 5d), camera),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);
        var peakWorldPosition = new Vector3(15f, 10f, 150f);
        var screenPoint = SurfaceProjectionMath.ProjectToScreen(peakWorldPosition, cameraFrame);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            cameraFrame,
            [tile],
            new Point(screenPoint.X, screenPoint.Y));

        state.ProbeResult.Should().BeNull();
        GetPropertyValue(state, "HoveredProbe").Should().BeNull();
        GetPropertyValue(state, "HoveredProbeScreenPosition").Should().BeNull();
    }

    [Fact]
    public void ProbeOverlay_CameraFramePicking_DoesNotFallBackToOverviewWhenDetailedPeakIsMasked()
    {
        var metadata = new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(0d, 10d));
        var detailTile = new SurfaceTile(
            new SurfaceTileKey(2, 2, 0, 0),
            new SurfaceTileBounds(0, 0, 5, 5),
            new SurfaceScalarField(
                width: 5,
                height: 5,
                values: new float[]
                {
                    0f, 1f, 2f, 1f, 0f,
                    1f, 3f, 5f, 3f, 1f,
                    2f, 5f, 10f, 5f, 2f,
                    1f, 3f, 5f, 3f, 1f,
                    0f, 1f, 2f, 1f, 0f,
                },
                range: metadata.ValueRange),
            colorField: null,
            mask: new SurfaceMask(
                width: 5,
                height: 5,
                values: new bool[]
                {
                    true, true, true, true, true,
                    true, true, true, true, true,
                    true, true, false, true, true,
                    true, true, true, true, true,
                    true, true, true, true, true,
                }));
        var overviewTile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 5, 5),
            new float[]
            {
                10f, 10f,
                10f, 10f,
            },
            metadata.ValueRange);
        var camera = new SurfaceCameraPose(
            target: new Vector3(15f, 5f, 150f),
            yawDegrees: 210d,
            pitchDegrees: 15d,
            distance: 40d,
            fieldOfViewDegrees: 45d);
        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(new SurfaceDataWindow(0d, 0d, 5d, 5d), camera),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);
        var peakWorldPosition = new Vector3(15f, 10f, 150f);
        var screenPoint = SurfaceProjectionMath.ProjectToScreen(peakWorldPosition, cameraFrame);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            cameraFrame,
            [overviewTile, detailTile],
            new Point(screenPoint.X, screenPoint.Y));

        state.ProbeResult.Should().BeNull();
        GetPropertyValue(state, "HoveredProbe").Should().BeNull();
        GetPropertyValue(state, "HoveredProbeScreenPosition").Should().BeNull();
    }

    private static bool UpdateProbeScreenPosition(SurfaceChartView view, Point probeScreenPosition)
    {
        var method = typeof(SurfaceChartView).GetMethod(
            "UpdateProbeScreenPosition",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        method.Should().NotBeNull("Task 9 requires a narrow control-side probe update path.");
        return (bool)method!.Invoke(view, [probeScreenPosition])!;
    }

    private static object GetOverlayState(SurfaceChartView view)
    {
        return SurfaceChartTestHelpers.GetOverlayCoordinator(view).ProbeState;
    }

    private static object GetHoveredProbe(object overlayState)
    {
        var hoveredProbe = GetPropertyValue(overlayState, "HoveredProbe");
        hoveredProbe.Should().NotBeNull("the overlay state should keep the currently hovered probe.");
        return hoveredProbe!;
    }

    private static object? GetPropertyValue(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"Expected property '{propertyName}' on {instance.GetType().FullName}.");
        return property!.GetValue(instance);
    }

    private static bool GetBooleanProperty(object instance, string propertyName)
    {
        return (bool)GetPropertyValue(instance, propertyName)!;
    }

    private static double GetDoubleProperty(object instance, string propertyName)
    {
        return (double)GetPropertyValue(instance, propertyName)!;
    }

    private static Vector3 GetVector3Property(object instance, string propertyName)
    {
        return (Vector3)GetPropertyValue(instance, propertyName)!;
    }

    private static string? GetStringProperty(object instance, string propertyName)
    {
        return (string?)GetPropertyValue(instance, propertyName);
    }

    private sealed class StaticTileSource : ISurfaceTileSource
    {
        public StaticTileSource(SurfaceMetadata metadata)
        {
            Metadata = metadata;
        }

        public SurfaceMetadata Metadata { get; }

        public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
