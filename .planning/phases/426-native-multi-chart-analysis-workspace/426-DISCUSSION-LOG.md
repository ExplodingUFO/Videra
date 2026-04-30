# Phase 426: Native Multi-Chart Analysis Workspace - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-30
**Phase:** 426-native-multi-chart-analysis-workspace
**Mode:** Autonomous smart discuss (auto-selected recommended defaults)

---

## Workspace State Model

| Option | Description | Selected |
|--------|-------------|----------|
| Host-owned record | Simple class tracking registered VideraChartView instances with metadata. Not a new Avalonia control. | ✓ |
| New Avalonia control | A WorkspaceControl that contains and manages chart controls. | |
| Attached property pattern | Attached properties on VideraChartView for workspace membership. | |

**Auto-selected:** Host-owned record — aligns with existing host-owned pattern (probe/selection/measurement are host-owned). Avoids creating a new control that could become a workbench shell.

---

## Per-Panel Metadata

| Option | Description | Selected |
|--------|-------------|----------|
| Minimal (id + label) | ChartId and Label only. | |
| Standard (id + label + kind + recipe) | ChartId, Label, ChartKind enum, optional RecipeContext. | ✓ |
| Rich (standard + dataset stats) | Standard plus point count, series count, memory usage. | |

**Auto-selected:** Standard — provides enough context for workspace evidence without pulling heavy runtime stats that belong in Phase 428 streaming evidence.

---

## Link Group Model

| Option | Description | Selected |
|--------|-------------|----------|
| Keep pairwise only | Don't extend LinkViewWith; workspace just tracks which pairs are linked. | |
| Group-based with policies | SurfaceChartLinkGroup owns a set of charts with CameraOnly/AxisOnly/FullViewState policies. | ✓ |
| Group-based, full state only | Group links all charts with full ViewState sync, no policy selection. | |

**Auto-selected:** Group-based with policies — extends the existing pairwise model naturally. Policies enable Phase 427 axis-only linking without a second refactor.

---

## Aggregate Status

| Option | Description | Selected |
|--------|-------------|----------|
| No aggregate | Consumers compose status themselves from per-chart records. | |
| Snapshot record | SurfaceChartWorkspaceStatus as a snapshot with chart count, readiness, link health. | ✓ |
| Live observable | Reactive status that fires events on change. | |

**Auto-selected:** Snapshot record — matches existing pattern (RenderingStatus is a snapshot). Simpler than observables, and consumers only need status at specific moments (evidence copy, diagnostics).

---

## Demo Integration

| Option | Description | Selected |
|--------|-------------|----------|
| New scenario in existing demo | Add AnalysisWorkspace scenario to the bounded scenario catalog. | ✓ |
| Separate demo app | A new workspace-specific demo project. | |
| Extend existing scenario | Add workspace features to an existing scenario like "first chart". | |

**Auto-selected:** New scenario — keeps scenario catalog bounded and separate per WORK-03. Avoids bloating existing scenarios.

---

## Claude's Discretion

- File organization (D-07): Library code in Workspace/ subfolder, tests in IntegrationTests/Workspace/, demo changes in existing demo structure. Standard project layout.
- Evidence format (D-05): Text-based snapshot following existing SurfaceDemoSupportSummary pattern. No new serialization format needed.

## Deferred Ideas

- Axis group facade — Phase 427
- Cross-chart probe/selection propagation — Phase 427
- Streaming workspace evidence — Phase 428
- Workspace cookbook recipes — Phase 429
- Workspace CI/release-readiness gates — Phase 430
