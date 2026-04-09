# Selection, Highlight, and Annotation Overlay Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add object-level selection, multi-select, box selection, and lightweight annotations to `VideraView` with host-owned state, built-in viewer-style interaction, and split 3D/2D overlay rendering.

**Architecture:** Keep durable business state outside the control. Add pure selection/query contracts in `Videra.Core`, use a dedicated overlay translation/rendering layer for 3D highlight truth plus 2D label UI, and extract Avalonia interaction into a thin controller instead of growing `VideraView` into a god object.

**Tech Stack:** .NET 8, Avalonia 11.3.9, existing Videra render-pipeline extensibility, xUnit, FluentAssertions

---

### Task 1: Add Stable Object Identity And Core Selection Contracts

**Files:**
- Create: `src/Videra.Core/Selection/SceneHitTestRequest.cs`
- Create: `src/Videra.Core/Selection/SceneHitTestResult.cs`
- Create: `src/Videra.Core/Selection/SceneBoxSelectionQuery.cs`
- Create: `src/Videra.Core/Selection/SceneBoxSelectionResult.cs`
- Create: `src/Videra.Core/Selection/SceneBoxSelectionMode.cs`
- Create: `src/Videra.Core/Selection/SceneHitTestService.cs`
- Create: `src/Videra.Core/Selection/SceneBoxSelectionService.cs`
- Modify: `src/Videra.Core/Graphics/Object3D.cs`
- Modify: `src/Videra.Core/Cameras/OrbitCamera.cs`
- Test: `tests/Videra.Core.Tests/Graphics/Object3DTests.cs`
- Test: `tests/Videra.Core.Tests/Selection/SceneHitTestServiceTests.cs`
- Test: `tests/Videra.Core.Tests/Selection/SceneBoxSelectionServiceTests.cs`

**Step 1: Write failing tests for stable identity and selection query contracts**

Add tests that verify:

- `Object3D` exposes a stable public `Id`
- a hit-test request returns nearest object first when two objects overlap in screen space
- box selection supports both `Touch` and `FullyInside`
- empty scenes return empty structured results rather than `null`

Example assertions:

```csharp
obj.Id.Should().NotBe(Guid.Empty);
result.PrimaryHit!.ObjectId.Should().Be(front.Id);
result.Hits.Select(hit => hit.ObjectId).Should().ContainInOrder(front.Id, back.Id);
selection.ObjectIds.Should().Contain(target.Id);
```

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Object3DTests|FullyQualifiedName~SceneHitTestServiceTests|FullyQualifiedName~SceneBoxSelectionServiceTests"
```

Expected: FAIL because `Object3D.Id` and the new selection query types do not exist yet.

**Step 3: Add stable object identity**

Modify `Object3D` to expose a durable public identity:

- add `public Guid Id { get; }`
- initialize it once during construction
- keep it immutable

Do not make identity depend on object name, scene order, or object reference equality.

**Step 4: Implement pure hit-test and box-selection contracts**

Add request/result DTOs and services in `src/Videra.Core/Selection`.

Implementation rules:

- no Avalonia types
- no control state
- no modifier-key knowledge
- operate only on camera state, viewport size, pointer/raycast input, and `Object3D`

First version should use:

- ray vs `Object3D.WorldBounds`
- screen-space projection of `WorldBounds` corners for box selection

**Step 5: Run targeted tests again**

Run the command from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add src/Videra.Core/Selection src/Videra.Core/Graphics/Object3D.cs src/Videra.Core/Cameras/OrbitCamera.cs tests/Videra.Core.Tests/Graphics/Object3DTests.cs tests/Videra.Core.Tests/Selection/SceneHitTestServiceTests.cs tests/Videra.Core.Tests/Selection/SceneBoxSelectionServiceTests.cs
git commit -m "feat: add core selection query contracts"
```

### Task 2: Add Annotation Anchor Contracts And Projection

**Files:**
- Create: `src/Videra.Core/Selection/Annotations/AnnotationAnchorKind.cs`
- Create: `src/Videra.Core/Selection/Annotations/AnnotationAnchorDescriptor.cs`
- Create: `src/Videra.Core/Selection/Annotations/AnnotationProjectionResult.cs`
- Create: `src/Videra.Core/Selection/Annotations/AnnotationAnchorProjector.cs`
- Test: `tests/Videra.Core.Tests/Selection/AnnotationAnchorProjectorTests.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/Object3DIntegrationTests.cs`

**Step 1: Write failing tests for object and world-point annotation projection**

