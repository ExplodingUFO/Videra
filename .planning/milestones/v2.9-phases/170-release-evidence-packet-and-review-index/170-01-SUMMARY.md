# Phase 170 Summary: Release Evidence Packet and Review Index

## Outcome

Release dry-run now generates a single release-candidate evidence index for reviewer signoff.

## Completed

- Added `eng/release-candidate-evidence.json`.
- Added `scripts/New-ReleaseCandidateEvidenceIndex.ps1`.
- Wired `scripts/Invoke-ReleaseDryRun.ps1` to generate `release-candidate-evidence-index.json` and `release-candidate-evidence-index.txt`.
- Added repository tests for the evidence contract, generator, docs vocabulary, and dry-run wiring.
- Updated `docs/releasing.md` to make the index the starting point for release-candidate review notes.

## Commit

`e785c54 feat(170): generate release candidate evidence index`

## Verification

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~ReleaseCandidateTruthRepositoryTests"`
- `pwsh -File .\scripts\New-ReleaseCandidateEvidenceIndex.ps1 -ExpectedVersion 0.1.0-alpha.7 -ReleaseDryRunSummaryPath <temporary-summary> -OutputRoot <temporary-output>`

## Next

Start Phase 171: Release Abort and Cutover Runbook Truth.
