# Phase 349: Minimal Authoring Sample Refresh

## Bead

`Videra-y9t`

## Outcome

Refreshed `Videra.MinimalAuthoringSample` to show the polished Core authoring path with placement affordances, presets, helpers, retained scene truth, and instance batches while staying importer-free.

## Changes

- Updated the sample program to use the polished authoring API.
- Updated the sample README with current APIs and boundaries.
- Added `MinimalAuthoringSampleContractTests` to keep the sample small and importer-free.

## Verification

```bash
dotnet build samples\Videra.MinimalAuthoringSample\Videra.MinimalAuthoringSample.csproj -c Debug --no-restore
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~MinimalAuthoringSampleContractTests|FullyQualifiedName~SceneAuthoringInstanceEvidenceTests|FullyQualifiedName~SceneAuthoringDiagnosticsTests"
dotnet run --project samples\Videra.MinimalAuthoringSample\Videra.MinimalAuthoringSample.csproj -c Debug --no-build
```

Results:

- Sample build passed with 0 warnings and 0 errors.
- Focused tests passed 9/9.
- Sample run printed retained scene counts and instance marker IDs.
