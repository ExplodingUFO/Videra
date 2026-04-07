# Library Usability Implementation Plan

**Goal:** Add typed viewer configuration, explicit backend diagnostics, and high-level scene and camera APIs so host apps can use `VideraView` without depending on low-level engine details.

**Architecture:** Keep the rendering core and native backends mostly intact. Add a thin usability facade in `Videra.Avalonia`, plus minimal geometry support in `Videra.Core` for framing operations. Preserve compatibility by keeping existing properties and `Engine` available while moving documentation and examples to the new API.

**Tech Stack:** .NET 8, Avalonia, existing Videra core/rendering abstractions, xUnit, FluentAssertions

---

### Task 1: Establish Options And Diagnostics Contracts

**Files:**
- Create: `src/Videra.Avalonia/Controls/VideraViewOptions.cs`
- Create: `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- Create: `src/Videra.Avalonia/Controls/ViewPreset.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Modify: `src/Videra.Avalonia/Rendering/RenderSession.cs`
- Modify: `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
- Test: `tests/Videra.Core.Tests/Graphics/GraphicsBackendFactoryTests.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`

**Step 1: Write failing tests for configuration precedence and diagnostics**

Add tests that verify:

- typed backend options can request software without relying on `VIDERA_BACKEND`
- environment overrides are ignored by default
- diagnostics record requested backend and resolved backend
- diagnostics record fallback to software when native resolution is unavailable

Example assertions:

```csharp
diagnostics.RequestedBackend.Should().Be(GraphicsBackendPreference.Vulkan);
diagnostics.ResolvedBackend.Should().Be(GraphicsBackendPreference.Software);
diagnostics.IsUsingSoftwareFallback.Should().BeTrue();
diagnostics.FallbackReason.Should().NotBeNullOrWhiteSpace();
```

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~GraphicsBackendFactoryTests"
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RenderSessionIntegrationTests"
```

Expected: failures because typed options/diagnostics do not exist yet.

**Step 3: Implement options and diagnostics contracts**

Add:

- `VideraViewOptions`
- nested or sibling backend/input/diagnostics option objects
- `VideraBackendDiagnostics` with immutable snapshot fields
- explicit environment override policy

Refactor backend creation flow so `RenderSession` passes a structured request and receives a structured result instead of relying on implicit environment parsing inside multiple call sites.

**Step 4: Expose diagnostics from `VideraView`**

Add:

- `public VideraViewOptions Options { get; set; }`
- `public VideraBackendDiagnostics BackendDiagnostics { get; }`
- `BackendStatusChanged`
- `InitializationFailed`

Keep current public properties working, but define and document precedence:

1. explicit compatibility properties
2. `Options`
3. environment overrides, only when allowed

**Step 5: Run targeted tests again**

Run the same commands from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add src/Videra.Avalonia/Controls/VideraViewOptions.cs src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs src/Videra.Avalonia/Controls/ViewPreset.cs src/Videra.Avalonia/Controls/VideraView.cs src/Videra.Avalonia/Rendering/RenderSession.cs src/Videra.Core/Graphics/GraphicsBackendFactory.cs tests/Videra.Core.Tests/Graphics/GraphicsBackendFactoryTests.cs tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs
git commit -m "feat: add typed viewer options and backend diagnostics"
```

### Task 2: Add Structured Scene Loading APIs

**Files:**
- Create: `src/Videra.Avalonia/Controls/ModelLoadResult.cs`
- Create: `src/Videra.Avalonia/Controls/VideraView.Scene.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Modify: `src/Videra.Core/IO/ModelImporter.cs`
- Test: `tests/Videra.Core.Tests/IO/ModelImporterTests.cs`
- Test: `tests/Videra.Core.IntegrationTests/IO/ModelImporterIntegrationTests.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`

**Step 1: Write failing tests for high-level scene loading**

Add tests that verify:

- `LoadModelAsync` returns a structured result with one object
- `LoadModelsAsync` returns both successes and failures
- `ReplaceScene` replaces all current objects
- `ClearScene` empties the scene

Example assertions:

```csharp
result.LoadedObjects.Should().HaveCount(1);
result.Failures.Should().BeEmpty();
view.ClearScene();
view.EngineSceneObjectCount().Should().Be(0);
```

If there is no public object-count API yet, add the smallest test-only observable seam through integration tests rather than exposing a count purely for convenience.

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ModelImporterTests"
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~ModelImporterIntegrationTests|FullyQualifiedName~RenderSessionIntegrationTests"
```

Expected: failures because load-result types and scene facade methods do not exist yet.

**Step 3: Implement structured load results**

Add result models:

- `ModelLoadResult`
- `ModelLoadBatchResult`
- `ModelLoadFailure`

Include:

- loaded objects
- failed paths
- exception message
- elapsed time
- whether auto-framing ran

**Step 4: Implement scene facade methods on `VideraView`**

Add:

- `LoadModelAsync`
- `LoadModelsAsync`
- `AddObject`
- `ReplaceScene`
- `ClearScene`

Use existing `ModelImporter.Load(...)` internally first. Do not rewrite importer architecture in this phase.

**Step 5: Run targeted tests again**

Run the commands from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add src/Videra.Avalonia/Controls/ModelLoadResult.cs src/Videra.Avalonia/Controls/VideraView.Scene.cs src/Videra.Avalonia/Controls/VideraView.cs src/Videra.Core/IO/ModelImporter.cs tests/Videra.Core.Tests/IO/ModelImporterTests.cs tests/Videra.Core.IntegrationTests/IO/ModelImporterIntegrationTests.cs tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs
git commit -m "feat: add high-level scene loading APIs"
```

