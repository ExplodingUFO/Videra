---
phase: 428-high-density-and-streaming-data-evidence
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartStreamingStatus.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs
  - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/SurfaceChartStreamingEvidenceTests.cs
  - samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs
  - samples/Videra.SurfaceCharts.Demo/Services/SurfaceChartWorkspaceService.cs
  - samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs
autonomous: true
requirements:
  - STREAM-01
  - STREAM-02
  - STREAM-03
must_haves:
  truths:
    - "Streaming status tracks per-chart update mode, retained point count, FIFO capacity, and dropped points"
    - "Workspace evidence includes streaming section with per-chart dataset scale and update mode"
    - "Evidence reports real scenario scope, dataset size, and explicit non-goals"
    - "Demo shows workspace with multiple charts using different streaming modes"
    - "Evidence does NOT claim benchmark thresholds or renderer-side window crop"
  artifacts:
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartStreamingStatus.cs"
      provides: "Per-chart streaming status record"
      contains: "record SurfaceChartStreamingStatus"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs"
      provides: "Workspace tracking streaming status per chart"
      contains: "StreamingStatus"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs"
      provides: "Extended evidence with streaming section"
      contains: "Streaming"
    - path: "tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/SurfaceChartStreamingEvidenceTests.cs"
      provides: "Streaming evidence format tests"
      contains: "SurfaceChartStreamingEvidenceTests"
    - path: "samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs"
      provides: "Streaming workspace demo scenario"
      contains: "StreamingWorkspace"
  key_links:
    - from: "SurfaceChartWorkspace.cs"
      to: "SurfaceChartStreamingStatus.cs"
      via: "stores streaming status per registered chart"
      pattern: "_streamingStatuses"
    - from: "SurfaceChartWorkspaceEvidence.cs"
      to: "SurfaceChartStreamingStatus.cs"
      via: "reads streaming status to format evidence text"
      pattern: "StreamingStatus"
---

<objective>
Add streaming and high-density evidence to the workspace.

Purpose: Phase 426-427 built workspace and linked interaction. Phase 428 adds
per-chart streaming status tracking, extends workspace evidence with streaming
details, and wires a demo scenario showing multiple charts with different
streaming modes. The evidence reports real scenario scope without benchmark
overclaims.

Output: SurfaceChartStreamingStatus record, workspace streaming tracking,
extended evidence format, streaming evidence tests, and demo streaming scenario.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/phases/428-high-density-and-streaming-data-evidence/428-CONTEXT.md

<interfaces>
<!-- Key types and contracts the executor needs. -->

From src/Videra.SurfaceCharts.Core/DataLogger3D.cs:
```csharp
public sealed class DataLogger3D
{
    public ScatterColumnarSeries Series { get; }
    public int Count { get; }
    public int? FifoCapacity { get; }
    public int AppendBatchCount { get; }
    public int ReplaceBatchCount { get; }
    public long TotalAppendedPointCount { get; }
    public long TotalDroppedPointCount { get; }
    public int LastDroppedPointCount { get; }
    public DataLogger3DLiveViewMode LiveViewMode { get; }
    public int? LatestWindowPointCount { get; }
    public DataLogger3DLiveViewEvidence CreateLiveViewEvidence();
}
```

From src/Videra.SurfaceCharts.Core/ScatterColumnarSeries.cs:
```csharp
public sealed class ScatterColumnarSeries
{
    public int Count { get; }
    public int? FifoCapacity { get; }
    public bool Pickable { get; }
    public int AppendBatchCount { get; }
    public int ReplaceBatchCount { get; }
    public long TotalAppendedPointCount { get; }
    public long TotalDroppedPointCount { get; }
    public int LastDroppedPointCount { get; }
}
```

From src/Videra.SurfaceCharts.Core/ScatterChartData.cs:
```csharp
public sealed class ScatterChartData
{
    public int ColumnarSeriesCount { get; }
    public int ColumnarPointCount { get; }
    public int PickablePointCount { get; }
    public int StreamingReplaceBatchCount { get; }
    public int StreamingAppendBatchCount { get; }
    public long StreamingDroppedPointCount { get; }
    public int LastStreamingDroppedPointCount { get; }
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs (after Phase 427):
```csharp
public sealed class SurfaceChartWorkspace : IDisposable
{
    public void Register(VideraChartView chart, SurfaceChartPanelInfo info);
    public void Unregister(string chartId);
    public SurfaceChartWorkspaceStatus CaptureWorkspaceStatus();
    public string CreateWorkspaceEvidence();
}
```

From samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingScenario.cs:
```csharp
public sealed record ScatterStreamingScenario(
    string Id, string DisplayName, ScatterStreamingUpdateMode UpdateMode,
    int InitialPointCount, int UpdatePointCount, int? FifoCapacity,
    bool Pickable, string Description);
