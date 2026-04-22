# Alpha Feedback

Videra is still an early alpha embeddable viewer stack. Good feedback is not just "it failed" — it needs enough integration context to reproduce the host, backend, and package path that produced the failure.

## Before filing

1. Confirm the install path:
   - `nuget.org` public packages
   - `GitHub Packages` preview validation
   - repository source build
2. Capture the exact package version or commit SHA.
3. Reproduce the issue with the smallest flow you can manage:
   - [Videra.MinimalSample](../samples/Videra.MinimalSample/README.md) for the default happy path
   - [Videra.ExtensibilitySample](../samples/Videra.ExtensibilitySample/README.md) for `VideraView.Engine`
   - [Videra.InteractionSample](../samples/Videra.InteractionSample/README.md) for controlled interaction
   - [Videra.Demo](../samples/Videra.Demo/README.md) for backend diagnostics and scene-pipeline visibility
   - `smoke/Videra.SurfaceCharts.ConsumerSmoke` for the packaged SurfaceCharts first-chart path on the supported host path
   - `Videra.SurfaceCharts.Demo` for the packaged chart repro path:
     - `Start here: In-memory first chart` before any cache-backed investigation
     - `Explore next: Cache-backed streaming` only after the first-chart path renders
     - `Try next: Waterfall proof` when you need the second control proof on the same chart shell; `ScatterChartView` ships in the Avalonia control line, but this demo does not expose a scatter path
4. Export a diagnostics snapshot with `VideraDiagnosticsSnapshotFormatter.Format(View3D.BackendDiagnostics)` or attach the `consumer smoke` `diagnostics-snapshot.txt` artifact; if transparency is involved, include `TransparentFeatureStatus` from that snapshot so support can read the shipped transparency contract.
5. If the issue is visual or inspection-related, attach either:
   - a snapshot exported through `ExportSnapshotAsync(...)`, or
   - an inspection bundle exported through `VideraInspectionBundleService.ExportAsync(...)`
     - check `CanReplayScene` and include `ReplayLimitation` whenever the bundle captured host-owned objects or other non-replayable scene state

## What to include in a bug report

- Operating system and version
- GPU and driver details when native rendering is involved
- Package install path and package version
- `PreferredBackend` or `VIDERA_BACKEND` value, if you overrode backend preference
- diagnostics snapshot from `VideraDiagnosticsSnapshotFormatter`
- exported inspection snapshot, when the issue affects clipping, measurements, labels, or camera state
- inspection bundle directory when you need camera, clipping, measurements, annotations, and imported assets to replay together
- `CanReplayScene` and `ReplayLimitation` from `VideraInspectionBundleService.ExportAsync(...)` whenever the bundle is exportable but not replayable
- `SurfaceCharts support summary` from either the packaged `smoke/Videra.SurfaceCharts.ConsumerSmoke` `surfacecharts-support-summary.txt` artifact or the `Videra.SurfaceCharts.Demo` `Support summary` panel when the issue is in `area: surfacecharts`
- use `Copy support summary` after reproducing `Start here: In-memory first chart`; continue to `Explore next: Cache-backed streaming` only if needed, and use `Try next: Waterfall proof` when the issue involves the second chart control; `ScatterChartView` is shipped in the Avalonia control line, but the demo evidence paths do not include a scatter UI
- SurfaceCharts demo-path choice when relevant:
  - `Start here: In-memory first chart`
  - `Explore next: Cache-backed streaming`
  - `Try next: Waterfall proof`
- `ScatterChartView` is shipped in `Videra.SurfaceCharts.Avalonia`; do not claim the demo exposes a scatter path unless you have a separate repository path to cite
- SurfaceCharts chart state when relevant:
  - `ViewState`
  - `InteractionQuality`
  - `RenderingStatus`
  - `OverlayOptions`
- cache manifest and payload sidecar path when the cache-backed route is involved
- Linux display-server context when relevant:
  - `ResolvedDisplayServer`
  - `DisplayServerFallbackUsed`
  - `DisplayServerFallbackReason`
  - `DisplayServerCompatibility`
- inspection workflow context when relevant:
  - `IsClippingActive`
  - `ActiveClippingPlaneCount`
  - `MeasurementCount`
  - `LastSnapshotExportStatus`
  - active `MeasurementSnapMode`
- Whether the issue reproduces in:
  - your host app
  - `Videra.MinimalSample`
  - `consumer smoke`
  - `smoke/Videra.SurfaceCharts.ConsumerSmoke`
- Smallest asset or scene that still reproduces the problem

## Support boundary reminders

- `Videra.Avalonia` + one matching platform package is the default public install path.
- Linux native hosting is still `X11` plus `XWayland` compatibility, not compositor-native Wayland embedding.
- `ResolvedDisplayServer = X11` means the direct supported X11 host path.
- `ResolvedDisplayServer = XWayland` means a Wayland session is using the documented X11 compatibility bridge, not compositor-native Wayland embedding.
- The `Videra.SurfaceCharts.*` package line is a separate public product family; include the exact package ids involved when the issue is chart-specific.
- `Videra.SurfaceCharts.Avalonia` + `Videra.SurfaceCharts.Processing` is the default public surface/cache-backed install path, not a requirement for every chart path.
- `smoke/Videra.SurfaceCharts.ConsumerSmoke` is the packaged surface/cache-backed proof on the supported host path and emits `surfacecharts-support-summary.txt` for support capture.
- `TransparentFeatureStatus` in diagnostics snapshots captures the shipped transparency contract: alpha mask rendering plus deterministic alpha blend ordering for per-object carried alpha sources.
- `Videra.SurfaceCharts.Demo` remains repository-only and is the support-ready repro/reference app for the `Start here`, `Explore next`, and `Try next` paths.

## Where to send feedback

- **Bug reports:** GitHub Issues with the bug template
- **Feature/API/docs ideas:** GitHub Issues with the feature template
- **Usage questions / design discussion:** GitHub Discussions
- **Private vulnerabilities:** [SECURITY.md](../SECURITY.md) only
