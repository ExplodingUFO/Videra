# Alpha Feedback

Videra is still an early alpha embeddable viewer stack. Good feedback is not just "it failed" — it needs enough integration context to reproduce the host, backend, and package path that produced the failure.

## Support artifact routing

Attach the smallest artifact set that explains the failure path:

| Situation | Primary artifact |
| --- | --- |
| Repository state or local setup | Attach `artifacts/doctor/doctor-report.json` and `artifacts/doctor/doctor-summary.txt` from `Videra Doctor`. |
| Viewer issue | Reproduce with `Videra.MinimalSample` and attach the diagnostics snapshot from `VideraDiagnosticsSnapshotFormatter`. |
| Import issue | Use `Videra.Demo` and attach the copied diagnostics bundle, import report, and smallest failing scene path. |
| Backend issue | Attach `artifacts/doctor/doctor-report.json`, the diagnostics snapshot, and `LastInitializationError` from copied diagnostics when startup is not-ready; include `FallbackReason` only if software fallback was explicitly enabled and actually selected. |
| Visual rendering or Performance Lab issue | Attach `artifacts/doctor/doctor-report.json`, `artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json`, `performance-lab-visual-evidence-summary.txt`, the relevant PNG files, and per-scenario diagnostics text. |
| Package issue | Attach `release-dry-run-summary.json`, `release-candidate-evidence-index.json`, `package-size-evaluation.json`, and `package-size-summary.txt` when the report concerns package metadata, package size budgets, or package contract drift; the evidence index also carries optional Doctor/Performance Lab visual evidence status when visual output context is relevant. |
| Release issue | Attach `public-release-preflight-summary.json`, `public-publish-before-summary.json`, `public-publish-after-summary.json`, and `public-release-notes.md`; include the Package matrix and Known alpha limitations section that shipped with the release. |
| Native-host issue | Attach the matching `artifacts/native-validation` output plus `artifacts/doctor/doctor-report.json`; include `wpf-smoke-diagnostics.txt` when the Windows WPF smoke path was involved. |
| Packaged viewer validation | Attach `artifacts/consumer-smoke/consumer-smoke-result.json` and `artifacts/consumer-smoke/diagnostics-snapshot.txt`. |
| SurfaceCharts issue | Attach `surfacecharts-support-summary.txt` from `smoke/Videra.SurfaceCharts.ConsumerSmoke` or the copied `Videra.SurfaceCharts.Demo` support summary; keep it separate from `VideraDiagnosticsSnapshotFormatter` output. |

SurfaceCharts support summaries are support evidence, not benchmark results, pixel-perfect visual-regression gates, or GPU performance guarantees. A structured summary must include these prefixes: `GeneratedUtc`, `EvidenceKind`, `EvidenceOnly`, `ChartControl`, `EnvironmentRuntime`, `AssemblyIdentity`, `BackendDisplayEnvironment`, and `RenderingStatus`.

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
  - `Videra.SurfaceCharts.Demo` for the repository-only chart repro/reference path:
  - `Start here: In-memory first chart` before any cache-backed investigation
  - `Explore next: Cache-backed streaming` only after the first-chart path renders
  - `Try next: Analytics proof` when you need explicit-coordinate, independent-`ColorField` probe behavior for analytic path checks
  - `Try next: Waterfall proof` when you need the second control proof on the same chart shell; `Try next: Scatter proof` when you need the repo-owned scatter proof path on the same chart shell
4. Export a diagnostics snapshot with `VideraDiagnosticsSnapshotFormatter.Format(View3D.BackendDiagnostics)` or attach the `consumer smoke` `diagnostics-snapshot.txt` artifact; if transparency is involved, include `TransparentFeatureStatus` from that snapshot so support can read the shipped transparency contract. If scene composition matters, include `LastFrameObjectCount`, `LastFrameOpaqueObjectCount`, and `LastFrameTransparentObjectCount`; those are backend-neutral scene counts, not draw-call metrics. If performance matters, include the metric source lines: draw-call, instance, vertex, and pickable-object counts may be `Unavailable`, upload bytes are measured by scene residency, and resident resource bytes are estimated from residency records. If a snapshot export was involved, include `LastSnapshotExportPath` and `LastSnapshotExportStatus` too.
5. If the issue is about imported-material fidelity, include the smallest reproducer plus the relevant imported-asset/runtime details: per-primitive material participation, baseColor texture use, occlusion texture binding/strength, `KHR_texture_transform` offset/scale/rotation, and texture-coordinate override. Those fields now affect shipped renderer output on the static-scene path.
6. If the issue is visual or inspection-related, attach either:
   - a snapshot exported through `ExportSnapshotAsync(...)`, or
   - an inspection bundle exported through `VideraInspectionBundleService.ExportAsync(...)`
     - check `CanReplayScene` and include `ReplayLimitation`; they describe replayability semantics and should travel with the bundle whenever it captured host-owned objects or other non-replayable scene state
7. If the issue involves Performance Lab visual output, generate the visual evidence bundle with `scripts/Invoke-PerformanceLabVisualEvidence.ps1`, rerun `scripts/Invoke-VideraDoctor.ps1`, and attach the Doctor report plus `performance-lab-visual-evidence-manifest.json`, `performance-lab-visual-evidence-summary.txt`, relevant PNG files, and per-scenario diagnostics text.
8. For release-candidate or package reports, attach `release-candidate-evidence-index.json` after the release dry run; use its `visualEvidence.performanceLabVisualEvidence` and `visualEvidence.doctorVisualEvidence` fields to show whether visual evidence was present, missing, or unavailable.

