---
phase: 364-demo-smoke-doctor-snapshot-evidence
plan: 01
type: execute
wave: 1
depends_on:
  - 363-chart-snapshot-capture-implementation
files_modified:
  - samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs
  - samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml
  - samples/Videra.AvaloniaWorkbenchSample/WorkbenchSupportCapture.cs
  - smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs
  - scripts/Invoke-VideraDoctor.ps1
autonomous: true
requirements:
  - DEMO-01
  - DEMO-02
  - DEMO-03
  - DEMO-04
  - VER-01
  - VER-02
  - VER-03

must_haves:
  truths:
    - "SurfaceCharts demo exposes a bounded CaptureSnapshot button that calls Plot.CaptureSnapshotAsync"
    - "SurfaceCharts demo support summary includes SnapshotStatus, SnapshotPath, and SnapshotManifest fields"
    - "Consumer smoke calls CaptureSnapshotAsync after chart readiness and validates manifest fields"
    - "Doctor parses SnapshotStatus (present/failed/unavailable/missing) without launching UI"
    - "Demo remains bounded — single snapshot action, no editor/batch/configuration"
  artifacts:
    - path: "samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs"
      provides: "Snapshot capture handler and snapshot support summary fields"
      contains: "CaptureSnapshotAsync"
    - path: "samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml"
      provides: "Capture Snapshot button in SUPPORT section"
      contains: "CaptureSnapshotButton"
    - path: "samples/Videra.AvaloniaWorkbenchSample/WorkbenchSupportCapture.cs"
      provides: "Snapshot evidence formatting for workbench support"
      contains: "SnapshotStatus"
    - path: "smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs"
      provides: "Snapshot capture and manifest validation in consumer smoke"
      contains: "CaptureSnapshotAsync"
    - path: "scripts/Invoke-VideraDoctor.ps1"
      provides: "Snapshot status parsing in Doctor report"
      contains: "snapshotStatus"
  key_links:
    - from: "samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs"
      to: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs"
      via: "Plot.CaptureSnapshotAsync call"
      pattern: "CaptureSnapshotAsync"
    - from: "smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs"
      to: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs"
      via: "Plot.CaptureSnapshotAsync call"
      pattern: "CaptureSnapshotAsync"
    - from: "scripts/Invoke-VideraDoctor.ps1"
      to: "samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs"
      via: "SnapshotStatus field in support summary parsed by Doctor"
      pattern: "SnapshotStatus"
---

<objective>
Refresh demo, consumer smoke, support summaries, and Doctor parsing around snapshot artifacts and manifests.

Purpose: Wire Phase 363's CaptureSnapshotAsync into the demo and consumer smoke surfaces, expose snapshot evidence in support summaries, and enable Doctor to parse snapshot status without launching UI.

Output: Updated demo with snapshot action, consumer smoke with snapshot validation, support summaries with snapshot fields, Doctor with snapshot parsing.
</objective>

<execution_context>
@$HOME/.config/opencode/get-shit-done/workflows/execute-plan.md
@$HOME/.config/opencode/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/phases/364-demo-smoke-doctor-snapshot-evidence/364-CONTEXT.md
@.planning/phases/363-chart-snapshot-capture-implementation/363-01-SUMMARY.md

