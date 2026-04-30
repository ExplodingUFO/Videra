# Phase 425C Streaming Performance Inventory

Task: `Videra-7tqx.1.3`
Scope: read-only inventory of streaming, live data, high-density, cache/window, PerformanceLab, benchmark/evidence, and support-summary surfaces relevant to v2.64.
Boundary: this document does not modify Beads state or production/test code.

## Owners And Files

| Area | Owner surface | Concrete files | Current role |
| --- | --- | --- | --- |
| Live scatter facade | `DataLogger3D` | `src/Videra.SurfaceCharts.Core/DataLogger3D.cs`; `tests/Videra.SurfaceCharts.Core.Tests/ScatterDataLogger3DTests.cs` | Mutable columnar live-data facade with append/replace, optional FIFO retention, latest-window evidence, and autoscale decision evidence. |
| High-density columnar scatter | `ScatterColumnarSeries`, `ScatterChartData` | `src/Videra.SurfaceCharts.Core/ScatterColumnarSeries.cs`; `src/Videra.SurfaceCharts.Core/ScatterChartData.cs`; `src/Videra.SurfaceCharts.Avalonia/Controls/ScatterChartRenderingStatus.cs` | Columnar storage, high-volume default `Pickable=false`, append/replace counters, retained point counts, FIFO dropped counts, and rendering-status projection. |
| Scatter demo scenarios | `ScatterStreamingScenarios`, evidence helpers | `samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingScenario.cs`; `samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingEvidence.cs`; `tests/Videra.Core.Tests/Samples/ScatterStreamingScenarioEvidenceTests.cs`; `tests/Videra.Core.Tests/Samples/PerformanceLabScenarioTests.cs` | Deterministic 100k replace, append, and FIFO-trim scenarios with evidence-only assertions for retained counts and dropped FIFO truth. |
| Surface cache ingestion | Surface cache reader/source | `src/Videra.SurfaceCharts.Processing/SurfaceCacheReader.cs`; `src/Videra.SurfaceCharts.Processing/SurfaceCacheTileSource.cs`; `src/Videra.SurfaceCharts.Processing/SurfaceCacheWriter.cs`; `tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCache*.cs` | Manifest+payload cache validation and lazy tile reads through `ISurfaceTileSource` / `ISurfaceTileBatchSource`. |
| View window and tile scheduling | `SurfaceDataWindow`, tile scheduler/cache | `src/Videra.SurfaceCharts.Core/SurfaceDataWindow.cs`; `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileScheduler.cs`; `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileCache.cs`; `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs` | Data-window contract, interaction-aware tile priority, generation-aware request/store logic, and pruning to retained visible/overview tile keys. |
| Rendering status/support truth | Chart rendering and support summaries | `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderingStatus.cs`; `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs`; `samples/Videra.SurfaceCharts.Demo/README.md`; `docs/alpha-feedback.md`; `docs/troubleshooting.md`; `docs/support-matrix.md` | Public support evidence for backend readiness, fallback state, native host usage, resident/visible tiles, resident bytes, dataset evidence, scenario metadata, snapshot status, and cache-load failure. |
| Performance Lab proof surface | Viewer and visual evidence | `samples/Videra.Demo/Services/PerformanceLabViewerScenario.cs`; `samples/Videra.Demo/Services/PerformanceLabEvidenceSnapshotBuilder.cs`; `samples/Videra.Demo/Views/MainWindow.axaml.cs`; `tools/Videra.PerformanceLabVisualEvidence/PerformanceLabVisualEvidenceCapture.cs`; `scripts/Invoke-PerformanceLabVisualEvidence.ps1`; `tests/Videra.Core.Tests/Repository/PerformanceLabVisualEvidenceTests.cs` | Deterministic small/medium/large viewer instance-batch datasets and evidence-only visual bundles, including scatter 100k PNG/diagnostics entries. |
| Benchmark truth | Benchmark suite and contracts | `benchmarks/Videra.SurfaceCharts.Benchmarks/SurfaceChartsStreamingBenchmarks.cs`; `benchmarks/Videra.SurfaceCharts.Benchmarks/SurfaceChartsCacheBenchmarks.cs`; `benchmarks/benchmark-contract.json`; `benchmarks/benchmark-thresholds.json`; `scripts/Run-Benchmarks.ps1`; `scripts/Test-BenchmarkThresholds.ps1`; `docs/benchmark-gates.md`; `tests/Videra.Core.Tests/Repository/BenchmarkContractRepositoryTests.cs` | Benchmark contract lists streaming/cache measurements; hard thresholds currently cover only selected viewer and SurfaceCharts slices, while streaming benchmarks are evidence-only. |
| Public docs/cookbook | Consumer-facing guidance | `README.md`; `samples/Videra.SurfaceCharts.Demo/Recipes/scatter-and-live-data.md`; `samples/Videra.SurfaceCharts.Demo/Recipes/surface-cache-backed.md`; `docs/zh-CN/README.md`; `docs/zh-CN/modules/videra-surfacecharts-processing.md` | Documents current streaming/cache semantics and explicit evidence-only/non-goal boundaries. |

