# Phase 7 Discussion Log

**Date:** 2026-04-08
**Mode:** manual design capture

## User-confirmed decisions

1. Priority order:
   - lifecycle safety first
   - software backend / wireframe / style contract consistency second
2. Lifecycle contract may be made explicit.
3. After `Dispose`, public entrypoints should prefer harmless `no-op` behavior rather than throwing.
4. A larger structural cleanup is acceptable; this phase should not be reduced to tiny guard patches.

## Resulting design direction

- Use explicit lifecycle states for both `VideraEngine` and `RenderSession`.
- Make `Dispose` terminal and dominant.
- Keep render-loop safety, handle rebinding, and suspend/resume behavior inside named states.
- Make software depth-state APIs real.
- Resolve the current mismatch between style-driven wireframe intent and actual pass selection.

---

*Phase: 07-render-contract-consistency-and-lifecycle-safety*
*Captured: 2026-04-08*
