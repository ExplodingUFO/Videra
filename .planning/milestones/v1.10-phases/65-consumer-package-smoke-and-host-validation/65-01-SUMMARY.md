---
phase: 65-consumer-package-smoke-and-host-validation
plan: 01
subsystem: consumer-smoke-app
tags: [packages, smoke, alpha]
provides:
  - package-based consumer smoke app
  - machine-readable smoke output
key-files:
  added:
    - smoke/Videra.ConsumerSmoke/Videra.ConsumerSmoke.csproj
    - smoke/Videra.ConsumerSmoke/Views/MainWindow.axaml.cs
requirements-completed: [CONS-01]
completed: 2026-04-17
---

# Phase 65 Plan 01 Summary

## Accomplishments

- Added `Videra.ConsumerSmoke` as a package-only consumer application.
- Implemented the consumer flow that waits for readiness, loads a small model, frames it, resets the camera, and captures `BackendDiagnostics`.
- Emitted a JSON smoke report so workflow and local validation can assert the public flow without scraping logs.

## Verification

- `pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -OutputRoot artifacts/consumer-smoke/local`
