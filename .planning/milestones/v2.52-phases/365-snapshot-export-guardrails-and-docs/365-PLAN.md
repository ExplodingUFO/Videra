---
phase: 365-snapshot-export-guardrails-and-docs
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - AGENTS.md
  - scripts/Test-SnapshotExportScope.ps1
  - docs/support-matrix.md
  - docs/capability-matrix.md
  - docs/ROADMAP.generated.md
  - .planning/ROADMAP.md
  - .planning/REQUIREMENTS.md
  - .planning/STATE.md
autonomous: true
requirements: [GUARD-01, GUARD-02, GUARD-03, GUARD-04, VER-01, VER-02, VER-03]

must_haves:
  truths:
    - "AGENTS.md contains snapshot export scope rules blocking old chart controls, direct Source API, PDF/vector export, backend expansion, generic plotting engine, compatibility wrappers, hidden fallback/downshift, and god-code"
    - "Guardrail script detects and blocks reintroduction of old chart view types, direct Source API, PDF/vector export, backend expansion scope creep, and hidden fallback patterns"
    - "Support matrix and capability matrix document chart-local snapshot export as a shipped capability"
    - "Public ROADMAP.generated.md reflects completed v2.52 milestone with all beads closed"
    - "All 5 Videra-lu9 beads are closed with milestone epic"
    - "Local planning state (ROADMAP.md, REQUIREMENTS.md, STATE.md) marks all phases complete"
  artifacts:
    - path: "AGENTS.md"
      provides: "Snapshot export scope boundary rules"
      contains: "Snapshot Export Scope Boundaries"
    - path: "scripts/Test-SnapshotExportScope.ps1"
      provides: "Automated guardrail verification script"
      exports: ["Test-SnapshotExportScope"]
    - path: "docs/support-matrix.md"
      provides: "Snapshot export capability in support matrix"
      contains: "Snapshot"
    - path: "docs/capability-matrix.md"
      provides: "Snapshot export in shipped capabilities"
      contains: "snapshot"
    - path: "docs/ROADMAP.generated.md"
      provides: "Clean public roadmap with completed v2.52"
  key_links:
    - from: "AGENTS.md"
      to: "scripts/Test-SnapshotExportScope.ps1"
      via: "AGENTS.md references guardrail script for enforcement"
      pattern: "Test-SnapshotExportScope"
    - from: "docs/support-matrix.md"
      to: "docs/capability-matrix.md"
      via: "Both document snapshot export as shipped capability"
      pattern: "snapshot"
---

<objective>
Close docs, public roadmap, repository guardrails, and Beads state around the chart-local bitmap snapshot export contract completed in Phases 361-364.

Purpose: Lock down the v2.52 milestone boundary so future work cannot reintroduce old chart controls, direct Source APIs, PDF/vector export, backend expansion, or other scope creep. Document the shipped Plot-owned snapshot export path for consumers.

Output: Guardrail script, updated docs, clean Beads state, completed milestone.
</objective>

<execution_context>
@$HOME/.config/opencode/get-shit-done/workflows/execute-plan.md
@$HOME/.config/opencode/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/REQUIREMENTS.md
@.planning/phases/365-snapshot-export-guardrails-and-docs/365-CONTEXT.md
@AGENTS.md
@docs/support-matrix.md
@docs/capability-matrix.md
@docs/ROADMAP.generated.md
</context>

<tasks>

