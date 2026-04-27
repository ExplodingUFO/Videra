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

## Residuals

- GitHub CI evidence must still be checked before opening or merging a public PR.
- Central package management remains deferred.
- Broader analyzer rule-family adoption remains deferred unless promoted into a dedicated milestone.
