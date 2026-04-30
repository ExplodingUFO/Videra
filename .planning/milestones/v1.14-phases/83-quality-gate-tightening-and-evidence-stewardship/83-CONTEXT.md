# Phase 83: Quality Gate Tightening and Evidence Stewardship - Context

**Gathered:** 2026-04-20  
**Status:** Executed and verified  
**Mode:** Autonomous

## Phase Boundary

Phase 83 turns quality signals from advisory context into a more actionable definition of alpha-ready green. The work focuses on warnings, curated evidence jobs, and documentation truth. It does not attempt to make benchmarks hard numeric blockers yet, but it does tighten how evidence is expected to influence merge decisions.

## Implementation Decisions

### Warning cleanup
- **D-01:** Remove analyzer findings on the freshly touched scene/inspection/runtime paths instead of papering over them.
- **D-02:** Prefer simplifying helper classes into static utilities when they carry no instance state.
- **D-03:** Replace fire-and-forget file writes with awaited async IO in the inspection bundle path.

### Evidence gating
- **D-04:** Add a dedicated `quality-gate-evidence` CI job for the curated consumer/core paths.
- **D-05:** Keep the consumer build in that job package-based by reusing `Invoke-ConsumerSmoke.ps1`, not a raw `dotnet build` against stale package references.
- **D-06:** Treat benchmark review as label-gated merge input rather than a hard numeric threshold in this milestone.

### Documentation truth
- **D-07:** Update README and release guidance so “green” means the same thing in CI, docs, and planning artifacts.
- **D-08:** Lock the new quality-gate semantics in repository tests to prevent future workflow drift.

## Specific Ideas

- Convert scene/runtime helpers into static classes to satisfy Sonar guidance and make ownership clearer.
- Extend the smoke script with a build-only warnings-as-errors mode so CI can validate the packaged consumer build without redundantly running the app.
- Keep benchmark docs honest: actionable merge-time signal, but still not a numeric blocker.

## Canonical References

### Milestone and requirements
- `.planning/ROADMAP.md` — Phase 83 goal and success criteria.
- `.planning/REQUIREMENTS.md` — `QUAL-01`, `QUAL-02`, and `QUAL-03`.

### Code and evidence surfaces
- `src/Videra.Avalonia/Runtime/Scene/SceneEngineApplicator.cs`
- `src/Videra.Avalonia/Runtime/Scene/SceneDeltaPlanner.cs`
- `src/Videra.Avalonia/Runtime/VideraSnapshotExportService.cs`
- `src/Videra.Avalonia/Controls/VideraInspectionBundleService.cs`
- `.github/workflows/ci.yml`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `docs/benchmark-gates.md`
- `docs/releasing.md`
- `README.md`

## Existing Code Insights

### Reusable assets
- Phase 82 had already promoted consumer/sample evidence into CI, so phase 83 could harden warnings and evidence semantics instead of building the whole surface from scratch.
- Repository contract tests were already the right place to pin workflow/doc truth once the quality gate definition changed.

### Risks carried into the phase
- Curated warning cleanup had not yet been encoded as a merge-time signal.
- A naive `dotnet build smoke/...` quality gate would not actually validate the packaged consumer path the repo claims to support.

## Deferred Ideas

- Hard numeric benchmark blocking.
- Whole-repo warnings-as-errors policy.
- Any new compatibility surface such as `OpenGL`.

---

*Phase: 83-quality-gate-tightening-and-evidence-stewardship*  
*Context gathered: 2026-04-20*