```
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Create SurfaceChartStreamingStatus and extend workspace tracking</name>
  <files>
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartStreamingStatus.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs,
    tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/SurfaceChartStreamingEvidenceTests.cs
  </files>
  <read_first>
    src/Videra.SurfaceCharts.Core/DataLogger3D.cs,
    src/Videra.SurfaceCharts.Core/ScatterColumnarSeries.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartPanelInfo.cs
  </read_first>
  <behavior>
    - SurfaceChartStreamingStatus stores: UpdateMode (string), RetainedPointCount, FifoCapacity, PickablePointCount, AppendBatchCount, ReplaceBatchCount, DroppedFifoPointCount, LiveViewMode, EvidenceOnly flag
    - Workspace stores streaming status per chart id
    - RegisterStreamingStatus(chartId, status) adds/updates status
    - Unregister removes streaming status
    - CaptureWorkspaceStatus includes streaming status count
    - Workspace evidence includes streaming section
  </behavior>
  <action>
Create `SurfaceChartStreamingStatus.cs` in `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/`:
```csharp
namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Per-chart streaming and high-density data status for workspace evidence.
/// </summary>
public sealed record SurfaceChartStreamingStatus
{
    public required string UpdateMode { get; init; }
    public required int RetainedPointCount { get; init; }
    public int? FifoCapacity { get; init; }
    public int PickablePointCount { get; init; }
    public int AppendBatchCount { get; init; }
    public int ReplaceBatchCount { get; init; }
    public long DroppedFifoPointCount { get; init; }
    public string? LiveViewMode { get; init; }
    public required bool EvidenceOnly { get; init; }
}
```

Modify `SurfaceChartWorkspace.cs`:
1. Add field: `private readonly Dictionary<string, SurfaceChartStreamingStatus> _streamingStatuses = new();`
2. Add method: `public void RegisterStreamingStatus(string chartId, SurfaceChartStreamingStatus status)`
3. Add method: `public SurfaceChartStreamingStatus? GetStreamingStatus(string chartId)`
4. Update `Unregister` to also remove from `_streamingStatuses`
5. Update `Dispose` to clear `_streamingStatuses`

Create `SurfaceChartStreamingEvidenceTests.cs`:
- `RegisterStreamingStatus_stores_status` — register and retrieve
- `Unregister_removes_streaming_status` — register, unregister, verify null
- `GetStreamingStatus_returns_null_for_unknown` — no registration returns null
- `RegisterStreamingStatus_updates_existing` — re-register with new status
  </action>
  <verify>
    <automated>dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj --no-restore 2>&amp;1 | tail -5</automated>
  </verify>
  <acceptance_criteria>
    - grep -q "record SurfaceChartStreamingStatus" src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartStreamingStatus.cs
    - grep -q "RegisterStreamingStatus" src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs
    - grep -q "_streamingStatuses" src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs
    - All streaming evidence tests pass
  </acceptance_criteria>
  <done>
    SurfaceChartStreamingStatus record exists with per-chart streaming metrics.
    Workspace tracks streaming status per chart id.
    Tests verify registration, retrieval, update, and removal.
  </done>
</task>

<task type="auto" tdd="true">
  <name>Task 2: Extend workspace evidence with streaming section</name>
  <files>
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs
  </files>
  <read_first>
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceStatus.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartStreamingStatus.cs
  </read_first>
  <behavior>
    - Workspace evidence includes Streaming section when streaming statuses exist
    - Per-chart streaming evidence shows: chart id, update mode, retained points, FIFO capacity, dropped points
    - Evidence includes StreamingBoundary line: "Streaming evidence is runtime truth; benchmark thresholds are separate."
    - Evidence does NOT claim benchmark thresholds
    - Existing evidence format is preserved (backward compatible)
  </behavior>
  <action>
Modify `SurfaceChartWorkspaceEvidence.cs`:

Add an overload that accepts streaming statuses:
```csharp
public static string Create(
    SurfaceChartWorkspaceStatus status,
    string? activeRecipeContext,
    IReadOnlyList<SurfaceChartLinkedInteractionState>? linkedInteractionStates = null,
    IReadOnlyDictionary<string, SurfaceChartStreamingStatus>? streamingStatuses = null)
