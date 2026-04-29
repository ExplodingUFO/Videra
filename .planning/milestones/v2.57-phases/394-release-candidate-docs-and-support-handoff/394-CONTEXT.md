# Phase 394 Context: Release Candidate Docs and Support Handoff

## Bead

- `Videra-v257.5` - Phase 394 Release Candidate Docs and Support Handoff
- Status at execution start: already claimed by parent

## Scope

Own release-candidate consumer documentation surfaces only:

- root README entrypoints
- SurfaceCharts demo/package README entrypoints
- migration/support handoff guidance
- Phase 394 planning artifacts

## Boundaries

- No runtime `src` changes except package README documentation.
- No scripts, CI, or validation implementation; Phase 393 owns that work.
- No shared `.planning/STATE.md`, `.planning/ROADMAP.md`, `.planning/REQUIREMENTS.md`, `.beads/issues.jsonl`, or `docs/ROADMAP.generated.md` edits.
- Preserve the ScottPlot boundary as ergonomic inspiration only, not API compatibility.
- Do not reintroduce old chart controls, direct public `Source`, compatibility wrappers, PDF/vector export, backend expansion, or hidden fallback/downshift language.

## Input Evidence

- Phase 392 completed local package consumer smoke and documented deterministic SurfaceCharts support artifacts.
- Required artifact names for support handoff: `consumer-smoke-result.json`, `diagnostics-snapshot.txt`, `surfacecharts-support-summary.txt`, `chart-snapshot.png`, trace/stdout/stderr/environment logs.
- Existing docs already described package matrix, alpha feedback, troubleshooting, and cookbook recipes, but did not provide one consumer-facing release-candidate handoff page for current migration/support truth.

