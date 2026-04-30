# Phase 426: Native Multi-Chart Analysis Workspace - Research

**Researched:** 2026-04-30
**Domain:** .NET/Avalonia multi-chart workspace state management, aggregate status, and demo wiring
**Confidence:** HIGH

## Summary

Phase 426 adds bounded multi-chart analysis workspace affordances to Videra SurfaceCharts. The workspace is a thin coordination layer over existing `VideraChartView` instances — NOT a new Avalonia control, generic workbench shell, or plugin host. The implementation follows three parallel child beads: 426A (workspace contracts and aggregate evidence), 426B (demo multi-chart layout), and 426C (docs and contract tests).

The existing codebase provides strong foundations: `VideraChartView` is the single shipped chart control with `Plot.Add.*` for all chart families (surface, waterfall, scatter, bar, contour), pairwise `LinkViewWith` for two-chart state synchronization, per-chart rendering status records, and `SurfaceDemoSupportSummary` for text-based support evidence. Phase 426 extends these patterns to N-chart workspace management without introducing compatibility adapters, hidden fallback behavior, or broad workbench scope.

The Beads child bead breakdown already identifies the correct dependency chain: contracts first (426A), then demo wiring (426B), then docs/tests (426C). This research confirms that ordering and provides implementation detail for each child.

**Primary recommendation:** Implement `SurfaceChartWorkspace` as a host-owned record that tracks registered `VideraChartView` instances with `SurfaceChartPanelInfo` metadata, extends pairwise `LinkViewWith` into `SurfaceChartLinkGroup`, and exposes `SurfaceChartWorkspaceStatus` as a snapshot record with a text-based workspace evidence formatter.

## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** Workspace is a host-owned record, NOT a new Avalonia control. Tracks registered `VideraChartView` instances with metadata. Does NOT own chart lifecycle.
- **D-02:** Each registered chart carries `SurfaceChartPanelInfo` with `ChartId`, `Label`, `ChartKind`, and optional `RecipeContext`.
- **D-03:** `SurfaceChartLinkGroup` extends pairwise `LinkViewWith` into group-based model. Link policies: `CameraOnly`, `AxisOnly`, `FullViewState` (default). Disposable. Chart can be in at most one group.
- **D-04:** `SurfaceChartWorkspaceStatus` is a snapshot record with chart count, per-chart rendering status, link group count/health, active chart identity, per-chart dataset scale, overall readiness.
- **D-05:** Workspace evidence is a bounded text record (like `SurfaceDemoSupportSummary`) describing active panel, per-chart status, link group membership, dataset scale, timestamp.
- **D-06:** Demo adds `SurfaceDemoScenario.AnalysisWorkspace` with 2-4 chart instances in a grid, toolbar, and "Copy workspace evidence" button. Fixed layout per scenario.
- **D-07:** New library code in `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/`. Tests in `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/`. Demo changes in `samples/Videra.SurfaceCharts.Demo/`.

### Claude's Discretion

None explicitly marked — all decisions are locked.

### Deferred Ideas (OUT OF SCOPE)

