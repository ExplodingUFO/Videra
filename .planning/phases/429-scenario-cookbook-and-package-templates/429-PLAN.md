---
phase: 429-scenario-cookbook-and-package-templates
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - samples/Videra.SurfaceCharts.Demo/Recipes/multi-chart-analysis.md
  - samples/Videra.SurfaceCharts.Demo/Recipes/linked-interaction.md
  - samples/Videra.SurfaceCharts.Demo/Recipes/streaming-workspace.md
  - samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs
  - tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs
autonomous: true
requirements:
  - COOK-01
  - COOK-02
  - COOK-03
must_haves:
  truths:
    - "Cookbook covers multi-chart analysis, linked interaction, streaming, and support evidence workflows"
    - "Recipes follow existing format: title, scenario id, prerequisites, code snippets, expected output"
    - "Recipe snippets use native VideraChartView + Plot.Add.* route"
    - "No compatibility claims, old chart controls, or hidden fallback behavior"
    - "Tests verify recipe coverage and snippet correctness"
  artifacts:
    - path: "samples/Videra.SurfaceCharts.Demo/Recipes/multi-chart-analysis.md"
      provides: "Multi-chart analysis workspace recipe"
      contains: "SurfaceChartWorkspace"
    - path: "samples/Videra.SurfaceCharts.Demo/Recipes/linked-interaction.md"
      provides: "Linked interaction recipe"
      contains: "SurfaceChartLinkGroup"
    - path: "samples/Videra.SurfaceCharts.Demo/Recipes/streaming-workspace.md"
      provides: "Streaming workspace recipe"
      contains: "DataLogger3D"
    - path: "samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs"
      provides: "Extended recipe catalog with new workflow entries"
      contains: "Multi-chart"
    - path: "tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs"
      provides: "Recipe coverage tests"
      contains: "MultiChartAnalysis"
  key_links:
    - from: "CookbookRecipeCatalog.cs"
      to: "multi-chart-analysis.md"
      via: "catalog entry references scenario id"
      pattern: "AnalysisWorkspace"
    - from: "CookbookRecipeCatalog.cs"
      to: "linked-interaction.md"
      via: "catalog entry references scenario id"
      pattern: "LinkedInteraction"
---

<objective>
Add cookbook recipes for v2.64 workflows and update recipe coverage tests.

Purpose: Phases 426-428 implemented workspace, linked interaction, and streaming
features. Phase 429 documents these as copyable recipes in the existing cookbook
format and verifies coverage.

Output: Three new recipe files, updated catalog, and coverage tests.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/429-scenario-cookbook-and-package-templates/429-CONTEXT.md

<interfaces>
<!-- Key types the recipes reference. -->

