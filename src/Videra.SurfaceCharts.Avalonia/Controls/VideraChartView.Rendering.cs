using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class VideraChartView
{
    private SurfaceRenderScene? _renderScene;

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        base.Render(context);
        var projection = _chartProjection ?? CreateChartProjection();
        if (RenderSnapshot.ActiveBackend == SurfaceChartRenderBackendKind.Software)
        {
            SurfaceScenePainter.DrawScene(context, _renderHost.SoftwareScene, projection);

            // Draw contour lines on top of surface
            var activeContourData = Plot.GetActiveContourData();
            foreach (var contourData in activeContourData)
            {
                var contourScene = CreateContourScene(contourData, activeContourData.Count);
                SurfaceScenePainter.DrawContourLines(context, contourScene, projection);
            }
        }
    }

    private void NotifyTilesChanged()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            InvalidateRenderScene();
            return;
        }

        Dispatcher.UIThread.Post(InvalidateRenderScene);
    }

    private void InvalidateRenderScene()
    {
        SyncRenderHost();
        InvalidateVisual();
        InvalidateOverlay();
    }

    private void SyncRenderHost()
    {
        var source = _runtime.Source;
        var tiles = _runtime.GetLoadedTiles();
        var renderSize = _overlayViewSize.Width > 0d && _overlayViewSize.Height > 0d
            ? _overlayViewSize
            : Bounds.Size;
        var colorMap = source is null ? null : Plot.ColorMap ?? CreateFallbackColorMap(source.Metadata.ValueRange);
        if (ShouldAttemptNativeHost(renderSize))
        {
            EnsureNativeHost();
        }

        var nativeHandle = _nativeHost?.CurrentHandle ?? IntPtr.Zero;
        var handleBound = nativeHandle != IntPtr.Zero;
        var renderScale = (float)(VisualRoot?.RenderScaling ?? 1.0);
        var cameraFrame = _runtime.CreateCameraFrame(renderSize, renderScale);

        _renderHost.UpdateInputs(
            new SurfaceChartRenderInputs
            {
                Metadata = source?.Metadata,
                LoadedTiles = tiles,
                ColorMap = colorMap,
                ViewState = _runtime.ViewState,
                CameraFrame = cameraFrame,
                ViewWidth = renderSize.Width,
                ViewHeight = renderSize.Height,
                NativeHandle = nativeHandle,
                HandleBound = handleBound,
                RenderScale = renderScale,
            });

        _renderScene = _renderHost.SoftwareScene;
        UpdateRenderingStatus(_renderHost.RenderingStatus);

        if (!RenderingStatus.UsesNativeSurface && _nativeHost is not null)
        {
            ReleaseNativeHost();
        }
    }

    /// <summary>
    /// Renders the chart view offscreen to a <see cref="RenderTargetBitmap"/> at the specified dimensions and scale.
    /// </summary>
    /// <param name="width">The target bitmap width in pixels.</param>
    /// <param name="height">The target bitmap height in pixels.</param>
    /// <param name="scale">The DPI scale factor.</param>
    /// <returns>A rendered bitmap containing the chart visualization.</returns>
    internal async Task<RenderTargetBitmap> RenderOffscreenAsync(int width, int height, double scale)
    {
        var pixelSize = new PixelSize(width, height);
        var dpi = new Vector(96 * scale, 96 * scale);
        var bitmap = new RenderTargetBitmap(pixelSize, dpi);

        if (Dispatcher.UIThread.CheckAccess())
        {
            RenderToBitmap(bitmap);
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() => RenderToBitmap(bitmap));
        }

        return bitmap;
    }

    private void RenderToBitmap(RenderTargetBitmap bitmap)
    {
        SyncRenderHost();
        bitmap.Render(this);
    }

    private static SurfaceColorMap CreateFallbackColorMap(SurfaceValueRange range)
    {
        return new SurfaceColorMap(range, SurfaceColorMapPresets.CreateDefault());
    }

    private SurfaceChartProjection? CreateChartProjection()
    {
        var source = _runtime.Source;
        if (source is null || _overlayViewSize.Width <= 0d || _overlayViewSize.Height <= 0d)
        {
            return null;
        }

        var loadedTiles = _runtime.GetLoadedTiles();
        if (loadedTiles.Count == 0)
        {
            return null;
        }

        var cameraFrame = _runtime.CreateCameraFrame(_overlayViewSize, 1f);
        if (cameraFrame is null)
        {
            return null;
        }

        var projection = SurfaceChartProjection.Create(
            _renderHost.SoftwareScene,
            cameraFrame.Value,
            SurfaceChartProjection.CreateChartBoundsPoints(source.Metadata, source.Metadata.ValueRange));
        _chartProjection = projection;
        return projection;
    }

    private bool ShouldAttemptNativeHost(Size renderSize)
    {
        return _renderHost.HasGpuBackend
            && _runtime.Source is not null
            && renderSize.Width > 0d
            && renderSize.Height > 0d
            && !RenderingStatus.IsFallback;
    }

    private void EnsureNativeHost()
    {
        if (_nativeHost is not null)
        {
            return;
        }

        var host = _nativeHostFactory.CreateHost();
        if (host is null)
        {
            return;
        }

        if (host is not Control nativeControl)
        {
            throw new InvalidOperationException("Surface-chart native host factory must return an Avalonia control.");
        }

        host.HandleCreated += OnNativeHandleCreated;
        host.HandleDestroyed += OnNativeHandleDestroyed;
        _nativeHost = host;
        _hostContainer.Children.Insert(0, nativeControl);

        if (host.CurrentHandle != IntPtr.Zero)
        {
            OnNativeHandleCreated(host.CurrentHandle);
        }
    }

    private void ReleaseNativeHost()
    {
        if (_nativeHost is null)
        {
            return;
        }

        _nativeHost.HandleCreated -= OnNativeHandleCreated;
        _nativeHost.HandleDestroyed -= OnNativeHandleDestroyed;

        if (_nativeHost is Control nativeControl)
        {
            _hostContainer.Children.Remove(nativeControl);
        }

        _nativeHost = null;
    }

    private void OnNativeHandleCreated(IntPtr handle)
    {
        _ = handle;
        SyncRenderHost();
        InvalidateVisual();
        _overlayLayer.InvalidateVisual();
    }

    private void OnNativeHandleDestroyed()
    {
        SyncRenderHost();
        InvalidateVisual();
        _overlayLayer.InvalidateVisual();
    }

    private void UpdateRenderingStatus(SurfaceChartRenderingStatus nextStatus)
    {
        ArgumentNullException.ThrowIfNull(nextStatus);

        if (RenderingStatus == nextStatus)
        {
            return;
        }

        RenderingStatus = nextStatus;
        RenderStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class SurfaceChartOverlayLayer : Control
    {
        private readonly VideraChartView _owner;

        public SurfaceChartOverlayLayer(VideraChartView owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
            _owner.RenderOverlay(context);
        }
    }
}