- Axis group facade (applying axis limits across multiple charts) — Phase 427
- Cross-chart probe/selection propagation — Phase 427
- Streaming workspace evidence — Phase 428
- Workspace cookbook recipes — Phase 429
- Workspace CI/release-readiness gates — Phase 430

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| WORK-01 | Users can compose a bounded multi-chart analysis layout from native SurfaceCharts panels without introducing a generic workbench shell. | `SurfaceChartWorkspace` registry + `SurfaceChartPanelInfo` metadata + demo grid layout with `VideraChartView` instances. |
| WORK-02 | Users can inspect active panel identity, chart kind, recipe context, dataset scale, and rendering status through explicit workspace evidence. | `SurfaceChartWorkspaceStatus` snapshot + workspace evidence text formatter + per-chart `Plot.CreateDatasetEvidence()` and `Plot.CreateOutputEvidence()`. |
| WORK-03 | Demo/sample code keeps layout, scenario catalog, and support summary responsibilities separated rather than accumulating god-code in a window code-behind. | Separated services: `SurfaceChartWorkspaceService` for workspace state, `SurfaceDemoScenario` for scenario catalog, `SurfaceDemoSupportSummary` for support text. |

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Chart registration & panel metadata | Library (Workspace/) | — | Library owns workspace state contracts; host registers charts |
| Active chart tracking | Library (Workspace/) | — | Library tracks which chart is focused; host decides when to change focus |
| Link group management | Library (Workspace/) | — | Library owns group lifecycle; extends existing pairwise `LinkViewWith` |
| Aggregate status snapshot | Library (Workspace/) | — | Library composes per-chart status into workspace-level snapshot |
| Workspace evidence text | Library (Workspace/) | — | Library provides bounded text formatter; host copies to clipboard |
| Multi-chart demo layout | Demo (Views/) | — | Demo owns AXAML grid layout and toolbar |
| Scenario catalog | Demo (Services/) | — | Demo owns scenario vocabulary and recipe registration |
| Contract tests | Tests (IntegrationTests/) | — | Tests verify workspace contracts without demo coupling |

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| VideraChartView | current | Single shipped chart control | Already owns Plot, rendering status, interaction, overlay |
| Plot3D.Add.* | current | Canonical data-loading path | Surface, waterfall, scatter, bar, contour series authoring |
| VideraChartViewLink | current | Pairwise view state sync | Disposable link pattern, extends to group model |
| SurfaceChartRenderingStatus | current | Per-chart readiness | IsReady, IsFallback, ActiveBackend, tile counts |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Plot3D.CreateDatasetEvidence() | current | Per-chart dataset snapshot | Workspace status needs per-chart series count, point count |
| Plot3D.CreateOutputEvidence() | current | Per-chart output evidence | Workspace evidence formatter |
| SurfaceDemoSupportSummary | current | Text-based support pattern | Model for workspace evidence text format |
| Avalonia Headless | 11.3.10 | Headless UI testing | Integration tests for workspace contracts |
| xUnit | 2.9.3 | Test framework | All test infrastructure |
| FluentAssertions | 8.9.0 | Assertion library | All test infrastructure |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Host-owned workspace record | New Avalonia control | Would couple workspace to UI lifecycle; host-owned is simpler |
| Snapshot status record | Live observable (IObservable) | Snapshot is deterministic for evidence; observable adds complexity |
| Per-chart status composition | Single aggregated event | Per-chart composition allows targeted evidence; aggregation loses detail |

## Architecture Patterns

### System Architecture Diagram

```
Consumer App / Demo
       |
       v
+-------------------+
| SurfaceChartWorkspace |  <-- host-owned record
|   Register(chart, info) |
|   SetActiveChart(id)   |
|   CaptureWorkspaceStatus() |
|   CreateWorkspaceEvidence() |
+-------------------+
       |         |         |
       v         v         v
+----------+ +----------+ +----------+
| Chart A  | | Chart B  | | Chart C  |
| Videra   | | Videra   | | Videra   |
| ChartView| | ChartView| | ChartView|
+----------+ +----------+ +----------+
       |         |         |
       v         v         v
+----------+ +----------+ +----------+
| Plot.Add | | Plot.Add | | Plot.Add |
| .Surface | | .Bar     | | .Scatter |
+----------+ +----------+ +----------+
       |         |         |
       v         v         v
+-----------------------------------+
| SurfaceChartLinkGroup             |
|   Add(chart) / Remove(chart)      |
|   LinkPolicy: CameraOnly|AxisOnly|
|                |FullViewState     |
|   Dispose() -> unlink all         |
+-----------------------------------+
```

### Recommended Project Structure

