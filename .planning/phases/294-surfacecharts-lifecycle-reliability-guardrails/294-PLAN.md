# Phase 294: SurfaceCharts Lifecycle Reliability Guardrails - Plan

**Status:** Complete  
**Bead:** Videra-0w9.3

## Goal

Add deterministic demo/test-host lifecycle evidence and timeout handling for automated verification.

## Tasks

1. Add timeout/cancellation handling around `AvaloniaHeadlessTestSession` dispatch.
2. Preserve existing helper call surface for current integration tests.
3. Add focused tests proving lifecycle timeout messages include useful context.
4. Verify targeted lifecycle integration tests.

## Verification

`dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Debug --no-restore -m:1 --filter "FullyQualifiedName~SurfaceChartViewLifecycleTests|FullyQualifiedName~AvaloniaHeadlessTestSession"` passed: 18/18.
