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
   - `Videra.SurfaceCharts.Demo` for the source-first chart path:
     - `Start here: In-memory first chart` before any cache-backed investigation
     - `Explore next: Cache-backed streaming` only after the first-chart path renders
4. Export a diagnostics snapshot with `VideraDiagnosticsSnapshotFormatter.Format(View3D.BackendDiagnostics)` or attach the `consumer smoke` `diagnostics-snapshot.txt` artifact.
5. If the issue is visual or inspection-related, attach either:
   - a snapshot exported through `ExportSnapshotAsync(...)`, or
   - a replayable inspection bundle exported through `VideraInspectionBundleService.ExportAsync(...)`

## What to include in a bug report

- Operating system and version
- GPU and driver details when native rendering is involved
- Package install path and package version
- `PreferredBackend` or `VIDERA_BACKEND` value, if you overrode backend preference
- diagnostics snapshot from `VideraDiagnosticsSnapshotFormatter`
- exported inspection snapshot, when the issue affects clipping, measurements, labels, or camera state
- inspection bundle directory when you need camera, clipping, measurements, annotations, and imported assets to replay together
- `SurfaceCharts support summary` copied from the `Videra.SurfaceCharts.Demo` `Support summary` panel when the issue is in `area: surfacecharts`
- use `Copy support summary` after reproducing `Start here: In-memory first chart`; continue to `Explore next: Cache-backed streaming` only if needed
- SurfaceCharts source-path choice when relevant:
  - `Start here: In-memory first chart`
  - `Explore next: Cache-backed streaming`
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
- Smallest asset or scene that still reproduces the problem

## Support boundary reminders

- `Videra.Avalonia` + one matching platform package is the default public install path.
- Linux native hosting is still `X11` plus `XWayland` compatibility, not compositor-native Wayland embedding.
- `ResolvedDisplayServer = X11` means the direct supported X11 host path.
- `ResolvedDisplayServer = XWayland` means a Wayland session is using the documented X11 compatibility bridge, not compositor-native Wayland embedding.
- `Videra.SurfaceCharts.*` remains source-first and is not part of the public package promise.

## Where to send feedback

- **Bug reports:** GitHub Issues with the bug template
- **Feature/API/docs ideas:** GitHub Issues with the feature template
- **Usage questions / design discussion:** GitHub Discussions
- **Private vulnerabilities:** [SECURITY.md](../SECURITY.md) only
