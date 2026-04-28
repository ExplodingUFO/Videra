# Videra.MinimalAuthoringSample

`Videra.MinimalAuthoringSample` is a Core-only scene authoring sample. It builds a `SceneDocument` directly from authored primitives and repeated marker instances, with no OBJ or glTF files.

## Public APIs exercised

- `SceneAuthoring.Create(...)`
- `SceneAuthoringBuilder.AddPlane(...)`
- `SceneAuthoringBuilder.AddGrid(...)`
- `SceneAuthoringBuilder.AddAxisTriad(...)`
- `SceneAuthoringBuilder.AddScaleBar(...)`
- `SceneAuthoringBuilder.AddSphere(...)`
- `SceneAuthoringBuilder.AddInstances(...)`
- `SceneGeometry.AxisLine(...)`
- `SceneGeometry.ScaleBar(...)`
- `SceneGeometry.Sphere(...)`
- `SceneDocument`
- `ImportedSceneAsset`
- `InstanceBatchEntry`

## Run

```bash
dotnet run --project samples/Videra.MinimalAuthoringSample/Videra.MinimalAuthoringSample.csproj
```

For build-only verification:

```bash
dotnet build samples/Videra.MinimalAuthoringSample/Videra.MinimalAuthoringSample.csproj -c Debug --no-restore
```

## Scope boundary

This sample intentionally stops at retained scene truth. It does not use a viewer, importer, backend, update loop, ECS layer, or material graph.
