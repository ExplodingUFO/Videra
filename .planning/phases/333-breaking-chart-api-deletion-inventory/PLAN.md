# Phase 333 Plan

## Work

1. Claim `Videra-rhb`.
2. Run parallel read-only inventory:
   - `src/` and `tests/` old View references.
   - docs, samples, smoke, package/support references.
3. Classify references as delete, migrate to `VideraChartView`, or retain only as non-View helper.
4. Define minimal Phase 334 API boundary.
5. Record handoff and close the bead.

## Verification

- `rg -n "\b(SurfaceChartView|WaterfallChartView|ScatterChartView)\b" src tests samples smoke docs README.md eng scripts`
- `bd ready --json`
- Regenerate `.beads/issues.jsonl` and `docs/ROADMAP.generated.md` after bead closure.
