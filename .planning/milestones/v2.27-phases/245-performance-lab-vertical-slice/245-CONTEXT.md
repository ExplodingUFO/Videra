# Phase 245: Performance Lab Vertical Slice - Context

**Gathered:** 2026-04-27
**Status:** Ready for planning
**Mode:** Autonomous continuation from v2.27 roadmap

<domain>
## Phase Boundary

Add one focused Performance Lab surface to `Videra.Demo` that demonstrates the performance foundation without reorganizing the existing demo. The lab should generate bounded normal-object and retained instance-batch datasets, show diagnostics/pick evidence, and provide a copyable snapshot.

</domain>

<decisions>
## Implementation Decisions

- Keep the lab inside the existing side panel instead of creating a broader demo shell.
- Use code-behind for viewport-specific dataset generation; keep the ViewModel focused on options and report state.
- Keep instance-batch mode honest: it records retained batch truth and diagnostics, but does not claim GPU instanced rendering.
- Keep object counts bounded for an interactive desktop demo.

</decisions>

<code_context>
## Existing Code Insights

- `MainWindowViewModel` already owns status text, support report state, and demo settings.
- `MainWindow.axaml.cs` already owns clipboard and viewport event wiring.
- `VideraView.AddInstanceBatch(...)` and batch diagnostics are available from Phase 243.
- `DemoSupportReportBuilder` and `VideraDiagnosticsSnapshotFormatter` already produce copyable support text.

</code_context>

<specifics>
## Specific Ideas

- Add object count, mode, and pickable controls.
- Add generate/copy snapshot buttons.
- Generate normal `Object3D` markers through the ready resource factory.
- Generate retained instance batches through `VideraView.AddInstanceBatch(...)`.
- Measure build/frame-time proxy and hit-test pick latency with `SceneHitTestService`.
- Include draw-call availability, upload bytes, resident bytes, retained instance count, and pickable count in the report.

</specifics>

<deferred>
## Deferred Ideas

- Real FPS instrumentation.
- GPU instanced drawing.
- Pixel-level demo screenshot automation.
- New app shell/navigation model.

</deferred>
