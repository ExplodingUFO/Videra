# Dependency Update Policy

This document defines how Videra handles recurring dependency robot PRs during alpha maintenance. It is intentionally narrow: keep shared tooling updates reviewable without introducing central package management or broad dependency modernization.

## Shared Test Tooling

The following test packages are treated as repo-wide shared test tooling:

- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`
- `FluentAssertions`
- `coverlet.collector`

These packages should stay aligned across test projects unless a dedicated phase documents why a split is required.

Run the drift check locally:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SharedTestToolingPackages.ps1
```

## Robot PR Triage

Use this flow for Dependabot dependency PRs:

1. Merge directly only when the PR is a patch or minor update, affects one bounded package family, and the normal verification path is green.
2. Rebase and retest when the PR is stale but still matches the repository policy.
3. Close and replace with a repo-owned PR when a robot PR updates only part of shared test tooling or analyzer policy needs to be changed.
4. Defer as a dedicated phase when the update is a major version, changes quality-gate meaning, or requires broad cleanup.

Analyzer major-version PRs follow `docs/analyzer-policy.md`. Shared test-tooling PRs should be grouped by Dependabot and validated with `scripts/Test-SharedTestToolingPackages.ps1`.

## Current Baseline

- `coverlet.collector` is aligned to `10.0.0` across test projects.
- `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, and `FluentAssertions` are also expected to remain single-version across test projects.
- Central package management remains deferred.