Add tests that verify:

- object-bound anchors can resolve from `Object3D.Id`
- world-point anchors project to screen coordinates
- clipped or camera-hidden anchors return a structured non-visible result
- missing object ids do not throw; they return a no-op/non-visible projection result

Example assertions:

```csharp
projection.IsVisible.Should().BeTrue();
projection.ScreenPosition.X.Should().BeGreaterThan(0);
projection.SourceKind.Should().Be(AnnotationAnchorKind.SceneObject);
missing.IsVisible.Should().BeFalse();
```

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~AnnotationAnchorProjectorTests|FullyQualifiedName~Object3DIntegrationTests"
```

Expected: FAIL because annotation anchor contracts do not exist yet.

**Step 3: Implement anchor descriptors and projector**

Create a projector that supports:

- scene-object anchor by `Object3D.Id`
- world-point anchor by `Vector3`

The projector should return:

- `IsVisible`
- `ScreenPosition`
- `ClipStatus` or equivalent visibility reason
- resolved object identity where applicable

**Step 4: Keep the model lightweight**

Do not add rich label templates or editing semantics in this task. This task is only about anchor truth and projection.

**Step 5: Run targeted tests again**

Run the command from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add src/Videra.Core/Selection/Annotations tests/Videra.Core.Tests/Selection/AnnotationAnchorProjectorTests.cs tests/Videra.Core.IntegrationTests/Rendering/Object3DIntegrationTests.cs
git commit -m "feat: add annotation anchor projection contracts"
```

### Task 3: Add Controlled Selection And Annotation Public Models

**Files:**
- Create: `src/Videra.Avalonia/Controls/Interaction/VideraInteractionMode.cs`
- Create: `src/Videra.Avalonia/Controls/Interaction/VideraInteractionOptions.cs`
- Create: `src/Videra.Avalonia/Controls/Interaction/VideraSelectionState.cs`
- Create: `src/Videra.Avalonia/Controls/Interaction/VideraAnnotation.cs`
- Create: `src/Videra.Avalonia/Controls/Interaction/VideraNodeAnnotation.cs`
- Create: `src/Videra.Avalonia/Controls/Interaction/VideraWorldPointAnnotation.cs`
- Create: `src/Videra.Avalonia/Controls/Interaction/SelectionRequestedEventArgs.cs`
- Create: `src/Videra.Avalonia/Controls/Interaction/AnnotationRequestedEventArgs.cs`
- Create: `src/Videra.Avalonia/Controls/Interaction/VideraInteractionDiagnostics.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/VideraViewExtensibilityIntegrationTests.cs`

**Step 1: Write failing tests for controlled state and public intent events**

Add tests that verify:

- `VideraView` exposes `SelectionState`, `Annotations`, `InteractionMode`, and `InteractionOptions`
- `SelectionRequested` and `AnnotationRequested` exist as structured events
- these properties remain queryable before initialization and after disposal where safe
- event args carry object ids and anchor descriptors instead of control-internal types

Example assertions:

```csharp
view.InteractionMode.Should().Be(VideraInteractionMode.Navigate);
typeof(VideraView).GetEvent("SelectionRequested").Should().NotBeNull();
args.Selection.ObjectIds.Should().Contain(objectId);
```

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~VideraViewExtensibilityIntegrationTests"
```

Expected: FAIL because the controlled interaction public API does not exist yet.

**Step 3: Add the public controlled models**

Create the new interaction models under `src/Videra.Avalonia/Controls/Interaction`.

Rules:

- host-owned state is plain public data
- event payloads describe intent
- no public type should expose internal controller state or session details

**Step 4: Expose the new surface on `VideraView`**

Add:

- public state properties
- public events
- no-op-safe defaults before backend readiness

Do not yet wire input behavior in this task; only establish the controlled API surface.

**Step 5: Run targeted tests again**

Run the command from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add src/Videra.Avalonia/Controls/Interaction src/Videra.Avalonia/Controls/VideraView.cs tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs tests/Videra.Core.IntegrationTests/Rendering/VideraViewExtensibilityIntegrationTests.cs
git commit -m "feat: add controlled interaction public API"
```

### Task 4: Split 3D Overlay Rendering From 2D Overlay UI

