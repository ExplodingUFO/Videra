---
phase: 65-consumer-package-smoke-and-host-validation
plan: 02
subsystem: smoke-workflow
tags: [workflow, packages, host-validation]
provides:
  - consumer smoke script
  - multi-host workflow entrypoint
key-files:
  added:
    - scripts/Invoke-ConsumerSmoke.ps1
    - .github/workflows/consumer-smoke.yml
requirements-completed: [CONS-01, CONS-02]
completed: 2026-04-17
---

# Phase 65 Plan 02 Summary

## Accomplishments

- Added `Invoke-ConsumerSmoke.ps1` to pack the current public packages, restore the smoke app from a local package source plus `nuget.org`, and validate the resulting first-scene report.
- Added `consumer-smoke.yml` with manual entrypoints for Windows, Linux X11, Linux XWayland, macOS, and an `all` fan-out.
- Published smoke artifacts so the supported install path is rerunnable and inspectable.

## Verification

- `pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -OutputRoot artifacts/consumer-smoke/local`
