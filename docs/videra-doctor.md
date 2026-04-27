# Videra Doctor

`Videra Doctor` is a repo-only, non-mutating support snapshot command for maintainers working from a local checkout. It is not a public package, global tool, supported API, or replacement for the existing validation scripts.

Run it from the repository root:

```powershell
pwsh -File ./scripts/Invoke-VideraDoctor.ps1
```

The command writes:

- `artifacts/doctor/doctor-report.json`
- `artifacts/doctor/doctor-summary.txt`

The report captures SDK/runtime, OS, git state, package and benchmark contract file presence, validation script presence, platform project presence, known support artifact paths, and an `evidencePacket` section for release-candidate triage. Doctor also reports whether the optional Performance Lab visual evidence bundle is `present`, `missing`, or `unavailable`.

Contract and validation references reported by Doctor stay aligned with the repository files that own them:

- `eng/public-api-contract.json`
- `benchmarks/benchmark-contract.json`
- `benchmarks/benchmark-thresholds.json`
- `scripts/Validate-Packages.ps1`
- `scripts/Run-Benchmarks.ps1`
- `scripts/Test-BenchmarkThresholds.ps1`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `scripts/run-native-validation.ps1`
- `scripts/Invoke-PublicReleasePreflight.ps1`
- `scripts/Invoke-PerformanceLabVisualEvidence.ps1`

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

Doctor complements `release-dry-run-evidence` and `public-release-preflight` summaries; it does not replace `Release Dry Run`, package validation, benchmark gates, consumer smoke, native validation, or public release preflight.

## Evidence Packet

`doctor-report.json` includes `evidencePacket` so maintainers can see which release-readiness inputs are present before deeper validation starts. It records repository state, machine state, package contracts, validation scripts, support artifact roots, the current Doctor output paths, and artifact references for:

- release dry run: `release-dry-run-summary.json` and `release-candidate-evidence-index.json`
- package validation: package-size evaluation and summary artifacts from `scripts/Validate-Packages.ps1`
- benchmark gates: viewer and SurfaceCharts `benchmark-manifest.json` files
- consumer smoke: `consumer-smoke-result.json`, `diagnostics-snapshot.txt`, and `surfacecharts-support-summary.txt`
- native validation: `artifacts/native-validation`
- public release preflight: `public-release-preflight-summary.json` and `public-release-preflight-summary.txt`
- demo support: diagnostics and SurfaceCharts support summaries copied from the demos
- Performance Lab visual evidence: `performance-lab-visual-evidence-manifest.json`, `performance-lab-visual-evidence-summary.txt`, PNG visual evidence, and per-scenario diagnostics under `artifacts/performance-lab-visual-evidence`

Doctor only reports whether those paths are present or missing. The owning scripts still produce and validate the artifacts.

`evidencePacket.performanceLabVisualEvidence` is the structured visual evidence discovery object. It includes:

- `status`: `present`, `missing`, or `unavailable`
- `captureStatus`: the manifest status when a manifest exists, such as `produced` or `unavailable`
- `manifestPath` and `summaryPath`
- `generatedAtUtc`, `schemaVersion`, and `evidenceKind`
- `screenshotPaths` and `diagnosticsPaths`
- per-scenario `entries` with scenario id/type/display name/status and artifact paths

Doctor does not generate screenshots by default. If visual evidence is needed for PR review or support, generate it explicitly:

```powershell
pwsh -File ./scripts/Invoke-PerformanceLabVisualEvidence.ps1 -Configuration Release -OutputRoot artifacts/performance-lab-visual-evidence
```

Then rerun Doctor and attach both `artifacts/doctor/*` and `artifacts/performance-lab-visual-evidence/*`.

Doctor does not publish packages, does not push packages or git remotes, does not create tags, alter package feeds, change git remotes, update machine configuration, or fix local setup. Use it to attach repository state to support reports before running deeper validation.
