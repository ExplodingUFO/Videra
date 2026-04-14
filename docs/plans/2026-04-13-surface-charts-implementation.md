# Surface Charts Implementation Plan

**Goal:** Build an independent high-performance Avalonia surface-chart control for very large offline matrix datasets, with overview-first LOD, cache-backed data support, an independent demo, and explicit separation from `VideraView`.

**Architecture:** The feature ships as a sibling module family rather than a `VideraView` mode. `Videra.SurfaceCharts.Core` owns chart-domain models and LOD, `Videra.SurfaceCharts.Processing` owns pyramid/cache generation, and `Videra.SurfaceCharts.Avalonia` owns the dedicated control shell plus UI interaction. Existing `Videra` viewer code may share only truly generic rendering substrate; chart semantics must remain separate.

**Tech Stack:** .NET 8, Avalonia 11, existing Videra cross-platform rendering substrate where reusable, xUnit + FluentAssertions, optional future Rust-backed processing behind a .NET contract.

---

## Execution Constraints

- Keep the chart control decoupled from `VideraView`.
- Do not let `SurfaceChartView` or any new controller become god code.
- All code comments and public XML docs must be in English.
- Prefer TDD for every task.
- Prefer `gpt-5.4-mini` for simple, bounded tasks.
- Use a stronger coding model for cross-project or rendering-boundary tasks.
- Update README and sample documentation as part of the implementation, not as an afterthought.
- Extend verification so the new demo and test projects are covered by repository validation.

## Task 1: Scaffold The Module Family

**Suggested worker:** `gpt-5.4-mini`

**Files:**
- Modify: `Videra.slnx`
- Create: `src/Videra.SurfaceCharts.Core/Videra.SurfaceCharts.Core.csproj`
- Create: `src/Videra.SurfaceCharts.Core/README.md`
- Create: `src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj`
- Create: `src/Videra.SurfaceCharts.Avalonia/README.md`
- Create: `src/Videra.SurfaceCharts.Processing/Videra.SurfaceCharts.Processing.csproj`
- Create: `src/Videra.SurfaceCharts.Processing/README.md`
- Create: `samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj`
- Create: `samples/Videra.SurfaceCharts.Demo/README.md`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj`
- Create: `tests/Videra.SurfaceCharts.Processing.Tests/Videra.SurfaceCharts.Processing.Tests.csproj`
- Create: `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj`
- Create: `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryLayoutTests.cs`

**Step 1: Write the failing test**

Add repository-layout assertions in `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryLayoutTests.cs` that check:

- the three new module projects exist
- the new demo exists
- the new test projects exist
- the new module README files exist
- `Videra.slnx` contains all new projects

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsRepositoryLayoutTests
```

Expected: FAIL because the projects and README files do not exist yet.

**Step 3: Write minimal implementation**

Create the new project shells and README placeholders with the intended package names and basic project references only. Update `Videra.slnx` so the new projects participate in solution build/test flow.

**Step 4: Run tests to verify it passes**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsRepositoryLayoutTests
dotnet build Videra.slnx -c Release
```

Expected: the repository-layout test passes and the scaffolded solution builds.

**Step 5: Commit**

```powershell
git add Videra.slnx src/Videra.SurfaceCharts.* samples/Videra.SurfaceCharts.Demo tests/Videra.SurfaceCharts.* tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryLayoutTests.cs
git commit -m "chore: scaffold surface charts module family"
```

## Task 2: Add Core Metadata And Tile Contracts

**Suggested worker:** `gpt-5.4-mini`

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/SurfaceMetadata.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfaceValueRange.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfaceTileKey.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfaceTile.cs`
- Create: `src/Videra.SurfaceCharts.Core/ISurfaceTileSource.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/SurfaceMetadataTests.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/SurfaceTileKeyTests.cs`

**Step 1: Write the failing tests**

Cover:

