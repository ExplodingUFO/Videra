---
phase: 38-core-interaction-picking-and-selection-services
plan: 01
subsystem: core-interaction-services
tags: [viewer, interaction, core]
provides:
  - core orbit camera manipulator
  - core picking service
  - core selection-box service
key-files:
  modified:
    - src/Videra.Core/Interaction/OrbitCameraManipulator.cs
    - src/Videra.Core/Picking/PickingService.cs
    - src/Videra.Core/Selection/SelectionBoxService.cs
requirements-completed: [INPUT-01]
completed: 2026-04-17
---

# Phase 38 Plan 01 Summary

## Accomplishments
- Added `OrbitCameraManipulator`, `PickingService`, and `SelectionBoxService` to move reusable interaction semantics into Core.
- Kept the new services free of Avalonia-specific routing types so the viewer can reuse them across pointer paths.
- Prepared the interaction controller to become an adapter instead of a behavior owner.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewInteractionIntegrationTests"`

## Notes
- These services narrow the Avalonia role to routing normalized input into core semantics.