## Existing Reusable Evidence

- `DataLogger3D` is the reusable live-data contract. It wraps one `ScatterColumnarSeries`, exposes retained count, append/replace counts, FIFO dropped counts, and `CreateLiveViewEvidence()` with `FullData` versus `LatestWindow` visible-window/autoscale truth.
- `ScatterColumnarSeries` already supports high-density append/replace via column arrays, optional positive `fifoCapacity`, default `pickable: false`, sorted-X validation, finite-coordinate validation, and FIFO trimming counters.
- `ScatterChartData` aggregates high-density diagnostics into dataset-level counters: columnar series count, retained point count, pickable point count, append/replace batch count, dropped FIFO points, last dropped FIFO points, and configured FIFO capacity.
- The demo has deterministic large scatter evidence through `scatter-replace-100k`, `scatter-append-100k`, and `scatter-fifo-trim-100k`; tests assert exact retained counts, update modes, FIFO capacity, pickability, and dropped-point totals.
- `SurfaceDemoSupportSummary` already projects scatter streaming evidence into copyable support summaries with scenario id/name/update mode, point counts, FIFO capacity, pickability, rendering status, dataset evidence, snapshot status, and `EvidenceOnly: true`.
- Cache-backed surface support exists through `SurfaceCacheReader` plus `SurfaceCacheTileSource`. The demo loads the committed manifest/sidecar path and keeps cache-load failure explicit instead of switching the Plot path silently.
- `SurfaceTileScheduler` and `SurfaceTileCache` already provide the chart-local request-window behavior: overview retention, interaction-aware prioritization, batch requests when the source supports them, generation checks, requested/loaded tile state, and pruning to retained keys.
- Performance Lab already provides deterministic viewer instance-batch scenarios (`viewer-instance-small`, `viewer-instance-medium`, `viewer-instance-large`) and an evidence-only snapshot builder. Visual evidence capture also includes SurfaceCharts scatter 100k scenarios and writes manifest, summary, PNG, and diagnostics files.
- Benchmark infrastructure already has a source-controlled contract, artifact manifests, and hard-threshold evaluation. `SurfaceChartsStreamingBenchmarks.AppendColumnarBatch`, `AppendColumnarBatch_WithFifoTrim`, and `StreamingDiagnosticsAggregation` are contract-listed evidence-only measurements.

## Gaps

- Ingestion truth is still split by surface family: cache-backed surface ingestion has manifest/payload validation, while scatter streaming uses in-memory deterministic scenarios. There is no single v2.64 inventory artifact that states which ingestion path owns live append, cache-backed tile reads, and large offline data.
- `DataLogger3D.UseLatestWindow(...)` reports visible-window evidence, but the current scatter render path still hands `ScatterChartData` the retained `Series`; the inventory should not imply an implemented renderer-side window crop unless Phase 428 explicitly adds/validates it.
- Cache status is visible indirectly through `CacheLoadFailure`, `RenderingStatus`, resident/visible tile counts, and support summary text. There is no richer public cache/window status for pending tile requests, cache hits/misses, pruned tile count, or current request generation.
- Large-data scenario evidence is deterministic but bounded: scatter scenarios are 100k/125k retained points, viewer Performance Lab large is 10k instance-batch objects, and cache-backed demo uses a committed sample cache. There is not yet evidence for real external high-density ingestion, multi-million point retention, or oversized cache manifests.
- Performance truth is intentionally limited. Streaming benchmarks are evidence-only and absent from `benchmark-thresholds.json`; Performance Lab visual evidence and support summaries explicitly are not benchmark guarantees.
- Benchmark coverage does not yet connect every user-facing scenario to a numeric measurement. For example, demo support-summary scatter status, visual evidence rendering, cache-backed interactive window changes, and latest-window evidence are not all hard-thresholded.
- Surface cache measurements are benchmarked through synthetic benchmark context/cache files; they do not prove end-user disk, filesystem, GPU driver, compositor, or OS cache behavior.

## Risks And Non-Goals

- Do not claim fake benchmark truth. Support summaries, Performance Lab snapshots, and visual evidence are evidence-only unless backed by `scripts/Run-Benchmarks.ps1` artifacts and, for hard gates, `scripts/Test-BenchmarkThresholds.ps1`.
- Do not hide data-path substitution. If cache-backed loading fails, keep the failure explicit and leave the previous Plot path active; do not silently downshift to in-memory data and call it cache evidence.
- Do not expand backend scope. v2.64 streaming/performance evidence should stay chart-local and viewer-demo-local; no renderer rewrite, GPU-driven culling promise, compositor claim, or new backend support should be implied.
- Do not introduce downshift behavior. Unsupported output, missing cache payloads, unavailable visual capture, and fallback/native-host state must remain explicit diagnostics rather than silent success paths.
- Do not treat `Pickable=false` as a loss of correctness. It is the current high-volume default; per-point hit participation must be explicit workflow scope.
- Do not promote evidence-only streaming benchmarks into a release blocker without stable CI history and updated threshold budgets.

