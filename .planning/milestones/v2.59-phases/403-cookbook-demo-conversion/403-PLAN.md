# Phase 403 Plan: Cookbook Demo Conversion

## Scope

- Keep work limited to Phase 403 cookbook demo/docs conversion for `Videra-v259.3` / `Videra-uft`.
- Align current visible Bar and Contour proof paths with the demo README, root README cookbook, and release cutover cookbook wording.
- Add bounded Bar and Contour recipes to the demo cookbook gallery without introducing a generic editor or product runtime API changes.
- Tighten cookbook snippets that relied on unexplained local variables where practical.
- Add focused text-contract coverage for cookbook/docs/demo drift.

## Non-Goals

- No product runtime API edits.
- No generic chart editor or workbench.
- No ScottPlot compatibility, parity, adapter, or migration claim.
- No changes to roadmap, beads, generated roadmap, or global planning state.

## Success Criteria

- Demo gallery includes bounded Bar and Contour cookbook recipes mapped to the existing visible proof paths.
- README and release cutover cookbook wording mention Bar and Contour as bounded current-demo proofs.
- Existing snippets avoid unexplained `x`, `y`, `z`, `scatterData`, `surfaceSource`, and `comparisonSource` variables where edited.
- Focused sample text-contract tests cover Bar/Contour cookbook alignment.
- Requested focused tests, demo build, `git diff --check`, and `git status --short` are run before commit.
