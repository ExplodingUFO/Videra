# Phase 359 Verification

**Status:** passed
**Verified:** 2026-04-29

## Checks

- Demo support summary includes Plot output evidence and dataset reproducibility evidence prefixes.
- Packaged consumer smoke producer includes the same evidence prefixes.
- Consumer smoke validation requires the new structured fields.
- Doctor parser reports the new fields in JSON and text summaries.
- Focused tests passed.

## Commands

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -m:1 --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~AlphaConsumerIntegrationTests|FullyQualifiedName~VideraDoctorRepositoryTests"
```

Result: 35 passed, 0 failed.

```powershell
git diff --check
```

Result: passed.

## Notes

A direct `dotnet build smoke\Videra.SurfaceCharts.ConsumerSmoke\Videra.SurfaceCharts.ConsumerSmoke.csproj --no-restore` is not the canonical smoke validation path because the smoke project consumes packed `Videra.SurfaceCharts.*` packages. The repository contract path remains `scripts/Invoke-ConsumerSmoke.ps1`, which packs local packages before building the smoke app.