```
src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/
    SurfaceChartPanelInfo.cs          # ChartId, Label, ChartKind, RecipeContext
    SurfaceChartWorkspace.cs          # Registration, active chart, enumeration, dispose
    SurfaceChartLinkGroup.cs          # N-chart link group with policy
    SurfaceChartWorkspaceStatus.cs    # Aggregate snapshot record
    SurfaceChartWorkspaceEvidence.cs  # Bounded text formatter

tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/
    SurfaceChartWorkspaceTests.cs     # Registration, active chart, status, evidence
    SurfaceChartLinkGroupTests.cs     # Group lifecycle, policy, disposal

samples/Videra.SurfaceCharts.Demo/Services/
    SurfaceChartWorkspaceService.cs   # Demo-owned workspace state helper
    (SurfaceDemoScenario.cs)          # Add AnalysisWorkspace scenario id
    (CookbookRecipeCatalog.cs)        # Add workspace recipe entry
```

### Pattern 1: Host-Owned Workspace Record

**What:** `SurfaceChartWorkspace` is a plain class (not an Avalonia control) that tracks registered chart views and their metadata. The host creates it, registers charts, and queries status.

**When to use:** Any consumer that needs to coordinate multiple `VideraChartView` instances for comparison or analysis.

**Example:**
```csharp
// Source: Pattern from VideraChartView.LinkedViews.cs extended to N charts
var workspace = new SurfaceChartWorkspace();

var chartA = new VideraChartView();
var chartB = new VideraChartView();
var chartC = new VideraChartView();

workspace.Register(chartA, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));
workspace.Register(chartB, new SurfaceChartPanelInfo("chart-b", "Bar B", Plot3DSeriesKind.Bar));
workspace.Register(chartC, new SurfaceChartPanelInfo("chart-c", "Scatter C", Plot3DSeriesKind.Scatter));

workspace.SetActiveChart("chart-a");

var status = workspace.CaptureWorkspaceStatus();
var evidence = workspace.CreateWorkspaceEvidence();
```

### Pattern 2: Link Group with Policy

**What:** `SurfaceChartLinkGroup` owns a set of linked `VideraChartView` instances with a configurable link policy. Disposing the group unlinks all members.

**When to use:** When the host wants to synchronize camera, axis, or view-state across 2+ charts.

**Example:**
```csharp
// Source: Pattern from VideraChartViewLink extended to N charts with policy
using var linkGroup = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.FullViewState);
linkGroup.Add(chartA);
linkGroup.Add(chartB);
linkGroup.Add(chartC);

// All three charts now share ViewState changes.
// Dispose unlinks all members.
```

### Pattern 3: Aggregate Status Snapshot

**What:** `SurfaceChartWorkspaceStatus` is a snapshot record composed from per-chart rendering status, dataset evidence, and link group state.

**When to use:** When the host needs a point-in-time view of workspace health for diagnostics or UI display.

**Example:**
```csharp
// Source: Pattern from SurfaceChartRenderingStatus composition
var status = workspace.CaptureWorkspaceStatus();
// status.ChartCount: 3
// status.ActiveChartId: "chart-a"
// status.Panels: [ { Id, ChartKind, IsReady, SeriesCount, PointCount }, ... ]
// status.LinkGroupCount: 1
// status.AllReady: true
```

### Anti-Patterns to Avoid

- **God-code in MainWindow:** Do not accumulate workspace logic in the demo window code-behind. Use `SurfaceChartWorkspaceService` or direct workspace API.
- **New Avalonia control for workspace:** The workspace is a host-owned record, not a control. Do not create `WorkspaceView` or `WorkspacePanel`.
- **Compatibility wrappers:** Do not wrap old chart controls or add adapter layers. `VideraChartView` is the single shipped control.
- **Live observables for status:** Use snapshot records, not reactive streams. Consumers call `CaptureWorkspaceStatus()` when needed.
- **Hidden fallback/downshift:** Unsupported output should remain explicit diagnostics, not silent fallback.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Chart registration tracking | Custom dictionary + lock | `SurfaceChartWorkspace` record | Standard pattern from existing codebase; disposable lifecycle |
| N-chart view sync | N-way PropertyChanged loop | `SurfaceChartLinkGroup` over `VideraChartViewLink` pattern | Extends proven pairwise pattern; handles re-entrancy |
| Per-chart status composition | Manual field copying | `Plot.CreateDatasetEvidence()` + `Plot.CreateOutputEvidence()` | Already returns structured evidence; no need to duplicate |
| Workspace evidence text | String concatenation in demo | `SurfaceChartWorkspaceEvidence` formatter | Keeps evidence format bounded and library-owned |
| Chart kind identification | String matching | `Plot3DSeriesKind` enum | Already exists with Surface, Waterfall, Scatter, Bar, Contour |

