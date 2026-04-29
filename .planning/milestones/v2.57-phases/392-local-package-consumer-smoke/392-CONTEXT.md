# Phase 392 Context: Local Package Consumer Smoke

## Bead

- `Videra-v257.3` — Phase 392 Local Package Consumer Smoke
- Status at execution start: `in_progress`
- Dependency: Phase 391 / `Videra-v257.2`

## Scope

Prove the SurfaceCharts consumer smoke can use locally built packages through package/public APIs only, and produce deterministic artifacts suitable for support handoff.

## Boundaries

- No old chart controls.
- No public direct `Source`.
- No compatibility wrappers.
- No hidden fallback/downshift.
- No public publish/tag.
- No shared library rewrite for a smoke-only artifact issue.

## Starting Evidence

- `smoke/Videra.SurfaceCharts.ConsumerSmoke/Videra.SurfaceCharts.ConsumerSmoke.csproj` already used `PackageReference` for `Videra.SurfaceCharts.Avalonia` and `Videra.SurfaceCharts.Processing`, with no `ProjectReference`.
- `scripts/Invoke-ConsumerSmoke.ps1` already packed local packages and restored the consumer against a local NuGet source.
- SurfaceCharts support summary included snapshot metadata, but the smoke app did not copy the PNG to the documented deterministic path `artifacts/consumer-smoke/chart-snapshot.png`.
- Full package consumer smoke exposed a real compile gap: the smoke support summary referenced `FormatFifoCapacity` without defining it in the consumer smoke project.

