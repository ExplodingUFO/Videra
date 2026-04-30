# Summary 107-02

- Added `MeshPrimitiveId` and `MeshPrimitive` to represent reusable geometry identity separately from node identity.
- Reworked `ImportedSceneAsset` so its public surface now exposes `Nodes`, `Primitives`, `RootNodes`, and computed metrics.
- Kept the combined `MeshPayload` internal to avoid exposing competing public scene truths.
