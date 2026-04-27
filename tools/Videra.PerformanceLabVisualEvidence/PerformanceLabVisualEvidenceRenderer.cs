using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Videra.Demo.Services;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Demo.Services;

namespace Videra.PerformanceLabVisualEvidence;

internal static class PerformanceLabVisualEvidenceRenderer
{
    public static Image<Rgba32> RenderViewerScenario(PerformanceLabViewerScenario scenario, int width, int height)
    {
        var image = CreateCanvas(width, height, new Rgba32(248, 250, 252));
        DrawHeader(image, scenario.Id, scenario.DisplayName, new Rgba32(15, 23, 42));

        var colors = PerformanceLabViewerScenarios.CreateColors(scenario);
        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(scenario.ObjectCount)));
        var previewCount = Math.Min(scenario.ObjectCount, 900);
        var plotX = 48;
        var plotY = 120;
        var plotW = width - 96;
        var plotH = height - 190;
        DrawRect(image, plotX, plotY, plotW, plotH, new Rgba32(226, 232, 240));

        for (var i = 0; i < previewCount; i++)
        {
            var row = i / columns;
            var col = i % columns;
            var px = plotX + 10 + (col * Math.Max(2, (plotW - 20) / columns));
            var py = plotY + 10 + (row * Math.Max(2, (plotH - 20) / Math.Max(1, previewCount / columns + 1)));
            var color = ToPixel(colors[i]);
            DrawRect(image, px, py, 4, 4, color);
        }

        DrawFooter(image, $"objects={scenario.ObjectCount} pickable={scenario.Pickable} evidence-only=true");
        return image;
    }

    public static Image<Rgba32> RenderScatterScenario(
        ScatterStreamingScenario scenario,
        ScatterColumnarSeries series,
        int width,
        int height)
    {
        var image = CreateCanvas(width, height, new Rgba32(247, 250, 252));
        DrawHeader(image, scenario.Id, scenario.DisplayName, new Rgba32(20, 83, 45));

        var plotX = 48;
        var plotY = 120;
        var plotW = width - 96;
        var plotH = height - 190;
        DrawRect(image, plotX, plotY, plotW, plotH, new Rgba32(220, 252, 231));

        var data = ScatterStreamingScenarios.CreateData(0, Math.Min(series.Count, 2000));
        for (var i = 0; i < data.X.Length; i++)
        {
            var x = plotX + 8 + (int)((i / (float)Math.Max(1, data.X.Length - 1)) * (plotW - 16));
            var yValue = Math.Clamp((data.Y.Span[i] + 22f) / 44f, 0f, 1f);
            var zValue = Math.Clamp((data.Z.Span[i] + 14f) / 28f, 0f, 1f);
            var y = plotY + plotH - 8 - (int)(yValue * (plotH - 16));
            var color = new Rgba32((byte)(40 + zValue * 120), (byte)(120 + yValue * 100), 170, 255);
            DrawRect(image, x, y, 2, 2, color);
        }

        DrawFooter(image, $"points={series.Count} update={scenario.UpdateMode} dropped={series.TotalDroppedPointCount} evidence-only=true");
        return image;
    }

    private static Image<Rgba32> CreateCanvas(int width, int height, Rgba32 background)
    {
        var image = new Image<Rgba32>(width, height);
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                accessor.GetRowSpan(y).Fill(background);
            }
        });
        return image;
    }

    private static void DrawHeader(Image<Rgba32> image, string id, string displayName, Rgba32 color)
    {
        DrawRect(image, 0, 0, image.Width, 84, color);
        DrawTokenBars(image, 48, 28, displayName.Length, new Rgba32(255, 255, 255, 255));
        DrawTokenBars(image, 48, 58, id.Length, new Rgba32(203, 213, 225, 255));
    }

    private static void DrawFooter(Image<Rgba32> image, string text)
    {
        DrawRect(image, 0, image.Height - 52, image.Width, 52, new Rgba32(241, 245, 249, 255));
        DrawTokenBars(image, 48, image.Height - 32, text.Length, new Rgba32(71, 85, 105, 255));
    }

    private static void DrawTokenBars(Image<Rgba32> image, int x, int y, int seed, Rgba32 color)
    {
        var remaining = Math.Clamp(seed, 8, 80);
        var currentX = x;
        var band = 0;
        while (remaining > 0 && currentX < image.Width - 48)
        {
            var length = Math.Min(remaining, 8 + ((seed + band) % 16));
            DrawRect(image, currentX, y, length * 5, 6, color);
            currentX += (length * 5) + 10;
            remaining -= length;
            band++;
        }
    }

    private static void DrawRect(Image<Rgba32> image, int x, int y, int width, int height, Rgba32 color)
    {
        var startX = Math.Clamp(x, 0, image.Width);
        var startY = Math.Clamp(y, 0, image.Height);
        var endX = Math.Clamp(x + width, 0, image.Width);
        var endY = Math.Clamp(y + height, 0, image.Height);
        image.ProcessPixelRows(accessor =>
        {
            for (var row = startY; row < endY; row++)
            {
                accessor.GetRowSpan(row)[startX..endX].Fill(color);
            }
        });
    }

    private static Rgba32 ToPixel(Videra.Core.Geometry.RgbaFloat color)
    {
        return new Rgba32(
            (byte)Math.Clamp(color.R * 255f, 0f, 255f),
            (byte)Math.Clamp(color.G * 255f, 0f, 255f),
            (byte)Math.Clamp(color.B * 255f, 0f, 255f),
            (byte)Math.Clamp(color.A * 255f, 0f, 255f));
    }
}