From src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs:
```csharp
public sealed class SurfaceChartWorkspace : IDisposable
{
    public void Register(VideraChartView chart, SurfaceChartPanelInfo info);
    public void Unregister(string chartId);
    public void SetActiveChart(string chartId);
    public SurfaceChartWorkspaceStatus CaptureWorkspaceStatus();
    public string CreateWorkspaceEvidence();
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartLinkGroup.cs:
```csharp
public sealed class SurfaceChartLinkGroup : IDisposable
{
    public SurfaceChartLinkGroup(SurfaceChartLinkPolicy policy = SurfaceChartLinkPolicy.FullViewState);
    public SurfaceChartLinkPolicy Policy { get; }
    public IReadOnlyList<VideraChartView> Members { get; }
    public void Add(VideraChartView chart);
    public void Remove(VideraChartView chart);
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartInteractionPropagator.cs:
```csharp
public sealed class SurfaceChartInteractionPropagator : IDisposable
{
    public SurfaceChartInteractionPropagator(
        SurfaceChartLinkGroup linkGroup,
        bool propagateSelection = false,
        bool propagateProbe = false,
        bool propagateMeasurement = false);
    public bool PropagateProbe(VideraChartView sourceChart, Point screenPosition);
    public SurfaceChartLinkedInteractionState CaptureInteractionState();
}
```

From src/Videra.SurfaceCharts.Core/DataLogger3D.cs:
```csharp
public sealed class DataLogger3D
{
    public DataLogger3D(uint color, string? label = null, bool isSortedX = false,
        bool containsNaN = false, bool pickable = false, int? fifoCapacity = null);
    public void Append(ScatterColumnarData data);
    public void Replace(ScatterColumnarData data);
    public void UseLatestWindow(int pointCount);
    public DataLogger3DLiveViewEvidence CreateLiveViewEvidence();
}
```

Existing recipe format (from axes-and-linked-views.md):
- Title heading
- Description paragraph
- Code blocks with snippets
- Explanation paragraphs between blocks
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Create multi-chart analysis recipe</name>
  <files>
    samples/Videra.SurfaceCharts.Demo/Recipes/multi-chart-analysis.md,
    samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs
  </files>
  <read_first>
    samples/Videra.SurfaceCharts.Demo/Recipes/axes-and-linked-views.md,
    samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartPanelInfo.cs
  </read_first>
  <behavior>
    - Recipe covers workspace creation, chart registration, active chart selection, and evidence copy
    - Code snippets use SurfaceChartWorkspace, SurfaceChartPanelInfo, Plot.Add.*
    - Recipe explains workspace is host-owned, not a new control
    - Recipe shows how to capture workspace status and evidence
  </behavior>
  <action>
Create `samples/Videra.SurfaceCharts.Demo/Recipes/multi-chart-analysis.md`:

Structure:
```markdown
# Multi-Chart Analysis Workspace

Description of the workspace pattern — host-owned coordination layer over VideraChartView instances.

## Creating a Workspace

```csharp
var workspace = new SurfaceChartWorkspace();
```

## Registering Charts

```csharp
var chartA = new VideraChartView();
chartA.Plot.Add.Surface(data, "Surface A");

var chartB = new VideraChartView();
chartB.Plot.Add.Contour(contourData, "Contour B");

workspace.Register(chartA, new SurfaceChartPanelInfo
{
    ChartId = "surface-a",
    Label = "Surface A",
    ChartKind = Plot3DSeriesKind.Surface,
});
workspace.Register(chartB, new SurfaceChartPanelInfo
{
    ChartId = "contour-b",
    Label = "Contour B",
    ChartKind = Plot3DSeriesKind.Contour,
});
```

## Setting Active Chart

```csharp
workspace.SetActiveChart("surface-a");
var activeId = workspace.GetActiveChartId();
```

## Capturing Evidence

```csharp
var status = workspace.CaptureWorkspaceStatus();
var evidence = workspace.CreateWorkspaceEvidence();
// Copy evidence to clipboard for diagnostics
```

## Cleanup

```csharp
workspace.Dispose();
```

Add explanation paragraphs between sections. Emphasize:
- Workspace does NOT own chart lifecycle
- Charts are created/destroyed by host
- Evidence is bounded text, not a report generator
```

Modify `CookbookRecipeCatalog.cs` — add entry:
```csharp
new(
    "Multi-chart",
    "Analysis workspace",
    "Host-owned workspace coordinating multiple VideraChartView instances for comparison.",
    ScenarioId: SurfaceDemoScenarios.AnalysisWorkspaceId,
    ScatterScenarioId: null,
    Snippet: """
        var workspace = new SurfaceChartWorkspace();
        workspace.Register(chartA, new SurfaceChartPanelInfo
        {
            ChartId = "chart-a",
            Label = "Surface A",
            ChartKind = Plot3DSeriesKind.Surface,
        });
        workspace.Register(chartB, new SurfaceChartPanelInfo
        {
            ChartId = "chart-b",
            Label = "Contour B",
            ChartKind = Plot3DSeriesKind.Contour,
        });
        var evidence = workspace.CreateWorkspaceEvidence();
        """),
```
  </action>
  <verify>
    <automated>dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj --no-restore 2>&amp;1 | tail -5</automated>
  </verify>
  <acceptance_criteria>
    - test -f samples/Videra.SurfaceCharts.Demo/Recipes/multi-chart-analysis.md
    - grep -q "SurfaceChartWorkspace" samples/Videra.SurfaceCharts.Demo/Recipes/multi-chart-analysis.md
    - grep -q "Multi-chart" samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs
    - Demo builds cleanly
  </acceptance_criteria>
  <done>
    Multi-chart analysis recipe exists with workspace creation, registration, and evidence snippets.
    Catalog includes the new recipe entry.
  </done>
</task>

<task type="auto" tdd="true">
  <name>Task 2: Create linked interaction recipe</name>
  <files>
    samples/Videra.SurfaceCharts.Demo/Recipes/linked-interaction.md,
    samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs
  </files>
  <read_first>
    samples/Videra.SurfaceCharts.Demo/Recipes/axes-and-linked-views.md,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartLinkGroup.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartInteractionPropagator.cs
  </read_first>
  <behavior>
    - Recipe covers link group creation, policy selection, and propagation setup
    - Code snippets use SurfaceChartLinkGroup, SurfaceChartInteractionPropagator
    - Recipe explains opt-in propagation and host-owned state
    - Recipe shows CameraOnly and AxisOnly policies
  </behavior>
  <action>
Create `samples/Videra.SurfaceCharts.Demo/Recipes/linked-interaction.md`:

Structure:
```markdown
# Linked Interaction

Description of link groups and propagation — host-owned coordination across charts.

## Creating a Link Group

```csharp
using var linkGroup = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.FullViewState);
linkGroup.Add(chartA);
linkGroup.Add(chartB);
// Camera, data window, and display space sync across members
```

## Filtered Link Policies

```csharp
// CameraOnly: syncs camera position/rotation only
using var cameraLink = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.CameraOnly);
cameraLink.Add(chartA);
cameraLink.Add(chartB);

