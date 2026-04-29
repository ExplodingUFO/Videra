using System.Numerics;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class VideraChartView
{
    private readonly SurfaceChartInteractionController _interactionController = new();
    private static readonly TimeSpan KeyboardCursorResetDelay = TimeSpan.FromMilliseconds(300);
    private CancellationTokenSource? _keyboardCursorCts;

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnKeyDown(e);

        if (!_runtime.CanInteract)
        {
            return;
        }

        var handled = e.Key switch
        {
            Key.OemPlus or Key.Add => ApplyKeyboardZoom(1d),
            Key.OemMinus or Key.Subtract => ApplyKeyboardZoom(-1d),
            Key.Left => ApplyKeyboardPan(-1d, 0d),
            Key.Right => ApplyKeyboardPan(1d, 0d),
            Key.Up => ApplyKeyboardPan(0d, -1d),
            Key.Down => ApplyKeyboardPan(0d, 1d),
            Key.Home => ApplyKeyboardResetCamera(),
            Key.F => ApplyKeyboardFitToData(),
            _ => false,
        };

        if (handled)
        {
            e.Handled = true;
            SynchronizeViewStateProperties(_runtime.ViewState);
            InvalidateOverlay();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerEntered(e);
        UpdateCursorForGesture();
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerExited(e);
        Cursor = Cursor.Default;
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerPressed(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);
        _overlayCoordinator.UpdatePointerPosition(pointerPosition);

        // Check toolbar button hit first
        if (HandleToolbarClick(pointerPosition))
        {
            e.Handled = true;
            return;
        }

        if (_interactionController.HandlePointerPressed(
                e.GetCurrentPoint(this).Properties.PointerUpdateKind,
                pointerPosition,
                e.KeyModifiers))
        {
            e.Pointer.Capture(this);
            UpdateCursorForGesture();
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerMoved(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);
        _overlayCoordinator.UpdatePointerPosition(pointerPosition);

        if (_interactionController.HandlePointerMoved(pointerPosition, _runtime))
        {
            e.Handled = true;
        }

        // Update overlay for toolbar hover effect
        _overlayLayer.InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerReleased(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);

        var releaseResult = _interactionController.HandlePointerReleased(
            e.InitialPressMouseButton,
            pointerPosition,
            e.KeyModifiers,
            _runtime);

        if (releaseResult.TogglePinnedProbe &&
            GetHoveredProbe() is SurfaceProbeInfo hoveredProbe)
        {
            TogglePinnedProbe(hoveredProbe);
        }

        if (releaseResult.Handled)
        {
            InvalidateOverlay();

            if (!_interactionController.HasActiveGesture &&
                ReferenceEquals(e.Pointer.Captured, this))
            {
                e.Pointer.Capture(null);
            }

            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerWheelChanged(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);

        if (SurfaceChartInteractionController.HandlePointerWheelChanged(e.Delta, _runtime, GetHoveredProbe()))
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        _interactionController.Reset();
        UpdateCursorForGesture();
        InvalidateOverlay();
        base.OnPointerCaptureLost(e);
    }

    private bool ApplyKeyboardZoom(double direction)
    {
        if (_runtime.Metadata is not SurfaceMetadata metadata ||
            _runtime.ViewSize.Width <= 0d ||
            _runtime.ViewSize.Height <= 0d)
        {
            return false;
        }

        const double wheelZoomStepFactor = 0.85d;
        const double minimumWindowSpan = 1d;

        _runtime.BeginInteraction();
        var currentViewState = _runtime.ViewState;
        var currentWindow = currentViewState.DataWindow;
        var zoomFactor = Math.Pow(wheelZoomStepFactor, direction);
        var nextWidth = Math.Clamp(currentWindow.Width * zoomFactor, minimumWindowSpan, metadata.Width);
        var nextHeight = Math.Clamp(currentWindow.Height * zoomFactor, minimumWindowSpan, metadata.Height);
        var anchorSampleX = currentWindow.StartX + (currentWindow.Width * 0.5d);
        var anchorSampleY = currentWindow.StartY + (currentWindow.Height * 0.5d);
        var anchorRatioX = 0.5d;
        var anchorRatioY = 0.5d;
        var nextWindow = new SurfaceDataWindow(
                anchorSampleX - (anchorRatioX * nextWidth),
                anchorSampleY - (anchorRatioY * nextHeight),
                nextWidth,
                nextHeight)
            .ClampTo(metadata);

        var widthScale = nextWindow.Width / currentWindow.Width;
        var heightScale = nextWindow.Height / currentWindow.Height;
        var nextCamera = RecenterCameraForKeyboard(metadata, nextWindow, currentViewState.Camera, (widthScale + heightScale) * 0.5d);

        _runtime.UpdateViewState(
            new SurfaceViewState(
                nextWindow,
                nextCamera,
                currentViewState.DisplaySpace));

        FlashCursor(StandardCursorType.Cross);
        return true;
    }

    private bool ApplyKeyboardPan(double horizontalDirection, double verticalDirection)
    {
        if (_runtime.Metadata is not SurfaceMetadata metadata ||
            _runtime.ViewSize.Width <= 0d ||
            _runtime.ViewSize.Height <= 0d)
        {
            return false;
        }

        const double panStepRatio = 0.05d;

        _runtime.BeginInteraction();
        var currentViewState = _runtime.ViewState;
        var currentWindow = currentViewState.DataWindow;
        var sampleDeltaX = horizontalDirection * currentWindow.Width * panStepRatio;
        var sampleDeltaY = verticalDirection * currentWindow.Height * panStepRatio;
        var nextWindow = new SurfaceDataWindow(
                currentWindow.StartX + sampleDeltaX,
                currentWindow.StartY + sampleDeltaY,
                currentWindow.Width,
                currentWindow.Height)
            .ClampTo(metadata);
        var nextCamera = RecenterCameraForKeyboard(metadata, nextWindow, currentViewState.Camera, distanceScale: 1d);

        _runtime.UpdateViewState(
            new SurfaceViewState(
                nextWindow,
                nextCamera,
                currentViewState.DisplaySpace));

        FlashCursor(StandardCursorType.SizeAll);
        return true;
    }

    private bool ApplyKeyboardResetCamera()
    {
        _runtime.ResetCamera();
        return true;
    }

    private bool ApplyKeyboardFitToData()
    {
        _runtime.FitToData();
        return true;
    }

    private void UpdateCursorForGesture()
    {
        if (_interactionController.HasActiveGesture)
        {
            Cursor = new Cursor(StandardCursorType.SizeAll);
            return;
        }

        Cursor = Cursor.Default;
    }

    private void FlashCursor(StandardCursorType cursorType)
    {
        _keyboardCursorCts?.Cancel();
        _keyboardCursorCts?.Dispose();

        var cts = new CancellationTokenSource();
        _keyboardCursorCts = cts;

        Cursor = new Cursor(cursorType);

        _ = ResetCursorAfterDelayAsync(cts);
    }

    private async Task ResetCursorAfterDelayAsync(CancellationTokenSource cts)
    {
        try
        {
            await Task.Delay(KeyboardCursorResetDelay, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!cts.IsCancellationRequested)
            {
                Cursor = Cursor.Default;
            }
        });
    }

    private static SurfaceCameraPose RecenterCameraForKeyboard(
        SurfaceMetadata metadata,
        SurfaceDataWindow dataWindow,
        SurfaceCameraPose currentCamera,
        double distanceScale)
    {
        var centeredCamera = SurfaceCameraPose.CreateDefault(metadata, dataWindow);
        return new SurfaceCameraPose(
            centeredCamera.Target,
            currentCamera.YawDegrees,
            currentCamera.PitchDegrees,
            Math.Max(currentCamera.Distance * distanceScale, 1d),
            currentCamera.FieldOfViewDegrees);
    }

    private bool HandleToolbarClick(Point pointerPosition)
    {
        var action = SurfaceChartToolbarOverlayPresenter.HitTest(
            _overlayCoordinator.ToolbarState,
            pointerPosition);

        if (action is null)
        {
            return false;
        }

        var handled = action.Value switch
        {
            SurfaceChartToolbarAction.ZoomIn => ApplyKeyboardZoom(1d),
            SurfaceChartToolbarAction.ZoomOut => ApplyKeyboardZoom(-1d),
            SurfaceChartToolbarAction.ResetCamera => ApplyKeyboardResetCamera(),
            SurfaceChartToolbarAction.FitToData => ApplyKeyboardFitToData(),
            _ => false,
        };

        if (handled)
        {
            SynchronizeViewStateProperties(_runtime.ViewState);
            InvalidateOverlay();
        }

        return handled;
    }
}
