# Phase 345: Scene Authoring API Gap Inventory

## Bead

`Videra-eqh`

## Goal

Inventory the existing Core scene authoring API before changing public shape, then define the smallest v2.49 polish surface that improves ease of use without adding a duplicate builder, compatibility layer, hidden fallback path, or god-code.

## Existing Authoring Surface

### Keep

- `SceneAuthoring.Create(string name)` as the single authoring entrypoint.
- `SceneAuthoringBuilder` as the single fluent builder.
- `SceneAuthoringBuilder.AddMesh(...)` for low-level authored mesh entry.
- Primitive helpers already on the builder: `AddTriangle`, `AddQuad`, `AddPlane`, `AddGrid`, `AddPolyline`, `AddPointCloud`, `AddCube`, `AddSphere`.
- Static helper semantics already on the builder: `AddAxisTriad`, `AddScaleBar`.
- `SceneAuthoringBuilder.AddInstances(...)` / `AddInstanceBatch(...)` as the explicit repeated-geometry path.
- `SceneGeometry` as the Core-only deterministic primitive factory.
- `SceneMaterials` as the renderer-neutral material preset location.
- `SceneAuthoringDiagnostic` / `SceneAuthoringResult` / `TryBuild()` as the explicit failure path.
- Output truth as `SceneDocument` plus `InstanceBatchEntry`, not a runtime object graph.

### Polish

- Placement ergonomics: current public methods require callers to build `Matrix4x4` manually for common translation/scale/rotation placement.
- Style ergonomics: current calls require material variables before primitive calls; common quick-start examples would read better with small reusable option/configuration affordances on the existing builder.
- Instance ergonomics: current `AddInstances(...)` is correct but still exposes raw `MeshData` plus `MaterialInstance`; follow-up should improve examples and possibly small overloads without automatic batching magic.
- Diagnostics: current validation covers empty scenes, duplicate ids, malformed mesh data, UV length, non-finite vertices/transforms, and index range; it does not validate instance-batch input shape in the authoring layer.
- Documentation/sample clarity: MinimalAuthoringSample demonstrates the right concepts, but the shortest happy path is still more verbose than the target "code-first static scene" story.

### Document

- `SceneGeometry.Buffer(...)` is the escape hatch for typed buffer geometry and should remain explicitly documented as lower-level.
- `SceneMaterials.Transparent(...)` creates `Blend`; docs should keep warning that transparent instance sorting remains out of scope.
- `AddAxisTriad(...)` and `AddScaleBar(...)` are retained line primitives, not runtime gizmos, overlays, measurement tools, or fallback paths.
- `AddInstances(...)` is the preferred path for repeated markers/cubes; repeated calls to `AddCube(...)` remain literal scene entries, not implicitly batched.

### Defer

- Automatic duplicate-geometry detection and implicit batching.
- Per-instance arbitrary material overrides.
- Transparent instance sorting.
- Multi-geometry batching.
- GPU-driven culling or indirect draw.
- Runtime editor/manipulator APIs.
- Text labels and overlay annotations in Core authoring.

### Reject

- A second public `SceneBuilder` stack parallel to `SceneAuthoringBuilder`.
- ECS, game-object/component model, update loop, scripting, physics, animation, skeleton, morph, shader graph, backend-specific authoring, plugin discovery, compatibility wrappers, hidden fallback/downshift, or silent repair of invalid authored data.

## Target Experience

The v2.49 target should keep the existing entrypoint and make the following kind of code feel natural:

```csharp
var scene = SceneAuthoring.Create("inspection-scene")
    .AddGrid("floor", SceneMaterials.Matte("grid", RgbaFloat.DarkGrey), width: 10f, depth: 10f)
    .AddCube("box", SceneMaterials.Matte("steel", RgbaFloat.LightGrey), size: 1f, transform: Matrix4x4.CreateTranslation(0f, 0.5f, 0f))
    .AddSphere("focus", SceneMaterials.Metal("focus", RgbaFloat.Blue), radius: 0.35f, transform: Matrix4x4.CreateTranslation(1f, 0.35f, 0f))
    .AddInstances("markers", markerMesh, markerMaterial, markerTransforms, markerColors, markerObjectIds, pickable: true)
    .Build();
```

Follow-up phases can improve the repeated `Matrix4x4.CreateTranslation(...)` and material/setup friction, but must preserve the same `SceneDocument` / `InstanceBatchEntry` truth.

## Follow-Up Boundaries

### Phase 346: Fluent Authoring Placement and Style Affordances

Write set should stay under `src/Videra.Core/Scene/*`, `tests/Videra.Core.Tests/Scene/*`, and authoring docs. It should add narrow placement/style affordances to the existing builder only.

### Phase 347: Primitive and Material Preset Polish

Write set should stay around `SceneGeometry`, `SceneMaterials`, tests, and docs. It should not touch builder flow except where a new preset needs a direct existing-builder usage example.

### Phase 348: Authoring Diagnostics and Failure Clarity

Write set should focus on validation and diagnostics, especially instance-batch validation and clearer error messages. It must not auto-repair invalid inputs.

### Phase 349: Minimal Authoring Sample Refresh

Write set should be limited to `samples/Videra.MinimalAuthoringSample`, sample README, and sample-facing tests.

### Phase 350: Instance-Aware Authoring Evidence

Write set should focus on tests/evidence/benchmark or demo proof that repeated geometry remains explicit instance truth. It should not rewrite the renderer.

## Verification

- Read current Core authoring sources:
  - `src/Videra.Core/Scene/SceneAuthoring.cs`
  - `src/Videra.Core/Scene/SceneAuthoringBuilder.cs`
  - `src/Videra.Core/Scene/SceneGeometry.cs`
  - `src/Videra.Core/Scene/SceneMaterials.cs`
  - `src/Videra.Core/Scene/SceneAuthoringDiagnostic.cs`
  - `src/Videra.Core/Scene/SceneAuthoringResult.cs`
- Read current authoring tests:
  - `tests/Videra.Core.Tests/Scene/SceneAuthoringBuilderTests.cs`
- Read current authoring sample/docs:
  - `samples/Videra.MinimalAuthoringSample/Program.cs`
  - `samples/Videra.MinimalAuthoringSample/README.md`
  - `src/Videra.Core/README.md`

No product code changed in this phase, so no build/test run was required.
