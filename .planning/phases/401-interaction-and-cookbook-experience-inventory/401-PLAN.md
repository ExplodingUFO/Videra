# Phase 401: Interaction and Cookbook Experience Inventory - Plan

bead: Videra-v259.1

## Goal

Complete a real-surface inventory for v2.59 and define small, dependency-aware
implementation slices for interaction/code-experience and cookbook work.

## Tasks

1. Claim `Videra-v259.1`.
2. Create child beads for interaction API, cookbook demo/docs, and validation
   guardrail inventory.
3. Run each child bead in an isolated worktree/branch with a single owned output
   file.
4. Synthesize the inventories into Phase 401 context, summary, plan, and
   verification artifacts.
5. Confirm Phase 402 and Phase 403 can proceed in parallel with disjoint write
   sets.
6. Close Phase 401 child beads and parent bead, export Beads, regenerate the
   public roadmap, update planning state, commit, push Dolt and Git.

## Phase 402 Candidate Scope

Recommended first implementation slice:

- Add typed `BarPlot3DSeries` and `ContourPlot3DSeries` handles if the change
  stays local to `Controls/Plot/*` plus focused Plot API tests.
- Add or document a compact command/probe/selection/draggable interaction recipe
  surface without remapping custom mouse actions or adding a generic annotation
  editor.

Suggested write boundary:

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/*`
- focused `Videra.SurfaceCharts.Avalonia.IntegrationTests`
- narrow docs/demo snippets only when needed to explain the API

## Phase 403 Candidate Scope

Recommended first cookbook slice:

- Align demo README/root docs/release cutover docs with current visible Bar and
  Contour proof paths.
- Add Bar and Contour cookbook recipes to the bounded demo gallery.
- Tighten existing snippets where variables are implied instead of copyable.
- Add text-contract coverage that demo source entries and cookbook/docs entries
  stay aligned.

Suggested write boundary:

- `samples/Videra.SurfaceCharts.Demo/*`
- `README.md`
- `docs/surfacecharts-release-cutover.md`
- focused `tests/Videra.Core.Tests/Samples/*` text-contract tests

## Dependency Model

Phase 402 and Phase 403 are independent after Phase 401:

- Phase 402 owns interaction/API code-experience polish.
- Phase 403 owns cookbook docs/demo/sample polish.
- Both should coordinate if either touches shared demo snippets.
- Phase 404 waits for both and owns final integration, Beads export, generated
  roadmap, and cleanup.

## Validation

Inventory-only validation:

- child worktree `git status --short` checks
- `git diff --check`
- Beads export and generated roadmap checks
- `BeadsPublicRoadmapTests`

Implementation validation commands are recorded in
`401C-VALIDATION-INVENTORY.md`.
