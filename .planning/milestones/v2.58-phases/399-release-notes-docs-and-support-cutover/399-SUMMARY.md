# Phase 399 Summary: Release Notes Docs and Support Cutover

## Status

Complete.

## What Changed

- Added `docs/surfacecharts-release-cutover.md` as the current consumer-facing SurfaceCharts v2.58 controlled release cutover page.
- Linked the cutover page from root README, docs index, package matrix, releasing, troubleshooting, `Videra.SurfaceCharts.Avalonia` README, and `Videra.SurfaceCharts.Demo` README.
- Updated `CHANGELOG.md` with the documentation cutover note.
- Added a focused repository docs contract test that checks the cutover page, support artifact names, ScottPlot inspiration-only boundary, repository-only demo split, and entry-point links.

## Boundaries Preserved

- Public publish, tags, and GitHub Release creation remain human-approved and outside Phase 399.
- `Videra.SurfaceCharts.Demo` and consumer smoke remain repository-only validation/support entries.
- ScottPlot is named only as recipe ergonomics inspiration.
- No compatibility wrappers, hidden fallback/downshift behavior, PDF/vector export, OpenGL/WebGL/backend expansion, or broad runtime rewrites were introduced.
