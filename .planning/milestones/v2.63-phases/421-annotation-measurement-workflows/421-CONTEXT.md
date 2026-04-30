# Phase 421 Context: Annotation and Measurement Workflows

## Beads

- Parent: `Videra-b5n`
- Entry bead: `Videra-b5n.1` - annotation anchor DTOs
- Blocked by 421A: `Videra-b5n.2` - measurement report helpers
- Blocked by 421A: `Videra-b5n.3` - selection report event

## Current Surface

`VideraChartView.Overlay.cs` already exposes host-owned interaction reports:

- `TryResolveProbe(Point, out SurfaceProbeInfo)`
- `TryCreateSelectionReport(Point, out SurfaceChartSelectionReport)`
- `TryCreateSelectionReport(Point, Point, out SurfaceChartSelectionReport)`
- draggable marker/range recipe creation

`SurfaceChartSelectionReport` carries screen, sample, axis, and optional
`SurfaceDataWindow` state. `SurfaceProbeInfo` carries sample, axis, value,
world position, tile key, and approximation state.

## Boundaries

- Keep annotation anchors immutable and host-owned.
- Do not add a chart-owned annotation collection or visual editor.
- Do not add measurement/statistics semantics in 421A.
- Do not add compatibility wrappers or hidden fallback behavior.