**Files:**
- Create: `src/Videra.Core/Selection/Rendering/SelectionOverlayRenderState.cs`
- Create: `src/Videra.Core/Selection/Rendering/AnnotationOverlayRenderState.cs`
- Create: `src/Videra.Core/Selection/Rendering/SelectionOverlayContributor.cs`
- Create: `src/Videra.Core/Selection/Rendering/AnnotationAnchorOverlayContributor.cs`
- Create: `src/Videra.Avalonia/Controls/VideraView.Overlay.cs`
- Modify: `src/Videra.Core/Graphics/VideraEngine.cs`
- Modify: `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/SelectionOverlayIntegrationTests.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineExtensibilityIntegrationTests.cs`

**Step 1: Write failing integration tests for overlay truth**

Add tests that verify:

- selected objects produce a stable 3D highlight overlay
- hover/object-anchor markers can be emitted without mutating scene state
- 2D overlay state can be derived from projected anchors without touching engine internals
- overlay contributors are ignored harmlessly after disposal

Example assertions:

```csharp
snapshot.ExecutedStages.Should().Contain(RenderPipelineStage.SolidGeometryPass);
overlayState.SelectedObjectIds.Should().Contain(objectId);
view.BackendDiagnostics.IsReady.Should().BeTrue();
```

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SelectionOverlayIntegrationTests|FullyQualifiedName~VideraEngineExtensibilityIntegrationTests"
```

Expected: FAIL because there is no overlay state/rendering split yet.

**Step 3: Add 3D overlay render-state models and contributors**

Implement 3D overlay support through pass contributors, not by merging selection logic into existing solid or wireframe paths.

Keep this task focused on:

- selected-object highlight
- hover highlight
- anchor markers / 3D line origins

Do not add 2D text layout logic to core rendering.

**Step 4: Add a thin Avalonia 2D overlay adapter**

Create a partial `VideraView.Overlay.cs` that:

- receives host-owned selection/annotation truth
- receives projected anchor truth
- updates the 2D overlay layer for labels and selection rectangles

Do not place text layout math inside `VideraView.cs` directly.

**Step 5: Run targeted tests again**

Run the command from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add src/Videra.Core/Selection/Rendering src/Videra.Core/Graphics/VideraEngine.cs src/Videra.Core/Graphics/VideraEngine.Rendering.cs src/Videra.Avalonia/Controls/VideraView.Overlay.cs src/Videra.Avalonia/Controls/VideraView.cs src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs tests/Videra.Core.IntegrationTests/Rendering/SelectionOverlayIntegrationTests.cs tests/Videra.Core.IntegrationTests/Rendering/VideraEngineExtensibilityIntegrationTests.cs
git commit -m "feat: add selection and annotation overlay rendering"
```

### Task 5: Extract A Dedicated Avalonia Interaction Controller

**Files:**
- Create: `src/Videra.Avalonia/Interaction/VideraInteractionController.cs`
- Create: `src/Videra.Avalonia/Interaction/VideraInteractionState.cs`
- Create: `src/Videra.Avalonia/Interaction/VideraPointerGestureSnapshot.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.Input.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/VideraViewInteractionIntegrationTests.cs`
- Test: `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`

**Step 1: Write failing interaction tests for viewer-style behavior**

Add tests that verify:

- `Navigate` mode preserves current camera interaction
- `Select` mode emits single-select, additive-select, and box-select requests
- `Annotate` mode emits node or world-point annotation requests
- empty-space clicks follow configured clear/no-clear behavior
- controller state does not retain durable host-owned selection data

Example assertions:

```csharp
capturedRequest.Operation.Should().Be(SelectionOperation.Replace);
capturedRequest.ObjectIds.Should().Contain(target.Id);
annotation.Anchor.Kind.Should().Be(AnnotationAnchorKind.WorldPoint);
```

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewInteractionIntegrationTests|FullyQualifiedName~VideraViewSceneIntegrationTests"
```

Expected: FAIL because current `VideraView.Input.cs` only handles camera gestures.

**Step 3: Extract the controller**

Create a dedicated interaction controller and move new interaction-state logic there.

Rules:

- `VideraView.Input.cs` becomes a forwarding shell
- controller stores only short-lived gesture state
- controller translates Avalonia input into core queries and public intent events
- durable selection and annotation state remain on public properties provided by the host

**Step 4: Wire `Navigate`, `Select`, and `Annotate`**

Implement:

- viewer-style default gestures
- drag threshold for box selection
- `Ctrl` / `Shift` additive semantics
- object and world-point annotation placement

Keep right-drag and wheel camera behavior intact across modes.

**Step 5: Run targeted tests again**

Run the command from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add src/Videra.Avalonia/Interaction src/Videra.Avalonia/Controls/VideraView.Input.cs src/Videra.Avalonia/Controls/VideraView.cs tests/Videra.Core.IntegrationTests/Rendering/VideraViewInteractionIntegrationTests.cs tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs
git commit -m "feat: add viewer interaction controller for selection and annotations"
```

