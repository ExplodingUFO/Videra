# Phase 222: Clean Package Consumer Smoke Matrix - Context

**Gathered:** 2026-04-26
**Status:** Ready for planning
**Mode:** Autonomous smart discuss

<domain>
## Phase Boundary

Prove clean packaged consumer validation for the alpha candidate matrix: viewer-only, viewer + OBJ, viewer + glTF, and SurfaceCharts.

</domain>

<decisions>
## Implementation Decisions

- Keep the consumer smoke implementation small and script-owned; do not add a new test harness or release tool.
- Preserve isolated local package-feed validation through `Invoke-ConsumerSmoke.ps1`.
- Add explicit scenarios instead of overloading one viewer path.
- Keep SurfaceCharts independent from `VideraView`.

</decisions>

<code_context>
## Existing Code Insights

- `scripts/Invoke-ConsumerSmoke.ps1` already packs canonical public packages, writes an isolated `NuGet.Config`, restores/builds through a local package cache, and emits support artifacts.
- `smoke/Videra.ConsumerSmoke` previously always referenced `Videra.Import.Obj` and always loaded `Assets/reference-cube.obj`.
- `smoke/Videra.SurfaceCharts.ConsumerSmoke` already validates the packaged SurfaceCharts path and support summary.
- Repository tests in `AlphaConsumerIntegrationTests` and `VideraDoctorRepositoryTests` guard script and docs contract truth.

</code_context>

<specifics>
## Specific Ideas

- Add `ViewerOnly`, `ViewerObj`, `ViewerGltf`, and `SurfaceCharts` scenarios.
- Use `VideraConsumerSmokeModelFormat` to control importer package references and compile-time importer wiring.
- Add a repo-owned minimal glTF asset for the clean `Videra.Import.Gltf` package path.
- Add per-scenario report metadata: scenario, package version, package IDs, model format, project path, and support artifact paths.

</specifics>

<deferred>
## Deferred Ideas

- Full graphical run of all smoke scenarios on every platform remains workflow/host dependent.
- Release publish/tag/feed mutation remains out of scope.
- Broader docs/changelog alignment is Phase 225.

</deferred>