## What to include in a bug report

- Operating system and version
- GPU and driver details when native rendering is involved
- Package install path and package version
- Release issue evidence when the report concerns publication, GitHub Release assets, or public package availability: `public-release-preflight-summary.json`, `public-publish-after-summary.json`, `public-release-notes.md`, the Package matrix section, and the Known alpha limitations section
- `PreferredBackend` or `VIDERA_BACKEND` value, if you overrode backend preference
- diagnostics snapshot from `VideraDiagnosticsSnapshotFormatter`
- Performance Lab visual evidence bundle when visual rendering evidence is relevant: `artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json`, `performance-lab-visual-evidence-summary.txt`, relevant PNG files, and per-scenario diagnostics text
- `LastFrameObjectCount`, `LastFrameOpaqueObjectCount`, and `LastFrameTransparentObjectCount` when the issue depends on scene composition
- `LastSnapshotExportPath` and `LastSnapshotExportStatus` when the report includes a snapshot export
- exported inspection snapshot, when the issue affects clipping, measurements, labels, or camera state
- inspection bundle directory when you need camera, clipping, measurements, annotations, and imported assets to replay together
- `CanReplayScene` and `ReplayLimitation` from `VideraInspectionBundleService.ExportAsync(...)` whenever the bundle is exportable but not replayable
- `SurfaceCharts support summary` from either the packaged `smoke/Videra.SurfaceCharts.ConsumerSmoke` `surfacecharts-support-summary.txt` artifact or the `Videra.SurfaceCharts.Demo` `Support summary` panel when the issue is in `area: surfacecharts`; keep it separate from `VideraDiagnosticsSnapshotFormatter` output
- use `Copy support summary` after reproducing `Start here: In-memory first chart`; continue to `Explore next: Cache-backed streaming` only if needed, and use `Try next: Analytics proof` for explicit-coordinate pinned-probe/analysis scenarios, `Try next: Waterfall proof` when the issue involves the second chart control, or `Try next: Scatter proof` when the issue involves the scatter control path; `ScatterChartView` is shipped in the Avalonia control line
- include the SurfaceCharts summary fields `GeneratedUtc`, `EvidenceKind`, `EvidenceOnly`, `ChartControl`, `EnvironmentRuntime`, `AssemblyIdentity`, `BackendDisplayEnvironment`, and `RenderingStatus`; include `CacheLoadFailure` when the cache-backed path falls back
- SurfaceCharts demo-path choice when relevant:
  - `Start here: In-memory first chart`
  - `Explore next: Cache-backed streaming`
  - `Try next: Analytics proof`
  - `Try next: Waterfall proof`
  - `Try next: Scatter proof`
- `ScatterChartView` is shipped in `Videra.SurfaceCharts.Avalonia`; the repository-owned `Try next: Scatter proof` path is the demo path to cite
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
- Linux native hosting is still `X11` plus the documented `XWayland` bridge, not compositor-native Wayland embedding.
- `ResolvedDisplayServer = X11` means the direct supported X11 host path.
- `ResolvedDisplayServer = XWayland` means a Wayland session is using the documented X11 bridge, not compositor-native Wayland embedding.
- The `Videra.SurfaceCharts.*` package line is a separate public product family; include the exact package ids involved when the issue is chart-specific.
- `Videra.SurfaceCharts.Avalonia` + `Videra.SurfaceCharts.Processing` is the default public surface/cache-backed install path, not a requirement for every chart path.
- `smoke/Videra.SurfaceCharts.ConsumerSmoke` is the packaged surface/cache-backed proof on the supported host path and emits `surfacecharts-support-summary.txt` for support capture.
- `TransparentFeatureStatus` in diagnostics snapshots captures the shipped transparency contract: alpha mask rendering plus deterministic alpha blend ordering for per-primitive carried alpha sources.
- `LastFrameObjectCount`, `LastFrameOpaqueObjectCount`, and `LastFrameTransparentObjectCount` are backend-neutral scene diagnostics, not draw-call metrics.
- `LastFrameDrawCallCount`, `LastFrameInstanceCount`, `LastFrameVertexCount`, and `PickableObjectCount` are `Unavailable` when the active backend path does not measure them; `LastFrameUploadBytes` is measured by scene residency, and `ResidentResourceBytes` is a residency estimate.
- Imported-material fidelity now reaches the shipped static-scene renderer path for baseColor texture sampling, occlusion texture binding/strength, emissive inputs, normal-map-ready inputs, and `KHR_texture_transform` / texture-coordinate override. Treat those details as shipped on-screen output concerns on the bounded static-scene seam, not as a broader lighting/shader/backend promise.
- `Videra.SurfaceCharts.Demo` remains repository-only and is the support-ready repro/reference app for the `Start here`, `Explore next`, and `Try next` paths.
- Performance Lab visual evidence is support/review evidence only. It is not a pixel-perfect visual-regression gate, stable benchmark guarantee, real GPU instancing proof, renderer parity proof, or new chart-family promise.
- Release-candidate evidence index visual evidence fields are optional support context. Missing or unavailable visual evidence should be classified as environment residual or not-run context unless the specific release issue depends on visual output.
- `FallbackReason` in diagnostics is fallback-specific evidence; default native startup failures without fallback should be framed as initialization/not-ready state via `IsReady = false` and `LastInitializationError`.

## Where to send feedback

- **Bug reports:** GitHub Issues with the bug template
- **Feature/API/docs ideas:** GitHub Issues with the feature template
- **Usage questions / design discussion:** GitHub Discussions
- **Private vulnerabilities:** [SECURITY.md](../SECURITY.md) only
