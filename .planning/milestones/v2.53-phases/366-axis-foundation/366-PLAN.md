---
phase: 366-axis-foundation
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs
  - tests/Videra.SurfaceCharts.Core.Tests/Axis/LogScaleAxisDescriptorTests.cs
  - tests/Videra.SurfaceCharts.Core.Tests/Axis/DateTimeAxisTickGeneratorTests.cs
  - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Axis/CustomFormatterIntegrationTests.cs
autonomous: true
requirements:
  - AXIS-01
  - AXIS-02
  - AXIS-03
  - AXIS-04
  - AXIS-05
must_haves:
  truths:
    - "User can set SurfaceAxisScaleKind.Log on an axis and construct SurfaceAxisDescriptor without exception"
    - "User receives ArgumentOutOfRangeException when setting Log axis with minimum <= 0"
    - "Log-scale axis generates powers-of-10 tick values (1, 10, 100, etc.) with correct log spacing"
    - "DateTime axis generates tick values at nice time intervals (1s, 5s, 1min, 1hr, 1day, etc.)"
    - "DateTime axis formats tick labels as human-readable UTC timestamps"
    - "User can set per-axis custom formatter (XAxisFormatter, YAxisFormatter, ZAxisFormatter) and see custom labels"
    - "DateTime axis values stored as UTC seconds (long) maintain full precision"
  artifacts:
    - path: "src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs"
      provides: "Log-axis validation (min > 0), Log scale kind accepted"
      contains: "SurfaceAxisScaleKind.Log"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs"
      provides: "Log tick generation, DateTime tick generation"
      exports: ["CreateLogTickValues", "CreateDateTimeTickValues"]
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs"
      provides: "Per-axis formatter properties"
      contains: "XAxisFormatter"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs"
      provides: "Scale-kind-aware tick generation dispatch, DateTime formatting, per-axis formatter wiring"
      contains: "SurfaceAxisScaleKind"
  key_links:
    - from: "SurfaceAxisDescriptor"
      to: "SurfaceGeometryGrid.MapNormalizedCoordinate"
      via: "ScaleKind property consumed by geometry grid"
      pattern: "axis\\.ScaleKind"
    - from: "SurfaceAxisOverlayPresenter.CreateAxisState"
      to: "SurfaceAxisTickGenerator.CreateLogTickValues"
      via: "ScaleKind dispatch in tick generation"
      pattern: "CreateLogTickValues"
    - from: "SurfaceAxisOverlayPresenter.CreateTicks"
      to: "SurfaceChartOverlayOptions.FormatLabel"
      via: "Per-axis formatter dispatch"
      pattern: "FormatLabel"
---

<objective>
Unblock log-scale axes, implement DateTime tick generation, and wire per-axis custom tick formatters across the SurfaceCharts axis pipeline.

Purpose: Axis semantics are the foundation for all chart type expansion — Bar, Contour, and enhanced Legend all depend on non-linear axis support. The single blocker is a `throw` in `SurfaceAxisDescriptor` on `SurfaceAxisScaleKind.Log`. Removing it with proper validation, implementing log/DateTime tick generation, and adding per-axis formatters unblocks the entire v2.53 milestone.

Output:
- Log-scale axes constructable with min > 0 validation
- Log tick generation producing powers-of-10 values
- DateTime tick generation producing nice time intervals
- DateTime label formatting producing human-readable UTC timestamps
- Per-axis custom formatter support (XAxisFormatter, YAxisFormatter, ZAxisFormatter)
- Unit tests for axis descriptor, tick generators, and formatters
</objective>

<execution_context>
@$HOME/.config/opencode/get-shit-done/workflows/execute-plan.md
@$HOME/.config/opencode/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/phases/366-axis-foundation/366-CONTEXT.md
@.planning/research/SUMMARY.md
@.planning/research/ARCHITECTURE.md

@src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs
@src/Videra.SurfaceCharts.Core/SurfaceAxisScaleKind.cs
@src/Videra.SurfaceCharts.Core/SurfaceGeometryGrid.cs
@src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs
@src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs
@src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs
@src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayState.cs

<interfaces>
<!-- Key types and contracts the executor needs. -->

