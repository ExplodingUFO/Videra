# Phase 304 Context: No-Downshift Contract Inventory

## Bead

`Videra-637`

## Goal

Inventory fallback/default/downshift wording and behavior after the v2.40 reduction pass and the follow-up change that made viewer software fallback explicit opt-in.

## Scope

- Core backend resolution defaults.
- Avalonia viewer backend options.
- Canonical docs and samples.
- Doctor/support evidence wording.
- SurfaceCharts chart-local fallback wording.
- Repository tests that lock these contracts.

## Key Assumption

This milestone should not remove intentional fallback wholesale. The target is hidden downshift behavior and stale language that implies automatic recovery where the current contract requires explicit opt-in or explicit failure evidence.
