# Phase 295: Performance Lab and Doctor Evidence Alignment - Plan

**Status:** Complete  
**Bead:** Videra-0w9.4

## Goal

Align Performance Lab, SurfaceCharts support reports, and Doctor/support discovery vocabulary and guardrails.

## Tasks

1. Add passive Doctor discovery for SurfaceCharts support summaries.
2. Reuse `present` / `missing` / `unavailable` evidence vocabulary.
3. Surface summary identity fields in structured Doctor output.
4. Document the passive discovery behavior and non-goals.
5. Add focused tests for missing and present SurfaceCharts support reports.

## Verification

`dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Debug --no-restore -m:1 --filter "FullyQualifiedName~VideraDoctorRepositoryTests"` passed: 9/9.
