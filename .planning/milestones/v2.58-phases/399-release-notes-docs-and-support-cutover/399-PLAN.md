# Phase 399 Plan: Release Notes Docs and Support Cutover

## Success Criteria

- Consumer-facing docs expose one current SurfaceCharts v2.58 cutover entry point.
- Release notes, package consumption, cookbook, migration, support handoff, and troubleshooting guidance preserve the controlled-release boundary.
- ScottPlot remains inspiration only, with no compatibility or migration-layer claim.
- Focused docs contract tests and grep checks verify the boundary.

## Scope

- Public docs and README/changelog entry points.
- Phase 399 artifacts under this directory.
- Focused repository docs contract tests only if needed to protect the cutover wording.

## Steps

1. Add a SurfaceCharts v2.58 release cutover page under `docs/`.
2. Link it from README, docs index, package/releasing/troubleshooting docs, and SurfaceCharts package/demo README entry points.
3. Add a focused docs contract test for release cutover boundaries.
4. Run focused tests and grep checks for unsupported claims.
