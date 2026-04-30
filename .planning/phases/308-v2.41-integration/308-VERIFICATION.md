# v2.41 Integration Verification

## Combined Check

Command:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~RepositoryArchitectureTests|FullyQualifiedName~VideraDoctorRepositoryTests|FullyQualifiedName~AlphaConsumerIntegrationTests|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests|FullyQualifiedName~SurfaceChartsRepositoryLayoutTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests"
```

Result:

- Passed: 67
- Failed: 0
- Skipped: 0

## Notes

Full CI was intentionally not run because the user asked to prioritize local progress and skip slow CI for this pass.
