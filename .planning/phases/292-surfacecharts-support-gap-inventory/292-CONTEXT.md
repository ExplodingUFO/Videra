# Phase 292: SurfaceCharts Support Gap Inventory - Context

**Gathered:** 2026-04-28  
**Status:** Ready for planning  
**Bead:** Videra-0w9.1

<domain>
## Phase Boundary

Map current SurfaceCharts demo, Performance Lab, Doctor, support evidence, and lifecycle gaps before changing product code.

This phase is read-only inventory. It should not add demo features, renderer behavior, chart families, CI skips, or package/public API surface.
</domain>

<decisions>
## Implementation Decisions

1. The next implementation work should be supportability and reliability closure, not broad architecture optimization.
2. SurfaceCharts support evidence remains separate from `VideraDiagnosticsSnapshotFormatter`.
3. SurfaceCharts remains an independent package/control family and must not become a `VideraView` mode.
4. Performance Lab and visual evidence remain evidence-only, not benchmark guarantees or visual-regression gates.
5. Phase 293 and Phase 294 can proceed in parallel after this inventory because their write surfaces can be kept disjoint.
</decisions>

<code_context>
## Existing Code Insights

### SurfaceCharts Demo

- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml` already exposes a visible `Support summary` panel and `Copy support summary` button.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` builds separate support summaries for surface/waterfall and scatter paths.
- Surface/waterfall summary already includes `GeneratedUtc`, `EvidenceKind`, `EvidenceOnly`, source path/details, `ViewState`, `InteractionQuality`, `RenderingStatus`, `OverlayOptions`, cache asset, and dataset.
- Scatter summary already includes scenario id/name/update mode, point counts, FIFO capacity, pickability, camera status, `RenderingStatus`, streaming counters, and overlay non-applicability.
- Missing support-report fields are mostly environment and identity fields: OS/runtime/architecture, repo commit or package version, exact chart/control type, relevant environment variables, and cache-load failure detail.

### Performance Lab, Doctor, and Evidence

- `tools/Videra.PerformanceLabVisualEvidence` uses `PerformanceLabVisualEvidenceManifest` with `schemaVersion`, `evidenceKind`, `evidenceOnly`, `status`, `generatedUtc`, dimensions, summary path, entries, and notes.
- `scripts/Invoke-VideraDoctor.ps1` passively discovers Performance Lab visual evidence under `artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json`.
- Doctor reports visual evidence as `present`, `missing`, or `unavailable`; it does not capture screenshots.
- Release evidence index also consumes visual evidence passively and keeps `publishBlocker = false`.
- Existing tests guard evidence-only wording, missing/present/unavailable states, and non-overclaim language.

### Lifecycle and Test Host

- SurfaceCharts integration tests share `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/AvaloniaHeadlessTestSession.cs`.
- Headless dispatch currently uses default cancellation and does not expose timeout evidence at the session wrapper boundary.
- Repository tests around Doctor script execution already use `WaitForExit` timeouts and `Kill(entireProcessTree: true)` as a reliable pattern.
- CI runs SurfaceCharts sample evidence and integration evidence through targeted filters in `.github/workflows/ci.yml`.
</code_context>

<specifics>
## Specific Ideas

Phase 293 should add a concise support-report refinement rather than another broad demo panel:

- stable chart/control type field
- environment/runtime identity field
- repo/package identity when available
- selected scenario/source identity
- cache-load failure detail field if a fallback happened
- docs/tests that preserve evidence-only wording

Phase 294 should add lifecycle guardrails around the existing headless test/session surface:

- deterministic timeout or cancellation behavior
- failure output that identifies timeout/shutdown context
- no blanket test skips or CI suppressions
- no rendering semantic changes

Phase 295 should align Doctor/support discovery after 293 and 294:

- passive discovery for SurfaceCharts support report when present
- consistent names with Performance Lab visual evidence
- missing/present/unavailable tests
- guardrails against benchmark or visual-regression overclaiming
</specifics>

<deferred>
## Deferred Ideas

- New SurfaceCharts chart families.
- Renderer/backend architecture rewrite.
- GPU-driven culling or generic benchmark editor.
- SurfaceCharts as a `VideraView` mode.
- Stable benchmark threshold promotion for demo or visual-evidence output.
- Pixel-perfect visual regression gate.
</deferred>