// AxisOnly: syncs data window/axis bounds only
using var axisLink = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.AxisOnly);
axisLink.Add(chartA);
axisLink.Add(chartB);
```

## Probe Propagation

```csharp
var propagator = new SurfaceChartInteractionPropagator(
    linkGroup,
    propagateProbe: true);

// Host calls when probe fires on a chart
propagator.PropagateProbe(sourceChart, screenPosition);
```

## Selection Propagation

```csharp
var propagator = new SurfaceChartInteractionPropagator(
    linkGroup,
    propagateSelection: true);
// SelectionReported events automatically propagate to linked charts
```

## Evidence

```csharp
var state = propagator.CaptureInteractionState();
// state.Policy, state.MemberCount, state.PropagateProbe, etc.
```

## Cleanup

```csharp
propagator.Dispose();
linkGroup.Dispose();
```

Add explanation paragraphs. Emphasize:
- Propagation is opt-in per link group
- Host owns all state — charts don't know about propagation
- Re-entrancy guard prevents infinite loops
```

Modify `CookbookRecipeCatalog.cs` — add entry:
```csharp
new(
    "Linked interaction",
    "Link group with probe propagation",
    "Host-owned link group synchronizing view state and propagating probes across charts.",
    ScenarioId: SurfaceDemoScenarios.LinkedInteractionId,
    ScatterScenarioId: null,
    Snippet: """
        using var linkGroup = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.FullViewState);
        linkGroup.Add(chartA);
        linkGroup.Add(chartB);

        var propagator = new SurfaceChartInteractionPropagator(
            linkGroup, propagateProbe: true);
        propagator.PropagateProbe(chartA, screenPosition);
        """),
```
  </action>
  <verify>
    <automated>dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj --no-restore 2>&amp;1 | tail -5</automated>
  </verify>
  <acceptance_criteria>
    - test -f samples/Videra.SurfaceCharts.Demo/Recipes/linked-interaction.md
    - grep -q "SurfaceChartLinkGroup" samples/Videra.SurfaceCharts.Demo/Recipes/linked-interaction.md
    - grep -q "Linked interaction" samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs
    - Demo builds cleanly
  </acceptance_criteria>
  <done>
    Linked interaction recipe exists with link group, policy, and propagation snippets.
    Catalog includes the new recipe entry.
  </done>
</task>

<task type="auto" tdd="true">
  <name>Task 3: Create streaming workspace recipe and update coverage tests</name>
  <files>
    samples/Videra.SurfaceCharts.Demo/Recipes/streaming-workspace.md,
    samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs,
    tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs
  </files>
  <read_first>
    samples/Videra.SurfaceCharts.Demo/Recipes/scatter-and-live-data.md,
    samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs,
    tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs,
    src/Videra.SurfaceCharts.Core/DataLogger3D.cs
  </read_first>
  <behavior>
    - Recipe covers streaming data with workspace tracking
    - Code snippets use DataLogger3D, ScatterColumnarSeries, workspace streaming status
    - Recipe explains evidence-only nature of streaming counters
    - Coverage tests verify all new recipes exist in catalog
  </behavior>
  <action>
Create `samples/Videra.SurfaceCharts.Demo/Recipes/streaming-workspace.md`:

Structure:
```markdown
# Streaming Workspace

Description of streaming data in a workspace context — multiple charts with different update modes.

## Live Scatter with FIFO

```csharp
var live = new DataLogger3D(0xFF2F80EDu, label: "Live scatter", fifoCapacity: 10_000);
live.Append(new ScatterColumnarData(xData, yData, zData));
live.UseLatestWindow(2_000);

