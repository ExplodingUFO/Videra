# Phase 106: Package Truth and Layering Validation - Context

**Gathered:** 2026-04-21
**Status:** Completed on 2026-04-21

<domain>
## Phase Boundary

Phase 103 defined the viewer/runtime `1.0` boundary. Phase 104 made the import split shippable. Phase 105 documented the canonical host boundary and added direct repository guards for the `Core` / `Import` / `UI adapter` split.

The remaining milestone gap is simpler: the consumer-facing package truth, release/support docs, smoke path, and automated repository checks still need one explicit closure pass so they all teach and verify the same canonical viewer stack.

</domain>

<decisions>
## Implementation Decisions

### Keep this phase documentation-and-validation focused

- Do not introduce new runtime or package abstractions here.
- Prefer tightening the existing docs and repository guards over adding new tooling layers.
- Reuse the canonical vocabulary already established in `docs/capability-matrix.md`, `docs/package-matrix.md`, and `docs/hosting-boundary.md`.

### Make the canonical stack mechanically visible

- The canonical viewer stack for public consumers is:
  - `Videra.Core`
  - optional `Videra.Import.Gltf` / `Videra.Import.Obj` on the core path
  - `Videra.Avalonia`
  - exactly one matching `Videra.Platform.*` package
- `Videra.SurfaceCharts.*` remains source-first and must stay out of the public package promise.
- Smoke scripts, package validation, workflows, and release docs should all reflect that exact truth.

</decisions>

<code_context>
## Existing Code Insights

- `scripts/Invoke-ConsumerSmoke.ps1` and `scripts/Validate-Packages.ps1` already know about the dedicated import packages.
- Release and support docs already mention the public package set, but the canonical package-stack story is still spread across multiple documents.
- `RepositoryReleaseReadinessTests` already guards broad release/documentation truth and is the natural place to add Phase 106 repository checks.

</code_context>

<specifics>
## Specific Ideas

1. Tighten `docs/package-matrix.md`, `docs/support-matrix.md`, `docs/release-policy.md`, and `docs/releasing.md` so they explicitly share the same canonical stack story and route readers to the same boundary docs.
2. Add repository guards that compare docs/package automation against the same package set and boundary language.
3. Keep the checks concrete enough that future package drift fails fast without forcing large-scale doc rewrites.

</specifics>

<deferred>
## Deferred Ideas

- Do not add new package-manager tooling or release orchestrators in this phase.
- Do not expand public package scope to `Videra.SurfaceCharts.*`.
- Do not reopen host/runtime architecture work that was already closed in Phase 105.

</deferred>