From src/Videra.SurfaceCharts.Core/SurfaceAxisScaleKind.cs:
```csharp
public enum SurfaceAxisScaleKind
{
    Linear = 0,
    Log = 1,
    DateTime = 2,
    ExplicitCoordinates = 3
}
```

From src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs:
```csharp
public sealed class SurfaceAxisDescriptor
{
    public SurfaceAxisDescriptor(string label, string? unit, double minimum, double maximum)
        : this(label, unit, minimum, maximum, SurfaceAxisScaleKind.Linear) { }

    public SurfaceAxisDescriptor(string label, string? unit, double minimum, double maximum, SurfaceAxisScaleKind scaleKind)
    {
        // Lines 46-51: throw on Log — THIS IS THE BLOCKER TO REMOVE
        if (scaleKind == SurfaceAxisScaleKind.Log)
        {
            throw new ArgumentException(
                "Logarithmic axis scaling is reserved until raw axis values and display-space coordinates are separated.",
                nameof(scaleKind));
        }
    }

    public string Label { get; }
    public string? Unit { get; }
    public SurfaceAxisScaleKind ScaleKind { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public double Span => Maximum - Minimum;
}
```

From src/Videra.SurfaceCharts.Core/SurfaceGeometryGrid.cs (line 127-143):
```csharp
protected static double MapNormalizedCoordinate(SurfaceAxisDescriptor axis, double normalized)
{
    return axis.ScaleKind switch
    {
        SurfaceAxisScaleKind.Linear or SurfaceAxisScaleKind.DateTime =>
            axis.Minimum + (axis.Span * normalized),
        SurfaceAxisScaleKind.Log =>
            Math.Pow(10d,
                Math.Log10(axis.Minimum) + ((Math.Log10(axis.Maximum) - Math.Log10(axis.Minimum)) * normalized)),
        // ...
    };
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs:
```csharp
internal static class SurfaceAxisTickGenerator
{
    public static IReadOnlyList<double> CreateMajorTickValues(double axisMinimum, double axisMaximum, double axisLength) { ... }
    public static IReadOnlyList<double> CreateMinorTickValues(IReadOnlyList<double> majorTickValues, int minorTickDivisions) { ... }
    private static double ComputeNiceStep(double roughStep) { ... }
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs:
```csharp
public sealed class SurfaceChartOverlayOptions
{
    public Func<string, double, string>? LabelFormatter { get; init; }
    // FormatLabel dispatches on axisKey ("X", "Y", "Z", "Legend")
    public string FormatLabel(string axisKey, double value) { ... }
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs:
```csharp
internal static class SurfaceAxisOverlayPresenter
{
    // CreateAxisState calls SurfaceAxisTickGenerator.CreateMajorTickValues
    // CreateTicks calls overlayOptions.FormatLabel(axisKey, tickValue)
    public static SurfaceAxisOverlayState CreateState(
        SurfaceMetadata? metadata,
        SurfaceChartProjection? projection,
        SurfaceChartOverlayOptions? overlayOptions) { ... }
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayState.cs:
```csharp
internal sealed class SurfaceAxisState
{
    public string AxisKey { get; }
    public IReadOnlyList<SurfaceAxisTickState> Ticks { get; }
    // ...
}

internal sealed class SurfaceAxisTickState
{
    public double Value { get; }
    public string LabelText { get; }
    // ...
}
```
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Unblock Log Scale + Implement Log Tick Generation + DateTime Tick Generation</name>
  <files>
    src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs,
    tests/Videra.SurfaceCharts.Core.Tests/Axis/LogScaleAxisDescriptorTests.cs,
    tests/Videra.SurfaceCharts.Core.Tests/Axis/DateTimeAxisTickGeneratorTests.cs
  </files>
  <read_first>
    src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs,
    src/Videra.SurfaceCharts.Core/SurfaceAxisScaleKind.cs,
    src/Videra.SurfaceCharts.Core/SurfaceGeometryGrid.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs,
    tests/Videra.SurfaceCharts.Core.Tests/SurfaceColorMapTests.cs
  </read_first>
  <behavior>
    - LogScaleAxisDescriptorTests:
      - `Constructor_LogScale_WithPositiveRange_Succeeds`: new SurfaceAxisDescriptor("Y", "dB", 1d, 1000d, SurfaceAxisScaleKind.Log) does not throw
      - `Constructor_LogScale_WithZeroMinimum_ThrowsArgumentOutOfRange`: throws ArgumentOutOfRangeException with message containing "positive"
      - `Constructor_LogScale_WithNegativeMinimum_ThrowsArgumentOutOfRange`: throws ArgumentOutOfRangeException
      - `Constructor_LogScale_WithZeroMaximum_ThrowsArgumentOutOfRange`: throws ArgumentOutOfRangeException
      - `Constructor_LogScale_WithNegativeMaximum_ThrowsArgumentOutOfRange`: throws ArgumentOutOfRangeException
      - `Constructor_LogScale_SetsScaleKind`: ScaleKind == SurfaceAxisScaleKind.Log
      - `Constructor_LogScale_MinimumEqualsMaximum_Succeeds`: new SurfaceAxisDescriptor("Y", null, 1d, 1d, SurfaceAxisScaleKind.Log) succeeds (degenerate but valid)
    - DateTimeAxisTickGeneratorTests (access via InternalsVisibleTo or reflection):
      - `CreateDateTimeTickValues_SubMinuteRange_ReturnsSecondSteps`: range 0..30, returns ticks at 5s intervals
      - `CreateDateTimeTickValues_MinuteRange_ReturnsMinuteSteps`: range 0..3600, returns ticks at 60s or 300s intervals
      - `CreateDateTimeTickValues_HourRange_ReturnsHourSteps`: range 0..86400, returns ticks at 3600s or 7200s intervals
      - `CreateDateTimeTickValues_DayRange_ReturnsDaySteps`: range 0..604800, returns ticks at 86400s intervals
      - `CreateDateTimeTickValues_ReturnsTicksInRange`: all returned values >= min and <= max
      - `CreateDateTimeTickValues_DegenerateRange_ReturnsMinMax`: min == max returns [min, max]
      - `CreateLogTickValues_StandardRange_ReturnsPowersOf10`: range 1..1000 returns [1, 10, 100, 1000]
      - `CreateLogTickValues_NarrowRange_ReturnsAppropriateTicks`: range 1..100 returns ticks at decade intervals
      - `CreateLogTickValues_DegenerateRange_ReturnsMinMax`: min == max returns [min, max]
      - `CreateLogTickValues_NonPositiveMin_ReturnsMinMax`: min <= 0 returns [min, max]
  </behavior>
  <action>
    **SurfaceAxisDescriptor.cs — per D-01 (Log unblock) and D-04 (Log validation):**

