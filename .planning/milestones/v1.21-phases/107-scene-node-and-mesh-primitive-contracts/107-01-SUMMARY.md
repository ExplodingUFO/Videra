# Summary 107-01

- Added `SceneNodeId` and `SceneNode` as the new public hierarchy contract in `Videra.Core.Scene`.
- Kept the shape direct: parent linkage plus local transform, without introducing a heavier scene graph object.
- Added importer tests that assert root/child wiring through `SceneNode` rather than only flattened mesh counts.