**Key insight:** The existing `VideraChartView` + `Plot3D` + `SurfaceChartRenderingStatus` already provide all per-chart data needed. The workspace layer only adds registration, active tracking, group linking, and evidence composition — NOT new data paths or chart capabilities.

## Common Pitfalls

### Pitfall 1: Link Group Re-Entrancy
**What goes wrong:** When syncing ViewState across N charts, a change on chart B triggers the sync, which changes chart A, which triggers again — infinite loop.
**Why it happens:** Each chart's PropertyChanged handler fires when ViewState is set programmatically.
**How to avoid:** Use a `_isSynchronizing` guard flag (same pattern as `VideraChartViewLink`). Set flag before propagating, clear after.
**Warning signs:** Stack overflow or UI freeze during linked view interaction.

### Pitfall 2: Workspace Owns Chart Lifecycle
**What goes wrong:** Workspace disposes charts when the workspace is disposed, or tracks chart creation.
**Why it happens:** Temptation to make workspace "smart" about chart management.
**How to avoid:** Workspace ONLY tracks registration. Host creates and destroys charts. Workspace.Dispose() clears references but does NOT dispose charts.
**Warning signs:** Charts disappearing when workspace is disposed, or charts not being garbage collected.

### Pitfall 3: Aggregate Status Treats All Charts Equally
**What goes wrong:** Workspace status reports "all ready" when surface charts are ready but scatter charts have different readiness semantics.
**Why it happens:** Different chart kinds have different rendering status record types.
**How to avoid:** Each chart's panel info should include the chart kind. Status composition should use kind-appropriate status projection (SurfaceChartRenderingStatus for surface/waterfall, ScatterChartRenderingStatus for scatter, etc.).
**Warning signs:** False "ready" status when a chart has no data loaded.

### Pitfall 4: Demo Accumulates Workspace Logic
**What goes wrong:** MainWindow.axaml.cs grows to handle workspace registration, status display, evidence copying, and link group management.
**Why it happens:** Quick path to get things working in the demo.
**How to avoid:** Extract workspace logic to `SurfaceChartWorkspaceService` or use workspace API directly. MainWindow should only wire UI events to workspace methods.
**Warning signs:** MainWindow exceeding 500 lines, or having workspace-specific private fields.

## Code Examples

### Workspace Registration and Status

```csharp
// Source: Extension of VideraChartView.LinkedViews.cs registration pattern
public sealed class SurfaceChartWorkspace : IDisposable
{
    private readonly Dictionary<string, (VideraChartView Chart, SurfaceChartPanelInfo Info)> _panels = new();
    private string? _activeChartId;
    private bool _disposed;

    public void Register(VideraChartView chart, SurfaceChartPanelInfo info)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(info);
        _panels[info.ChartId] = (chart, info);
        _activeChartId ??= info.ChartId;
    }

    public void Unregister(string chartId)
    {
        _panels.Remove(chartId);
        if (_activeChartId == chartId)
            _activeChartId = _panels.Keys.FirstOrDefault();
    }

    public void SetActiveChart(string chartId)
    {
        if (!_panels.ContainsKey(chartId))
            throw new InvalidOperationException($"Chart '{chartId}' is not registered.");
        _activeChartId = chartId;
    }

    public SurfaceChartWorkspaceStatus CaptureWorkspaceStatus() { /* ... */ }
    public string CreateWorkspaceEvidence() { /* ... */ }

    public void Dispose()
    {
        if (_disposed) return;
        _panels.Clear();
        _activeChartId = null;
        _disposed = true;
    }
}
```

### Panel Info Record

