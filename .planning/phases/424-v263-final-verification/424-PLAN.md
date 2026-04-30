# Phase 424 Plan: v2.63 Final Verification

## Goal

Close v2.63 with synchronized Beads state, generated public roadmap, scope
guardrails, release-readiness evidence, phase archive, Dolt/Git push, and clean
worktree handoff.

## Beads

### 424A: Roadmap and Scope Evidence

Bead: `Videra-7ip.1`

Write scope:

- `.beads/issues.jsonl` through `bd`
- `docs/ROADMAP.generated.md`
- `.planning/phases/424-v263-final-verification/*`

Validation:

```powershell
pwsh -File ./scripts/Export-BeadsRoadmap.ps1
pwsh -File ./scripts/Test-SnapshotExportScope.ps1
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BeadsPublicRoadmapTests|FullyQualifiedName~SurfaceChartsCiTruthTests|FullyQualifiedName~ReleaseDryRunRepositoryTests" --no-restore
git diff --check
```

### 424B: Final Release-Readiness Evidence

Bead: `Videra-7ip.2`

Write scope:

- `artifacts/v263-release-readiness-final/**` if generated and intentionally
  retained as evidence
- `.planning/phases/424-v263-final-verification/424-VERIFICATION.md`

Validation:

```powershell
pwsh -File ./scripts/Invoke-ReleaseReadinessValidation.ps1 -ExpectedVersion 0.1.0-alpha.7 -Configuration Release -OutputRoot artifacts/v263-release-readiness-final
git diff --check
```

## Closeout

- Close `Videra-7ip.1`, `Videra-7ip.2`, `Videra-7ip`, and v2.63 epic only
  after their owning evidence exists.
- Push Beads through Dolt. If direct `bd dolt push` fails with the known Windows
  host path issue, use `scripts/Push-BeadsDoltViaHost.ps1`.
- Run `git pull --rebase`, `git push`, and verify `git status` is clean and up
  to date with origin.
