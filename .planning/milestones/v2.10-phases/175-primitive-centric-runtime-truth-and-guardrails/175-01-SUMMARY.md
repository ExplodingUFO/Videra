# Phase 175 Summary: Primitive-Centric Runtime Truth and Guardrails

## Outcome

Aligned docs, localized mirrors, and repository guardrails with the primitive-centric runtime story shipped in Phases 173 and 174.

## What Changed

1. Updated viewer/runtime truth docs.
   - Replaced stale object-centric wording that said mixed `Blend` / non-`Blend` imports remained guarded outright.
   - Documented the canonical runtime bridge as one imported entry expanding into multiple internal runtime objects.

2. Updated scene-delta and upload-queue vocabulary.
   - Docs now describe typed retained-entry changes instead of one coarse reupload bucket.
   - Docs now describe entry-coalesced upload work plus attached-first interactive draining on the same queue path used by backend rebind.

3. Updated repository guardrails.
   - Repository tests now assert the primitive-centric runtime wording and the new delta/upload vocabulary.

## Files

- `README.md`
- `ARCHITECTURE.md`
- `docs/capability-matrix.md`
- `docs/extensibility.md`
- `docs/hosting-boundary.md`
- `docs/package-matrix.md`
- `docs/release-policy.md`
- `src/Videra.Avalonia/README.md`
- `src/Videra.Core/README.md`
- `docs/zh-CN/README.md`
- `docs/zh-CN/ARCHITECTURE.md`
- `docs/zh-CN/modules/demo.md`
- `docs/zh-CN/modules/videra-avalonia.md`
- `docs/zh-CN/modules/videra-core.md`
- `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`
