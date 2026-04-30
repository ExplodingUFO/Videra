# Phase 198 Summary: Repo-Owned Broader-Lighting Proof and Validation Wiring

## Outcome

`v2.16` now proves the bounded broader-lighting baseline through the existing repository-owned desktop proof hosts and wires explicit `10`-second survival holds through every desktop validation entrypoint used by that proof:

- wired `-LightingProofHoldSeconds 10` through repository validation wrappers and GitHub workflow entrypoints that run viewer, SurfaceCharts, and WPF desktop proof hosts
- extended the existing hold seam to `Videra.SurfaceCharts.ConsumerSmoke` so the milestone's desktop validation truth stays honest
- kept the proof repository-owned and validation-focused instead of widening the public runtime or package surface

The phase intentionally did not widen into:

- a public advanced-lighting sample or package line
- shadows, environment maps, or post-processing
- broader runtime or backend abstractions
- chart-family expansion
- compatibility adapters or transitional host paths

## Verification Shape

- focused repository contract tests locking explicit `10`-second hold wiring in scripts, workflows, and sample configuration
- real `10`-second proof runs for `ConsumerSmoke` viewer mode, `SurfaceCharts.ConsumerSmoke`, and `WpfSmoke`
- build-only packaged SurfaceCharts consumer smoke validation to confirm the repository-owned proof path still builds cleanly
- no whitespace or merge-noise regressions

## Next Phase

- Phase 199: broader-lighting truth and guardrails
