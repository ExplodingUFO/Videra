# Videra Doctor

`Videra Doctor` is a repo-only, non-mutating support snapshot command for maintainers working from a local checkout. It is not a public package, global tool, supported API, or replacement for the existing validation scripts.

Run it from the repository root:

```powershell
pwsh -File ./scripts/Invoke-VideraDoctor.ps1
```

The command writes:

- `artifacts/doctor/doctor-summary.txt`
- `artifacts/doctor/doctor-report.json`

The report captures SDK/runtime, OS, git state, package and benchmark contract file presence, validation script presence, platform project presence, and known support artifact paths.

Doctor does not publish packages, alter package feeds, change git remotes, update machine configuration, or fix local setup. Use it to attach repository state to support reports before running deeper validation.