- metadata validation for width/height/value range
- stable tile-key equality and hash semantics
- tile boundary and shape invariants
- source contract null/argument guards

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceMetadataTests|FullyQualifiedName~SurfaceTileKeyTests
```

Expected: FAIL because the new types do not exist.

**Step 3: Write minimal implementation**

Implement the public core models with XML docs and English comments only where needed to explain non-obvious invariants.

**Step 4: Run tests to verify they pass**

Run the same filtered command and expect PASS.

**Step 5: Commit**

```powershell
git add src/Videra.SurfaceCharts.Core tests/Videra.SurfaceCharts.Core.Tests
git commit -m "feat: add surface chart metadata contracts"
```

## Task 3: Add Viewport And LOD Selection Models

**Suggested worker:** `gpt-5.4-mini`

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/SurfaceViewport.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfaceViewportRequest.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfaceLodSelection.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfaceLodPolicy.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/SurfaceViewportTests.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/SurfaceLodPolicyTests.cs`

**Step 1: Write the failing tests**

Cover:

- viewport clamping and normalization
- mapping zoom density to target pyramid level
- overview-first default behavior
- stable tile request ranges for a viewport

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceViewportTests|FullyQualifiedName~SurfaceLodPolicyTests
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Keep this layer purely mathematical. Do not introduce Avalonia or rendering concerns.

**Step 4: Run tests to verify they pass**

Run the same filtered command and expect PASS.

**Step 5: Commit**

```powershell
git add src/Videra.SurfaceCharts.Core tests/Videra.SurfaceCharts.Core.Tests
git commit -m "feat: add surface viewport and lod policy"
```

## Task 4: Add In-Memory Matrix Source And Temporary Pyramid Builder

**Suggested worker:** `gpt-5.4`

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/SurfaceMatrix.cs`
- Create: `src/Videra.SurfaceCharts.Core/InMemorySurfaceTileSource.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfacePyramidLevel.cs`
- Create: `src/Videra.SurfaceCharts.Core/SurfacePyramidBuilder.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/InMemorySurfaceTileSourceTests.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/SurfacePyramidBuilderTests.cs`

**Step 1: Write the failing tests**

Cover:

- building an overview level from a source matrix
- reading tiles from the source at multiple levels
- preserving metadata consistency across levels
- bounded memory behavior for temporary pyramid generation

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter FullyQualifiedName~InMemorySurfaceTileSourceTests|FullyQualifiedName~SurfacePyramidBuilderTests
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Implement a simple in-process pyramid builder with a conservative downsampling rule. Keep the contract stable enough that `Videra.SurfaceCharts.Processing` can later replace the builder for large datasets.

**Step 4: Run tests to verify they pass**

Run the same filtered command and expect PASS.

**Step 5: Commit**

```powershell
git add src/Videra.SurfaceCharts.Core tests/Videra.SurfaceCharts.Core.Tests
git commit -m "feat: add in-memory surface pyramid source"
```

## Task 5: Add Processing Cache Contracts And File Reader/Writer

**Suggested worker:** `gpt-5.4`

**Files:**
- Create: `src/Videra.SurfaceCharts.Processing/SurfaceCacheHeader.cs`
- Create: `src/Videra.SurfaceCharts.Processing/SurfaceCacheWriter.cs`
- Create: `src/Videra.SurfaceCharts.Processing/SurfaceCacheReader.cs`
- Create: `src/Videra.SurfaceCharts.Processing/SurfaceCacheTileSource.cs`
- Create: `tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCacheRoundTripTests.cs`
- Create: `tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCacheTileSourceTests.cs`

**Step 1: Write the failing tests**

Cover:

- round-trip metadata persistence
- round-trip tile persistence across levels
- reading overview and detail tiles from the same cache
- invalid cache header handling

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Processing.Tests/Videra.SurfaceCharts.Processing.Tests.csproj -c Release
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Implement a simple versioned cache header plus tile payload serialization. Keep the format explicit and testable; do not prematurely optimize into a complex binary container.

**Step 4: Run tests to verify they pass**

Run the same command and expect PASS.

**Step 5: Commit**

```powershell
git add src/Videra.SurfaceCharts.Processing tests/Videra.SurfaceCharts.Processing.Tests
git commit -m "feat: add surface chart cache format"
```

## Task 6: Add Color Maps And Picking Contracts

**Suggested worker:** `gpt-5.4-mini`

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/ColorMaps/SurfaceColorMap.cs`
- Create: `src/Videra.SurfaceCharts.Core/ColorMaps/SurfaceColorMapPalette.cs`
- Create: `src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeResult.cs`
- Create: `src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeRequest.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/SurfaceColorMapTests.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/SurfaceProbeResultTests.cs`

