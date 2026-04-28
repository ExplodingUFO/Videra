# Videra.MinimalAuthoringSample

`Videra.MinimalAuthoringSample` is a Core-only scene authoring sample. It builds a `SceneDocument` directly from authored primitives and repeated marker instances, with no OBJ or glTF files.

The program is intentionally small: it creates a ground plane, grid, axis triad, scale bar, placed focus sphere, outline preset, and three pickable marker instances. It then calls `TryBuild()` so diagnostics are surfaced explicitly before retained scene truth is printed.

## Public APIs exercised

- `SceneAuthoring.Create(...)`
- `SceneAuthoringPlacement.Identity`
- `SceneAuthoringPlacement.At(...)`
- `SceneAuthoringPlacement.From(...)`
- `SceneAuthoringBuilder.AddPlane(...)`
- `SceneAuthoringBuilder.AddGrid(...)`
- `SceneAuthoringBuilder.AddAxisTriad(...)`
- `SceneAuthoringBuilder.AddScaleBar(...)`
- `SceneAuthoringBuilder.AddMesh(...)`
- `SceneAuthoringBuilder.AddSphere(...)`
- `SceneAuthoringBuilder.AddInstances(...)`
- `SceneMaterials.Matte(...)`
- `SceneMaterials.Metal(...)`
- `SceneMaterials.Emissive(...)`
- `SceneGeometry.BoxOutline(...)`
- `SceneGeometry.Sphere(...)`
- `SceneDocument`
- `ImportedSceneAsset`
- `InstanceBatchEntry`
- `SceneAuthoringResult`
- `SceneAuthoringDiagnostic`

## Run

```bash
dotnet run --project samples/Videra.MinimalAuthoringSample/Videra.MinimalAuthoringSample.csproj
```

For build-only verification:

```bash
dotnet build samples/Videra.MinimalAuthoringSample/Videra.MinimalAuthoringSample.csproj -c Debug --no-restore
```

## Scope boundary

This sample intentionally stops at retained scene truth. It does not use a viewer, importer, backend, asset file, update loop, ECS layer, editor/workbench surface, shader graph, animation system, or hidden fallback path.

Diagnostics are part of the authoring contract. Hosts should surface `SceneAuthoringDiagnostic` entries from `TryBuild()` instead of silently repairing invalid geometry, substituting fallback meshes, or downshifting unsupported material choices.
