# Phase 313 Summary: Authoring Polish Inventory

## Result

Phase 313 completed the v2.43 implementation inventory. The next work can split cleanly:

- Phase 314 owns Core authoring polish and a minimal authoring sample.
- Phase 315 owns SurfaceCharts chart-local numeric precision policy.
- Phase 317 owns Beads public roadmap generation.
- Phase 316 waits for Phase 314 and Phase 315, then adds a bounded optional workbench first slice.

## Phase 314 Handoff

Recommended scope:

- Keep `SceneAuthoring`/`SceneAuthoringBuilder` as the current alpha API name for this milestone. Do not rename or add aliases until there is stronger evidence that the rename is worth churn.
- Add only high-value static-scene helpers that fit the existing data-oriented contract. The clearest gap is `SceneGeometry.Sphere(...)` plus `SceneAuthoringBuilder.AddSphere(...)`.
- Add a minimal authoring sample that loads no model files and builds an interactive scene with authored primitives plus instance-aware markers.
- Keep output as `SceneDocument`, `ImportedSceneAsset`, `MeshPrimitive`, `MaterialInstance`, and `InstanceBatch` truth.

Suggested ownership:

- `src/Videra.Core/Scene/*`
- `tests/Videra.Core.Tests/Scene/*`
- `samples/Videra.MinimalAuthoringSample/*`
- relevant sample docs/project registration

Validation:

- Focused Core authoring tests.
- Build or test coverage for the new sample if project patterns support it.

## Phase 315 Handoff

Recommended scope:

- Reuse `SurfaceChartOverlayOptions` as the chart-local numeric policy.
- Extend probe state/presenter readouts to use the same policy that axis and legend labels use.
- Avoid broader style preset expansion unless it is small, tested, and directly tied to precision/readout consistency.
- Keep all semantics in SurfaceCharts packages.

Suggested ownership:

- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayState.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/*`
- SurfaceCharts docs if public behavior changes

Validation:

- Focused overlay/probe tests for fixed, engineering, scientific, and delta readouts.

## Phase 316 Handoff

Recommended scope:

- Wait until Phase 314 and Phase 315 finish.
- Prefer a sample-first workbench slice unless an existing package boundary clearly supports reusable tooling without package sprawl.
- Compose public APIs only: diagnostics snapshot/support capture, scene loading/authoring entry, and chart precision settings.
- Use snapshot/throttled refresh behavior rather than per-frame UI churn.

Suggested ownership:

- `samples/*`
- sample docs
- focused tests around sample configuration or reusable view model behavior

Validation:

- Build/test the touched sample surface.
- Confirm no dependency from `Videra.Core` or base `Videra.Avalonia` to workbench/tooling.

## Phase 317 Handoff

Recommended scope:

- Generate a deterministic public roadmap artifact from `.beads/issues.jsonl`.
- Document that Beads remains authoritative and the generated markdown is a read-only public summary.
- Avoid requiring external readers to run Docker/Dolt or inspect Beads JSON manually.

Suggested ownership:

- `scripts/Export-BeadsRoadmap.ps1`
- `docs/ROADMAP.generated.md` or equivalent public docs path
- `docs/beads-coordination.md`
- repository tests for deterministic output

Validation:

- Script output is deterministic.
- Repository tests cover the docs/script contract.

## Risks

- Renaming authoring APIs now would create churn without improving runtime truth. Defer.
- Workbench can become a god demo if it tries to cover every viewer and chart feature. Keep the first slice narrow.
- Probe precision can duplicate formatting again if it remains in presenter/state string interpolation. Route through one helper.
