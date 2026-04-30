# Phase 429: Scenario Cookbook and Package Templates - Context

**Gathered:** 2026-04-30
**Status:** Blocked (depends on Phase 428)
**Mode:** Autonomous (smart discuss)
**Bead:** `Videra-7tqx.5`

## Phase Boundary

Phase 429 converts v2.64 workflows into copyable native recipes and package
consumer templates. It covers multi-chart analysis, linked interaction,
high-density data, streaming updates, and support evidence in cookbook docs,
and demonstrates supported public package workflows with copyable code.

## User Constraints (from Phase 425)

- Beads are the task, state, and handoff spine.
- Split tasks small and identify dependencies before implementation.
- Use isolated worktrees and branches for parallel beads when write scopes do
  not block each other.
- Every worker must have a responsibility boundary, write scope, validation
  command, and handoff notes.
- Avoid god code.
- Do not add compatibility layers, downshift behavior, fallback behavior, old
  chart controls, or fake validation evidence.
- Keep implementation simple and direct.

## Decisions

### D-01: Cookbook Scope

The cookbook covers five workflow areas (from Phase 425B inventory):
1. **Multi-chart analysis** — composing multiple VideraChartView instances
2. **Linked interaction** — camera/axis/probe/selection propagation
3. **Streaming data** — live append, FIFO, cache-backed surfaces
4. **High-density evidence** — large datasets with evidence records
5. **Support evidence** — workspace and linked interaction evidence copy

### D-02: Package Template Model

Package templates are based on the existing `smoke/Videra.SurfaceCharts.ConsumerSmoke` pattern:
- Package-reference-only (no ProjectReference)
- Writes support artifacts (support summary, PNG snapshot, diagnostics)
- Minimal project layout for each workflow area
- Copyable, not a `dotnet new` template (out of scope)

### D-03: Recipe Format

Recipes follow existing `samples/Videra.SurfaceCharts.Demo/Recipes/*.md` format:
- Title, scenario id, prerequisites
- Step-by-step code snippets
- Expected output/evidence
- Non-goals and boundaries

### D-04: Test Contract

Tests verify:
- Cookbook recipe coverage (all workflow areas documented)
- Recipe snippet correctness (code compiles)
- Template project structure (files exist, references correct)
- Support wording accuracy (no overclaims)

## Canonical References

- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425B-DEMO-COOKBOOK-TEMPLATE-INVENTORY.md` — full inventory
- `samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs` — recipe catalog
- `samples/Videra.SurfaceCharts.Demo/Recipes/*.md` — existing recipes
- `smoke/Videra.SurfaceCharts.ConsumerSmoke/` — package consumer smoke app
- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs` — coverage tests

## Existing Code Insights

### Reusable Assets
- `CookbookRecipeCatalog.cs` — scenario-addressable recipe catalog
- Existing recipe files — format and structure to follow
- `SurfaceCharts.ConsumerSmoke` — package-reference-only template seed
- `SurfaceDemoSupportSummary` — evidence format pattern
- Cookbook coverage matrix tests — test pattern

### Established Patterns
- Scenario-addressable catalog with group/title/snippet
- Package-reference-only consumer apps
- Bounded text evidence records
- Text-contract tests for docs/templates

### Integration Points
- `samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs` — add new recipes
- `samples/Videra.SurfaceCharts.Demo/Recipes/` — new recipe files
- `smoke/` — new template projects
- `tests/Videra.Core.Tests/Samples/` — new coverage tests

## Deferred Ideas

- `dotnet new` template integration — out of scope for v2.64
- Automated snippet compilation testing — complex, may belong to CI phase
- Package template CI validation — Phase 430

---

*Phase: 429-scenario-cookbook-and-package-templates*
*Context gathered: 2026-04-30*
