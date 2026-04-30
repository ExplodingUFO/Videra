# Phase 329: Viewer Interaction Evidence Polish - Plan

**Bead:** Videra-6oi

## Plan

1. Add a public `VideraInteractionEvidence` report model.
2. Add `VideraInteractionEvidenceFormatter` that creates/formats evidence from `VideraInspectionState` and optional `VideraInteractionDiagnostics`.
3. Cover deterministic populated and empty formatting behavior with focused tests.

## Verification

Run the focused Avalonia test class for the new formatter.
