# Phase 392 Verification

Status: passed

## Commands

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests|FullyQualifiedName~VideraDoctorRepositoryTests"
```

Result: passed, 11/11.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Test-SnapshotExportScope.ps1
```

Result: passed.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -BuildOnly -OutputRoot artifacts/phase392-consumer-smoke-build
```

Result: passed. The smoke project built with 0 warnings and 0 errors against locally packed packages.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -OutputRoot artifacts/phase392-consumer-smoke-run
```

Result: passed.

Key output:

- `Consumer smoke passed (SurfaceCharts).`
- `ActiveBackend: Software`
- `ResidentTileCount: 1`
- `Chart snapshot: F:\CodeProjects\DotnetCore\Videra-v257-392-consumer-smoke\artifacts\phase392-consumer-smoke-run\chart-snapshot.png`

## Review Follow-Up

Reviewer found failure-branch issues around stale failed snapshot results and truthful snapshot paths. Fixes were applied before closeout:

- Late readiness now retries when `_snapshotResult` is absent or unsuccessful.
- JSON `ChartSnapshotPath` is emitted only when the deterministic snapshot file exists.
- Support summary emits `SnapshotPath: none` unless a successful/persisted snapshot exists.
- `Invoke-ConsumerSmoke.ps1` validates `SnapshotStatus` and `SnapshotPath` consistency.

The focused tests, scope guardrail, build-only smoke, and full SurfaceCharts smoke were rerun after these fixes and passed.

## Notes

- The first attempted `dotnet test --no-restore` failed in the fresh worktree because `project.assets.json` did not exist yet. The restored test run passed.
- The first full SurfaceCharts consumer run exposed readiness ordering issues; those were fixed and the final run passed.
