# Maintenance Quality Gates

This page records the v2.24 analyzer and dependency hygiene evidence path. It is for maintainer release-readiness review, not public package publication.

## Normal Verification

Run the normal repository verification path:

```powershell
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

That path now includes:

- shared test-tooling drift check through `scripts/Test-SharedTestToolingPackages.ps1`
- solution build
- solution tests, including repository guards for analyzer and dependency policy
- demo and sample builds

GitHub CI exercises this through the `verify` job in `.github/workflows/ci.yml`.

## Focused Maintenance Checks

Run these when reviewing analyzer or dependency robot PRs:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SharedTestToolingPackages.ps1
dotnet build Videra.slnx -c Release -p:TreatWarningsAsErrors=true -p:RestoreIgnoreFailedSources=true
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release -p:RestoreIgnoreFailedSources=true
```

The `quality-gate-evidence` job in `.github/workflows/ci.yml` also builds core evidence with `TreatWarningsAsErrors`.

## Performance Lab Visual Evidence

When reviewing Performance Lab changes or support reports that need visual context, generate the evidence-only visual bundle:

```powershell
pwsh -File ./scripts/Invoke-PerformanceLabVisualEvidence.ps1 -Configuration Release -OutputRoot artifacts/performance-lab-visual-evidence
```

The bundle contains:

- `performance-lab-visual-evidence-manifest.json`
- `performance-lab-visual-evidence-summary.txt`
- PNG visual evidence for produced scenarios
- per-scenario diagnostics text

GitHub CI publishes the same bundle from the `performance-lab-visual-evidence` job. Treat these files as review/support artifacts, not pixel-perfect visual-regression gates, stable benchmark guarantees, real GPU instancing evidence, renderer parity evidence, or new chart-family promises.

## Residuals

- GitHub CI evidence must still be checked before opening or merging a public PR.
- Central package management remains deferred.
- Broader analyzer rule-family adoption remains deferred unless promoted into a dedicated milestone.
