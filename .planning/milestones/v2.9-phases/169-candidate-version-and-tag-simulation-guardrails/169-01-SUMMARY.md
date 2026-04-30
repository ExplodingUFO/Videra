# Phase 169 Summary: Candidate Version and Tag Simulation Guardrails

## Outcome

Candidate version/tag simulation is now part of the release dry-run path.

## Completed

- Added `scripts/Test-ReleaseCandidateVersion.ps1`.
- Wired `scripts/Invoke-ReleaseDryRun.ps1` to run the guard before package packing and after `release-dry-run-summary.json` is generated.
- Guarded `Directory.Build.props`, `eng/public-api-contract.json`, simulated `v*` tag spelling, package project existence, summary version, summary contract path, and summary package IDs.
- Updated release docs and repository tests to keep the dry-run path non-publishing and tag-free.

## Commit

`2a9f5a6 feat(169): validate release candidate version simulation`

## Verification

- `pwsh -File .\scripts\Test-ReleaseCandidateVersion.ps1 -ExpectedVersion 0.1.0-alpha.7 -CandidateTag v0.1.0-alpha.7`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release -m:1 --filter "FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~ReleaseCandidateTruthRepositoryTests"`

## Next

Start Phase 170: Release Evidence Packet and Review Index.