var chart = new VideraChartView();
var scatterData = new ScatterChartData(
    new ScatterChartMetadata(
        new SurfaceAxisDescriptor("Time", "s", 0, 10),
        new SurfaceAxisDescriptor("Band", "Hz", 0, 10),
        new SurfaceValueRange(0, 1)),
    [],
    [live.Series]);
chart.Plot.Add.Scatter(scatterData, "Live scatter");
```

## Streaming Evidence

```csharp
var evidence = live.CreateLiveViewEvidence();
// evidence.LiveViewMode, evidence.RetainedCount, evidence.VisibleCount
// Evidence is runtime truth, not a benchmark threshold
```

## Workspace Streaming Status

```csharp
workspace.RegisterStreamingStatus("live-scatter", new SurfaceChartStreamingStatus
{
    UpdateMode = "Append",
    RetainedPointCount = live.Count,
    FifoCapacity = live.FifoCapacity,
    EvidenceOnly = true,
});

var workspaceEvidence = workspace.CreateWorkspaceEvidence();
// Includes Streaming section with per-chart details
```

## Evidence Boundary

Streaming evidence reports runtime truth: point counts, FIFO drops, update mode.
It does NOT claim benchmark thresholds, renderer-side window crop, or performance
guarantees. Use `scripts/Run-Benchmarks.ps1` for numeric measurements.
```

Modify `CookbookRecipeCatalog.cs` — add entry:
```csharp
new(
    "Streaming",
    "Streaming workspace with evidence",
    "Multiple charts with different streaming modes and workspace evidence tracking.",
    ScenarioId: SurfaceDemoScenarios.StreamingWorkspaceId,
    ScatterScenarioId: "scatter-fifo-trim-100k",
    Snippet: """
        var live = new DataLogger3D(0xFF2F80EDu, label: "Live", fifoCapacity: 10_000);
        live.Append(data);
        live.UseLatestWindow(2_000);

        workspace.RegisterStreamingStatus("live", new SurfaceChartStreamingStatus
        {
            UpdateMode = "Append",
            RetainedPointCount = live.Count,
            FifoCapacity = live.FifoCapacity,
            EvidenceOnly = true,
        });
        """),
```

Modify `SurfaceChartsCookbookCoverageMatrixTests.cs` — add assertions:
- `MultiChartAnalysis` recipe exists in catalog
- `LinkedInteraction` recipe exists in catalog
- `StreamingWorkspace` recipe exists in catalog
  </action>
  <verify>
    <automated>dotnet test tests/Videra.Core.Tests/ --filter "FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests" --no-restore 2>&amp;1 | tail -10</automated>
  </verify>
  <acceptance_criteria>
    - test -f samples/Videra.SurfaceCharts.Demo/Recipes/streaming-workspace.md
    - grep -q "DataLogger3D" samples/Videra.SurfaceCharts.Demo/Recipes/streaming-workspace.md
    - grep -q "Streaming" samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs
    - grep -q "MultiChartAnalysis" tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs
    - All cookbook coverage tests pass
  </acceptance_criteria>
  <done>
    Streaming workspace recipe exists with DataLogger3D and evidence snippets.
    All three new recipes are in the catalog.
    Coverage tests verify recipe existence.
  </done>
</task>

</tasks>

<threat_model>
## Trust Boundaries

| Boundary | Description |
|----------|-------------|
| docs→consumers | Recipe snippets are copyable code that consumers will use |

## STRIDE Threat Register

| Threat ID | Category | Component | Disposition | Mitigation Plan |
|-----------|----------|-----------|-------------|-----------------|
| T-429-01 | Tampering | Recipe snippets | accept | Snippets use only public API surface; cannot modify chart internals |
| T-429-02 | Information Disclosure | Evidence recipes | accept | Evidence contains only chart metadata, no sensitive data |
</threat_model>

<verification>
1. All three recipe files exist
2. CookbookRecipeCatalog includes all new entries
3. Coverage tests pass
4. Demo builds cleanly
</verification>

<success_criteria>
- Multi-chart analysis recipe covers workspace creation, registration, evidence
- Linked interaction recipe covers link groups, policies, propagation
- Streaming workspace recipe covers DataLogger3D, evidence, workspace tracking
- All recipes use native VideraChartView + Plot.Add.* route
- No compatibility claims or old chart controls
- Coverage tests verify all recipes exist
</success_criteria>

<output>
After completion, create `.planning/phases/429-scenario-cookbook-and-package-templates/429-SUMMARY.md`
</output>
