---
phase: 39-overlay-projection-and-layout-service-extraction
plan: 01
subsystem: core-overlay-services
tags: [viewer, overlay, core]
provides:
  - core overlay projection contracts
  - selection outline projector
  - annotation label layout service
key-files:
  modified:
    - src/Videra.Core/Overlay/SelectionOutlineProjection.cs
    - src/Videra.Core/Overlay/AnnotationLabelProjection.cs
    - src/Videra.Core/Overlay/SelectionOutlineProjector.cs
    - src/Videra.Core/Overlay/AnnotationLabelLayoutService.cs
    - src/Videra.Core/Overlay/OverlayProjectionService.cs
requirements-completed: [OVERLAY-01]
completed: 2026-04-17
---

# Phase 39 Plan 01 Summary

## Accomplishments
- Added overlay projection/layout contracts and services inside Core so bridge math no longer needs to own selection-outline and annotation-layout logic.
- Made the projection layer reusable for future non-Avalonia overlay paths.
- Preserved the separation between render-side 3D overlay truth and 2D presentation layout.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSessionBridgeIntegrationTests|FullyQualifiedName~SelectionOverlayIntegrationTests"`

## Notes
- The new overlay services are internal support seams, not new public APIs.
