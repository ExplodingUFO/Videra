# Phase 392 Summary: Local Package Consumer Smoke

## Result

Phase 392 is complete.

## Changes

- SurfaceCharts consumer smoke now copies successful `Plot.CaptureSnapshotAsync(...)` PNG output to a deterministic `chart-snapshot.png` beside the other smoke artifacts.
- The consumer smoke report now includes `ChartSnapshotPath`.
- The support summary now reports the deterministic snapshot path.
- The consumer smoke script now validates `chart-snapshot.png` for the SurfaceCharts scenario and includes it in `SupportArtifactPaths`.
- The smoke app keeps the first Surface series as the active series after adding bar and contour coverage, so the packaged first-chart readiness check reflects the first-chart proof.
- The consumer-smoke-only missing `FormatFifoCapacity` helper was added.
- The late readiness observation window was widened from 4 seconds to 8 seconds to handle slow Windows software backend readiness without hiding failure.
- Reviewer follow-up tightened failure-artifact truthfulness: failed snapshot states now report `SnapshotPath: none`, report JSON only includes `ChartSnapshotPath` after the deterministic file exists, and late readiness retries failed snapshot results instead of reusing stale failed results.

## Files Changed

- `smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `tests/Videra.Core.Tests/Samples/SurfaceChartsConsumerSmokeConfigurationTests.cs`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`

## Handoff

Phase 393 and Phase 394 are now unblocked and can run in parallel with independent worktrees:

- Phase 393 should own release-readiness script and CI/manual validation alignment.
- Phase 394 should own README/demo/migration/support documentation.
