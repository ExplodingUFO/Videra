# Phase 295: Performance Lab and Doctor Evidence Alignment - Summary

**Status:** complete  
**Bead:** Videra-0w9.4  
**Commit:** b2ce865

## Delivered

- Added `evidencePacket.surfaceChartsSupportReport` to `scripts/Invoke-VideraDoctor.ps1`.
- Added passive discovery of `artifacts/consumer-smoke/surfacecharts-support-summary.txt`.
- Added structured fields for `generatedAtUtc`, `evidenceKind`, `evidenceOnly`, `chartControl`, `environmentRuntime`, and `renderingStatusPresent`.
- Updated Doctor docs to explain that Doctor does not run SurfaceCharts demo or consumer smoke by default.
- Added repository tests for missing and present SurfaceCharts support-report states.

## Changed Files

- `scripts/Invoke-VideraDoctor.ps1`
- `docs/videra-doctor.md`
- `tests/Videra.Core.Tests/Repository/VideraDoctorRepositoryTests.cs`