    1. Remove lines 46-51 (the `throw` on `SurfaceAxisScaleKind.Log`).

    2. Replace with validation that rejects min <= 0 or max <= 0 for Log scale:
    ```csharp
    if (scaleKind == SurfaceAxisScaleKind.Log)
    {
        if (minimum <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimum),
                "Logarithmic axis minimum must be positive (greater than zero).");
        }

        if (maximum <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximum),
                "Logarithmic axis maximum must be positive (greater than zero).");
        }
    }
    ```
    Place this BEFORE the `maximum < minimum` check (line 53) so the error message is specific.

    3. Update the constructor XML doc to document the Log validation:
    - Add `/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="scaleKind"/> is Log and <paramref name="minimum"/> or <paramref name="maximum"/> is not positive.</exception>`

    **SurfaceAxisTickGenerator.cs — per D-01 (Log ticks) and D-02 (DateTime ticks):**

    4. Add `CreateLogTickValues` public static method:
    ```csharp
    public static IReadOnlyList<double> CreateLogTickValues(double axisMinimum, double axisMaximum, double axisLength)
    {
        if (axisMinimum <= 0d || axisMaximum <= axisMinimum)
        {
            return [axisMinimum, axisMaximum];
        }

        var logMin = Math.Log10(axisMinimum);
        var logMax = Math.Log10(axisMaximum);
        var logSpan = logMax - logMin;
        var targetTickCount = Math.Clamp((int)Math.Round(axisLength / 72d), 2, 7);
        var logStep = ComputeNiceStep(logSpan / targetTickCount);
        var firstLogTick = Math.Ceiling(logMin / logStep) * logStep;
        var ticks = new List<double>();

        for (var logTick = firstLogTick; logTick <= logMax + (logStep * 0.5d); logTick += logStep)
        {
            var value = Math.Pow(10d, logTick);
            if (value >= axisMinimum && value <= axisMaximum)
            {
                ticks.Add(Math.Round(value, 12, MidpointRounding.AwayFromZero));
            }
        }

        if (ticks.Count == 0)
        {
            return [axisMinimum, axisMaximum];
        }

        return ticks;
    }
    ```

