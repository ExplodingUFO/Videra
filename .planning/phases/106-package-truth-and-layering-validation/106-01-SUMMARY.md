# Phase 106 Summary: Package Truth and Layering Validation

## Outcome

Phase 106 closed the remaining `v1.20` gap by making docs, smoke paths, package validation, publish workflows, and repository guards describe the same canonical public viewer stack.

## Shipped Changes

1. Tightened `docs/package-matrix.md`, `docs/support-matrix.md`, `docs/release-policy.md`, and `docs/releasing.md` so they all point back to the same canonical viewer stack:
   - `Videra.Avalonia`
   - exactly one matching `Videra.Platform.*` package
   - explicit `Videra.Import.Gltf` / `Videra.Import.Obj` packages on the core path
2. Extended `RepositoryReleaseReadinessTests` to guard:
   - canonical package-stack language across docs
   - package-line alignment across docs, consumer smoke, package validation, and publish workflows
   - the same package truth in the Chinese onboarding mirrors
3. Updated `docs/zh-CN/README.md`, `docs/zh-CN/index.md`, and `docs/zh-CN/modules/videra-avalonia.md` so they no longer lag the English package truth around transitive import packages and the canonical `VideraViewOptions -> LoadModelAsync(...) -> FrameAll() / ResetCamera() -> BackendDiagnostics` flow.
4. Closed the remaining automation gaps:
   - `ci.yml` now runs `Validate-Packages.ps1` against the packaged consumer-smoke artifacts on the PR line
   - `publish-existing-public-release.yml` now runs packaged consumer smoke and the same package-validation script as the main public publish flow, while failing fast for tags that predate the current canonical package line/helper scripts
   - `Validate-Packages.ps1` now rejects unexpected package IDs instead of accepting supersets of the public package line
   - package validation and repository layering guards now also enforce that platform packages stay `Videra.Core`-based and do not pick up Avalonia/import/platform-peer dependencies

## Verification

- `dotnet restore Videra.slnx`
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~CanonicalViewerPackageStack|FullyQualifiedName~CanonicalPublicPackageTruth|FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~AlphaConsumerIntegrationTests"`

## Notes

- This phase stayed documentation-and-validation only. It did not widen runtime/package abstractions or change the shipped package graph.
- With `VALD-01` complete, `v1.20` phase work is done and the milestone is ready for audit/closeout.
