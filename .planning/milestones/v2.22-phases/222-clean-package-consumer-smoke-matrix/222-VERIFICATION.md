---
status: passed
phase: 222
commit: 502eb01
verified_at: 2026-04-26
---

# Phase 222 Verification

## Result

Passed.

## Evidence

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~AlphaConsumerIntegrationTests|FullyQualifiedName~VideraDoctorRepositoryTests" -p:RestoreIgnoreFailedSources=true`
  - Result: passed, 13 tests.
- `scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario ViewerOnly -BuildOnly`
  - Result: passed.
- `scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario ViewerObj -BuildOnly`
  - Result: passed.
- `scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario ViewerGltf -BuildOnly`
  - Result: passed.
- `scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -BuildOnly`
  - Result: passed.
- Same four scenarios with `-TreatWarningsAsErrors`
  - Result: passed, 0 warnings in consumer smoke builds.
- `dotnet build Videra.slnx -c Release -p:RestoreIgnoreFailedSources=true`
  - Result: passed.

## Notes

- `dotnet build Videra.slnx -c Release --no-restore` failed first in the fresh worktree because multiple projects had no `project.assets.json`; the restore-enabled build passed.
- Full solution build retained two pre-existing SurfaceCharts CS0067 warnings unrelated to this phase.
