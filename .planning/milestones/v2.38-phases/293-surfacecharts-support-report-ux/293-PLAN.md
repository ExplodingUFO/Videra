# Phase 293: SurfaceCharts Support Report UX - Plan

**Status:** Complete  
**Bead:** Videra-0w9.2

## Goal

Add a focused support report refinement to `Videra.SurfaceCharts.Demo` using existing diagnostics truth.

## Tasks

1. Add stable chart/control identity to support summaries.
2. Add runtime/environment and assembly identity fields.
3. Add backend/display environment variables relevant to triage.
4. Preserve cache-load failure detail when cache-backed streaming falls back.
5. Update docs and focused sample tests.

## Verification

`dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests"` passed: 14/14.