    5. Add `CreateDateTimeTickValues` public static method:
    ```csharp
    private static readonly double[] NiceTimeSteps =
    [
        1, 2, 5, 10, 15, 30,           // seconds
        60, 120, 300, 600, 1800, 3600,  // minutes/hours
        7200, 14400, 43200, 86400,      // hours/day
        172800, 604800, 2592000,        // days/weeks/months
        7776000, 15552000, 31536000,    // months/year
    ];

    public static IReadOnlyList<double> CreateDateTimeTickValues(double axisMinimum, double axisMaximum, double axisLength)
    {
        if (axisMaximum <= axisMinimum)
        {
            return [axisMinimum, axisMaximum];
        }

        var axisSpan = axisMaximum - axisMinimum;
        var targetTickCount = Math.Clamp((int)Math.Round(axisLength / 100d), 2, 7);
        var roughStep = axisSpan / targetTickCount;

        // Find the smallest nice step >= roughStep
        var step = NiceTimeSteps.FirstOrDefault(s => s >= roughStep);
        if (step == 0d) step = NiceTimeSteps[^1];

        var firstTick = Math.Ceiling(axisMinimum / step) * step;
        var ticks = new List<double>();

        for (var tick = firstTick; tick <= axisMaximum + (step * 0.5d); tick += step)
        {
            ticks.Add(Math.Round(tick, 6, MidpointRounding.AwayFromZero));
        }

        if (ticks.Count == 0)
        {
            return [axisMinimum, axisMaximum];
        }

        return ticks;
    }
    ```

    **Tests — LogScaleAxisDescriptorTests.cs (new file):**

    6. Create `tests/Videra.SurfaceCharts.Core.Tests/Axis/LogScaleAxisDescriptorTests.cs`:
    - Namespace: `Videra.SurfaceCharts.Core.Tests`
    - Use FluentAssertions + xUnit
    - Tests as specified in `<behavior>` block above
    - Constructor tests instantiate `SurfaceAxisDescriptor` directly
    - Verify exception type is `ArgumentOutOfRangeException` (not `ArgumentException`)
    - Verify exception message contains "positive"

    **Tests — DateTimeAxisTickGeneratorTests.cs (new file):**

