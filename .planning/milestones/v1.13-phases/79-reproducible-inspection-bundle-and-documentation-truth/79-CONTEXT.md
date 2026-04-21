# Phase 79: Reproducible Inspection Bundle and Documentation Truth - Context

**Gathered:** 2026-04-19  
**Status:** Ready for planning and execution  
**Mode:** Autonomous

## Phase Boundary

Phase 79 turns inspection support into a replayable artifact workflow without widening `VideraView` into a larger project/package format. The phase must produce one repeatable bundle contract, teach it in the interaction sample, exercise it in consumer smoke, and align the public docs around the same fidelity story.

## Implementation Decisions

### Support artifact contract
- **D-01:** Export the inspection bundle as a directory artifact rather than introducing zip packaging in this milestone.
- **D-02:** The bundle must contain inspection state, annotations, diagnostics, rendered snapshot, and an asset manifest.
- **D-03:** Import should restore the scene only when the manifest proves the bundle is replayable from imported assets; unsupported host-owned scene mutations must stay explicit.

### Public surface
- **D-04:** Bundle export/import belongs in a dedicated `VideraInspectionBundleService` instead of adding more support-specific methods to `VideraView`.
- **D-05:** `CaptureInspectionState()`, `ApplyInspectionState(...)`, and `ExportSnapshotAsync(...)` remain the core viewer inspection surface; the bundle service composes those primitives.

### Support/workflow coverage
- **D-06:** `Videra.InteractionSample` should both export and import bundles so the feature is teachable and manually replayable.
- **D-07:** consumer smoke should at least export and validate the same bundle contract from packed public packages.
- **D-08:** README, Avalonia README, architecture, troubleshooting, alpha-feedback, and Chinese docs must all describe the same inspection-fidelity story and non-goals.

### the agent's Discretion
- Exact JSON schema shape for state, annotations, and asset-manifest documents, as long as object-id remapping and replay limitations remain explicit.
- Whether bundle metadata should include extra human-readable status text in addition to the core replay contract.

## Specific Ideas

- Capture asset-manifest data from the runtime scene document so import can rebuild the scene from the same imported assets without teaching hosts a new persistence API.
- Remap object identifiers on import so restored selection, measurements, and annotations survive a scene reload instead of assuming object ids remain stable across reloads.
- Treat docs truth as part of the phase deliverable, not as a postscript, because alpha support depends on the public wording being aligned.

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone and requirements
- `.planning/ROADMAP.md` — Phase 79 goal, requirements, and success criteria.
- `.planning/REQUIREMENTS.md` — `DIAG-06`, `DIAG-07`, and `DOC-10`.
- `docs/alpha-feedback.md` — current support ask and evidence contract.

### Existing public/runtime seams
- `src/Videra.Avalonia/Controls/VideraView.Inspection.cs` — current public inspection workflow surface.
- `src/Videra.Avalonia/Runtime/VideraViewRuntime.Inspection.cs` — inspection-state and snapshot seams the bundle should compose.
- `samples/Videra.InteractionSample` — canonical viewer-first inspection sample.
- `scripts/Invoke-ConsumerSmoke.ps1` and `smoke/Videra.ConsumerSmoke` — consumer evidence workflow that now needs bundle coverage.

## Existing Code Insights

### Reusable Assets
- `VideraInspectionState` already captures the persisted inspection truth the bundle needs.
- Diagnostics snapshot and snapshot export already exist, so the bundle phase is mostly orchestration plus replay metadata.
- The runtime scene document already knows which imported assets were used to build the scene and can provide a replay manifest.

### Established Patterns
- Recent milestones have kept `VideraView` narrow and pushed orchestration logic into runtime/services; the bundle flow should follow the same pattern.
- Repository tests already enforce doc/source/sample contract terms, which makes docs truth part of the executable surface.
- Consumer smoke already packs local packages before restoring the public sample, so bundle coverage there validates the real consumer path.

### Integration Points
- `VideraInspectionBundleService` can compose export/import without widening `VideraView`.
- `VideraViewRuntime` is the right place to expose internal asset-manifest capture for the bundle service.
- `InteractionSampleConfigurationTests` and repository contract tests can lock the new outward story.

## Deferred Ideas

- Zip packaging or upload-ready support bundles.
- Replay of arbitrary host-owned scene mutations beyond imported-asset scenes.
- A broader project/session format beyond the inspection support bundle.

---

*Phase: 79-reproducible-inspection-bundle-and-documentation-truth*  
*Context gathered: 2026-04-19*
