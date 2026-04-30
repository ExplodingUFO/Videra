# Summary 108-01

- Added `MaterialInstance` and `MaterialInstanceId` under `Videra.Core.Scene`.
- Extended `MeshPrimitive` with `MaterialId` and `ImportedSceneAsset` with a material catalog.
- Updated OBJ/glTF import paths and focused tests so imported scene assets now expose explicit material truth instead of burying it inside importer-local state.
