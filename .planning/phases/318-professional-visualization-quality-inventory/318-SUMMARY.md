# Phase 318 Summary

## Result

Phase 318 completed the v2.44 inventory and confirmed the recommended execution order:

- Phase 319 and Phase 320 can run in parallel after this phase.
- Phase 321 waits for both because the workbench should consume the improved authoring/chart presentation surfaces.
- Phase 322 waits for implementation phases and closes docs/guardrails.

## Phase 319 Handoff: Authoring Visual Semantics

Recommended scope:

- Add one bounded Core authoring helper area for professional static visualization scenes.
- Best first target: axis/triad or scale-line helper because current authored scenes have grid/sphere/instances but no semantic coordinate helper.
- Keep helper output as ordinary line/mesh primitives in `SceneDocument` truth.
- Keep repeated markers instance-aware through existing `AddInstances(...)`.

Suggested ownership:

- `src/Videra.Core/Scene/SceneGeometry.cs`
- `src/Videra.Core/Scene/SceneAuthoringBuilder.cs`
- `tests/Videra.Core.Tests/Scene/SceneAuthoringBuilderTests.cs`
- `samples/Videra.MinimalAuthoringSample/*` only if demonstrating the helper is useful and small

Validation:

- Focused Core authoring tests proving topology, names, material ids, retained document truth, and no duplicate id regressions.
- Optional sample build if the sample changes.

Non-goals:

- Do not add ECS, scene graph editor, runtime update behavior, material graph, or backend-specific promise.

## Phase 320 Handoff: SurfaceCharts Professional Presentation

Recommended scope:

- Add one small chart-local presentation improvement.
- Best first target: formalize one professional multi-stop color-map preset or overlay preset currently living in demo-only code.
- Keep numeric precision consistent with `SurfaceChartOverlayOptions` and existing probe/axis/legend policy.

Suggested ownership:

- `src/Videra.SurfaceCharts.Core/ColorMaps/SurfaceColorMapPresets.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/SurfaceColorMapTests.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs` if overlay presets change
- SurfaceCharts README files if public preset names change

Validation:

- Focused color-map/preset tests.
- SurfaceCharts overlay tests only if overlay behavior changes.

Non-goals:

- Do not add new chart families.
- Do not move chart semantics into `VideraView`.

## Phase 321 Handoff: Workbench Evidence

Recommended scope:

- Consume Phase 319/320 improvements in `samples/Videra.AvaloniaWorkbenchSample`.
- Make support capture more structured and useful while staying sample-first.
- Include named fields such as scene name/version, node count, primitive count, instance batch count, instance count, selected marker id, chart precision profile, and diagnostics snapshot status.
- Keep diagnostics explicit or event-driven; do not refresh every frame.

Suggested ownership:

- `samples/Videra.AvaloniaWorkbenchSample/*`
- focused sample docs
- repository/sample tests if useful

Validation:

- Build `samples/Videra.AvaloniaWorkbenchSample`.
- Add focused tests only if support capture formatting is factored into a testable helper.

Non-goals:

- Do not introduce a reusable package unless a clear boundary already exists.
- Do not change `VideraView` or `Videra.Core`.

## Phase 322 Handoff: Guardrails and Docs

Recommended scope:

- Update public docs and guardrail tests for v2.44.
- Update `BeadsPublicRoadmapTests` so it no longer hardcodes stale v2.43-only expectations when v2.44 is active.
- Add or update guardrails that capture professional visualization boundaries: no backend expansion, no hidden fallback, no broad chart-family expansion, no god-code.

Suggested ownership:

- `docs/ROADMAP.generated.md`
- `scripts/Export-BeadsRoadmap.ps1` only if generation semantics need adjustment
- `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs`
- relevant docs/README guardrail tests

Validation:

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore -m:1 --filter FullyQualifiedName~BeadsPublicRoadmapTests`
- Any additional focused repository tests touched by docs changes.

## Parallelization

After Phase 318:

- Phase 319 and Phase 320 are independent and can run in separate worktrees.
- Phase 321 waits for both.
- Phase 322 waits for 319/320/321.
