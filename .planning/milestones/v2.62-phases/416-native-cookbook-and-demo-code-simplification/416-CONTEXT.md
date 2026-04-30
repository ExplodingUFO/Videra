# Phase 416 Context: Native Cookbook and Demo Code Simplification

## Beads

- Parent phase: `Videra-w2t`
- Child beads:
  - `Videra-dtc`: extract cookbook recipe catalog
  - `Videra-b6e`: extract support summary/snapshot helpers

## Boundary

This phase owns demo/cookbook simplification only. It must not edit
SurfaceCharts rendering code, chart view rendering/overlay code, CI workflows, or
release scripts.

## Required Outcomes

- Cookbook recipe definitions move out of `MainWindow.axaml.cs` into bounded demo
  model/catalog code.
- Support summary and snapshot summary formatting move out of
  `MainWindow.axaml.cs` into bounded helper/service code.
- Recipe content, support summary fields, snapshot summary fields, and user
  behavior remain unchanged.
- The demo must not become a generic plotting workbench or compatibility layer.

## Non-Goals

- No code/API fallback changes.
- No compatibility or old-code bridge.
- No generic workbench abstraction.
- No behavior changes beyond structural simplification.
