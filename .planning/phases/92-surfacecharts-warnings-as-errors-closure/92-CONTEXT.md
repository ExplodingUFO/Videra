# Phase 92: SurfaceCharts Warnings-as-Errors Closure - Context

**Gathered:** 2026-04-20
**Status:** Completed and verified
**Mode:** Auto-generated (discuss skipped via autonomous infrastructure detection)

<domain>
## Phase Boundary

Validate whether the reported SurfaceCharts warnings-as-errors failures still reproduce on the current workspace, and only patch code if the strict-build red line is real.

</domain>

<decisions>
## Implementation Decisions

### the agent's Discretion
- Treat the exact CI-equivalent strict builds as the source of truth; do not refactor code just because a roadmap hypothesis names likely warning IDs.
- If the strict builds already pass, preserve the existing analyzer policy and close the phase through verification artifacts instead of speculative cleanup.
- Keep any future opportunistic analyzer cleanups deferred until they are backed by an active failing gate.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `quality-gate-evidence` in `.github/workflows/ci.yml` already defines the repo's strict-build path through packaged consumer smoke plus curated Core evidence projects.
- `Directory.Build.props` and existing project files already encode the current analyzer policy; no policy changes are needed if the strict builds are green.

### Established Patterns
- The repo validates warnings-as-errors through targeted evidence builds instead of a whole-repo strict build.
- Consumer-smoke quality evidence runs through `scripts/Invoke-ConsumerSmoke.ps1 -BuildOnly -TreatWarningsAsErrors`.

### Integration Points
- `tests/Videra.Core.Tests/Videra.Core.Tests.csproj -p:TreatWarningsAsErrors=true`
- `src/Videra.SurfaceCharts.Processing/Videra.SurfaceCharts.Processing.csproj -p:TreatWarningsAsErrors=true`
- `src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj -p:TreatWarningsAsErrors=true`
- `tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -p:TreatWarningsAsErrors=true`
- `samples/Videra.MinimalSample/Videra.MinimalSample.csproj -p:TreatWarningsAsErrors=true`
- `scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -BuildOnly -TreatWarningsAsErrors`

</code_context>

<specifics>
## Specific Ideas

- The roadmap was seeded from a prior CI report naming `CA2007`, `S3267`, `S2325`, and `S4200`, but the current workspace must be re-validated before assuming code edits are still necessary.
- A green strict-build chain satisfies the phase even if no source changes are required.

</specifics>

<deferred>
## Deferred Ideas

- Opportunistic analyzer/style cleanup in files that do not currently trip the active warning gate.

</deferred>
