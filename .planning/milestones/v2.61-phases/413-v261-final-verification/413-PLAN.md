# Phase 413 Plan: v2.61 Final Verification

## Goal

Complete v2.61 by proving the cookbook/demo/CI truth work still passes as a
composed milestone, then synchronize Beads, generated roadmap, archived phase
artifacts, and remote state.

## Steps

1. Verify focused milestone gates:
   - `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsCiTruthTests|FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsPerformanceTruthTests|FullyQualifiedName~BeadsPublicRoadmapTests" --no-restore`
   - `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
   - `git diff --check`
2. Write Phase 413 closure artifacts and mark the v2.61 roadmap/state complete.
3. Archive v2.61 phase artifacts under `.planning/milestones/v2.61-phases`.
4. Close `Videra-q10` and the `Videra-uqv` epic.
5. Export Beads and regenerate `docs/ROADMAP.generated.md`.
6. Re-run final Beads/generated-roadmap and whitespace checks.
7. Commit, push Beads Dolt state, push Git state, and verify clean handoff.

## Success Criteria

- Focused tests and scope guardrail pass honestly.
- Beads status, `.beads/issues.jsonl`, and `docs/ROADMAP.generated.md` agree.
- v2.61 phase artifacts are archived.
- Git and Beads remote sync complete.
- No v2.61 worktree or active branch cleanup remains for this milestone.