## Candidate Phase 428 Write Scopes

| Candidate scope | Likely files | Why it is safe/valuable | Parallelization notes |
| --- | --- | --- | --- |
| Add streaming inventory docs to release planning | `.planning/phases/...`; `docs/benchmark-gates.md` if public wording is needed | Clarifies evidence-only versus hard performance truth without touching runtime. | Safe to parallelize with code work if write sets stay docs-only and distinct. |
| Strengthen support summary cache/window fields | `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs`; matching sample tests | Could add explicit current data-window/cache-source/status wording without changing data paths. | Parallelize with benchmark work only after field names are agreed; tests will touch sample/repository assertions. |
| Add focused latest-window render/evidence tests | `tests/Videra.SurfaceCharts.Core.Tests/ScatterDataLogger3DTests.cs`; possible `SurfaceChartsCookbookScatterLiveRecipeTests.cs` | Safe if limited to existing `CreateLiveViewEvidence()` semantics and docs, not renderer cropping. | Can run in parallel with cache-status docs because code paths are separate. |
| Add cache/window scheduler evidence tests | `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs`; `SurfaceTileScheduler` only if a real gap is found | Validates visible-window tile selection, retained keys, and batch-source behavior. | Parallelize with scatter streaming work; avoid touching processing cache files unless required. |
| Add benchmark evidence run notes/artifact index | `docs/benchmark-gates.md`; benchmark artifact docs; possibly release evidence index scripts if explicitly scoped | Improves performance truth traceability while preserving no-fake-claims boundary. | Docs/script work can parallelize with tests; threshold promotion should be serialized with CI history review. |
| Add or adjust benchmark methods | `benchmarks/Videra.SurfaceCharts.Benchmarks/*`; `benchmarks/benchmark-contract.json`; benchmark contract tests | Useful for measuring cache window changes or latest-window aggregation if Phase 428 needs numeric evidence. | Safe to parallelize from demo UI work, but serialize changes to `benchmark-contract.json` and threshold docs. |
| Generate Performance Lab visual evidence | `scripts/Invoke-PerformanceLabVisualEvidence.ps1` output under `artifacts/...` | Safe evidence collection; no code changes needed. | Can run independently of source edits; should not be committed unless the phase explicitly asks for artifacts. |

## Suggested Focused Validation

Targeted repository discovery:

```powershell
rg -n "DataLogger3D|CreateLiveViewEvidence|UseLatestWindow|ScatterColumnarSeries|AppendRange|ReplaceRange|fifoCapacity|Pickable=false" src tests samples docs README.md -S
rg -n "SurfaceChartsStreamingBenchmarks|AppendColumnarBatch|AppendColumnarBatch_WithFifoTrim|StreamingDiagnosticsAggregation|benchmark-contract|benchmark-thresholds|evidence-only" benchmarks docs tests README.md -S
rg -n "SurfaceCacheReader|SurfaceCacheTileSource|SurfaceDataWindow|SurfaceTileScheduler|SurfaceTileCache|CacheLoadFailure|ResidentTileCount|VisibleTileCount" src tests samples docs README.md -S
rg -n "Performance Lab|PerformanceLab|visual evidence|scatter-replace-100k|viewer-instance-large|StableBenchmarkGuarantee|support summary" samples tools scripts docs tests README.md -S
```

Focused test candidates if Phase 428 changes runtime/testable behavior:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~ScatterDataLogger3DTests"
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "FullyQualifiedName~ScatterStreamingScenarioEvidenceTests|FullyQualifiedName~PerformanceLabScenarioTests|FullyQualifiedName~BenchmarkContractRepositoryTests|FullyQualifiedName~SurfaceChartsPerformanceTruthTests"
dotnet test tests/Videra.SurfaceCharts.Processing.Tests/Videra.SurfaceCharts.Processing.Tests.csproj
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartTileSchedulingTests"
```

Evidence-generation commands:

```powershell
pwsh -File ./scripts/Run-Benchmarks.ps1 -Suite SurfaceCharts -Configuration Release
pwsh -File ./scripts/Test-BenchmarkThresholds.ps1 -Suite SurfaceCharts -ArtifactsRoot artifacts/benchmarks/surfacecharts
pwsh -File ./scripts/Invoke-PerformanceLabVisualEvidence.ps1 -Configuration Release -OutputRoot artifacts/performance-lab-visual-evidence
pwsh -File ./scripts/Invoke-VideraDoctor.ps1
```

Closeout hygiene:

```powershell
git diff --check
git status --short --branch
```
