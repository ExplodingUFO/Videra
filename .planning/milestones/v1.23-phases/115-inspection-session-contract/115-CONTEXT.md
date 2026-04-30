# Phase 115: Inspection Session Contract - Context

**Gathered:** 2026-04-21
**Status:** Ready for planning and execution

<domain>
## Phase Boundary

The public inspection surface already exists, but the session story is asymmetric:

- `SelectionState` and `Annotations` are host-owned
- `Measurements` and `ClippingPlanes` are direct viewer surfaces
- `CaptureInspectionState()` / `ApplyInspectionState(...)` round-trip camera, selection, snap mode, clipping, and measurements
- annotations currently round-trip only through `VideraInspectionBundleService`

This phase should make the typed inspection-session contract internally consistent without widening `VideraView` into a broader project-format API.

</domain>

<decisions>
## Implementation Decisions

### Include annotations directly in typed inspection state

Use one direct public session contract instead of preserving a split between `inspection state` and `bundle-only annotations`.

### Keep scope on the session contract and its public truth

This phase should cover the typed state, docs/sample wording, and tests that lock the contract. Replay/export hardening stays in the next phase.

</decisions>

<code_context>
## Existing Code Insights

- `src/Videra.Avalonia/Controls/VideraInspectionState.cs` owns the public typed inspection-state contract.
- `src/Videra.Avalonia/Runtime/VideraViewRuntime.Inspection.cs` captures and applies inspection state.
- `src/Videra.Avalonia/Controls/VideraInspectionBundleService.cs` currently stores annotations in a separate `annotations.json`.
- `samples/Videra.InteractionSample/Views/MainWindow.axaml.cs` and its README already teach the inspection flow, but they still describe saved view state and bundle replay as partially separate stories.
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewInspectionIntegrationTests.cs` and `VideraInspectionBundleIntegrationTests.cs` already cover the main session/export seams.

</code_context>

<specifics>
## Specific Ideas

- Add `Annotations` to `VideraInspectionState`.
- Make `CaptureInspectionState()` and `ApplyInspectionState(...)` clone and restore annotations safely.
- Fold inspection bundle annotation persistence into `inspection-state.json` so the typed session contract is the single source of truth.
- Update sample/docs/tests to teach that saved inspection state includes host-owned annotations.

</specifics>

<deferred>
## Deferred Ideas

- Bundle asset-manifest failure-path hardening
- Replay/export diagnostics and support-artifact tightening
- Any editor/project-format or non-Avalonia expansion

</deferred>