<interfaces>
<!-- Key types and contracts the executor needs. Extracted from Phase 362-63. -->
<!-- Executor should use these directly — no codebase exploration needed. -->

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs:
```csharp
// Phase 363: CaptureSnapshotAsync is public on Plot3D
public async Task<PlotSnapshotResult> CaptureSnapshotAsync(PlotSnapshotRequest request)
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs:
```csharp
public sealed class PlotSnapshotRequest
{
    public PlotSnapshotRequest(int width, int height, double scale, PlotSnapshotBackground background, PlotSnapshotFormat format)
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs:
```csharp
public sealed class PlotSnapshotResult
{
    public bool Succeeded { get; }
    public string? Path { get; }
    public PlotSnapshotManifest? Manifest { get; }
    public PlotSnapshotDiagnostic? Failure { get; }
    public TimeSpan Duration { get; }
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs:
```csharp
public sealed class PlotSnapshotManifest
{
    public int Width { get; }
    public int Height { get; }
    public string OutputEvidenceKind { get; }
    public string DatasetEvidenceKind { get; }
    public string ActiveSeriesIdentity { get; }
    public PlotSnapshotFormat Format { get; }
    public PlotSnapshotBackground Background { get; }
    public DateTime CreatedUtc { get; }
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs:
```csharp
public sealed class PlotSnapshotDiagnostic
{
    public string Code { get; }
    public string Message { get; }
}
```

Enums:
```csharp
public enum PlotSnapshotFormat { Png }
public enum PlotSnapshotBackground { Transparent, Opaque }
```

Existing support summary format (line-based Key: Value):
```
SurfaceCharts support summary
GeneratedUtc: 2026-04-29T08:49:23.0000000+00:00
EvidenceKind: SurfaceChartsDatasetProof
...
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Add snapshot capture and support summary fields to SurfaceCharts demo</name>
  <files>
    samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs,
    samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml
  </files>
  <read_first>
    samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs,
    samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs
  </read_first>
  <action>
1. In `MainWindow.axaml`, add a "Capture Snapshot" button in the SUPPORT section (next to the existing "Copy Support Summary" button). Use pattern: `<Button Name="CaptureSnapshotButton" Content="Capture Snapshot" Click="OnCaptureSnapshotClicked" HorizontalAlignment="Stretch"/>`. Place it in the same Grid column layout as the existing support buttons (per D-01: bounded single action).

2. In `MainWindow.axaml.cs`, add a click handler `OnCaptureSnapshotClicked`:
   - Get the active chart view (use `ActiveSurfaceFamilyChartView` for surface/waterfall, `_scatterChartView` for scatter — match the existing `IsScatterProofActive` pattern).
   - Create a `PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png)`.
   - Call `await chartView.Plot.CaptureSnapshotAsync(request)`.
   - On success: set `_statusText.Text` to `"Snapshot captured: {result.Path}"` and update `_supportSummaryStatusText.Text` with snapshot status.
   - On failure: set `_statusText.Text` to `"Snapshot failed: {result.Failure.Message}"`.
   - Add `using Videra.SurfaceCharts.Avalonia.Controls;` if not already present (for PlotSnapshotRequest/Result types).

3. Add snapshot fields to the `UpdateSupportSummaryText()` method. After the existing `OutputCapabilityDiagnostics` line, add:
   ```
   $"SnapshotStatus: {CreateSnapshotStatusSummary()}\n" +
   $"SnapshotPath: {CreateSnapshotPathSummary()}\n" +
   $"SnapshotWidth: {CreateSnapshotWidthSummary()}\n" +
   $"SnapshotHeight: {CreateSnapshotHeightSummary()}\n" +
   $"SnapshotFormat: {CreateSnapshotFormatSummary()}\n" +
   $"SnapshotBackground: {CreateSnapshotBackgroundSummary()}\n" +
   $"SnapshotOutputEvidenceKind: {CreateSnapshotOutputEvidenceKindSummary()}\n" +
   $"SnapshotDatasetEvidenceKind: {CreateSnapshotDatasetEvidenceKindSummary()}\n" +
   $"SnapshotActiveSeriesIdentity: {CreateSnapshotActiveSeriesIdentitySummary()}\n" +
   $"SnapshotCreatedUtc: {CreateSnapshotCreatedUtcSummary()}\n"
   ```
   Add these for BOTH the scatter path and the surface/waterfall path in `UpdateSupportSummaryText()`.

4. Add private fields and helper methods:
   - `private PlotSnapshotResult? _lastSnapshotResult;`
   - `private string CreateSnapshotStatusSummary()` — returns `"present"` if `_lastSnapshotResult?.Succeeded == true`, `"failed"` if `_lastSnapshotResult?.Succeeded == false`, or `"none"` if `_lastSnapshotResult` is null.
   - `private string CreateSnapshotPathSummary()` — returns `_lastSnapshotResult?.Path ?? "none"`.
   - `private string CreateSnapshotWidthSummary()` — returns `_lastSnapshotResult?.Manifest?.Width.ToString() ?? "none"`.
   - `private string CreateSnapshotHeightSummary()` — returns `_lastSnapshotResult?.Manifest?.Height.ToString() ?? "none"`.
   - `private string CreateSnapshotFormatSummary()` — returns `_lastSnapshotResult?.Manifest?.Format.ToString() ?? "none"`.
   - `private string CreateSnapshotBackgroundSummary()` — returns `_lastSnapshotResult?.Manifest?.Background.ToString() ?? "none"`.
   - `private string CreateSnapshotOutputEvidenceKindSummary()` — returns `_lastSnapshotResult?.Manifest?.OutputEvidenceKind ?? "none"`.
   - `private string CreateSnapshotDatasetEvidenceKindSummary()` — returns `_lastSnapshotResult?.Manifest?.DatasetEvidenceKind ?? "none"`.
   - `private string CreateSnapshotActiveSeriesIdentitySummary()` — returns `_lastSnapshotResult?.Manifest?.ActiveSeriesIdentity ?? "none"`.
   - `private string CreateSnapshotCreatedUtcSummary()` — returns `_lastSnapshotResult?.Manifest?.CreatedUtc.ToString("O") ?? "none"`.

5. In the click handler, store `_lastSnapshotResult = result;` before updating status text.

6. Bounded scope check: The button calls CaptureSnapshotAsync with hardcoded 1920x1080, no configuration UI, no batch, no gallery. Single click → single capture → status update. This satisfies D-01 (bounded snapshot action) and D-04 (demo remains bounded).
  </action>
  <verify>
    <automated>dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj --no-restore 2>&1 | Select-String -Pattern "error|Build succeeded"</automated>
  </verify>
  <done>
    SurfaceCharts demo has a Capture Snapshot button that calls Plot.CaptureSnapshotAsync, stores the result, and includes 10 snapshot fields in the support summary text. Build succeeds with no errors.
  </done>
</task>

<task type="auto">
  <name>Task 2: Add snapshot evidence to WorkbenchSupportCapture and consumer smoke</name>
  <files>
    samples/Videra.AvaloniaWorkbenchSample/WorkbenchSupportCapture.cs,
    smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs
  </files>
  <read_first>
    samples/Videra.AvaloniaWorkbenchSample/WorkbenchSupportCapture.cs,
    smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs
  </read_first>
  <action>
**Part A: WorkbenchSupportCapture.cs**

1. Add a new record `WorkbenchSnapshotEvidence`:
   ```csharp
   internal sealed record WorkbenchSnapshotEvidence(
       string Status,
       string? Path,
       int? Width,
       int? Height,
       string? Format,
       string? Background,
       string? OutputEvidenceKind,
       string? DatasetEvidenceKind,
       string? ActiveSeriesIdentity,
       string? CreatedUtc);
   ```

2. Add `FormatSnapshotEvidence(WorkbenchSnapshotEvidence evidence)` method:
   ```csharp
   public static string FormatSnapshotEvidence(WorkbenchSnapshotEvidence evidence)
   {
       ArgumentNullException.ThrowIfNull(evidence);
       return string.Join(
           Environment.NewLine,
           $"SnapshotStatus: {evidence.Status}",
           $"SnapshotPath: {evidence.Path ?? "none"}",
           $"SnapshotWidth: {evidence.Width?.ToString(CultureInfo.InvariantCulture) ?? "none"}",
           $"SnapshotHeight: {evidence.Height?.ToString(CultureInfo.InvariantCulture) ?? "none"}",
           $"SnapshotFormat: {evidence.Format ?? "none"}",
           $"SnapshotBackground: {evidence.Background ?? "none"}",
           $"SnapshotOutputEvidenceKind: {evidence.OutputEvidenceKind ?? "none"}",
           $"SnapshotDatasetEvidenceKind: {evidence.DatasetEvidenceKind ?? "none"}",
           $"SnapshotActiveSeriesIdentity: {evidence.ActiveSeriesIdentity ?? "none"}",
           $"SnapshotCreatedUtc: {evidence.CreatedUtc ?? "none"}");
   }
   ```
   Add `using System.Globalization;` if not present.

3. Update `FormatSupportCapture` to accept an optional `WorkbenchSnapshotEvidence? snapshotEvidence` parameter (default null). After the chart evidence section, if snapshot evidence is not null, append:
   ```
   "SnapshotEvidence:",
   FormatSnapshotEvidence(snapshotEvidence)
   ```
   Keep backward compatibility — existing callers pass null and get no snapshot section.

**Part B: Consumer Smoke MainWindow.axaml.cs**

4. After `IsFirstChartReady()` returns true and before `CompleteAsync` is called, add snapshot capture:
   - In `TryCompleteWhenReadyAsync()`, after the readiness check, call `await CaptureSnapshotAsync()` and store the result.
   - Add a new method `CaptureSnapshotAsync()`:
     ```csharp
     private async Task<PlotSnapshotResult?> CaptureSnapshotAsync()
     {
         try
         {
             var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);
             return await _chartView.Plot.CaptureSnapshotAsync(request).ConfigureAwait(true);
         }
         catch (Exception ex)
         {
             Trace($"Snapshot capture failed: {ex.Message}");
             return null;
         }
     }
     ```
   - Add `using Videra.SurfaceCharts.Avalonia.Controls;` if not present.
   - Add field `private PlotSnapshotResult? _snapshotResult;`.

5. Update `CreateSupportSummary()` to include snapshot fields after `OutputCapabilityDiagnostics`:
   ```
   $"SnapshotStatus: {CreateSnapshotStatusSummary()}\n" +
   $"SnapshotPath: {_snapshotResult?.Path ?? "none"}\n" +
   $"SnapshotWidth: {_snapshotResult?.Manifest?.Width.ToString(CultureInfo.InvariantCulture) ?? "none"}\n" +
   $"SnapshotHeight: {_snapshotResult?.Manifest?.Height.ToString(CultureInfo.InvariantCulture) ?? "none"}\n" +
   $"SnapshotFormat: {_snapshotResult?.Manifest?.Format.ToString() ?? "none"}\n" +
   $"SnapshotBackground: {_snapshotResult?.Manifest?.Background.ToString() ?? "none"}\n" +
   $"SnapshotOutputEvidenceKind: {_snapshotResult?.Manifest?.OutputEvidenceKind ?? "none"}\n" +
   $"SnapshotDatasetEvidenceKind: {_snapshotResult?.Manifest?.DatasetEvidenceKind ?? "none"}\n" +
   $"SnapshotActiveSeriesIdentity: {_snapshotResult?.Manifest?.ActiveSeriesIdentity ?? "none"}\n" +
   $"SnapshotCreatedUtc: {_snapshotResult?.Manifest?.CreatedUtc.ToString("O") ?? "none"}\n"
   ```
   Add helper: `private string CreateSnapshotStatusSummary()` returning "present"/"failed"/"none".

6. In `CompleteAsync`, after calling `CreateSupportSummary()`, the snapshot fields will be included. Also update `CreateDiagnosticsSnapshot()` to add a line: `$"SnapshotStatus: {CreateSnapshotStatusSummary()}\n"`.

7. Wire snapshot capture: In `TryCompleteWhenReadyAsync`, before calling `CompleteAsync`, capture the snapshot:
   ```csharp
   _snapshotResult = await CaptureSnapshotAsync().ConfigureAwait(true);
   ```
  </action>
  <verify>
    <automated>dotnet build samples/Videra.AvaloniaWorkbenchSample/Videra.AvaloniaWorkbenchSample.csproj --no-restore 2>&1 | Select-String -Pattern "error|Build succeeded" ; dotnet build smoke/Videra.SurfaceCharts.ConsumerSmoke/Videra.SurfaceCharts.ConsumerSmoke.csproj --no-restore 2>&1 | Select-String -Pattern "error|Build succeeded"</automated>
  </verify>
  <done>
    WorkbenchSupportCapture has FormatSnapshotEvidence with 10 snapshot fields. Consumer smoke calls CaptureSnapshotAsync after chart readiness and includes snapshot fields in support summary. Both projects build successfully.
  </done>
</task>

<task type="auto">
  <name>Task 3: Add snapshot parsing to Doctor and update Doctor tests</name>
  <files>
    scripts/Invoke-VideraDoctor.ps1
  </files>
  <read_first>
    scripts/Invoke-VideraDoctor.ps1,
    tests/Videra.Core.Tests.Repository/VideraDoctorRepositoryTests.cs
  </read_first>
  <action>
1. In `Get-SurfaceChartsSupportReport` function, add snapshot field parsing after the existing `datasetActiveSeriesMetadata` parsing (around line 374). Add:
   ```powershell
   $snapshotStatus = Get-SurfaceChartsSupportValue -Prefix "SnapshotStatus:"
   $snapshotPath = Get-SurfaceChartsSupportValue -Prefix "SnapshotPath:"
   $snapshotWidth = Get-SurfaceChartsSupportValue -Prefix "SnapshotWidth:"
   $snapshotHeight = Get-SurfaceChartsSupportValue -Prefix "SnapshotHeight:"
   $snapshotFormat = Get-SurfaceChartsSupportValue -Prefix "SnapshotFormat:"
   $snapshotBackground = Get-SurfaceChartsSupportValue -Prefix "SnapshotBackground:"
   $snapshotOutputEvidenceKind = Get-SurfaceChartsSupportValue -Prefix "SnapshotOutputEvidenceKind:"
   $snapshotDatasetEvidenceKind = Get-SurfaceChartsSupportValue -Prefix "SnapshotDatasetEvidenceKind:"
   $snapshotActiveSeriesIdentity = Get-SurfaceChartsSupportValue -Prefix "SnapshotActiveSeriesIdentity:"
   $snapshotCreatedUtc = Get-SurfaceChartsSupportValue -Prefix "SnapshotCreatedUtc:"
   ```

2. Snapshot fields are OPTIONAL — do NOT add them to the `$missingFields` array. Snapshot fields being absent means no snapshot was captured (status: "none"), which is valid.

3. Add snapshot fields to the returned ordered hashtable in all three return paths (missing, unavailable, present):
   ```powershell
   snapshotStatus = $snapshotStatus
   snapshotPath = $snapshotPath
   snapshotWidth = $snapshotWidth
   snapshotHeight = $snapshotHeight
   snapshotFormat = $snapshotFormat
   snapshotBackground = $snapshotBackground
   snapshotOutputEvidenceKind = $snapshotOutputEvidenceKind
   snapshotDatasetEvidenceKind = $snapshotDatasetEvidenceKind
   snapshotActiveSeriesIdentity = $snapshotActiveSeriesIdentity
   snapshotCreatedUtc = $snapshotCreatedUtc
   ```
   For the "missing" and "unavailable" return paths, set all snapshot fields to empty string `""`.

4. In the `$report` evidence packet section, add a `snapshotArtifact` reference:
   ```powershell
   (New-EvidenceArtifact -Id "consumer-smoke-snapshot" -Category "consumer-smoke" -Path "artifacts/consumer-smoke/chart-snapshot.png" -ProducedBy "Videra.SurfaceCharts.ConsumerSmoke")
   ```

5. In the `$summaryLines` section, after the SurfaceCharts support report block, add snapshot status reporting:
   ```powershell
   $summaryLines.Add("") | Out-Null
   $summaryLines.Add("Chart snapshot evidence:") | Out-Null
   $summaryLines.Add(("- status: {0}" -f $surfaceChartsSupportReport.snapshotStatus)) | Out-Null
   if (-not [string]::IsNullOrWhiteSpace($surfaceChartsSupportReport.snapshotPath))
   {
       $summaryLines.Add(("- path: {0}" -f $surfaceChartsSupportReport.snapshotPath)) | Out-Null
   }
   if (-not [string]::IsNullOrWhiteSpace($surfaceChartsSupportReport.snapshotWidth) -and -not [string]::IsNullOrWhiteSpace($surfaceChartsSupportReport.snapshotHeight))
   {
       $summaryLines.Add(("- dimensions: {0}x{1}" -f $surfaceChartsSupportReport.snapshotWidth, $surfaceChartsSupportReport.snapshotHeight)) | Out-Null
   }
   ```

6. Doctor parsing logic for snapshot status:
   - `"present"` = snapshot captured successfully, manifest fields populated
   - `"failed"` = snapshot capture attempted but failed
   - `"none"` or empty = no snapshot captured (valid state — not all consumers capture snapshots)
   - No `"unavailable"` distinction needed for snapshot itself — that's a support summary status

7. Bounded scope check: Doctor parses snapshot STATUS and manifest fields from the support summary text. It does NOT parse the PNG binary, does NOT render charts, does NOT launch UI. This satisfies D-03 (Doctor parses without launching UI).
  </action>
  <verify>
    <automated>pwsh -NoProfile -Command "& { $script = Get-Content 'scripts/Invoke-VideraDoctor.ps1' -Raw; $hasSnapshotStatus = $script -match 'snapshotStatus'; $hasSnapshotPath = $script -match 'snapshotPath'; $hasSnapshotWidth = $script -match 'snapshotWidth'; Write-Host \"snapshotStatus: $hasSnapshotStatus, snapshotPath: $hasSnapshotPath, snapshotWidth: $hasSnapshotWidth\"; if ($hasSnapshotStatus -and $hasSnapshotPath -and $hasSnapshotWidth) { Write-Host 'PASS' } else { Write-Host 'FAIL'; exit 1 } }"</automated>
  </verify>
  <done>
    Doctor parses SnapshotStatus, SnapshotPath, SnapshotWidth, SnapshotHeight, SnapshotFormat, SnapshotBackground, SnapshotOutputEvidenceKind, SnapshotDatasetEvidenceKind, SnapshotActiveSeriesIdentity, and SnapshotCreatedUtc from support summary. Snapshot fields are optional (not in missingFields). Summary report includes chart snapshot evidence section. Doctor test assertions pass.
  </done>
</task>

</tasks>

<verification>
1. `dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj` succeeds
2. `dotnet build samples/Videra.AvaloniaWorkbenchSample/Videra.AvaloniaWorkbenchSample.csproj` succeeds
3. `dotnet build smoke/Videra.SurfaceCharts.ConsumerSmoke/Videra.SurfaceCharts.ConsumerSmoke.csproj` succeeds
4. Doctor script contains `snapshotStatus`, `snapshotPath`, `snapshotWidth` parsing
5. Support summary format includes `SnapshotStatus:` field
6. Demo has "Capture Snapshot" button wired to `CaptureSnapshotAsync`
</verification>

<success_criteria>
1. SurfaceCharts demo exposes CaptureSnapshot button calling Plot.CaptureSnapshotAsync with hardcoded 1920x1080
2. Demo support summary includes 10 Snapshot* fields (Status, Path, Width, Height, Format, Background, OutputEvidenceKind, DatasetEvidenceKind, ActiveSeriesIdentity, CreatedUtc)
3. Consumer smoke calls CaptureSnapshotAsync after chart readiness and includes snapshot fields in support summary
4. WorkbenchSupportCapture has FormatSnapshotEvidence method with 10 fields
5. Doctor parses all 10 snapshot fields from support summary (optional, not in missingFields)
6. Doctor summary includes chart snapshot evidence section
7. All modified projects build successfully
</success_criteria>

<output>
After completion, create `.planning/phases/364-demo-smoke-doctor-snapshot-evidence/364-01-SUMMARY.md`
</output>
</tasks>
