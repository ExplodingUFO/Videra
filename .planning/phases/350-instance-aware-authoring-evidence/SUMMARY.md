# Phase 350: Instance-Aware Authoring Evidence

## Bead

`Videra-6f8`

## Outcome

Added focused evidence that repeated authored geometry stays explicit, instance-aware, and batch-friendly through `AddInstances(...)` / `InstanceBatchEntry` truth.

## Changes

- Added 1k and 10k repeated cube-marker evidence tests.
- Asserted one retained `InstanceBatchEntry`, shared mesh/material truth, retained transforms/colors/object IDs, and no expanded scene primitives.
- Clarified in `Videra.Core` README when to use `AddInstances(...)` instead of duplicate primitive calls.

## Verification

Worker verification:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SceneAuthoringInstanceEvidenceTests
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~Videra.Core.Tests.Scene
```

Results: passed 2/2 and 40/40.

Integration verification:

```bash
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~MinimalAuthoringSampleContractTests|FullyQualifiedName~SceneAuthoringInstanceEvidenceTests|FullyQualifiedName~SceneAuthoringDiagnosticsTests"
```

Result: passed 9/9.
