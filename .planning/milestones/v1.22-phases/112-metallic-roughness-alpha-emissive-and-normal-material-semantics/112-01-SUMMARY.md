# 112-01 Summary

## Outcome

Phase 112 extended the viewer-path material contract so static glTF imports retain the core PBR material semantics already present in source assets, without widening into renderer or backend feature work.

## Delivered

- Added direct runtime material contracts for metallic-roughness, alpha semantics, emissive inputs, and normal-map-ready texture usage.
- Extended `MaterialInstance` so the imported material catalog keeps static PBR truth on the shipped viewer/runtime path instead of collapsing back to base-color-only semantics.
- Updated the glTF importer to map:
  - metallic factor, roughness factor, and metallic-roughness texture usage
  - alpha mode, alpha cutoff, and double-sidedness
  - emissive factor and emissive texture usage
  - normal texture usage and normal scale
- Added focused Core and Avalonia tests proving the imported material catalog still survives `ImportedSceneAsset` retention and `SceneImportService` on the current runtime path.

## Intentional Non-Goals

- No textured shader path or renderer-side material consumption was added.
- No transparency pipeline expansion, sorting work, or lighting-system widening was introduced.
- No compatibility wrapper or transitional material adapter was kept for the old base-color-only shape.
