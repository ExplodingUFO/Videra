using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Runtime.InteropServices;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.Inspection;
using Videra.Core.Selection.Annotations;
using Videra.Core.Selection.Rendering;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime;

internal static class VideraSnapshotExportService
{
    public static Task ExportAsync(
        string path,
        uint width,
        uint height,
        VideraEngine sourceEngine,
        IReadOnlyList<Object3D> sceneObjects,
        VideraSelectionState selectionState,
        IReadOnlyList<VideraAnnotation> annotations,
        IReadOnlyList<VideraMeasurement> measurements,
        VideraViewOverlayState overlayState,
        ISoftwareBackend? preferredReadbackBackend,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(sourceEngine);
        ArgumentNullException.ThrowIfNull(sceneObjects);
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(annotations);
        ArgumentNullException.ThrowIfNull(measurements);
        ArgumentNullException.ThrowIfNull(overlayState);
        ArgumentNullException.ThrowIfNull(logger);

        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        if (TryExportFromPreferredReadback(
                path,
                width,
                height,
                overlayState,
                preferredReadbackBackend,
                logger,
                cancellationToken))
        {
            return Task.CompletedTask;
        }

        using var exportEngine = new VideraEngine();
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, (int)width, (int)height);
        exportEngine.Initialize(backend, backend);
        exportEngine.Resize(width, height);
        CopyVisualState(sourceEngine, exportEngine, backend.GetResourceFactory());

        var cloneMap = CloneScene(sceneObjects, exportEngine);
        exportEngine.SetSelectionOverlayState(CreateSelectionOverlay(selectionState, cloneMap));
        exportEngine.SetAnnotationOverlayState(CreateAnnotationOverlay(annotations, measurements));
        exportEngine.Camera.SetOrbit(
            sourceEngine.Camera.Target,
            sourceEngine.Camera.Radius,
            sourceEngine.Camera.Yaw,
            sourceEngine.Camera.Pitch);
        exportEngine.Camera.FieldOfView = sourceEngine.Camera.FieldOfView;
        exportEngine.Camera.UpdateProjection(width, height);

        cancellationToken.ThrowIfCancellationRequested();
        exportEngine.Draw();

