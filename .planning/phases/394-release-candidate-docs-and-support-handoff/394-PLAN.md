# Phase 394 Plan: Release Candidate Docs and Support Handoff

## Success Criteria

- Package consumers can find SurfaceCharts package consumption, cookbook recipes, migration notes, and support artifacts from public docs without internal planning docs.
- Root README and SurfaceCharts README entrypoints link the release-candidate handoff.
- Migration guidance names the Plot-owned model, removed old controls, direct `Source` removal, non-goals, and support artifact bundle.
- Validation records focused docs/configuration checks or `rg` evidence.
- No scripts, CI, runtime implementation, shared planning status, or Beads state files are changed.

## Steps

1. Add a public SurfaceCharts release-candidate handoff doc under `docs/`.
   - Verify: handoff doc contains package consumption, cookbook path, migration notes, non-goals, and exact Phase 392 support artifact names.
2. Link the handoff from root README, docs index, `src/Videra.SurfaceCharts.Avalonia/README.md`, and `samples/Videra.SurfaceCharts.Demo/README.md`.
   - Verify: focused docs tests and `rg` checks confirm linked path visibility.
3. Record Phase 394 summary and verification artifacts.
   - Verify: phase directory contains `394-CONTEXT.md`, `394-PLAN.md`, `394-SUMMARY.md`, and `394-VERIFICATION.md`.

## Out Of Scope

- Phase 393 validation-script/CI/manual-validation implementation.
- Runtime API or behavior changes.
- Shared planning/roadmap/beads status updates.
- Publishing, tagging, merging, or pushing.