**Step 1: Write the failing tests**

Cover:

- value-to-color mapping at boundaries and midpoint
- palette validation
- probe result coordinate and value semantics

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceColorMapTests|FullyQualifiedName~SurfaceProbeResultTests
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Keep the color-map API simple. Do not over-design a full chart theme system in the first release.

**Step 4: Run tests to verify they pass**

Run the same filtered command and expect PASS.

**Step 5: Commit**

```powershell
git add src/Videra.SurfaceCharts.Core tests/Videra.SurfaceCharts.Core.Tests
git commit -m "feat: add surface color maps and probe models"
```

## Task 7: Add Dedicated Surface Rendering Path

**Suggested worker:** `gpt-5.4`

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/Rendering/SurfacePatchGeometry.cs`
- Create: `src/Videra.SurfaceCharts.Core/Rendering/SurfacePatchGeometryBuilder.cs`
- Create: `src/Videra.SurfaceCharts.Core/Rendering/SurfaceRenderTile.cs`
- Create: `src/Videra.SurfaceCharts.Core/Rendering/SurfaceRenderScene.cs`
- Create: `src/Videra.SurfaceCharts.Core/Rendering/SurfaceRenderer.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfacePatchGeometryBuilderTests.cs`
- Create: `tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceRendererInputTests.cs`

**Step 1: Write the failing tests**

Cover:

- patch topology generation
- shared index pattern expectations
- render input generation from a tile
- handling empty or degenerate tiles safely

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfacePatchGeometryBuilderTests|FullyQualifiedName~SurfaceRendererInputTests
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Build a dedicated surface render path. Do not route this through generic viewer `Object3D` scene management.

**Step 4: Run tests to verify they pass**

Run the same filtered command and expect PASS.

**Step 5: Commit**

```powershell
git add src/Videra.SurfaceCharts.Core tests/Videra.SurfaceCharts.Core.Tests
git commit -m "feat: add dedicated surface rendering path"
```

## Task 8: Add Avalonia Control Shell And Internal Controllers

**Suggested worker:** `gpt-5.4`

**Files:**
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Properties.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartController.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceCameraController.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileScheduler.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileCache.cs`
- Create: `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartViewLifecycleTests.cs`
- Create: `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs`

**Step 1: Write the failing tests**

Cover:

- control initialization without data
- source assignment triggering overview-first requests
- viewport changes requesting new tiles
- controller separation expectations through public behavior rather than private fields

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Keep `SurfaceChartView` as a shell. Put input interpretation, camera behavior, and scheduling in separate classes. Do not mirror `VideraView` internals.

**Step 4: Run tests to verify they pass**

Run the same command and expect PASS.

**Step 5: Commit**

```powershell
git add src/Videra.SurfaceCharts.Avalonia tests/Videra.SurfaceCharts.Avalonia.IntegrationTests
git commit -m "feat: add avalonia surface chart control shell"
```

## Task 9: Add Probe Overlay And Readout Behavior

**Suggested worker:** `gpt-5.4-mini`