### Task 3: Add Bounds Support And Camera Intent APIs

**Files:**
- Create: `src/Videra.Core/Geometry/BoundingBox3.cs`
- Create: `src/Videra.Avalonia/Controls/VideraView.Camera.cs`
- Modify: `src/Videra.Core/Graphics/Object3D.cs`
- Modify: `src/Videra.Core/Cameras/OrbitCamera.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Test: `tests/Videra.Core.Tests/Graphics/Object3DTests.cs`
- Test: `tests/Videra.Core.Tests/Cameras/OrbitCameraTests.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineIntegrationTests.cs`

**Step 1: Write failing tests for bounds and framing**

Add tests that verify:

- `Object3D` can report world bounds after initialization
- `ResetCamera` restores known default camera state
- `SetViewPreset(ViewPreset.Top)` changes camera orientation predictably
- `FrameAll()` returns `false` when scene is empty and `true` when bounds exist

Example assertions:

```csharp
framed.Should().BeTrue();
camera.Target.Should().Be(expectedCenter);
camera.Position.Distance(expectedPosition).Should().BeLessThan(tolerance);
```

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Object3DTests|FullyQualifiedName~OrbitCameraTests"
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraEngineIntegrationTests"
```

Expected: failures because bounds and intent APIs do not exist yet.

**Step 3: Implement bounds support**

Add `BoundingBox3` and compute local bounds from mesh vertices when `Object3D.Initialize(...)` is called.

Add:

- local bounds cache
- world-bounds projection based on current transform

Keep implementation axis-aligned and minimal.

**Step 4: Implement high-level camera methods**

Add:

- `ResetCamera()`
- `FrameAll()`
- `Frame(Object3D obj)`
- `SetViewPreset(ViewPreset preset)`

Use aggregate scene bounds for `FrameAll()`. Do not introduce selection logic.

**Step 5: Run targeted tests again**

Run the commands from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add src/Videra.Core/Geometry/BoundingBox3.cs src/Videra.Avalonia/Controls/VideraView.Camera.cs src/Videra.Core/Graphics/Object3D.cs src/Videra.Core/Cameras/OrbitCamera.cs src/Videra.Avalonia/Controls/VideraView.cs tests/Videra.Core.Tests/Graphics/Object3DTests.cs tests/Videra.Core.Tests/Cameras/OrbitCameraTests.cs tests/Videra.Core.IntegrationTests/Rendering/VideraEngineIntegrationTests.cs
git commit -m "feat: add viewer framing and camera intent APIs"
```

### Task 4: Update Demo And Public Docs To Use The New Path

**Files:**
- Modify: `samples/Videra.Demo/ViewModels/MainWindowViewModel.cs`
- Modify: `samples/Videra.Demo/Views/MainWindow.axaml`
- Modify: `samples/Videra.Demo/Services/AvaloniaModelImporter.cs`
- Modify: `README.md`
- Modify: `src/Videra.Avalonia/README.md`
- Modify: `samples/Videra.Demo/README.md`
- Modify: `docs/zh-CN/README.md`
- Modify: `docs/zh-CN/modules/videra-avalonia.md`
- Modify: `docs/zh-CN/modules/demo.md`
- Test: `tests/Videra.Core.Tests/Samples/DemoConfigurationTests.cs`
- Test: `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`
- Test: `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`

**Step 1: Write failing tests for doc/demo expectations**

Add or extend tests to verify:

- README examples no longer use `view.Engine.AddObject(...)` as the default path
- demo reflects backend diagnostics and import feedback
- Chinese and English docs stay aligned on the new API story

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DemoConfigurationTests|FullyQualifiedName~RepositoryLocalizationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: failures because docs and demo have not been updated yet.

**Step 3: Update demo to consume the new facade**

Make the demo:

- show current resolved backend and fallback state
- use structured import results
- disable import actions when backend is not ready
- call `FrameAll()` after successful load when configured

Do not expand demo scope beyond consuming the new library APIs.

**Step 4: Update public docs**

Change examples so the primary public path is:

- configure `Options`
- load models through high-level methods
- inspect diagnostics through explicit status objects

Keep advanced `Engine` usage documented only as an escape hatch.

**Step 5: Run targeted tests again**

Run the command from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add samples/Videra.Demo/ViewModels/MainWindowViewModel.cs samples/Videra.Demo/Views/MainWindow.axaml samples/Videra.Demo/Services/AvaloniaModelImporter.cs README.md src/Videra.Avalonia/README.md samples/Videra.Demo/README.md docs/zh-CN/README.md docs/zh-CN/modules/videra-avalonia.md docs/zh-CN/modules/demo.md tests/Videra.Core.Tests/Samples/DemoConfigurationTests.cs tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
git commit -m "docs: promote high-level viewer API"
```

### Task 5: Run Full Verification And Stabilize

**Files:**
- Modify as needed: only files touched by Tasks 1-4

**Step 1: Run repository verification**

Run:

```bash
pwsh -File ./verify.ps1 -Configuration Release
```

Expected: all checks pass.

**Step 2: Run native validation if backend-resolution behavior changed materially**

Run:

```bash
pwsh -File ./scripts/run-native-validation.ps1 -Platform Windows -Configuration Release
```

If Linux/macOS-specific backend status behavior changed, also run the hosted workflow or matching-host validation.

**Step 3: Fix regressions minimally**

Only patch failures directly caused by the new usability layer. Do not start unrelated cleanup.

**Step 4: Final commit**

```bash
git add .
git commit -m "feat: improve VideraView usability for host applications"
```

Plan complete and saved to `docs/plans/2026-04-08-library-usability-implementation.md`.
