# Phase 427 Wave 2 Summary: Interaction Propagator, Evidence, and Demo

**Plan:** 427-02
**Completed:** 2026-04-30
**Status:** Code complete, human demo verification pending

## Commits

- `752236e`: feat(427-02): implement SurfaceChartInteractionPropagator and linked interaction state
- `5f60c08`: feat(427-02): extend workspace evidence with linked interaction tracking and demo scenario

## Files Created

| File | Purpose |
|------|---------|
| `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartInteractionPropagator.cs` | Host-owned probe/selection/measurement propagation across linked charts |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartLinkedInteractionState.cs` | Record describing active interaction surfaces per link group |
| `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/SurfaceChartInteractionPropagatorTests.cs` | Propagation lifecycle and opt-in tests |

## Files Modified

| File | Changes |
|------|---------|
| `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs` | Added link group tracking, RegisterLinkGroup/UnregisterLinkGroup, CaptureLinkedInteractionStates |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs` | Extended with link group details, interaction surfaces, evidence boundary |
| `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs` | Added LinkedInteraction enum value |
| `samples/Videra.SurfaceCharts.Demo/Services/SurfaceChartWorkspaceService.cs` | Extended for linked interaction scenario support |
| `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` | Added SetupLinkedInteractionScenario() with two linked charts, probe propagation, evidence copy |

## Must-Have Verification

| Truth | Status |
|-------|--------|
| Probe propagation forwards probe coordinates from one linked chart to all other members | Implemented (host-called PropagateProbe method) |
| Selection propagation creates selection reports on linked charts using sample-space coordinates | Implemented (SelectionReported event handler) |
| Measurement propagation creates measurement reports on linked charts using same anchor points | Implemented (propagateMeasurement flag) |
| Propagation is opt-in per link group — propagator must be explicitly created by host | Verified (constructor flags, all default false) |
| Propagator uses re-entrancy guard to prevent infinite propagation loops | Verified (_isPropagating guard) |
| Workspace evidence includes link group policies, member counts, and active interaction surfaces | Verified (LinkGroup, InteractionSurfaces, EvidenceBoundary sections) |
| Demo shows two linked charts with probe propagation enabled | Code complete, visual verification pending |

## Human Verification Needed

The demo scenario needs visual verification:
1. Run: `dotnet run --project samples/Videra.SurfaceCharts.Demo/`
2. Select "Linked Interaction" scenario
3. Verify two charts with FullViewState sync (orbit/zoom mirrors)
4. Verify probe propagation (hover highlights on both charts)
5. Verify "Copy linked interaction evidence" button produces correct evidence text

## Integration with Phase 427 Wave 1

Wave 2 builds on Wave 1's CameraOnly/AxisOnly link policies:
- `SurfaceChartInteractionPropagator` works with any link group policy
- Evidence reports the policy for each link group
- Demo uses FullViewState policy (simplest to demonstrate)

## Evidence Format

The extended workspace evidence now includes:
```
LinkGroupCount: 1
LinkGroup[0]: Policy=FullViewState | Members=2 | Propagation=Probe
InteractionSurfaces: Selection=inactive | Probe=active | Measurement=inactive
EvidenceBoundary: Propagation is runtime truth; linked chart data presence is runtime truth.
```
