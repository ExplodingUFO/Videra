# Phase 116: Inspection Replay and Export Hardening - Context

**Gathered:** 2026-04-21
**Status:** Ready for planning and execution

<domain>
## Phase Boundary

Phase `115` unified the typed inspection session. The remaining gap is replay/export failure truth:

- `VideraInspectionBundleService.ExportAsync(...)` can produce a non-replayable bundle when the current scene contains host-owned objects.
- `asset-manifest.json` records `CanReplayScene` and `ReplayLimitation`.
- `ImportAsync(...)` currently only blocks `!CanReplayScene` when the manifest still has bundled asset entries.
- A host-owned-object scene with zero bundled assets can therefore fall through and apply stale object-backed state into an unrelated view.

This phase should make replay boundaries explicit and auditable without widening the bundle format into a larger project system.

</domain>

<decisions>
## Implementation Decisions

### Fail import for any non-replayable bundle

Treat `CanReplayScene == false` as a hard import boundary, regardless of bundled asset count. This keeps replay truth direct and avoids silent partial restore behavior.

### Surface replay limitation at export time

Expose the replay limitation through the export result and sample/support wording so callers can explain why a bundle is exportable but not replayable.

</decisions>

<code_context>
## Existing Code Insights

- `src/Videra.Avalonia/Controls/VideraInspectionBundleService.cs` owns bundle export/import truth and currently keeps `ReplayLimitation` internal to the asset manifest.
- `src/Videra.Avalonia/Runtime/VideraViewRuntime.InspectionBundle.cs` marks scenes with host-owned objects as non-replayable.
- `samples/Videra.InteractionSample/Views/MainWindow.axaml.cs` currently only shows `Replayable scene: {bool}` after export.
- `tests/Videra.Core.IntegrationTests/Rendering/VideraInspectionBundleIntegrationTests.cs` covers only replayable happy paths today.
- `docs/alpha-feedback.md` and `docs/troubleshooting.md` already position inspection bundles as support artifacts, but they do not call out replay limitations explicitly.

</code_context>

<specifics>
## Specific Ideas

- Reject import immediately when `assetManifest.CanReplayScene` is false.
- Add a public `ReplayLimitation` surface to the export result so samples and support docs can report the reason directly.
- Add a focused integration test for non-replayable host-owned-object bundles.
- Update interaction sample/support docs to describe exportable-but-non-replayable bundles truthfully.

</specifics>

<deferred>
## Deferred Ideas

- Any broader bundle schema versioning or migration logic
- Any editor/workspace persistence model
- Any fallback/partial-restore behavior for non-replayable bundles

</deferred>
