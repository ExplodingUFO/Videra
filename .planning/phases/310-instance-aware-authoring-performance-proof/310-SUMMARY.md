---
phase: 310
name: Instance-Aware Authoring Performance Proof
status: complete
bead: Videra-oc6
completed_at: 2026-04-28T16:55:00+08:00
---

# Phase 310 Summary

Added a first-class instance-aware authoring helper:

- `SceneAuthoringBuilder.AddInstances(...)`
- constructs the existing `InstanceBatchDescriptor` path from one mesh, one material, and per-instance transforms/colors/object ids
- keeps authored repeated geometry as one retained instance batch rather than many scene entries

Verification:

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore -m:1 --filter "FullyQualifiedName~SceneAuthoringBuilderTests"` passed, 6/6.

