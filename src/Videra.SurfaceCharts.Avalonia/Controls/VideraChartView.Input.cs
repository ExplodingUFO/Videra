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

    /// <summary>
    /// Identifies the <see cref="InteractionProfile"/> property.
    /// </summary>
    public static readonly StyledProperty<SurfaceChartInteractionProfile> InteractionProfileProperty =
        AvaloniaProperty.Register<VideraChartView, SurfaceChartInteractionProfile>(
            nameof(InteractionProfile),
            defaultValue: SurfaceChartInteractionProfile.Default);

    /// <summary>
    /// Gets or sets the chart-local built-in interaction and command profile.
    /// </summary>
    public SurfaceChartInteractionProfile InteractionProfile
    {
        get => GetValue(InteractionProfileProperty) ?? SurfaceChartInteractionProfile.Default;
        set => SetValue(InteractionProfileProperty, value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnKeyDown(e);

        if (!_runtime.CanInteract || !InteractionProfile.IsKeyboardShortcutsEnabled)
        {
            return;
        }

        var handled = e.Key switch
        {
            Key.OemPlus or Key.Add => ExecuteChartCommandCore(SurfaceChartCommand.ZoomIn),
            Key.OemMinus or Key.Subtract => ExecuteChartCommandCore(SurfaceChartCommand.ZoomOut),
            Key.Left => ExecuteChartCommandCore(SurfaceChartCommand.PanLeft),
            Key.Right => ExecuteChartCommandCore(SurfaceChartCommand.PanRight),
            Key.Up => ExecuteChartCommandCore(SurfaceChartCommand.PanUp),
            Key.Down => ExecuteChartCommandCore(SurfaceChartCommand.PanDown),
            Key.Home => ExecuteChartCommandCore(SurfaceChartCommand.ResetCamera),
            Key.F => ExecuteChartCommandCore(SurfaceChartCommand.FitToData),
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

        if (InteractionProfile.FocusOnPointerPressed)
        {
            Focus();
        }

        if (TryHandleToolbarClick(pointerPosition, out var toolbarHit))
        {
            e.Handled = true;
            return;
        }

        if (toolbarHit)
        {
            e.Handled = true;
            return;
        }

        if (_interactionController.HandlePointerPressed(
                e.GetCurrentPoint(this).Properties.PointerUpdateKind,
                pointerPosition,
                e.KeyModifiers,
                InteractionProfile))
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

        if (SurfaceChartInteractionController.HandlePointerWheelChanged(
                e.Delta,
                _runtime,
                GetHoveredProbe(),
                InteractionProfile))
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

    /// <summary>
    /// Executes one built-in chart-local command if it is enabled and currently available.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <returns><c>true</c> when the command changed chart state; otherwise, <c>false</c>.</returns>
    public bool TryExecuteChartCommand(SurfaceChartCommand command)
    {
        var handled = ExecuteChartCommandCore(command);
        if (handled)
        {
            SynchronizeViewStateProperties(_runtime.ViewState);
            InvalidateOverlay();
        }

        return handled;
    }

    private bool ExecuteChartCommandCore(SurfaceChartCommand command)
    {
        return command switch
        {
            SurfaceChartCommand.ZoomIn => ApplyCommandZoom(1d),
            SurfaceChartCommand.ZoomOut => ApplyCommandZoom(-1d),
            SurfaceChartCommand.PanLeft => ApplyCommandPan(-1d, 0d),
            SurfaceChartCommand.PanRight => ApplyCommandPan(1d, 0d),
            SurfaceChartCommand.PanUp => ApplyCommandPan(0d, -1d),
            SurfaceChartCommand.PanDown => ApplyCommandPan(0d, 1d),
            SurfaceChartCommand.ResetCamera => ApplyCommandResetCamera(),
            SurfaceChartCommand.FitToData => ApplyCommandFitToData(),
            _ => false,
        };
    }

    private bool ApplyCommandZoom(double direction)
    {
        if (!InteractionProfile.IsDollyEnabled)
        {
            return false;
        }

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

    private bool ApplyCommandPan(double horizontalDirection, double verticalDirection)
    {
        if (!InteractionProfile.IsPanEnabled)
        {
            return false;
        }

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

    private bool ApplyCommandResetCamera()
    {
        if (!InteractionProfile.IsResetCameraEnabled)
        {
            return false;
        }

        _runtime.ResetCamera();
        return true;
    }

    private bool ApplyCommandFitToData()
    {
        if (!InteractionProfile.IsFitToDataEnabled)
        {
            return false;
        }

        _runtime.FitToData();
        return true;
    }

    private void UpdateCursorForGesture()
    {
        var cursor = _interactionController.ActiveGestureMode switch
        {
            SurfaceChartGestureMode.Orbit => StandardCursorType.DragCopy,
            SurfaceChartGestureMode.Pan => StandardCursorType.SizeAll,
            SurfaceChartGestureMode.FocusSelection => StandardCursorType.Cross,
            _ => (StandardCursorType?)null,
        };

        Cursor = cursor is not null
            ? new Cursor(cursor.Value)
            : Cursor.Default;
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

    private bool TryHandleToolbarClick(Point pointerPosition, out bool toolbarHit)
    {
        var action = SurfaceChartToolbarOverlayPresenter.HitTest(
            _overlayCoordinator.ToolbarState,
            pointerPosition);

        if (action is null)
        {
            toolbarHit = false;
            return false;
        }

        toolbarHit = true;
        if (!InteractionProfile.IsToolbarEnabled)
        {
            return false;
        }

        var command = action.Value switch
        {
            SurfaceChartToolbarAction.ZoomIn => SurfaceChartCommand.ZoomIn,
            SurfaceChartToolbarAction.ZoomOut => SurfaceChartCommand.ZoomOut,
            SurfaceChartToolbarAction.ResetCamera => SurfaceChartCommand.ResetCamera,
            SurfaceChartToolbarAction.FitToData => SurfaceChartCommand.FitToData,
            _ => (SurfaceChartCommand?)null,
        };

        var handled = command is SurfaceChartCommand chartCommand && ExecuteChartCommandCore(chartCommand);
        if (handled)
        {
            SynchronizeViewStateProperties(_runtime.ViewState);
            InvalidateOverlay();
        }

        return handled;
    }
}
