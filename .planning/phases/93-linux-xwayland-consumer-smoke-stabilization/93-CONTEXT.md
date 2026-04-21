# Phase 93: Linux XWayland Consumer Smoke Stabilization - Context

**Gathered:** 2026-04-20
**Status:** In progress
**Mode:** Auto-generated (discuss skipped via autonomous infrastructure detection)

<domain>
## Phase Boundary

Stabilize the Linux `XWayland` consumer smoke path so failures always emit actionable artifacts, and make the workflow run inside a verifiable Wayland-session-with-XWayland environment before claiming the runtime path is fixed.

</domain>

<decisions>
## Implementation Decisions

### the agent's Discretion
- Treat the `consumer-smoke.yml` / `publish-public.yml` Linux `xwayland` jobs as the source of truth; they are separate from `native-validation.yml`.
- Fix workflow/session-contract and wrapper-artifact gaps before touching runtime/viewer code.
- Do not mark `SMOKE-02` satisfied until the repaired `linux-xwayland` smoke path is executed on a real Linux `XWayland` host or CI runner.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `scripts/Invoke-ConsumerSmoke.ps1` already owns the smoke output root, child-process launch, and expected artifact contract.
- `smoke/Videra.ConsumerSmoke/Views/MainWindow.axaml.cs` emits managed trace breadcrumbs plus final JSON/snapshot artifacts when the app reaches `CompleteAsync(...)`.
- Historical artifact `artifacts/gh-run-24610316740/consumer-smoke-trace.log` shows the nominal XWayland job resolved `display=X11` and produced no `consumer-smoke-result.json`.

### Established Patterns
- The native-validation XWayland workflow prints `DISPLAY`, `WAYLAND_DISPLAY`, `XDG_RUNTIME_DIR`, and `XDG_SESSION_TYPE` before launch and runs under `bash -lc`.
- Repository contract tests in `tests/Videra.Core.Tests/Repository/AlphaConsumerIntegrationTests.cs` already guard smoke workflow/script expectations through source-level assertions.

### Integration Points
- `.github/workflows/consumer-smoke.yml`
- `.github/workflows/publish-public.yml`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `artifacts/gh-run-24610316740/*`

</code_context>

<specifics>
## Specific Ideas

- Root-cause evidence points to two layers: the XWayland workflow was not asserting or exposing the Wayland-session environment, and the smoke wrapper was not persisting fallback artifacts when the child exited before managed completion.
- Wrapper-side fallback artifacts can satisfy `SMOKE-01` / `SMOKE-03` even when the smoke app exits early, but only a real Linux `XWayland` run can satisfy `SMOKE-02`.

</specifics>

<deferred>
## Deferred Ideas

- Runtime/viewer changes around `CompleteAsync(...)` or native host lifetime until the repaired workflow is rerun on a real Linux `XWayland` environment.

</deferred>