        var pixels = CaptureFramePixels(backend, (int)width, (int)height);
        SavePixelsWithOverlay(path, pixels, (int)width, (int)height, overlayState);
        return Task.CompletedTask;
    }

    private static bool TryExportFromPreferredReadback(
        string path,
        uint width,
        uint height,
        VideraViewOverlayState overlayState,
        ISoftwareBackend? preferredReadbackBackend,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (preferredReadbackBackend is null)
        {
            return false;
        }

        if (preferredReadbackBackend.Width != width || preferredReadbackBackend.Height != height)
        {
            return false;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pixels = CaptureFramePixels(preferredReadbackBackend, (int)width, (int)height);
            SavePixelsWithOverlay(path, pixels, (int)width, (int)height, overlayState);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Snapshot live-readback fast path failed; falling back to software export.");
            return false;
        }
    }

    private static byte[] CaptureFramePixels(ISoftwareBackend backend, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(backend);

        var stride = checked(width * 4);
        var pixels = new byte[checked(stride * height)];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            backend.CopyFrameTo(handle.AddrOfPinnedObject(), stride);
            return pixels;
        }
        finally
        {
            handle.Free();
        }
    }

    private static void SavePixelsWithOverlay(
        string path,
        byte[] pixels,
        int width,
        int height,
        VideraViewOverlayState overlayState)
    {
        ArgumentNullException.ThrowIfNull(pixels);

        var stride = checked(width * 4);
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            var imageInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(imageInfo, handle.AddrOfPinnedObject(), stride);
            RenderOverlay(surface.Canvas, overlayState);
            surface.Canvas.Flush();
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var output = File.Create(path);
            data.SaveTo(output);
        }
        finally
        {
            handle.Free();
        }
    }

    private static void RenderOverlay(SKCanvas canvas, VideraViewOverlayState overlayState)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(overlayState);

        foreach (var outline in overlayState.SelectionOutlines)
        {
            using var stroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = outline.IsPrimary ? 2f : 1f,
                IsAntialias = true,
                Color = ToSkColor(outline.Color)
            };
            var bounds = outline.ScreenBounds;
            canvas.DrawRect(
                SKRect.Create((float)bounds.X, (float)bounds.Y, (float)bounds.Width, (float)bounds.Height),
                stroke);
        }

        foreach (var label in overlayState.Labels)
        {
            DrawLabel(canvas, label.Text, label.Color, label.ScreenPosition.X, label.ScreenPosition.Y);
        }

        foreach (var measurement in overlayState.Measurements)
        {
            using var stroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true,
                Color = ToSkColor(measurement.Color)
            };
            using var fill = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = ToSkColor(measurement.Color)
            };

            var start = new SKPoint(measurement.StartScreenPosition.X, measurement.StartScreenPosition.Y);
            var end = new SKPoint(measurement.EndScreenPosition.X, measurement.EndScreenPosition.Y);
            canvas.DrawLine(start, end, stroke);
            canvas.DrawCircle(start, 3f, fill);
            canvas.DrawCircle(end, 3f, fill);

            var centerX = (measurement.StartScreenPosition.X + measurement.EndScreenPosition.X) * 0.5f;
            var centerY = (measurement.StartScreenPosition.Y + measurement.EndScreenPosition.Y) * 0.5f;
            DrawLabel(canvas, measurement.Text, measurement.Color, centerX, centerY);
        }
    }

    private static void DrawLabel(SKCanvas canvas, string text, global::Avalonia.Media.Color color, float screenX, float screenY)
    {
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = ToSkColor(color),
            TextSize = 12f,
            Typeface = SKTypeface.FromFamilyName("Consolas")
        };
        using var bubbleFill = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = new SKColor(24, 24, 24, 235)
        };
        using var bubbleStroke = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            Color = SKColors.White
        };
        using var markerPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = ToSkColor(color)
        };
        using var leaderPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            Color = ToSkColor(color)
        };

        var metrics = textPaint.FontMetrics;
        var textWidth = textPaint.MeasureText(text);
        var textHeight = metrics.Descent - metrics.Ascent;
        var bubbleX = screenX + 4f;
        var bubbleY = screenY - textHeight - 12f;
        var bubbleRect = new SKRoundRect(
            new SKRect(bubbleX, bubbleY, bubbleX + textWidth + 8f, bubbleY + textHeight + 4f),
            4f,
            4f);

        canvas.DrawLine(screenX, screenY, bubbleRect.Rect.Left, bubbleRect.Rect.Bottom, leaderPaint);
        canvas.DrawCircle(screenX, screenY, 3f, markerPaint);
        canvas.DrawRoundRect(bubbleRect, bubbleFill);
        canvas.DrawRoundRect(bubbleRect, bubbleStroke);
        canvas.DrawText(text, bubbleX + 4f, bubbleY + 2f - metrics.Ascent, textPaint);
    }

    private static SKColor ToSkColor(global::Avalonia.Media.Color color)
        => new(color.R, color.G, color.B, color.A);

    private static void CopyVisualState(VideraEngine sourceEngine, VideraEngine exportEngine, Videra.Core.Graphics.Abstractions.IResourceFactory resourceFactory)
    {
        exportEngine.BackgroundColor = sourceEngine.BackgroundColor;
        exportEngine.RenderScale = sourceEngine.RenderScale;
        exportEngine.ShowAxis = sourceEngine.ShowAxis;
        exportEngine.StyleService.UpdateParameters(sourceEngine.StyleService.CurrentParameters.Clone());
        exportEngine.Grid.IsVisible = sourceEngine.Grid.IsVisible;
        exportEngine.Grid.Height = sourceEngine.Grid.Height;
        exportEngine.Grid.GridColor = sourceEngine.Grid.GridColor;
        exportEngine.Grid.Rebuild(resourceFactory);
        exportEngine.Wireframe.Mode = sourceEngine.Wireframe.Mode;
        exportEngine.Wireframe.LineColor = sourceEngine.Wireframe.LineColor;
        exportEngine.Camera.InvertX = sourceEngine.Camera.InvertX;
        exportEngine.Camera.InvertY = sourceEngine.Camera.InvertY;
    }

    private static Dictionary<Guid, Object3D> CloneScene(IReadOnlyList<Object3D> sceneObjects, VideraEngine exportEngine)
    {
        var cloneMap = new Dictionary<Guid, Object3D>();
        foreach (var source in sceneObjects)
        {
            var payload = source.MeshPayload ?? source.SourceMeshPayload;
            if (payload is null)
            {
                continue;
            }

            var clone = new Object3D
            {
                Name = source.Name,
                Position = source.Position,
                Rotation = source.Rotation,
                Scale = source.Scale
            };
            clone.PrepareDeferredMesh(payload, source.CpuMeshRetentionPolicy);
            exportEngine.AddObject(clone);
            cloneMap[source.Id] = clone;
        }

        return cloneMap;
    }

    private static SelectionOverlayRenderState CreateSelectionOverlay(
        VideraSelectionState selectionState,
        IReadOnlyDictionary<Guid, Object3D> cloneMap)
    {
        var selectedCloneIds = selectionState.ObjectIds
            .Select(originalId => cloneMap.TryGetValue(originalId, out var clone) ? clone.Id : (Guid?)null)
            .Where(static cloneId => cloneId.HasValue)
            .Select(static cloneId => cloneId!.Value)
            .ToArray();

        return new SelectionOverlayRenderState(
            selectedCloneIds,
            hoverObjectId: null,
            selectedLineColor: new RgbaFloat(0f, 0f, 0f, 1f),
            hoverLineColor: new RgbaFloat(0f, 1f, 0f, 1f));
    }

    private static AnnotationOverlayRenderState CreateAnnotationOverlay(
        IReadOnlyList<VideraAnnotation> annotations,
        IReadOnlyList<VideraMeasurement> measurements)
    {
        var anchors = annotations
            .Where(static annotation => annotation.IsVisible)
            .Select(static annotation => new AnnotationOverlayAnchor(annotation.Id, annotation.Anchor))
            .Concat(measurements
                .Where(static measurement => measurement.IsVisible)
                .SelectMany(static measurement => new[]
                {
                    new AnnotationOverlayAnchor(CreateMeasurementAnchorId(measurement.Id, isStart: true), AnnotationAnchorDescriptor.ForWorldPoint(measurement.Start.WorldPoint)),
                    new AnnotationOverlayAnchor(CreateMeasurementAnchorId(measurement.Id, isStart: false), AnnotationAnchorDescriptor.ForWorldPoint(measurement.End.WorldPoint))
                }))
            .ToArray();

        return new AnnotationOverlayRenderState(
            anchors,
            markerColor: new RgbaFloat(1f, 0f, 0f, 1f),
            markerWorldSize: 0.08f);
    }

    private static Guid CreateMeasurementAnchorId(Guid measurementId, bool isStart)
    {
        var bytes = measurementId.ToByteArray();
        bytes[15] ^= isStart ? (byte)0x41 : (byte)0x82;
        return new Guid(bytes);
    }
}