### Task 6: Add A Focused Sample, Docs, And Repository Guards

**Files:**
- Create: `samples/Videra.InteractionSample/Videra.InteractionSample.csproj`
- Create: `samples/Videra.InteractionSample/README.md`
- Create: `samples/Videra.InteractionSample/App.axaml`
- Create: `samples/Videra.InteractionSample/App.axaml.cs`
- Create: `samples/Videra.InteractionSample/Views/MainWindow.axaml`
- Create: `samples/Videra.InteractionSample/Views/MainWindow.axaml.cs`
- Modify: `Videra.sln`
- Modify: `README.md`
- Modify: `src/Videra.Avalonia/README.md`
- Modify: `docs/zh-CN/README.md`
- Modify: `docs/zh-CN/modules/videra-avalonia.md`
- Test: `tests/Videra.Core.Tests/Samples/InteractionSampleConfigurationTests.cs`
- Test: `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`
- Test: `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`

**Step 1: Write failing guard tests for sample and documentation truth**

Add tests that verify:

- a dedicated interaction sample exists
- docs mention host-owned state and built-in interaction modes
- public API names in docs match the shipped contract
- Chinese mirror docs cover the same core contract

Example assertions:

```csharp
readme.Should().Contain("SelectionState");
readme.Should().Contain("Annotations");
readme.Should().Contain("SelectionRequested");
localized.Should().Contain("InteractionMode");
```

**Step 2: Run targeted tests to confirm failure**

Run:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~InteractionSampleConfigurationTests|FullyQualifiedName~RepositoryArchitectureTests|FullyQualifiedName~RepositoryLocalizationTests"
```

Expected: FAIL because the sample and docs do not exist yet.

**Step 3: Create a focused interaction sample**

The sample should demonstrate:

- host-owned selection state
- host-owned lightweight annotations
- mode switching between `Navigate`, `Select`, and `Annotate`
- default interaction flow
- overlay feedback

Do not fold this into `Videra.ExtensibilitySample`.

**Step 4: Update docs and guards**

Document:

- controlled-state ownership model
- built-in interaction modes
- object-level selection scope
- node and world-point annotations
- 3D/2D overlay split at a conceptual level

Then add repository guards so the contract cannot silently drift later.

**Step 5: Run targeted tests again**

Run the command from Step 2.

Expected: PASS.

**Step 6: Commit**

```bash
git add samples/Videra.InteractionSample Videra.sln README.md src/Videra.Avalonia/README.md docs/zh-CN/README.md docs/zh-CN/modules/videra-avalonia.md tests/Videra.Core.Tests/Samples/InteractionSampleConfigurationTests.cs tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs
git commit -m "feat: add interaction sample and documentation guards"
```

### Task 7: Full Verification Pass

**Files:**
- Modify: `docs/plans/2026-04-09-selection-annotation-overlay-design.md`
- Modify: `docs/plans/2026-04-09-selection-annotation-overlay-implementation.md`

**Step 1: Run focused suites in execution order**

Run:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Object3DTests|FullyQualifiedName~SceneHitTestServiceTests|FullyQualifiedName~SceneBoxSelectionServiceTests|FullyQualifiedName~AnnotationAnchorProjectorTests|FullyQualifiedName~InteractionSampleConfigurationTests|FullyQualifiedName~RepositoryArchitectureTests|FullyQualifiedName~RepositoryLocalizationTests"
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SelectionOverlayIntegrationTests|FullyQualifiedName~VideraViewInteractionIntegrationTests|FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~VideraViewExtensibilityIntegrationTests|FullyQualifiedName~Object3DIntegrationTests"
```

Expected: PASS.

**Step 2: Run repository-level verification**

Run:

```bash
pwsh -File ./verify.ps1 -Configuration Release
```

Expected:

- build passes
- tests pass
- demo/sample build passes

**Step 3: Update plan docs with any execution notes**

If file paths or final public type names differed from the original plan, update the design and plan docs so future sessions do not inherit stale names.

**Step 4: Commit final verification/documentation adjustments**

```bash
git add docs/plans/2026-04-09-selection-annotation-overlay-design.md docs/plans/2026-04-09-selection-annotation-overlay-implementation.md
git commit -m "docs: finalize selection overlay planning notes"
```
