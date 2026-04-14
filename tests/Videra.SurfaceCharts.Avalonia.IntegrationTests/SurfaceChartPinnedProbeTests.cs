using System.Collections;
using Avalonia;
using Avalonia.Input;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;
using Pointer = Avalonia.Input.Pointer;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartPinnedProbeTests
{
    [Fact]
    public Task PinnedProbe_ShiftClick_TogglesProbeBubble()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedProbeTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);
            var point = new Point(64, 32);

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));
            view.Viewport = new SurfaceViewport(128, 64, 256, 128);
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.RoutePointerMoved(pointer, point, RawInputModifiers.None);

            var initialState = GetOverlayState(view);
            GetPropertyValue(initialState, "HoveredProbe").Should().NotBeNull();
            GetPinnedProbes(initialState).Should().BeEmpty();

            view.RoutePointerPressed(
                pointer,
                point,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Shift);
            view.RoutePointerReleased(
                pointer,
                point,
                RawInputModifiers.Shift,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Shift);

            var pinnedState = GetOverlayState(view);
            GetPinnedProbes(pinnedState).Should().HaveCount(1);

            view.RoutePointerMoved(pointer, point, RawInputModifiers.None);
            view.RoutePointerPressed(
                pointer,
                point,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Shift);
            view.RoutePointerReleased(
                pointer,
                point,
                RawInputModifiers.Shift,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Shift);

            var unpinnedState = GetOverlayState(view);
            GetPinnedProbes(unpinnedState).Should().BeEmpty();
        });
    }

    private static object GetOverlayState(SurfaceChartView view)
    {
        var field = typeof(SurfaceChartView).GetField(
            "_overlayState",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        field.Should().NotBeNull("Task 2 should keep probe overlay state on the control.");
        return field!.GetValue(view)!;
    }

    private static object? GetPropertyValue(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"Expected property '{propertyName}' on {instance.GetType().FullName}.");
        return property!.GetValue(instance);
    }

    private static object[] GetPinnedProbes(object overlayState)
    {
        var pinned = GetPropertyValue(overlayState, "PinnedProbes");
        pinned.Should().BeAssignableTo<IEnumerable>();
        return ((IEnumerable)pinned!).Cast<object>().ToArray();
    }

    private sealed class RoutedProbeTestView : SurfaceChartView
    {
        public void RoutePointerPressed(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            PointerUpdateKind updateKind,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerPressedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, clickCount: 1);
            base.OnPointerPressed(args);
        }

        public void RoutePointerMoved(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerEventArgs(InputElement.PointerMovedEvent, this, pointer, this, position, timestamp: 0, properties, keyModifiers);
            base.OnPointerMoved(args);
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
            var args = new PointerReleasedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, mouseButton);
            base.OnPointerReleased(args);
        }
    }
}