    7. Create `tests/Videra.SurfaceCharts.Core.Tests/Axis/DateTimeAxisTickGeneratorTests.cs`:
    - Namespace: `Videra.SurfaceCharts.Core.Tests`
    - Use FluentAssertions + xUnit
    - Access `SurfaceAxisTickGenerator` via reflection (it's `internal static` in Avalonia assembly)
    - OR add `[assembly: InternalsVisibleTo("Videra.SurfaceCharts.Core.Tests")]` to Avalonia assembly if not already present
    - Tests as specified in `<behavior>` block above
    - For DateTime tests, use UTC seconds values (e.g., 0 = epoch, 30 = 30s later, 3600 = 1hr later)
    - For Log tests, use power-of-10 ranges (1..1000, 1..100, 0.01..100)

    8. Run tests:
    ```bash
    dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "LogScaleAxisDescriptor|DateTimeAxisTickGenerator" --no-build
    ```
  </action>
  <verify>
    <automated>dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "LogScaleAxisDescriptor|DateTimeAxisTickGenerator" -v normal</automated>
  </verify>
  <done>
    - SurfaceAxisDescriptor accepts SurfaceAxisScaleKind.Log with positive min/max
    - SurfaceAxisDescriptor throws ArgumentOutOfRangeException for Log with min <= 0 or max <= 0
    - SurfaceAxisTickGenerator.CreateLogTickValues returns powers-of-10 for standard ranges
    - SurfaceAxisTickGenerator.CreateDateTimeTickValues returns ticks at nice time intervals
    - All LogScaleAxisDescriptor tests pass (7 tests)
    - All DateTimeAxisTickGenerator tests pass (10 tests)
  </done>
</task>

<task type="auto" tdd="true">
  <name>Task 2: DateTime Label Formatting + Per-Axis Custom Formatters + Overlay Wiring</name>
  <files>
    src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs,
    tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Axis/CustomFormatterIntegrationTests.cs
  </files>
  <read_first>
    src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayState.cs,
    src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs,
    tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs
  </read_first>
  <behavior>
    - CustomFormatterIntegrationTests:
      - `FormatLabel_WithXAxisFormatter_UsesXFormatter`: setting XAxisFormatter to custom func, FormatLabel("X", 42) returns custom result
      - `FormatLabel_WithYAxisFormatter_UsesYFormatter`: setting YAxisFormatter, FormatLabel("Y", 42) returns custom result
      - `FormatLabel_PerAxisFormatter_TakesPriorityOverLabelFormatter`: both set, per-axis wins
      - `FormatLabel_NoFormatter_FallsBackToNumeric`: no formatters set, returns numeric string
      - `FormatLabel_LabelFormatter_UsedWhenNoPerAxis`: only LabelFormatter set, it's used for all axes
      - `FormatDateTimeLabel_SubHourRange_ShowsTimeOnly`: span < 3600 shows "HH:mm:ss" format
      - `FormatDateTimeLabel_SubDayRange_ShowsDateAndTime`: span < 86400 shows "MM-dd HH:mm" format
      - `FormatDateTimeLabel_LongRange_ShowsDateOnly`: span >= 86400 shows "yyyy-MM-dd" format
  </behavior>
  <action>
    **SurfaceChartOverlayOptions.cs — per D-03 (custom formatters):**

    1. Add three per-axis formatter properties:
    ```csharp
    /// <summary>
    /// Gets the optional custom formatter for the horizontal (X) axis tick labels.
    /// When set, this takes priority over <see cref="LabelFormatter"/> for X axis values.
    /// </summary>
    public Func<double, string>? XAxisFormatter { get; init; }

    /// <summary>
    /// Gets the optional custom formatter for the value (Y) axis tick labels.
    /// When set, this takes priority over <see cref="LabelFormatter"/> for Y axis values.
    /// </summary>
    public Func<double, string>? YAxisFormatter { get; init; }

    /// <summary>
    /// Gets the optional custom formatter for the depth (Z) axis tick labels.
    /// When set, this takes priority over <see cref="LabelFormatter"/> for Z axis values.
    /// </summary>
    public Func<double, string>? ZAxisFormatter { get; init; }
    ```

    2. Modify `FormatLabel` method to dispatch per-axis formatters first:
    ```csharp
    public string FormatLabel(string axisKey, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(axisKey);

        // Per-axis formatters take priority (per D-03)
        var perAxisFormatter = axisKey switch
        {
            "X" => XAxisFormatter,
            "Y" => YAxisFormatter,
            "Z" => ZAxisFormatter,
            _ => null,
        };

        if (perAxisFormatter is not null)
        {
            return perAxisFormatter(value);
        }

        if (LabelFormatter is not null)
        {
            return LabelFormatter(axisKey, value);
        }

        if (string.Equals(axisKey, "Legend", StringComparison.Ordinal))
        {
            return FormatNumericLabel(value, LegendLabelFormat, LegendLabelPrecision);
        }

        return FormatNumericLabel(value, TickLabelFormat, TickLabelPrecision);
    }
    ```

    3. Add `FormatDateTimeLabel` internal static method for DateTime axis formatting:
    ```csharp
    /// <summary>
    /// Formats a UTC-seconds value as a human-readable DateTime label.
    /// </summary>
    /// <param name="utcSeconds">The UTC time expressed as seconds since Unix epoch.</param>
    /// <param name="axisSpanSeconds">The total axis span in seconds, used to select format granularity.</param>
    /// <returns>A formatted date/time string.</returns>
    internal static string FormatDateTimeLabel(double utcSeconds, double axisSpanSeconds)
    {
        var dto = DateTimeOffset.FromUnixTimeSeconds((long)utcSeconds);

        return axisSpanSeconds switch
        {
            < 3600d => dto.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            < 86400d => dto.ToString("MM-dd HH:mm", CultureInfo.InvariantCulture),
            _ => dto.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        };
    }
    ```

    **SurfaceAxisOverlayPresenter.cs — per D-01 (log dispatch), D-02 (DateTime dispatch), D-03 (formatter wiring):**

    4. Modify `CreateAxisState` to accept `SurfaceAxisScaleKind` parameter:
    ```csharp
    private static SurfaceAxisState CreateAxisState(
        string axisKey,
        string titleText,
        double axisMinimum,
        double axisMaximum,
        SurfaceAxisScaleKind scaleKind,  // NEW parameter
        Point start,
        Point end,
        Point projectedCenter,
        SurfaceChartOverlayOptions overlayOptions)
    {
        var outwardNormal = GetOutwardNormal(start, end, projectedCenter);
        var titleAnchor = GetOuterEndpoint(start, end, projectedCenter);
        var titlePosition = titleAnchor + (outwardNormal * TitleOffset);

        // Dispatch tick generation based on scale kind (per D-01, D-02)
        var majorTickValues = scaleKind switch
        {
            SurfaceAxisScaleKind.Log =>
                SurfaceAxisTickGenerator.CreateLogTickValues(axisMinimum, axisMaximum, GetDistance(start, end)),
            SurfaceAxisScaleKind.DateTime =>
                SurfaceAxisTickGenerator.CreateDateTimeTickValues(axisMinimum, axisMaximum, GetDistance(start, end)),
            _ =>
                SurfaceAxisTickGenerator.CreateMajorTickValues(axisMinimum, axisMaximum, GetDistance(start, end)),
        };

        var ticks = CreateTicks(axisKey, majorTickValues, start, end, outwardNormal, overlayOptions, scaleKind);
        var minorTicks = overlayOptions.ShowMinorTicks
            ? CreateMinorTicks(
                SurfaceAxisTickGenerator.CreateMinorTickValues(majorTickValues, overlayOptions.MinorTickDivisions),
                axisMinimum,
                axisMaximum,
                start,
                end,
                outwardNormal)
            : Array.Empty<SurfaceAxisLineGeometry>();

        return new SurfaceAxisState(
            axisKey,
            new SurfaceAxisLineGeometry(start, end),
            titleText,
            titlePosition,
            SurfaceAxisLayoutEngine.CullDenseLabels(ticks),
            minorTicks,
            ticks.Count);
    }
    ```

    5. Modify `CreateTicks` to accept `SurfaceAxisScaleKind` and format DateTime labels:
    ```csharp
    private static IReadOnlyList<SurfaceAxisTickState> CreateTicks(
        string axisKey,
        IReadOnlyList<double> tickValues,
        Point start,
        Point end,
        AvaVector outwardNormal,
        SurfaceChartOverlayOptions overlayOptions,
        SurfaceAxisScaleKind scaleKind)  // NEW parameter
    {
        List<SurfaceAxisTickState> ticks = [];
        if (tickValues.Count == 0)
        {
            return ticks;
        }

        var axisMinimum = tickValues[0];
        var axisMaximum = tickValues[^1];
        var axisSpan = axisMaximum - axisMinimum;

        foreach (var tickValue in tickValues)
        {
            var t = axisSpan <= 0d ? 0d : (tickValue - axisMinimum) / axisSpan;
            t = Math.Clamp(t, 0d, 1d);

            var tickStart = Lerp(start, end, t);
            var tickEnd = tickStart + (outwardNormal * TickLength);
            var labelPosition = tickEnd + (outwardNormal * LabelOffset);

            // Format label: DateTime uses specialized formatter, others use overlay options (per D-02, D-03)
            var labelText = scaleKind == SurfaceAxisScaleKind.DateTime
                ? SurfaceChartOverlayOptions.FormatDateTimeLabel(tickValue, axisSpan)
                : overlayOptions.FormatLabel(axisKey, tickValue);

            ticks.Add(
                new SurfaceAxisTickState(
                    tickValue,
                    labelText,
                    new SurfaceAxisLineGeometry(tickStart, tickEnd),
                    labelPosition));
        }

        return ticks;
    }
    ```

    6. Update all three `CreateAxisState` call sites in `CreateState` to pass the axis `ScaleKind`:
    - xAxis call: pass `metadata.HorizontalAxis.ScaleKind`
    - yAxis call: pass a computed scale kind from valueRange (use Linear by default; if the caller sets a value-axis scale kind, pass it through — for now, value axis is always Linear)
    - zAxis call: pass `metadata.VerticalAxis.ScaleKind`

    For the Y axis (value axis), the `SurfaceMetadata` does not currently carry a ScaleKind for the value dimension. For this phase, default to `SurfaceAxisScaleKind.Linear`. A future phase can add value-axis scale kind support. The log scale support is primarily for the horizontal and depth axes.

    **Tests — CustomFormatterIntegrationTests.cs (new file):**

    7. Create `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Axis/CustomFormatterIntegrationTests.cs`:
    - Namespace: `Videra.SurfaceCharts.Avalonia.IntegrationTests`
    - Use FluentAssertions + xUnit
    - Tests operate on `SurfaceChartOverlayOptions` directly (it's a public class)
    - Tests for `FormatLabel` dispatch with per-axis formatters
    - Tests for `FormatDateTimeLabel` with different span ranges
    - Tests as specified in `<behavior>` block above

    8. Run tests:
    ```bash
    dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests --filter "CustomFormatter" --no-build
    ```
  </action>
  <verify>
    <automated>dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests --filter "CustomFormatter" -v normal</automated>
  </verify>
  <done>
    - SurfaceChartOverlayOptions has XAxisFormatter, YAxisFormatter, ZAxisFormatter properties
    - FormatLabel dispatches per-axis formatters before LabelFormatter
    - FormatDateTimeLabel produces human-readable UTC timestamps with span-aware format
    - SurfaceAxisOverlayPresenter dispatches to CreateLogTickValues/CreateDateTimeTickValues based on ScaleKind
    - CreateTicks formats DateTime labels via FormatDateTimeLabel
    - All CustomFormatterIntegration tests pass (8 tests)
  </done>
</task>

</tasks>

<verification>
```bash
# Run all axis-related tests
dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "LogScaleAxisDescriptor|DateTimeAxisTickGenerator" -v normal
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests --filter "CustomFormatter" -v normal

# Full test suite (no regressions)
dotnet test tests/ --no-build

# Verify Log scale kind is accepted
grep -n "SurfaceAxisScaleKind.Log" src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs

# Verify no remaining throw on Log
grep -n "throw.*reserved" src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs
# Expected: no matches

# Verify per-axis formatters exist
grep -n "XAxisFormatter\|YAxisFormatter\|ZAxisFormatter" src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs

# Verify DateTime formatting exists
grep -n "FormatDateTimeLabel" src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs

# Verify tick generators exist
grep -n "CreateLogTickValues\|CreateDateTimeTickValues" src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs
```
</verification>

<success_criteria>
- SurfaceAxisDescriptor accepts SurfaceAxisScaleKind.Log with positive min/max (no throw)
- SurfaceAxisDescriptor throws ArgumentOutOfRangeException for Log with min <= 0 or max <= 0
- SurfaceAxisTickGenerator.CreateLogTickValues returns powers-of-10 for standard ranges
- SurfaceAxisTickGenerator.CreateDateTimeTickValues returns ticks at nice time intervals (1s, 5s, 1min, 1hr, 1day)
- SurfaceChartOverlayOptions has per-axis formatter properties (XAxisFormatter, YAxisFormatter, ZAxisFormatter)
- FormatLabel dispatches per-axis formatters first, then LabelFormatter, then numeric defaults
- FormatDateTimeLabel produces span-aware human-readable UTC timestamps
- SurfaceAxisOverlayPresenter dispatches tick generation based on axis ScaleKind
- All 15+ tests pass (7 Log descriptor + 10 tick generators + 8 formatter tests)
- Full test suite passes with no regressions
</success_criteria>

<output>
After completion, create `.planning/phases/366-axis-foundation/366-01-SUMMARY.md`
</output>
