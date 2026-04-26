# Videra Doctor

`Videra Doctor` is a repo-only, non-mutating support snapshot command for maintainers working from a local checkout. It is not a public package, global tool, supported API, or replacement for the existing validation scripts.

Run it from the repository root:

```powershell
pwsh -File ./scripts/Invoke-VideraDoctor.ps1
```

The command writes:

- `artifacts/doctor/doctor-report.json`
- `artifacts/doctor/doctor-summary.txt`

The report captures SDK/runtime, OS, git state, package and benchmark contract file presence, validation script presence, platform project presence, and known support artifact paths.

Contract and validation references reported by Doctor stay aligned with the repository files that own them:

- `eng/public-api-contract.json`
- `benchmarks/benchmark-contract.json`
- `benchmarks/benchmark-thresholds.json`
- `scripts/Validate-Packages.ps1`
- `scripts/Run-Benchmarks.ps1`
- `scripts/Test-BenchmarkThresholds.ps1`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `scripts/run-native-validation.ps1`

## Validation References

Doctor always writes validation entries into `doctor-report.json`. Default execution is non-mutating: validation entries stay `skip` unless the caller explicitly opts into a validator.

Status values:

- `pass`: the referenced validator was invoked and completed successfully.
- `fail`: the referenced validator was invoked and returned a failure; inspect the `logPath`.
- `skip`: Doctor did not invoke the validator, usually because the matching run switch was not supplied.
- `unavailable`: a required script, artifact, host, or input path is missing.

Opt-in validation switches:

- `RunPackageValidation` invokes `scripts/Validate-Packages.ps1` when `-PackageRoot` and `-ExpectedVersion` are supplied.
- `RunBenchmarkThresholds` invokes `scripts/Test-BenchmarkThresholds.ps1` after benchmark artifacts from `scripts/Run-Benchmarks.ps1` exist.
- `RunConsumerSmoke` invokes `scripts/Invoke-ConsumerSmoke.ps1`.
- `RunNativeValidation` invokes `scripts/run-native-validation.ps1` on a matching host.

Doctor complements `release-dry-run-evidence`; it does not replace `Release Dry Run`, package validation, benchmark gates, consumer smoke, or native validation.

Doctor does not publish packages, does not push packages or git remotes, does not create tags, alter package feeds, change git remotes, update machine configuration, or fix local setup. Use it to attach repository state to support reports before running deeper validation.
