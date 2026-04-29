using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Extracts iso-line segments from a 2D scalar field using the marching squares algorithm.
/// </summary>
public static class MarchingSquaresExtractor
{
    /// <summary>
    /// Extracts contour segments at the specified iso-value from the scalar field.
    /// </summary>
    /// <param name="field">The 2D scalar field.</param>
    /// <param name="isoValue">The iso-value to extract.</param>
    /// <param name="mask">Optional mask where <see langword="true"/> means the sample is present.</param>
    /// <returns>The extracted contour segments in normalized coordinates.</returns>
    public static IReadOnlyList<ContourSegment> Extract(
        SurfaceScalarField field,
        float isoValue,
        SurfaceMask? mask = null)
    {
        ArgumentNullException.ThrowIfNull(field);

        var segments = new List<ContourSegment>();
        var values = field.Values.Span;
        var width = field.Width;
        var height = field.Height;

        // Process each cell (2x2 neighborhood)
        for (var y = 0; y < height - 1; y++)
        {
            for (var x = 0; x < width - 1; x++)
            {
                // Get corner values (counter-clockwise from bottom-left)
                // Convention: bottom-left=0, bottom-right=1, top-right=2, top-left=3
                var v00 = values[y * width + x];         // bottom-left
                var v10 = values[y * width + (x + 1)];   // bottom-right
                var v11 = values[(y + 1) * width + (x + 1)]; // top-right
                var v01 = values[(y + 1) * width + x];   // top-left

                // Check mask
                if (mask is not null)
                {
                    var maskValues = mask.Values.Span;
                    if (!maskValues[y * width + x] ||
                        !maskValues[y * width + (x + 1)] ||
                        !maskValues[(y + 1) * width + (x + 1)] ||
                        !maskValues[(y + 1) * width + x])
                    {
                        continue;
                    }
                }

                // Skip cells with non-finite values
                if (!float.IsFinite(v00) || !float.IsFinite(v10) ||
                    !float.IsFinite(v11) || !float.IsFinite(v01))
                {
                    continue;
                }

                // Compute case index (which corners are above iso-value)
                var caseIndex = 0;
                if (v00 >= isoValue) caseIndex |= 1;  // bit 0: bottom-left
                if (v10 >= isoValue) caseIndex |= 2;  // bit 1: bottom-right
                if (v11 >= isoValue) caseIndex |= 4;  // bit 2: top-right
                if (v01 >= isoValue) caseIndex |= 8;  // bit 3: top-left

                // Skip empty (0) and full (15) cells
                if (caseIndex == 0 || caseIndex == 15)
                {
                    continue;
                }

                // Compute interpolation points on edges
                // Edge 0: bottom (v00 to v10)
                // Edge 1: right (v10 to v11)
                // Edge 2: top (v01 to v11)
                // Edge 3: left (v00 to v01)
                var bottom = InterpolateEdge(v00, v10, isoValue, x, y, x + 1, y);
                var right = InterpolateEdge(v10, v11, isoValue, x + 1, y, x + 1, y + 1);
                var top = InterpolateEdge(v01, v11, isoValue, x, y + 1, x + 1, y + 1);
                var left = InterpolateEdge(v00, v01, isoValue, x, y, x, y + 1);

                // Generate segments based on case
                AddSegmentsForCase(segments, caseIndex, bottom, right, top, left, v00, v10, v11, v01, isoValue);
            }
        }

        return segments;
    }

    private static Vector3 InterpolateEdge(
        float v1, float v2, float isoValue,
        int x1, int y1, int x2, int y2)
    {
        // Handle degenerate case where both values are equal
        if (Math.Abs(v2 - v1) < 1e-10f)
        {
            return new Vector3((x1 + x2) * 0.5f, (y1 + y2) * 0.5f, 0f);
        }

        var t = (isoValue - v1) / (v2 - v1);
        t = Math.Clamp(t, 0f, 1f);

        return new Vector3(
            x1 + t * (x2 - x1),
            y1 + t * (y2 - y1),
            0f);
    }

    private static void AddSegmentsForCase(
        List<ContourSegment> segments,
        int caseIndex,
        Vector3 bottom, Vector3 right, Vector3 top, Vector3 left,
        float v00, float v10, float v11, float v01,
        float isoValue)
    {
        switch (caseIndex)
        {
            case 1:   // 0001: bottom-left above → line from bottom to left
                segments.Add(new ContourSegment(bottom, left));
                break;
            case 2:   // 0010: bottom-right above → line from right to bottom
                segments.Add(new ContourSegment(right, bottom));
                break;
            case 3:   // 0011: bottom two above → line from right to left
                segments.Add(new ContourSegment(right, left));
                break;
            case 4:   // 0100: top-right above → line from top to right
                segments.Add(new ContourSegment(top, right));
                break;
            case 5:   // 0101: diagonal (saddle) → use asymptotic decider
                var center = (v00 + v10 + v11 + v01) * 0.25f;
                if (center >= isoValue)
                {
                    // Connect bottom-left to top-right regions
                    segments.Add(new ContourSegment(bottom, left));
                    segments.Add(new ContourSegment(top, right));
                }
                else
                {
                    // Connect bottom-right to top-left regions
                    segments.Add(new ContourSegment(right, bottom));
                    segments.Add(new ContourSegment(left, top));
                }
                break;
            case 6:   // 0110: right two above → line from top to bottom
                segments.Add(new ContourSegment(top, bottom));
                break;
            case 7:   // 0111: only top-left below → line from top to left
                segments.Add(new ContourSegment(top, left));
                break;
            case 8:   // 1000: top-left above → line from left to top
                segments.Add(new ContourSegment(left, top));
                break;
            case 9:   // 1001: left two above → line from bottom to top
                segments.Add(new ContourSegment(bottom, top));
                break;
            case 10:  // 1010: diagonal (saddle) → use asymptotic decider
                var center2 = (v00 + v10 + v11 + v01) * 0.25f;
                if (center2 >= isoValue)
                {
                    // Connect bottom-right to top-left regions
                    segments.Add(new ContourSegment(right, bottom));
                    segments.Add(new ContourSegment(left, top));
                }
                else
                {
                    // Connect bottom-left to top-right regions
                    segments.Add(new ContourSegment(bottom, left));
                    segments.Add(new ContourSegment(top, right));
                }
                break;
            case 11:  // 1011: only top-right below → line from right to top
                segments.Add(new ContourSegment(right, top));
                break;
            case 12:  // 1100: top two above → line from left to right
                segments.Add(new ContourSegment(left, right));
                break;
            case 13:  // 1101: only bottom-right below → line from bottom to right
                segments.Add(new ContourSegment(bottom, right));
                break;
            case 14:  // 1110: only bottom-left below → line from left to bottom
                segments.Add(new ContourSegment(left, bottom));
                break;
                // cases 0 and 15 are handled before calling this method
        }
    }
}
