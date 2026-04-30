# Phase 191 Summary: WPF Repository Truth and Guardrails

## Outcome

The repository docs and guardrails now describe `smoke/Videra.WpfSmoke` with the same bounded truth already implemented in `Phase 190`:

- it is repository-only validation/support evidence on the Avalonia-first public viewer path
- it is not a second public UI package or release path
- the Windows native-validation lane explicitly documents the hosted `verify.ps1 -> Invoke-WpfSmoke.ps1 -> wpf-smoke-diagnostics.txt` chain

## Files Tightened

- `README.md`
- `docs/package-matrix.md`
- `docs/support-matrix.md`
- `docs/native-validation.md`
- `docs/zh-CN/native-validation.md`
- repository guard tests in `RepositoryNativeValidationTests` and `RepositoryReleaseReadinessTests`

## Boundaries Preserved

- no runtime/backend/import/chart changes
- no workflow widening beyond the existing repository-native validation lane
- no second public UI package line
- no compatibility or transitional wording
