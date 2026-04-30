---
phase: 46-scene-pipeline-lab-and-truthful-entry-points
plan: 01
subsystem: scene-pipeline-lab
tags: [demo, viewer, scene-pipeline]
provides:
  - visible Scene Pipeline Lab guidance
  - truthful import status copy
  - narrow contract-focused demo behavior
key-files:
  modified:
    - samples/Videra.Demo/Views/MainWindow.axaml
    - samples/Videra.Demo/ViewModels/MainWindowViewModel.cs
    - samples/Videra.Demo/Services/AvaloniaModelImporter.cs
    - samples/Videra.Demo/README.md
requirements-completed: [LAB-01]
completed: 2026-04-17
---

# Phase 46 Plan 01 Summary

## Accomplishments
- Added a visible Scene Pipeline Lab panel to `Videra.Demo` that explains the new scene/import/rebind contract.
- Changed import-status and framing behavior so partially successful batches no longer look like full-scene replacement.
- Kept the demo scoped to contract validation instead of widening it into a larger feature showcase.

## Verification
- `pwsh -File ./scripts/verify.ps1 -Configuration Release`
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DemoConfigurationTests"`

## Notes
The lab exists to prove the new core contract, not to become another broad demo surface.
