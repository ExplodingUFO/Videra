# Phase 81: Linux Host Compatibility Truth - Context

**Gathered:** 2026-04-20  
**Status:** Executed and verified  
**Mode:** Autonomous

## Phase Boundary

Phase 81 makes Linux compatibility truth explicit across runtime diagnostics, consumer-smoke artifacts, and support docs. The target is not new Linux rendering capability; it is one consistent statement of what `X11`, `XWayland`, and unsupported compositor-native Wayland hosting mean in the current stack.

## Implementation Decisions

### Diagnostics truth
- **D-01:** Surface display-server compatibility as a first-class diagnostics sentence, not just a loose combination of raw fields.
- **D-02:** Keep the compatibility summary stable and copy-pasteable so smoke artifacts and bug reports can carry the same wording.

### Consumer/support evidence
- **D-03:** Push the compatibility summary into consumer-smoke traces and generated reports so the packaged consumer path emits support-ready evidence.
- **D-04:** Update troubleshooting, alpha-feedback, and Linux README wording to explain that `XWayland` is a compatibility fallback, not native compositor embedding.

### Scope control
- **D-05:** Do not present `OpenGL` as a fix for compositor-native Wayland limitations.
- **D-06:** Keep the truth centered on host/display-server boundaries, because that is where the current Linux compatibility ceiling actually lives.

## Specific Ideas

- Introduce one formatter helper to turn `ResolvedDisplayServer` into a stable compatibility sentence.
- Reuse the same wording in snapshot text, consumer-smoke JSON, and support docs so Linux feedback does not fork into multiple interpretations.
- Lock the wording in repository tests because Linux host truth is easy to regress through “helpful” but inaccurate docs edits.

## Canonical References

### Milestone and requirements
- `.planning/ROADMAP.md` — Phase 81 goal and success criteria.
- `.planning/REQUIREMENTS.md` — `HOST-01`, `HOST-02`, and `HOST-03`.

### Runtime and support surfaces
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- `src/Videra.Avalonia/Controls/VideraDiagnosticsSnapshotFormatter.cs`
- `smoke/Videra.ConsumerSmoke/Views/MainWindow.axaml.cs`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `docs/alpha-feedback.md`
- `docs/troubleshooting.md`
- `src/Videra.Platform.Linux/README.md`

## Existing Code Insights

### Reusable assets
- `VideraBackendDiagnostics` already carried the raw display-server fields needed to derive a compatibility summary.
- Consumer smoke already exported JSON and trace output, so the new display-server truth could piggyback on existing evidence paths.

### Risks carried into the phase
- Linux docs and runtime output made it too easy to conflate `XWayland` fallback with native Wayland support.
- The repo needed stronger wording to keep “missing `OpenGL` backend” from sounding like the root cause of Wayland host limitations.

## Deferred Ideas

- Native compositor-hosted Wayland embedding.
- Any new Linux backend line or graphics API expansion.

---

*Phase: 81-linux-host-compatibility-truth*  
*Context gathered: 2026-04-20*
