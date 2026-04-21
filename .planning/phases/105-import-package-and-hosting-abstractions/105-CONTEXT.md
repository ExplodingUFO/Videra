# Phase 105: Import Package and Hosting Abstractions - Context

**Gathered:** 2026-04-21
**Status:** Completed on 2026-04-21

<domain>
## Phase Boundary

Phase 104 already extracted glTF/OBJ import into dedicated public packages and made the public package path shippable again. The next gap is not another structural rewrite. It is making the canonical viewer composition and the host/runtime seams explicit enough that the repository does not drift back into implicit `Core` / `Import` / `UI adapter` coupling.

</domain>

<decisions>
## Implementation Decisions

### Keep the solution direct

- Do not add broad new abstraction layers just to satisfy the roadmap wording.
- Prefer explicit docs plus repository guards over speculative new interfaces.
- Keep `VideraView` as the public Avalonia shell, and keep runtime/session/native-host helpers internal.

### Tighten the boundary through truth and tests

- Add one focused hosting-boundary doc that explains the canonical composition story and the important internal seams.
- Add reflection-based repository tests that prove the `Videra.Avalonia` public API does not leak import-package types or internal host/session types.
- Add repository truth assertions that the import packages stay core-based and do not pick up Avalonia/backend dependencies.

</decisions>

<code_context>
## Existing Code Insights

- `VideraView` is the public UI shell; it owns an internal `INativeHostFactory` seam and delegates runtime work to `VideraViewRuntime`.
- `VideraViewRuntime` creates `RenderSession`, `VideraViewSessionBridge`, and `RuntimeFramePrelude`, keeping scene/runtime coordination internal.
- `RenderSession` and `RenderSessionOrchestrator` already separate render-loop shell behavior from backend/device/surface orchestration.
- `SceneImportService` is internal to Avalonia and now composes directly with `Videra.Import.Gltf` / `Videra.Import.Obj`.
- Public docs currently describe the package stack, but they do not yet have one focused document for host/runtime seam ownership.

</code_context>

<specifics>
## Specific Ideas

1. Add `docs/hosting-boundary.md` as the canonical explanation of `Core` / `Import` / `Backend` / `UI adapter` / `Charts` composition and the key internal seam owners.
2. Link that doc from `docs/index.md`, `README.md`, `ARCHITECTURE.md`, and package docs where appropriate.
3. Add reflection/repository tests that guard:
   - `Videra.Avalonia` public API does not expose `Videra.Import.*` types.
   - `Videra.Avalonia` public API does not expose internal host/runtime/session seam types.
   - `Videra.Import.*` public APIs stay anchored on `Videra.Core` runtime types plus standard system/logging contracts.

</specifics>

<deferred>
## Deferred Ideas

- Do not add new public host-factory or runtime-session abstractions in this phase.
- Do not generalize the viewer shell into a multi-UI host framework here.
- Do not widen backend-specific public API in the name of “explicit seams”.

</deferred>