```csharp
// Source: D-02 locked decision
public sealed record SurfaceChartPanelInfo(
    string ChartId,
    string Label,
    Plot3DSeriesKind ChartKind,
    string? RecipeContext = null);
```

### Link Group with Policy

```csharp
// Source: Extension of VideraChartViewLink pattern from LinkedViews.cs
public enum SurfaceChartLinkPolicy
{
    FullViewState,
    CameraOnly,
    AxisOnly,
}

public sealed class SurfaceChartLinkGroup : IDisposable
{
    private readonly List<VideraChartView> _members = [];
    private readonly List<IDisposable> _pairwiseLinks = [];
    private readonly SurfaceChartLinkPolicy _policy;
    private bool _disposed;

    public SurfaceChartLinkGroup(SurfaceChartLinkPolicy policy = SurfaceChartLinkPolicy.FullViewState)
    {
        _policy = policy;
    }

    public void Add(VideraChartView chart)
    {
        // Link to all existing members via pairwise links
        foreach (var existing in _members)
        {
            _pairwiseLinks.Add(existing.LinkViewWith(chart));
        }
        _members.Add(chart);
    }

    public void Dispose()
    {
        if (_disposed) return;
        foreach (var link in _pairwiseLinks) link.Dispose();
        _pairwiseLinks.Clear();
        _members.Clear();
        _disposed = true;
    }
}
```

### Workspace Status Snapshot

```csharp
// Source: Extension of SurfaceChartRenderingStatus composition pattern
public sealed record SurfaceChartWorkspaceStatus(
    int ChartCount,
    string? ActiveChartId,
    IReadOnlyList<SurfaceChartPanelStatus> Panels,
    int LinkGroupCount,
    bool AllReady);

public sealed record SurfaceChartPanelStatus(
    string ChartId,
    string Label,
    Plot3DSeriesKind ChartKind,
    bool IsReady,
    int SeriesCount,
    long PointCount);
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Pairwise `LinkViewWith` only | N-chart `SurfaceChartLinkGroup` | Phase 426 (new) | Enables multi-chart analysis without pairwise explosion |
| No workspace state | `SurfaceChartWorkspace` record | Phase 426 (new) | Host can track and inspect multi-chart layouts |
| Per-chart support summary only | Workspace-level evidence | Phase 426 (new) | Single text block describes entire workspace state |

**Deprecated/outdated:**
- None — Phase 426 is additive. Existing pairwise `LinkViewWith` remains for backward compatibility.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `Plot3DSeriesKind` enum values (Surface, Waterfall, Scatter, Bar, Contour) are sufficient for `SurfaceChartPanelInfo.ChartKind` | Pattern 1 | May need additional kind values if new chart families are added in v2.64 |
| A2 | Pairwise `LinkViewWith` can be composed into N-chart groups by creating N*(N-1)/2 pairwise links | Pattern 2 | If link sync has ordering issues, group behavior may be unpredictable |
| A3 | `Plot.CreateDatasetEvidence()` and `Plot.CreateOutputEvidence()` are safe to call from any thread | Pattern 3 | If they access UI-bound state, workspace status capture may need dispatcher marshaling |
| A4 | The demo MainWindow already has enough named chart views (ChartView, WaterfallPlotView, ScatterPlotView, BarChartPlotView, ContourPlotView) to compose a 4-chart analysis layout | D-06 | May need additional chart views in AXAML if 5 are needed |

## Open Questions

1. **Thread safety of workspace status capture**
   - What we know: `CaptureWorkspaceStatus()` reads per-chart rendering status and plot evidence.
   - What's unclear: Whether these reads are safe from a background thread or require UI thread dispatch.
   - Recommendation: Document that `CaptureWorkspaceStatus()` should be called from the UI thread, same as chart property access. If background capture is needed later, add dispatcher marshaling.

2. **Link group policy enforcement**
   - What we know: `LinkViewWith` always copies full `ViewState`. D-03 specifies `CameraOnly` and `AxisOnly` policies.
   - What's unclear: Whether `CameraOnly` and `AxisOnly` can be implemented by filtering which `ViewState` fields are synced, or require new link infrastructure.
   - Recommendation: For Phase 426, implement `FullViewState` only (matches existing behavior). Add `CameraOnly`/`AxisOnly` as stubs that throw `NotSupportedException` with a clear message pointing to Phase 427. This keeps Phase 426 bounded.

3. **Maximum panel count**
   - What we know: D-06 specifies 2-4 charts in the demo layout.
   - What's unclear: Whether the workspace should enforce a maximum panel count or leave it unbounded.
   - Recommendation: Do not enforce a limit in the library. The demo uses 4 charts. Performance constraints are a Phase 428 concern.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET 8 SDK | Build | Yes | net8.0 target | — |
| Avalonia | UI framework | Yes | 11.3.10 (test package) | — |
| xUnit | Testing | Yes | 2.9.3 | — |
| FluentAssertions | Testing | Yes | 8.9.0 | — |
| Avalonia.Headless | Integration tests | Yes | 11.3.10 | — |

**Missing dependencies with no fallback:** None — all required dependencies are available.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 + FluentAssertions 8.9.0 |
| Config file | `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj` |
| Quick run command | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/ --filter "FullyQualifiedName~Workspace"` |
| Full suite command | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/` |

### Phase Requirements to Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| WORK-01 | Register charts, enumerate, set active | unit | `dotnet test ... --filter "FullyQualifiedName~SurfaceChartWorkspaceTests"` | No — Wave 0 |
| WORK-02 | CaptureWorkspaceStatus returns correct panel data | unit | `dotnet test ... --filter "FullyQualifiedName~SurfaceChartWorkspaceTests"` | No — Wave 0 |
| WORK-02 | CreateWorkspaceEvidence returns bounded text | unit | `dotnet test ... --filter "FullyQualifiedName~SurfaceChartWorkspaceTests"` | No — Wave 0 |
| WORK-03 | Demo scenario wires workspace without god-code | integration | `dotnet test ... --filter "FullyQualifiedName~SurfaceChartWorkspaceTests"` | No — Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/ --filter "FullyQualifiedName~Workspace"`
- **Per wave merge:** `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/`
- **Phase gate:** Full integration test suite green before verify-work

