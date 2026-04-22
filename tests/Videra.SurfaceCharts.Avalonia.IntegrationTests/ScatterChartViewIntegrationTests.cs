using System.Reflection;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;
using Xunit;
using Pointer = Avalonia.Input.Pointer;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class ScatterChartViewIntegrationTests
{
    [Fact]
    public void ScatterChartView_PublishesReadySoftwareScene_AndPreservesScatterRenderTruth()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata();
            var source = new ScatterChartData(
                metadata,
                [
                    new ScatterSeries(
                        [
                            new ScatterPoint(1d, 2d, 3d, 0xFFAA5500),
                            new ScatterPoint(4d, 5d, 6d),
                        ],
                        0xFF336699,
                        "Alpha"),
                ]);

            var view = new ScatterChartView();
            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            view.Source.Should().BeSameAs(source);
            view.RenderingStatus.BackendKind.Should().Be(SurfaceChartRenderBackendKind.Software);
            view.RenderingStatus.IsReady.Should().BeTrue();

            var scene = GetRenderScene(view);
            scene.Should().NotBeNull();
            scene!.Metadata.Should().BeSameAs(metadata);
            scene.Series.Should().ContainSingle();

            var series = scene.Series[0];
            series.Label.Should().Be("Alpha");
            series.Points.Should().HaveCount(2);
            series.Points[0].Should().Be(new ScatterRenderPoint(new System.Numerics.Vector3(1f, 2f, 3f), 0xFFAA5500u));
            series.Points[1].Should().Be(new ScatterRenderPoint(new System.Numerics.Vector3(4f, 5f, 6f), 0xFF336699u));
        });
    }

    [Fact]
    public void ScatterChartView_Render_ConnectsEnabledSeries_AndStillDrawsPoints()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata();
            var source = new ScatterChartData(
                metadata,
                [
                    new ScatterSeries(
                        [
                            new ScatterPoint(1d, 2d, 3d, 0xFFAA5500),
                            new ScatterPoint(4d, 5d, 6d, 0xFF336699),
                            new ScatterPoint(7d, 6d, 5d, 0xFF114488),
                        ],
                        0xFF336699,
                        "Alpha",
                        connectPoints: true),
                ]);

            var view = new ScatterChartView();
            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            using var drawingContext = new RecordingDrawingContext();

            view.Render(drawingContext.Context);

            drawingContext.GeometryDrawCallCount.Should().Be(5);
            drawingContext.FilledGeometryDrawCallCount.Should().Be(3);
            drawingContext.StrokeOnlyGeometryDrawCallCount.Should().Be(2);
        });
    }

    [Fact]
    public void ScatterChartView_FitToData_AndResetCamera_StayBoundToCurrentData()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata();
            var source = new ScatterChartData(
                metadata,
                [
                    new ScatterSeries(
                        [
                            new ScatterPoint(2d, 2d, 3d, 0xFFAA5500),
                            new ScatterPoint(8d, 7d, 9d),
                        ],
                        0xFF336699,
                        "Alpha"),
                ]);

            var view = new ScatterChartView();
            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            var scene = GetRenderScene(view);
            scene.Should().NotBeNull();

            var expectedBounds = new SurfacePlotBounds(
                new System.Numerics.Vector3(2f, 2f, 3f),
                new System.Numerics.Vector3(8f, 7f, 9f));
            var expectedCenter = expectedBounds.Center;
            var expectedFitDistance = GetFitDistance(expectedBounds.Size, 60d);
            var expectedResetDistance = GetFitDistance(expectedBounds.Size, SurfaceCameraPose.DefaultFieldOfViewDegrees);

            SetCamera(view, new SurfaceCameraPose(
                new System.Numerics.Vector3(99f, 88f, 77f),
                73d,
                -17d,
                42d,
                60d));

            view.FitToData();

            var fitCamera = GetCamera(view);
            fitCamera.Target.Should().Be(expectedCenter);
            fitCamera.YawDegrees.Should().Be(73d);
            fitCamera.PitchDegrees.Should().Be(-17d);
            fitCamera.Distance.Should().BeApproximately(expectedFitDistance, 0.0001d);
            fitCamera.FieldOfViewDegrees.Should().Be(60d);
            view.RenderingStatus.CameraTarget.Should().Be(expectedCenter);
            view.RenderingStatus.CameraDistance.Should().BeApproximately(expectedFitDistance, 0.0001d);
            view.Source.Should().BeSameAs(source);
            GetRenderScene(view).Should().BeSameAs(scene);

            view.ResetCamera();

            var resetCamera = GetCamera(view);
            resetCamera.Target.Should().Be(expectedCenter);
            resetCamera.YawDegrees.Should().Be(SurfaceCameraPose.DefaultYawDegrees);
            resetCamera.PitchDegrees.Should().Be(SurfaceCameraPose.DefaultPitchDegrees);
            resetCamera.Distance.Should().BeApproximately(expectedResetDistance, 0.0001d);
            resetCamera.FieldOfViewDegrees.Should().Be(SurfaceCameraPose.DefaultFieldOfViewDegrees);
            view.RenderingStatus.CameraTarget.Should().Be(expectedCenter);
            view.RenderingStatus.CameraDistance.Should().BeApproximately(expectedResetDistance, 0.0001d);
            view.Source.Should().BeSameAs(source);
            GetRenderScene(view).Should().BeSameAs(scene);
        });
    }

    [Fact]
    public void ScatterChartView_LeftDrag_Orbits_AndTracksInteractionState()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata();
            var source = new ScatterChartData(
                metadata,
                [
                    new ScatterSeries(
                        [
                            new ScatterPoint(1d, 2d, 3d, 0xFFAA5500),
                            new ScatterPoint(4d, 5d, 6d),
                        ],
                        0xFF336699,
                        "Alpha"),
                ]);
            var view = new ScatterChartView();
            var routedInput = new RoutedInteractionTestScatterChartView(view);
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            var initialCamera = GetCamera(view);
            var start = new Point(120d, 80d);
            var end = new Point(160d, 112d);

            routedInput.RoutePointerPressed(pointer, start, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RenderingStatus.IsInteracting.Should().BeTrue();

            routedInput.RoutePointerMoved(pointer, end, RawInputModifiers.LeftMouseButton);
            view.RenderingStatus.IsInteracting.Should().BeTrue();

            routedInput.RoutePointerReleased(pointer, end, RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            var nextCamera = GetCamera(view);
            view.RenderingStatus.IsInteracting.Should().BeFalse();
            view.RenderingStatus.CameraDistance.Should().BeApproximately(initialCamera.Distance, 0.0001d);
            view.RenderingStatus.CameraTarget.Should().Be(initialCamera.Target);
            nextCamera.YawDegrees.Should().NotBe(initialCamera.YawDegrees);
            nextCamera.PitchDegrees.Should().NotBe(initialCamera.PitchDegrees);
        });
    }

    [Fact]
    public void ScatterChartView_MouseWheel_Dollies_Inward_By_One_Notch()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata();
            var source = new ScatterChartData(
                metadata,
                [
                    new ScatterSeries(
                        [
                            new ScatterPoint(1d, 2d, 3d, 0xFFAA5500),
                            new ScatterPoint(4d, 5d, 6d),
                        ],
                        0xFF336699,
                        "Alpha"),
                ]);
            var view = new ScatterChartView();
            var routedInput = new RoutedInteractionTestScatterChartView(view);
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            var initialDistance = view.RenderingStatus.CameraDistance;

            routedInput.RoutePointerWheel(pointer, new Point(120d, 80d), RawInputModifiers.None, new Vector(0d, 1d));

            view.RenderingStatus.CameraDistance.Should().BeLessThan(initialDistance);
        });
    }

    private static ScatterChartMetadata CreateMetadata()
    {
        return new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Horizontal", null, 0d, 10d),
            new SurfaceAxisDescriptor("Depth", null, 0d, 10d),
            new SurfaceValueRange(0d, 10d));
    }

    private static ScatterRenderScene? GetRenderScene(ScatterChartView view)
    {
        var property = typeof(ScatterChartView).GetProperty(
            "RenderScene",
            BindingFlags.Instance | BindingFlags.NonPublic);
        property.Should().NotBeNull();

        return (ScatterRenderScene?)property!.GetValue(view);
    }

    private static SurfaceCameraPose GetCamera(ScatterChartView view)
    {
        var field = typeof(ScatterChartView).GetField(
            "_camera",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();

        return (SurfaceCameraPose)field!.GetValue(view)!;
    }

    private static void SetCamera(ScatterChartView view, SurfaceCameraPose camera)
    {
        var field = typeof(ScatterChartView).GetField(
            "_camera",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();

        field!.SetValue(view, camera);
    }

    private static double GetFitDistance(System.Numerics.Vector3 size, double fieldOfViewDegrees)
    {
        var diagonal = Math.Sqrt((size.X * size.X) + (size.Y * size.Y) + (size.Z * size.Z));
        var halfFieldOfViewRadians = (fieldOfViewDegrees * (Math.PI / 180d)) * 0.5d;
        return Math.Max((Math.Max(diagonal, 1d) * 0.5d) / Math.Tan(halfFieldOfViewRadians), 1d);
    }

    private sealed class RoutedInteractionTestScatterChartView
    {
        private static readonly MethodInfo OnPointerPressedMethod = typeof(ScatterChartView).GetMethod(
            "OnPointerPressed",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        private static readonly MethodInfo OnPointerMovedMethod = typeof(ScatterChartView).GetMethod(
            "OnPointerMoved",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        private static readonly MethodInfo OnPointerReleasedMethod = typeof(ScatterChartView).GetMethod(
            "OnPointerReleased",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        private static readonly MethodInfo OnPointerWheelChangedMethod = typeof(ScatterChartView).GetMethod(
            "OnPointerWheelChanged",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        private readonly ScatterChartView _view;

        public RoutedInteractionTestScatterChartView(ScatterChartView view)
        {
            _view = view;
        }

        public void RoutePointerPressed(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            PointerUpdateKind updateKind,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerPressedEventArgs(_view, pointer, _view, position, timestamp: 0, properties, keyModifiers, clickCount: 1);
            OnPointerPressedMethod.Invoke(_view, [args]);
        }

        public void RoutePointerMoved(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerEventArgs(InputElement.PointerMovedEvent, _view, pointer, _view, position, timestamp: 0, properties, keyModifiers);
            OnPointerMovedMethod.Invoke(_view, [args]);
        }

        public void RoutePointerReleased(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            PointerUpdateKind updateKind,
            MouseButton mouseButton,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerReleasedEventArgs(_view, pointer, _view, position, timestamp: 0, properties, keyModifiers, mouseButton);
            OnPointerReleasedMethod.Invoke(_view, [args]);
        }

        public void RoutePointerWheel(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            Vector delta,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerWheelEventArgs(_view, pointer, _view, position, timestamp: 0, properties, keyModifiers, delta);
            OnPointerWheelChangedMethod.Invoke(_view, [args]);
        }
    }

    private sealed class RecordingDrawingContext : IDisposable
    {
        private readonly DrawingGroup _drawingGroup = new();
        private readonly DrawingContext _drawingContext;
        private bool _completed;
        private int? _geometryDrawCallCount;
        private int? _filledGeometryDrawCallCount;
        private int? _strokeOnlyGeometryDrawCallCount;

        public RecordingDrawingContext()
        {
            _drawingContext = _drawingGroup.Open();
        }

        public DrawingContext Context => _drawingContext;

        public int GeometryDrawCallCount
        {
            get
            {
                CompleteRecording();
                return _geometryDrawCallCount!.Value;
            }
        }

        public int FilledGeometryDrawCallCount
        {
            get
            {
                CompleteRecording();
                return _filledGeometryDrawCallCount!.Value;
            }
        }

        public int StrokeOnlyGeometryDrawCallCount
        {
            get
            {
                CompleteRecording();
                return _strokeOnlyGeometryDrawCallCount!.Value;
            }
        }

        public void Dispose()
        {
            CompleteRecording();
        }

        private void CompleteRecording()
        {
            if (_completed)
            {
                return;
            }

            _drawingContext.Dispose();
            _geometryDrawCallCount = CountGeometryDrawings(_drawingGroup);
            _filledGeometryDrawCallCount = CountGeometryDrawings(_drawingGroup, static drawing => drawing.Brush is not null);
            _strokeOnlyGeometryDrawCallCount = CountGeometryDrawings(_drawingGroup, static drawing => drawing.Brush is null);
            _completed = true;
        }

        private static int CountGeometryDrawings(Drawing drawing)
        {
            return drawing switch
            {
                GeometryDrawing => 1,
                DrawingGroup group => group.Children.Sum(CountGeometryDrawings),
                _ => 0,
            };
        }

        private static int CountGeometryDrawings(Drawing drawing, Func<GeometryDrawing, bool> predicate)
        {
            return drawing switch
            {
                GeometryDrawing geometryDrawing when predicate(geometryDrawing) => 1,
                GeometryDrawing => 0,
                DrawingGroup group => group.Children.Sum(child => CountGeometryDrawings(child, predicate)),
                _ => 0,
            };
        }
    }
}
