# Phase 184 Context: Package Install Boundary Scope Lock

## Milestone

`v2.13 Package Install Boundary Tightening`

## Requirement

`PIS-01`: Before implementation starts, lock `v2.13` to package/install-surface tightening on the shipped Avalonia viewer line while freezing explicit non-goals around renderer/runtime/chart/platform breadth, broader importer coverage, release workflow changes, compatibility shims, and migration adapters.

## Scope

- Default `Videra.Avalonia` dependency/install surface only.
- Importer-backed `LoadModelAsync(...)` and `LoadModelsAsync(...)` packaging/install truth only.
- Package-size/install-surface evidence, docs, samples, support wording, and repository guardrails only where they must match the tightened boundary.
- Focused consumer/package validation only.

## Starting Point

- `v2.11` already tightened runtime truth around static glTF/PBR renderer consumption.
- `v2.12` already tightened `SurfaceCharts` residency/probe efficiency and aligned benchmark/docs/guardrails with that efficiency story.
- The next bounded gap from the existing roadmap is product-shape work: the default Avalonia install path still carries importer packages transitively, while the repo wants a clearer split between viewer-only hosts and importer-enabled hosts.
- This milestone should stay on package/install boundaries instead of widening into new runtime, renderer, chart, backend, or importer-feature work.
