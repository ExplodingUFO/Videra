# Phase 198 Context: Repo-Owned Broader-Lighting Proof and Validation Wiring

## Why this phase exists

`Phase 197` shipped the bounded fill-light contract, but the repository-owned proof surface still relies on ad hoc hold usage unless the validation entrypoints request it explicitly.

The current hold seam already exists in:

- `scripts/Invoke-ConsumerSmoke.ps1`
- `scripts/Invoke-WpfSmoke.ps1`
- `smoke/Videra.ConsumerSmoke/Views/MainWindow.axaml.cs`
- `smoke/Videra.WpfSmoke/MainWindow.xaml.cs`

The current gap is in the entrypoints that actually run desktop proof programs:

- `scripts/verify.ps1`
- `.github/workflows/consumer-smoke.yml`
- `.github/workflows/ci.yml`
- `.github/workflows/publish-public.yml`
- `.github/workflows/publish-existing-public-release.yml`

## Scope

- keep proof on the repository-owned viewer hosts only
- wire explicit `LightingProofHoldSeconds=10` where those hosts are used in validation/proof entrypoints
- update repository tests so the wiring is locked in
- keep `SurfaceCharts.ConsumerSmoke` out of scope unless a validation entrypoint in this milestone actually depends on it

## Expected outcome

- every desktop program used in this milestone's validation survives `10` seconds without crashing
- broader-lighting proof stays repository-owned and non-public
- validation and release-entrypoint truth matches the actual proof behavior

## Non-goals

- no new proof host
- no SurfaceCharts hold-seam expansion in this phase
- no docs/support truth pass yet
- no renderer/material/runtime widening beyond the already-shipped fill-light contract