```

When `streamingStatuses` is not null and not empty, append:
```
StreamingChartCount: {count}
Streaming[{id}]: Mode={mode} | Retained={points} | FIFO={capacity} | Dropped={dropped}
Streaming[{id}]: ...
StreamingBoundary: Streaming evidence is runtime truth; benchmark thresholds are separate.
```

Update the existing overloads to pass null for streaming statuses (backward compatible).

Also update `SurfaceChartWorkspace.CreateWorkspaceEvidence()` to pass `_streamingStatuses` to the evidence formatter.
  </action>
  <verify>
    <automated>dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj --no-restore 2>&amp;1 | tail -5</automated>
  </verify>
  <acceptance_criteria>
    - grep -q "Streaming" src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs
    - grep -q "StreamingBoundary" src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs
    - grep -q "_streamingStatuses" src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs
    - Project builds cleanly
  </acceptance_criteria>
  <done>
    Workspace evidence includes streaming section with per-chart details.
    Evidence reports real scenario scope without benchmark overclaims.
    StreamingBoundary line explicitly separates runtime truth from benchmark thresholds.
  </done>
</task>

<task type="auto" tdd="true">
  <name>Task 3: Wire demo streaming workspace scenario</name>
  <files>
    samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs,
    samples/Videra.SurfaceCharts.Demo/Services/SurfaceChartWorkspaceService.cs,
    samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs
  </files>
  <read_first>
    samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs,
    samples/Videra.SurfaceCharts.Demo/Services/SurfaceChartWorkspaceService.cs,
    samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs,
    samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingScenario.cs,
    samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingEvidence.cs
  </read_first>
  <behavior>
    - Demo adds StreamingWorkspace scenario id
    - Scenario creates two charts: one with replace mode, one with append+FIFO mode
    - Both charts load 100k scatter data
    - Workspace tracks streaming status for each chart
    - Toolbar shows streaming info per chart
    - "Copy streaming evidence" button produces evidence with streaming section
  </behavior>
  <action>
Modify `SurfaceDemoScenario.cs`:
- Add `StreamingWorkspace` to the enum

Modify `MainWindow.axaml.cs`:
- Add `SetupStreamingWorkspaceScenario()` method:
  1. Create two `VideraChartView` instances
  2. Create `DataLogger3D` for chart A with replace mode (100k points)
  3. Create `DataLogger3D` for chart B with append+FIFO mode (100k points, FIFO=100k)
  4. Load data into charts via `Plot.Add.Scatter()`
  5. Register both charts in workspace
  6. Register streaming status for each chart
  7. Add toolbar showing per-chart streaming info (mode, point count, FIFO)
  8. Add "Copy streaming evidence" button
- Wire into scenario switch

Follow the pattern from `SetupAnalysisWorkspaceScenario()` and `SetupLinkedInteractionScenario()`.
  </action>
  <verify>
    <automated>dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj --no-restore 2>&amp;1 | tail -5</automated>
  </verify>
  <acceptance_criteria>
    - grep -q "StreamingWorkspace" samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs
    - grep -q "StreamingWorkspace" samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs
    - grep -q "RegisterStreamingStatus" samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs
    - Demo project builds successfully
  </acceptance_criteria>
  <done>
    Demo has a StreamingWorkspace scenario with two charts using different streaming modes.
    Workspace tracks streaming status per chart.
    Evidence includes streaming section with per-chart details.
    Demo builds cleanly.
  </done>
</task>

<task type="checkpoint:human-verify" gate="blocking">
  <name>Task 4: Verify streaming workspace demo scenario</name>
  <what-built>
    A new "Streaming Workspace" demo scenario with two SurfaceCharts using different streaming modes
    (replace and append+FIFO), workspace tracking, and streaming evidence copy.
  </what-built>
  <how-to-verify>
    1. Run the demo: `dotnet run --project samples/Videra.SurfaceCharts.Demo/`
    2. Select the "Streaming Workspace" scenario
    3. Verify two charts appear with scatter data loaded
    4. Click the "Copy streaming evidence" button
    5. Paste into a text editor — verify the evidence text includes:
       - StreamingChartCount line
       - Streaming[{id}] with Mode, Retained, FIFO, Dropped
       - StreamingBoundary line
  </how-to-verify>
  <resume-signal>Type "approved" or describe issues to fix</resume-signal>
</task>

</tasks>

<threat_model>
## Trust Boundaries

| Boundary | Description |
|----------|-------------|
| workspace→evidence | Streaming status data flows from workspace into evidence text |

## STRIDE Threat Register

| Threat ID | Category | Component | Disposition | Mitigation Plan |
|-----------|----------|-----------|-------------|-----------------|
| T-428-01 | Information Disclosure | Evidence text | accept | Evidence contains only chart metadata and streaming counters, no sensitive data |
| T-428-02 | Elevation of Privilege | Streaming status | accept | Status is read-only evidence; cannot modify chart behavior |
</threat_model>

<verification>
1. `dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj` succeeds
2. `dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj` succeeds
3. `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/ --filter "FullyQualifiedName~Workspace"` — all workspace tests pass
4. Evidence text includes streaming section with per-chart details
5. StreamingBoundary line explicitly separates runtime truth from benchmark thresholds
</verification>

<success_criteria>
- SurfaceChartStreamingStatus record stores per-chart streaming metrics
- Workspace tracks streaming status per chart
- Evidence includes streaming section with real scenario scope
- Demo StreamingWorkspace scenario works end-to-end
- Evidence does NOT claim benchmark thresholds
- All tests pass
</success_criteria>

<output>
After completion, create `.planning/phases/428-high-density-and-streaming-data-evidence/428-SUMMARY.md`
</output>