### Wave 0 Gaps
- [ ] `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/SurfaceChartWorkspaceTests.cs` — covers WORK-01, WORK-02
- [ ] `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/SurfaceChartLinkGroupTests.cs` — covers D-03 link group lifecycle
- [ ] `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs` — workspace state record
- [ ] `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartPanelInfo.cs` — panel info record
- [ ] `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartLinkGroup.cs` — link group
- [ ] `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceStatus.cs` — aggregate status
- [ ] `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs` — evidence formatter

## Sources

### Primary (HIGH confidence)
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.LinkedViews.cs` — existing pairwise link API, disposable pattern, re-entrancy guard
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` — chart control shell, rendering status, Plot ownership
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs` — Plot model, series, evidence hooks (CreateDatasetEvidence, CreateOutputEvidence)
- `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs` — text-based support evidence pattern
- `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs` — scenario catalog pattern
- `samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs` — recipe registration pattern
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderingStatus.cs` — per-chart status record pattern
- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425A-API-WORKSPACE-INVENTORY.md` — API seams and gap analysis
- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425B-DEMO-COOKBOOK-TEMPLATE-INVENTORY.md` — demo/cookbook gaps
- `.planning/phases/426-native-multi-chart-analysis-workspace/426-CONTEXT.md` — locked decisions D-01 through D-07

### Secondary (MEDIUM confidence)
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/AvaloniaHeadlessTestSession.cs` — headless test session pattern for workspace tests

### Tertiary (LOW confidence)
- None — all findings are based on direct codebase inspection.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all components are existing, verified in codebase
- Architecture: HIGH — patterns directly extend existing `VideraChartViewLink` and `SurfaceChartRenderingStatus`
- Pitfalls: HIGH — re-entrancy and lifecycle issues are well-understood from existing pairwise link code

**Research date:** 2026-04-30
**Valid until:** 2026-05-14 (stable — depends on existing APIs that are not changing)
