# Phase 417 Context: No-Compat Guardrails and CI Truth

## Beads

- Parent: `Videra-r9q`
- Script guardrail child: `Videra-5j5`
- CI/release truth child: `Videra-raj`

## Upstream State

Phase 415 removed chart-local GPU fallback/downshift, compatibility camera-frame
backfill, and stale compatibility/fallback test vocabulary. Phase 416 extracted
the cookbook catalog and support summary helpers from the SurfaceCharts demo.

## Scope

Phase 417 hardens repository guardrails so those cleanup boundaries remain
durable:

- broaden `scripts/Test-SnapshotExportScope.ps1` to catch public old chart
  controls with modifiers, multi-line public direct `Source`, and hidden
  fallback/downshift patterns relevant to SurfaceCharts;
- pin CI/release truth to current cookbook/demo/support evidence, packaged
  SurfaceCharts smoke, generated roadmap, and scope guardrails;
- keep checks focused and deterministic.

## Non-Goals

- No compatibility layer, migration shim, old chart control, direct public
  `Source`, hidden fallback/downshift, fake evidence, generic audit framework,
  or release publishing action.
- Do not rewrite historical docs that intentionally mention forbidden behavior
  as negative guardrails.

## Parallel Split

`Videra-5j5` can run independently from `Videra-raj` because the script work is
confined to the scope guardrail script and script-focused tests, while CI truth
work owns workflow/release-readiness tests and filters.