<task type="auto">
  <name>Task 1: Add snapshot export scope rules to AGENTS.md and create guardrail script</name>
  <files>AGENTS.md, scripts/Test-SnapshotExportScope.ps1</files>
  <read_first>
    - AGENTS.md (current content and structure)
    - .planning/phases/365-snapshot-export-guardrails-and-docs/365-CONTEXT.md (scope rules from CONTEXT.md)
    - .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md (non-goals and rejected items)
    - .planning/phases/362-plot-snapshot-export-contract/362-01-SUMMARY.md (contract types)
    - .planning/phases/363-chart-snapshot-capture-implementation/363-01-SUMMARY.md (capture API)
    - .planning/phases/364-demo-smoke-doctor-snapshot-evidence/364-01-SUMMARY.md (evidence wiring)
    - docs/support-matrix.md (existing boundary notes pattern)
  </read_first>
  <action>
    **1. Update AGENTS.md — add Snapshot Export Scope Boundaries section** (per GUARD-01, GUARD-02)

    Insert a new section after the "Session Completion" section (before `<!-- END BEADS INTEGRATION -->`). The section must contain:

    ```markdown
    ## Snapshot Export Scope Boundaries (v2.52)

    Chart-local bitmap snapshot export is the shipped v2.52 capability. These boundaries prevent scope creep:

    ### What Exists

    - `VideraChartView.Plot.CaptureSnapshotAsync(PlotSnapshotRequest)` — Plot-owned PNG capture
    - `PlotSnapshotRequest` / `PlotSnapshotResult` / `PlotSnapshotManifest` — contract types in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/`
    - `Plot3DOutputCapabilityDiagnostic` reports `ImageExport` as supported
    - Doctor parses snapshot status (present/failed/unavailable/missing)
    - Demo exposes bounded CaptureSnapshot button (1920x1080, no config UI)

    ### Blocked — Do Not Reintroduce

    - **Old chart view controls**: `SurfaceChartView`, `WaterfallChartView`, `ScatterChartView` — `VideraChartView` is the single shipped control
    - **Direct public `Source` API**: `Plot.Add.Surface/Waterfall/Scatter` is the data-loading path
    - **PDF/vector export**: This milestone is PNG/bitmap only
    - **Backend expansion**: Snapshot stays chart-local via Avalonia `RenderTargetBitmap`
    - **Generic plotting engine**: Scoped to current 3D chart model
    - **Compatibility wrappers**: Removed alpha APIs stay removed
    - **Hidden fallback/downshift**: Unsupported output = explicit diagnostics
    - **God-code workbench**: Demo/support stays bounded and sample-first

    ### Guardrail Verification

    Run `scripts/Test-SnapshotExportScope.ps1` to verify no scope violations exist.
    ```

    **2. Create scripts/Test-SnapshotExportScope.ps1** (per GUARD-01, GUARD-02)

    This PowerShell script must:
    - Search `src/` for reintroduction of old chart view types: `SurfaceChartView`, `WaterfallChartView`, `ScatterChartView` as public class declarations (not test references)
    - Search for direct `Source` property as public API on chart controls
    - Search for PDF or vector export code patterns (e.g., `PdfExport`, `VectorExport`, `SvgExport` as enabled/implemented features, not as diagnostic markers)
    - Search for `VideraSnapshotExportService` references from chart-local code (chart snapshot must not couple to viewer-level export service)
    - Search for hidden fallback patterns: `FallbackReason` with non-empty string values in snapshot paths (not diagnostic evidence)
    - Output PASS/FAIL per check with file paths if violations found
    - Exit 0 if all checks pass, exit 1 if any violation found
    - Follow existing script patterns (see `scripts/verify.ps1` for structure: `$ErrorActionPreference = "Stop"`, Write-Host with color, clear pass/fail)

    The script should NOT modify any files — read-only verification only.
  </action>
  <verify>
    <automated>pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SnapshotExportScope.ps1</automated>
  </verify>
  <done>
    - AGENTS.md contains "Snapshot Export Scope Boundaries" section with blocked items list
    - Test-SnapshotExportScope.ps1 exists and runs without errors
    - Script exits 0 (no violations found — expected since Phases 361-364 are complete)
    - grep -q "Snapshot Export Scope Boundaries" AGENTS.md returns 0
  </done>
</task>

<task type="auto">
  <name>Task 2: Update docs with snapshot export capability and regenerate public roadmap</name>
  <files>docs/support-matrix.md, docs/capability-matrix.md, docs/ROADMAP.generated.md</files>
  <read_first>
    - docs/support-matrix.md (full content — add snapshot to boundary notes)
    - docs/capability-matrix.md (full content — add snapshot to shipped capabilities)
    - docs/ROADMAP.generated.md (full content — regenerate with completed beads)
    - docs/index.md (verify no changes needed)
    - .planning/phases/363-chart-snapshot-capture-implementation/363-01-SUMMARY.md (capture API for docs)
    - .planning/phases/364-demo-smoke-doctor-snapshot-evidence/364-01-SUMMARY.md (evidence for docs)
  </read_first>
  <action>
    **1. Update docs/support-matrix.md** (per GUARD-03)

    In the "Boundary notes" section, add a new bullet after the existing `VideraChartView` bullet (line 44):

    ```markdown
    - Chart-local PNG/bitmap snapshot export is a shipped v2.52 capability. Use `Plot.CaptureSnapshotAsync(PlotSnapshotRequest)` to capture chart snapshots; the result includes a `PlotSnapshotManifest` with deterministic metadata (dimensions, active series identity, output/dataset evidence kinds). Snapshot capture uses Avalonia `RenderTargetBitmap` and stays chart-local — it does not expand into PDF/vector export, backend rewrite, or viewer-level `VideraSnapshotExportService` coupling.
    ```

    In the SurfaceCharts stack table, update the `Videra.SurfaceCharts.Avalonia` row Notes column to append: "Includes `Plot.CaptureSnapshotAsync` for PNG snapshot export with manifest evidence."

    **2. Update docs/capability-matrix.md** (per GUARD-03)

    In the "Shipped in the 1.0 Line" table, update the `SurfaceCharts` package family row's `Current truth` column. After the existing text about `Plot.Add.*` entry package, append: "`Plot.CaptureSnapshotAsync` provides chart-local PNG/bitmap snapshot export with deterministic manifest evidence; PDF/vector export remains deferred."

    **3. Regenerate docs/ROADMAP.generated.md** (per GUARD-04)

    Run `scripts/Export-BeadsRoadmap.ps1` to regenerate the public roadmap from Beads state. This will reflect the current bead statuses after Task 3 closes them.

    **Note:** If `Export-BeadsRoadmap.ps1` fails (Dolt not running), manually update ROADMAP.generated.md to reflect that Videra-lu9.1 through Videra-lu9.5 are complete and Videra-lu9 epic is closed, moving them from Ready/Blocked to Recently Closed.
  </action>
  <verify>
    <automated>grep -q "CaptureSnapshotAsync" docs/support-matrix.md && grep -q "snapshot" docs/capability-matrix.md && grep -q "Videra-lu9" docs/ROADMAP.generated.md && echo "ALL DOCS UPDATED" || echo "MISSING DOC UPDATES"</automated>
  </verify>
  <done>
    - docs/support-matrix.md references CaptureSnapshotAsync in boundary notes and SurfaceCharts table
    - docs/capability-matrix.md references snapshot export as shipped capability
    - docs/ROADMAP.generated.md reflects completed v2.52 milestone with all beads closed or in Recently Closed
  </done>
</task>

<task type="auto">
  <name>Task 3: Close Beads and update local planning state to mark milestone complete</name>
  <files>.planning/ROADMAP.md, .planning/REQUIREMENTS.md, .planning/STATE.md</files>
  <read_first>
    - .planning/ROADMAP.md (update phase statuses)
    - .planning/REQUIREMENTS.md (check GUARD requirements)
    - .planning/STATE.md (update state)
    - AGENTS.md (beads workflow reference)
  </read_first>
  <action>
    **1. Close Beads** (per GUARD-04)

    Close beads in dependency order (children first, then epic):
    ```bash
    bd close Videra-lu9.1 --reason "Phase 361 complete — inventory classified all gaps" --json
    bd close Videra-lu9.2 --reason "Phase 362 complete — 6 contract types with 20 tests" --json
    bd close Videra-lu9.3 --reason "Phase 363 complete — CaptureSnapshotAsync with 11 tests" --json
    bd close Videra-lu9.4 --reason "Phase 364 complete — demo/smoke/Doctor snapshot evidence wired" --json
    bd close Videra-lu9.5 --reason "Phase 365 complete — guardrails, docs, and Beads state clean" --json
    bd close Videra-lu9 --reason "v2.52 milestone complete — chart-local PNG snapshot export shipped" --json
    ```

    **Note:** If `bd` commands fail (Dolt not running), document the bead closure intent in the plan summary and proceed with local planning state updates.

    **2. Update .planning/ROADMAP.md** (per GUARD-04)

    - Change Phase 363 status from "planned" to "Complete"
    - Change Phase 364 status from "planned" to "Complete"
    - Change Phase 365 status from "planned" to "Complete"
    - Update "Next" section to indicate milestone is complete
    - Mark all checkboxes in Plans lists as complete: `[x]`

    **3. Update .planning/REQUIREMENTS.md**

    - Mark GUARD-01, GUARD-02, GUARD-03, GUARD-04 as `[x]` (complete)
    - Update traceability table: change "Planned" to "Complete" for all GUARD rows

    **4. Update .planning/STATE.md**

    - Update `status` to `active`
    - Update `stopped_at` to "Completed Phase 365"
    - Update `last_updated` to current timestamp
    - Update `progress`:
      - `completed_phases`: 5
      - `completed_plans`: 5 (or appropriate count)
      - `percent`: 100
    - Update `Current Position` section: Phase 365, complete, milestone complete
    - Update `Beads` table: all beads complete
    - Update `Session Continuity`: next action is milestone archival or new milestone
  </action>
  <verify>
    <automated>grep -q "Complete" .planning/ROADMAP.md && grep -q "\[x\].*GUARD-01" .planning/REQUIREMENTS.md && grep -q "percent: 100" .planning/STATE.md && echo "PLANNING STATE CLEAN" || echo "PLANNING STATE INCOMPLETE"</automated>
  </verify>
  <done>
    - All 5 Videra-lu9.x beads closed (or closure intent documented if Dolt unavailable)
    - Videra-lu9 epic closed
    - .planning/ROADMAP.md shows all 5 phases as Complete
    - .planning/REQUIREMENTS.md shows all GUARD requirements as [x]
    - .planning/STATE.md shows 100% progress and milestone complete
  </done>
</task>

</tasks>

<verification>
1. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SnapshotExportScope.ps1` — exits 0 (no scope violations)
2. `grep -q "Snapshot Export Scope Boundaries" AGENTS.md` — exits 0
3. `grep -q "CaptureSnapshotAsync" docs/support-matrix.md` — exits 0
4. `grep -q "snapshot" docs/capability-matrix.md` — exits 0
5. `grep -q "Complete" .planning/ROADMAP.md` — all phases show Complete
6. `grep -q "\[x\].*GUARD-01" .planning/REQUIREMENTS.md` — exits 0
7. `dotnet build Videra.slnx --configuration Release -v q` — build succeeds (no regressions from doc/script changes)
</verification>

<success_criteria>
1. AGENTS.md contains snapshot export scope boundary rules that block old chart controls, direct Source API, PDF/vector export, backend expansion, generic plotting engine, compatibility wrappers, hidden fallback/downshift, and god-code
2. Test-SnapshotExportScope.ps1 passes with no violations detected
3. docs/support-matrix.md and docs/capability-matrix.md document snapshot export as shipped capability
4. docs/ROADMAP.generated.md reflects completed v2.52 milestone
5. All Videra-lu9 beads closed
6. .planning/ROADMAP.md, REQUIREMENTS.md, and STATE.md show milestone 100% complete
7. Build still passes after all changes
</success_criteria>

<output>
After completion, create `.planning/phases/365-snapshot-export-guardrails-and-docs/SUMMARY.md`
</output>
