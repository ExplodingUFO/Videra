# Summary 107-03

- Added `ImportedSceneAssetPayloadBuilder` so the existing deferred upload/reupload path still consumes one internal flattened payload.
- Updated OBJ/glTF importers to emit node/primitive scene truth and preserve glTF partial-import behavior when malformed primitives lack `POSITION`.
- Updated benchmark and focused test coverage so transformed shared-instance flattening is directly asserted.
