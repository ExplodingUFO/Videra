# Phase 103: Viewer Product Boundary and Capability Matrix - Context

**Gathered:** 2026-04-21
**Status:** In progress
**Mode:** Auto-generated (discuss skipped via autonomous execution)

<domain>
## Phase Boundary

Freeze `Videra 1.0` as a native desktop viewer / inspection / surface-chart product, not a Three.js-style general runtime, and publish one explicit capability + package-layer truth that downstream phases can build against.

</domain>

<decisions>
## Implementation Decisions

### the agent's Discretion
- Keep this phase doc-first and narrow: establish product boundary and vocabulary before touching package extraction code.
- Do not add compatibility framing, transition messaging, or speculative future package contracts that are not shipped today.
- Prefer one canonical capability/layer matrix document, then make entry docs link to it instead of duplicating drifting prose.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `README.md`, `ARCHITECTURE.md`, `docs/package-matrix.md`, and `docs/index.md` already define most of the public truth for package boundaries.
- `src/Videra.Core/README.md` currently still describes `Videra.Core` as owning the import pipeline directly, which is accurate today but too blurry for the new milestone boundary.
- `tests/Videra.Core.Tests/Repository/*` already contains repository-truth tests for README, package matrix, architecture, release policy, and SurfaceCharts source-first boundaries.

### Established Patterns
- Repository doc phases usually add a focused public truth document and update the guarded entry docs rather than editing every doc page in one pass.
- Phase artifacts use one `CONTEXT.md`, one or more numbered `PLAN.md` files, and numbered `SUMMARY.md` files with requirement metadata.

### Integration Points
- `README.md`
- `ARCHITECTURE.md`
- `docs/index.md`
- `docs/package-matrix.md`
- `src/Videra.Core/README.md`
- `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`

</code_context>

<specifics>
## Specific Ideas

- Add a dedicated `docs/capability-matrix.md` that separates shipped `1.0` viewer/runtime capabilities from deferred `2.0` engine-style features.
- Use the same document to publish one package-layer matrix for `Core`, `Import`, `Backend`, `UI adapter`, and `Charts`.
- Route README and architecture docs to that matrix so the repo no longer feels like an implicit “general engine later” story.

</specifics>

<deferred>
## Deferred Ideas

- The actual `Videra.Core` importer/logging extraction belongs to Phase 104.
- New package projects or hosting contracts belong to Phases 104-105.
- Repository guards that enforce csproj layering belong to Phase 106.

</deferred>
