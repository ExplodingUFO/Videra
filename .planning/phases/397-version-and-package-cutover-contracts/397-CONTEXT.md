# Phase 397: Version and Package Cutover Contracts - Context

**Gathered:** 2026-04-30
**Status:** Ready for execution
**Mode:** Autonomous from Phase 396 approval packet
**Bead:** `Videra-v258.2`

<domain>

## Phase Boundary

Phase 397 finalizes the version/package contract evidence needed before gated release automation. It may inspect and validate package assets, tighten narrow package evidence contracts, and record package surface truth. It must not publish packages, create tags, create GitHub releases, mutate feeds, widen public APIs, or introduce compatibility/fallback behavior.

</domain>

<decisions>

## Implementation Decisions

- Use `0.1.0-alpha.7` from `Directory.Build.props` as the current release-candidate version truth.
- Use `eng/public-api-contract.json` as the canonical package/public API surface contract.
- Use `eng/package-size-budgets.json` and `scripts/Validate-Packages.ps1` for package asset and size validation.
- Treat the Phase 396 weak evidence note as a package evidence contract bug: successful SurfaceCharts consumer smoke metadata should list only scenario-relevant support artifacts that exist.

</decisions>

<code_context>

## Existing Contract Surfaces

- Version and common package metadata: `Directory.Build.props`
- Package/public API contract: `eng/public-api-contract.json`
- Package size budgets: `eng/package-size-budgets.json`
- Dry-run package generation and validation: `scripts/Invoke-ReleaseDryRun.ps1`
- Package validation: `scripts/Validate-Packages.ps1`
- Release-readiness wrapper: `scripts/Invoke-ReleaseReadinessValidation.ps1`
- SurfaceCharts consumer smoke producer: `scripts/Invoke-ConsumerSmoke.ps1`
- SurfaceCharts consumer smoke repository contract test: `tests/Videra.Core.Tests/Samples/SurfaceChartsConsumerSmokeConfigurationTests.cs`

</code_context>

<specifics>

## Specific Work

- Tighten `SupportArtifactPaths` in `scripts/Invoke-ConsumerSmoke.ps1` so SurfaceCharts smoke reports support summary and chart snapshot, while viewer smoke reports inspection snapshot and bundle.
- Filter `SupportArtifactPaths` to files/directories that actually exist.
- Add a focused repository test that locks the scenario-specific support artifact selection behavior.
- Produce package dry-run evidence under `artifacts/phase397-release-dry-run`.
- Produce SurfaceCharts consumer smoke evidence under `artifacts/phase397-surfacecharts-consumer-smoke`.

</specifics>

<deferred>

## Deferred Ideas

- Phase 398 should lift this verified package evidence into the gated dry-run/manual-gate automation surface.
- Phase 399 should describe package-support artifact expectations in consumer-facing release support docs.

</deferred>
