# Phase 241 Summary: Performance Diagnostics Contract

## Outcome

Completed the diagnostics contract foundation for v2.27 without changing renderer architecture.

## Changed

- Added viewer diagnostics fields:
  - `LastFrameDrawCallCount`
  - `LastFrameInstanceCount`
  - `LastFrameVertexCount`
  - `LastFrameUploadBytes`
  - `ResidentResourceCount`
  - `ResidentResourceBytes`
  - `PickableObjectCount`
- Kept existing scene object counts source-compatible and explicitly labeled them as scene counts, not draw-call metrics.
- Backed viewer upload bytes with existing scene residency telemetry and resident resource bytes with residency estimates.
- Added SurfaceCharts `VisibleTileCount` and `ResidentTileBytes` through `SurfaceChartRenderSnapshot` and `SurfaceChartRenderingStatus`.
- Populated SurfaceCharts GPU resident bytes from GPU resident tile resources and software bytes from tile geometry estimates.
- Updated formatter, support docs, English/Chinese docs, SurfaceCharts demo support text, smoke support text, and repository contract tests.

## Verification

- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~VideraDiagnosticsSnapshotFormatterTests"`: passed.
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderHostTests"`: passed on rerun. First attempt hit a transient local file lock from security software on `Videra.Core.dll`.
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSessionBridgeIntegrationTests"`: passed.
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests|FullyQualifiedName~RepositoryArchitectureTests|FullyQualifiedName~RepositoryLocalizationTests"`: passed.
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests"`: passed.
- `dotnet build smoke/Videra.SurfaceCharts.ConsumerSmoke/Videra.SurfaceCharts.ConsumerSmoke.csproj -c Release`: not applicable from source checkout without a local packaged feed; restore could not find `Videra.SurfaceCharts.Avalonia` or `Videra.SurfaceCharts.Processing` packages.

## Deferred

- Real draw-call, instance, vertex, and pickable object measurement remains for the instance-batch runtime work in Phase 243.
- Benchmark evidence and threshold promotion remain for Phase 244.
