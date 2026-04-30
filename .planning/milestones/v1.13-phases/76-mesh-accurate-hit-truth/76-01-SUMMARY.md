# Phase 76 Summary 01

- Expanded `SceneHitTestResult.SceneHit` from object-only distance truth into a richer hit record carrying `WorldPoint`, `WorldNormal`, and `PrimitiveIndex`.
- Added lazy per-mesh triangle acceleration in `MeshTriangleHitAcceleration` and routed triangle `Object3D` picking through exact ray/triangle intersection instead of world-bounds-only hits.
- Kept the compatibility path narrow: non-triangle or payload-less objects still fall back to bounds-based object hits instead of failing selection.