**Files:**
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayState.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`
- Modify: `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`
- Create: `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs`

**Step 1: Write the failing tests**

Cover:

- hovered/probed value readout
- no-data overlay state
- overlay response to viewport/source changes

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter FullyQualifiedName~SurfaceChartProbeOverlayTests
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Keep overlay presentation separate from tile scheduling and render-path logic.

**Step 4: Run tests to verify they pass**

Run the same filtered command and expect PASS.

**Step 5: Commit**

```powershell
git add src/Videra.SurfaceCharts.Avalonia tests/Videra.SurfaceCharts.Avalonia.IntegrationTests
git commit -m "feat: add surface chart probe overlay"
```

## Task 10: Add The Independent Demo Application

**Suggested worker:** `gpt-5.4-mini`

**Files:**
- Create: `samples/Videra.SurfaceCharts.Demo/App.axaml`
- Create: `samples/Videra.SurfaceCharts.Demo/App.axaml.cs`
- Create: `samples/Videra.SurfaceCharts.Demo/Program.cs`
- Create: `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml`
- Create: `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`
- Create: `samples/Videra.SurfaceCharts.Demo/Assets/sample-surface-cache/` (or equivalent seed data)
- Modify: `samples/Videra.SurfaceCharts.Demo/README.md`
- Create: `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs`

**Step 1: Write the failing test**

Add sample configuration assertions that check:

- the demo project exists
- the demo references the new Avalonia chart module
- the demo includes both an in-memory example and a cache-backed example entry path

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoConfigurationTests
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Build an independent demo. Do not add this showcase to `Videra.Demo`.

**Step 4: Run tests to verify they pass**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoConfigurationTests
dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj -c Release
```

Expected: PASS and clean demo build.

**Step 5: Commit**

```powershell
git add samples/Videra.SurfaceCharts.Demo tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs
git commit -m "feat: add surface charts demo"
```

## Task 11: Add README, Localization, And Verification Coverage

**Suggested worker:** `gpt-5.4-mini`

**Files:**
- Modify: `README.md`
- Modify: `docs/zh-CN/README.md`
- Create: `docs/zh-CN/modules/videra-surfacecharts-core.md`
- Create: `docs/zh-CN/modules/videra-surfacecharts-avalonia.md`
- Modify: `verify.ps1`
- Modify: `verify.sh`
- Create: `tests/Videra.Core.Tests/Repository/SurfaceChartsDocumentationTerms.cs`
- Create: `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`

**Step 1: Write the failing tests**

Add repository/documentation guards that check:

- README describes the chart modules as independent from `VideraView`
- docs use the intended `SurfaceChartView` vocabulary
- localization coverage includes the new modules
- verification scripts build the new demo

**Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDocumentationTerms|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests|FullyQualifiedName~RepositoryLocalizationTests
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Update English and Chinese documentation, plus verification entrypoints, with explicit boundary language.

**Step 4: Run tests to verify they pass**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDocumentationTerms|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests|FullyQualifiedName~RepositoryLocalizationTests
pwsh -File ./verify.ps1 -Configuration Release
```

Expected: PASS.

**Step 5: Commit**

```powershell
git add README.md docs/zh-CN verify.ps1 verify.sh tests/Videra.Core.Tests/Repository
git commit -m "docs: add surface charts module guidance"
```

## Task 12: Final Integration Verification

**Suggested worker:** `gpt-5.4`

**Files:**
- Modify only if required by verification failures discovered during this task

**Step 1: Run focused project suites**

Run:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release
dotnet test tests/Videra.SurfaceCharts.Processing.Tests/Videra.SurfaceCharts.Processing.Tests.csproj -c Release
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release
dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj -c Release
```

Expected: PASS.

**Step 2: Run full repository verification**

Run:

```powershell
pwsh -File ./verify.ps1 -Configuration Release
```

Expected: PASS.

**Step 3: Fix any issues through the normal TDD loop**

If any command fails:

- add or update the smallest failing test
- make the minimal fix
- rerun the affected command before rerunning the full suite

**Step 4: Confirm clean branch**

Run:

```powershell
git status --short
```

Expected: no output.

**Step 5: Commit final verification-only adjustments if needed**

```powershell
git add -A
git commit -m "test: finalize surface charts verification"
```

Only commit if this task required real code or doc changes.
