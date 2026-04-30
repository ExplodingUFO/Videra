# Phase 300 Inventory: Redundancy and Fallback Reduction

## Scope

This inventory classifies compatibility seams, legacy adapters, fallback/downshift behavior, deprecated APIs, and god-code hotspots for v2.40. It separates documented product fallback from hidden "new path failed, use old path" behavior.

## Classification Legend

- `intentional product boundary`: documented behavior that should not be removed in this milestone without a product decision.
- `removable debt`: likely safe target for a removal or hardening phase.
- `needs evidence`: plausible target, but removal needs consumer/API/test evidence first.
- `false positive`: keyword hit that is not compatibility/fallback debt.

## Viewer and Backend Seams

| Finding | Classification | Evidence | Recommendation |
|---------|----------------|----------|----------------|
| `VideraEngine.UpdateMesh(MeshData)` in `src/Videra.Core/Graphics/VideraEngine.cs` | removable debt | Single `[Obsolete]` method; `rg` found no calls outside its declaration; body only logs warning and ignores input. | Phase 301 should remove this method and its logger if public API contract permits. Verify Core graphics/API tests and public contract tests. |
| `LegacyGraphicsBackendAdapter` in `src/Videra.Core/Graphics/Abstractions/LegacyGraphicsBackendAdapter.cs` | needs evidence | `RenderSessionOrchestrator` wraps any `IGraphicsBackend` that is not already `IGraphicsDevice`; used as a bridge for old monolithic backend contract. | Do not remove blindly. Phase 301 should first prove all shipped backends implement `IGraphicsDevice`; if external backend compatibility is no longer supported, narrow the fallback creation path and update docs/tests. |
| `AllowSoftwareFallback` / `FallbackReason` viewer path | intentional product boundary | Public docs, diagnostics, consumer smoke, and tests treat software fallback as explicit diagnostic/runtime behavior. | Keep. Do not treat as hidden downshift unless a call path suppresses failure without diagnostics. |
| XWayland display compatibility path | intentional product boundary | Linux docs and diagnostics explicitly distinguish XWayland compatibility from compositor-native Wayland. | Keep. It is platform support truth, not accidental fallback. |
| Advanced backend APIs throwing `UnsupportedOperationException` | intentional product boundary with guardrails | Docs require `RenderCapabilities` guards for shader/resource-set seams; backend tests assert unsupported operations. | Keep for now. Future cleanup could make unsupported seams less visible, but not part of Phase 301 unless API contract changes. |
| `VideraViewSessionBridge` | intentional product boundary / god-code risk | Internal bridge between `VideraView`, `RenderSession`, overlays, diagnostics, and session lifecycle; not a legacy/fallback seam. | Do not remove as compatibility debt. If touched later, split diagnostics/overlay builders rather than replacing it with another large coordinator. |
| `NativeLibraryHelper.TryLoadWithFallback` | false positive | Alternate native soname probing, not product downgrade or old-renderer fallback. | No v2.40 action. |

## SurfaceCharts Seams

| Finding | Classification | Evidence | Recommendation |
|---------|----------------|----------|----------------|
| `SurfaceChartView.Viewport` compatibility bridge beside `ViewState` | needs evidence | `ViewportProperty`, `OnViewportChanged`, and `SynchronizeViewStateProperties` intentionally mirror the old viewport shell; docs and repository tests currently promise this compatibility bridge. | Do not remove in Phase 302 unless docs/tests are updated and public API impact is accepted. Candidate for future deprecation, not immediate deletion. |
| `LoadAndApplyCacheSourceAsync` catch applies `_inMemorySource` and sets selector back to `0` after cache failure | removable debt | `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` catches all exceptions and reverts to "Start here: In-memory first chart" with "Start here fallback." | Phase 302 should stop silently changing the selected source and active data source. Keep explicit error text/support summary, but leave the failed selection visible. |
| SurfaceCharts render-host GPU-to-software fallback | intentional product boundary | Rendering status exposes `IsFallback` and `FallbackReason`; docs describe GPU-first with explicit software fallback. | Keep. This is user-visible diagnostics, not hidden downshift. |
| `SurfaceChartRenderHost.CreateDefaultGpuBackend()` catches backend creation failures and returns `null` | removable debt | GPU discovery failure silently creates a normal software render host path before a render status can report the discovery failure. | Phase 302 should preserve discovery failure in `RenderingStatus.FallbackReason` or fail explicitly instead of treating it as clean software mode. |
| `SurfaceTileScheduler` catches tile request exceptions and raises internal events | needs evidence | Failures are not obviously public diagnostics for support unless the view projects them through `LastTileFailure`/support summaries. | Phase 302 can defer unless touching active tile-failure diagnostics. |
| `CacheLoadFailure` support summary field | intentional diagnostic | Captures the cache load error for support. | Keep, and reuse it when hardening the cache path. |

## God-Code Hotspots

| Finding | Classification | Evidence | Recommendation |
|---------|----------------|----------|----------------|
| `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` | god-code risk | ~903 lines; mixes scenario definitions, cache load, support summary, selection wiring, chart switching, and UI text. | Phase 303 should extract a narrow helper from this file only if it overlaps with Phase 302. Best first target: cache source loading/error state or support-summary formatting. |
| Large repository tests under `tests/Videra.Core.Tests/Repository/*` | acceptable large test fixtures | Many are contract/guardrail aggregations. | Avoid refactoring in this milestone unless adding a size guardrail. |
| `src/Videra.Platform.Linux/VulkanBackend.cs` and resource factories | acceptable large implementation / needs evidence | Platform interop and native resource code is naturally verbose. | Do not split without a focused native-backend phase. |
| `src/Videra.Import.Gltf/GltfModelImporter.cs` | needs evidence | Single importer implementation with parsing/material/runtime mapping responsibilities. | Not first target; importer changes carry format-regression risk. |
| `src/Videra.Avalonia/Controls/VideraInspectionBundleService.cs` | needs evidence | Medium-large service with export/bundle responsibilities. | Candidate only if future inspection work touches it. |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs` | god-code risk | Mixes render-host sizing, shared resources, tile residency, release queue, color-map upload, frame rendering, and normal derivation. | Candidate for a future rendering-specific refactor, not first v2.40 target. |
| Platform resource factories | god-code risk / needs evidence | Large native resource factories mix buffer/pipeline creation and inline shader/pipeline details. | Defer unless native backend work is active. |

## Recommended Phase Targets

1. Phase 301: remove `VideraEngine.UpdateMesh(MeshData)` and its logger. Defer `LegacyGraphicsBackendAdapter` removal until a separate explicit external-backend compatibility decision.
2. Phase 302: harden SurfaceCharts demo cache-backed load failure so it no longer silently reverts to the in-memory start-here path; also inspect `SurfaceChartRenderHost.CreateDefaultGpuBackend()` for a focused discovery-failure diagnostic fix if small.
3. Phase 303: add a repository guardrail for god-code hotspots and/or extract the cache-source failure handling touched by Phase 302, keeping the split surgical.

## Verification Map

- Phase 301: `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore -m:1 --filter "FullyQualifiedName~VideraEngine|FullyQualifiedName~RepositoryArchitectureTests|FullyQualifiedName~PublicApi"`
- Phase 302: `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore -m:1 --filter "FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests"`
- Phase 303: focused repository test for file-size/known-hotspot guardrail, or tests for any extracted helper.
