# Requirements: v2.58 SurfaceCharts Controlled Release Cutover

## Goal

Turn the completed v2.57 SurfaceCharts release-readiness evidence into a controlled release cutover package: approval packet, version/package contract finalization, gated dry-run automation, release notes/support docs, and final handoff. Actual public NuGet publish, public tag creation, and GitHub release publication remain out of scope unless separately approved by a human.

## Active Requirements

### Approval and Evidence Packet

- **APPROVAL-01**: Inventory the v2.57 final validation artifacts, release-readiness script output, consumer smoke evidence, docs handoff, Beads state, generated roadmap, and archive locations.
- **APPROVAL-02**: Produce a release cutover approval packet that distinguishes evidence already proven from actions still requiring human approval.
- **APPROVAL-03**: Define the release abort/hold criteria for package, docs, validation, Beads, Git, Dolt, and remote-state failures.
- **APPROVAL-04**: Keep Beads as the source of task status, ownership, dependencies, validation expectations, and handoff notes.

### Version and Package Contracts

- **PKG-01**: Confirm the SurfaceCharts package version, package metadata, dependency surface, symbols/assets, and README/package links before any cutover.
- **PKG-02**: Build and inspect local package assets without mutating public feeds or credentials.
- **PKG-03**: Keep the public API contract aligned with the shipped `VideraChartView.Plot` model and reject unexpected public API or package-scope drift.
- **PKG-04**: Preserve the single shipped `VideraChartView` control and `Plot.Add.*` data-loading path.

### Gated Release Automation

- **GATE-01**: Provide a single non-interactive release dry-run path that exercises package build, package inspection, consumer smoke, focused tests, docs checks, and scope guardrails.
- **GATE-02**: Make public publish, tag creation, and GitHub release steps explicit manual gates that fail closed by default.
- **GATE-03**: Separate pass/fail/skipped/manual-gate states in command output and generated evidence.
- **GATE-04**: Avoid hidden fallback/downshift behavior in release validation; unsupported or blocked release actions must be explicit.

### Consumer Docs and Support

- **DOC-01**: Prepare release notes/changelog content from the current package surface and v2.55-v2.57 SurfaceCharts outcomes.
- **DOC-02**: Keep package-consumption, cookbook, migration, and support paths discoverable from public-facing docs.
- **DOC-03**: Name exact support artifacts and commands package consumers should attach when package restore, rendering, snapshot, or cookbook smoke fails.
- **DOC-04**: Preserve the ScottPlot inspiration boundary without claiming ScottPlot API compatibility.

### Final Verification and Handoff

- **VERIFY-01**: Run focused validation for package build/inspection, consumer smoke, public API guardrails, cookbook/demo docs, snapshot scope, and support artifacts.
- **VERIFY-02**: Synchronize Beads state, generated public roadmap, phase evidence, milestone archive, branch/worktree cleanup, Git push, and Dolt Beads push.
- **VERIFY-03**: Record any blocked publish/tag/GitHub release actions as gated follow-up, not as completed release work.

## Out of Scope

- Public NuGet publish, public tag creation, or GitHub release publication without explicit human approval.
- Full ScottPlot compatibility or a ScottPlot API compatibility layer.
- Restoring `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- Restoring a public direct `Source` API.
- Compatibility wrappers for removed alpha APIs.
- PDF/vector export or broad export-format expansion.
- Renderer/backend/platform expansion.
- Hidden fallback/downshift behavior.
- Generic plotting-engine or god-code demo/workbench expansion.

## Traceability

- **Phase 396**: APPROVAL-01, APPROVAL-02, APPROVAL-03, APPROVAL-04
- **Phase 397**: PKG-01, PKG-02, PKG-03, PKG-04
- **Phase 398**: GATE-01, GATE-02, GATE-03, GATE-04
- **Phase 399**: DOC-01, DOC-02, DOC-03, DOC-04
- **Phase 400**: VERIFY-01, VERIFY-02, VERIFY-03
