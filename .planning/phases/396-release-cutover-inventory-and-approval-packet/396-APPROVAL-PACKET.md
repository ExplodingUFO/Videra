# Phase 396 Approval Packet: v2.58 Controlled Release Cutover

## Release Boundary

v2.58 may prepare and validate a controlled SurfaceCharts release cutover. It may not publish to NuGet, create a public tag, publish a GitHub release, mutate public package feeds, or replay an existing public release without explicit human approval.

## Proven v2.57 Evidence

| Evidence | Path | Approval Meaning |
|---|---|---|
| v2.57 roadmap archive | `.planning/milestones/v2.57-ROADMAP.md` | Phase sequence 390-395 and success criteria are archived. |
| v2.57 requirements archive | `.planning/milestones/v2.57-REQUIREMENTS.md` | INV/API/PKG/CONSUME/CI/DOC/VER requirements are complete. |
| v2.57 phase evidence | `.planning/milestones/v2.57-phases/` | Phase-level context, plans, summaries, and verification were archived. |
| Final validation record | `.planning/phases/395-integration-guardrails-and-milestone-evidence/395-VERIFICATION.md` | Final release-readiness validation passed. |
| Release-readiness summary | `artifacts/phase395-release-readiness-final/release-readiness-validation-summary.json` | Package build, full SurfaceCharts consumer smoke, focused tests, and snapshot scope guardrails passed; publish/tag/release actions skipped. |
| Release dry-run summary | `artifacts/phase395-release-readiness-final/release-dry-run/release-dry-run-summary.json` | Local package dry-run passed for the public package set. |
| Release evidence index | `artifacts/phase395-release-readiness-final/release-dry-run/release-candidate-evidence-index.json` | Required and optional evidence are indexed. |
| SurfaceCharts consumer smoke | `artifacts/phase395-release-readiness-final/surfacecharts-consumer-smoke/consumer-smoke-result.json` | Packaged SurfaceCharts public API smoke passed and produced chart snapshot evidence. |
| SurfaceCharts support summary | `artifacts/phase395-release-readiness-final/surfacecharts-consumer-smoke/surfacecharts-support-summary.txt` | Support artifact has chart identity, output capability, and rendering status fields. |
| Diagnostics snapshot | `artifacts/phase395-release-readiness-final/surfacecharts-consumer-smoke/diagnostics-snapshot.txt` | Snapshot status was present. |

## Package and Script Truth

- Version source: `Directory.Build.props` contains `0.1.0-alpha.7`.
- Package contract: `eng/public-api-contract.json`.
- Package size budgets: `eng/package-size-budgets.json`.
- Release-candidate evidence contract: `eng/release-candidate-evidence.json`.
- Main dry-run path: `scripts/Invoke-ReleaseDryRun.ps1`.
- Readiness wrapper: `scripts/Invoke-ReleaseReadinessValidation.ps1`.
- Public preflight path: `scripts/Invoke-PublicReleasePreflight.ps1`.
- Final non-mutating simulation path: `scripts/Invoke-FinalReleaseSimulation.ps1`.
- Package validation: `scripts/Validate-Packages.ps1`.
- Scope guardrail: `scripts/Test-SnapshotExportScope.ps1`.
- Public release workflows are manual-gated in `.github/workflows/publish-public.yml` and `.github/workflows/publish-existing-public-release.yml`.

## Approval Gates

Before any public release action, the next phases must confirm:

1. Package version and package asset metadata match the intended release.
2. Package contract and package-size budget files are synchronized with any intentional package surface change.
3. Dry-run output separates pass, fail, skipped, and manual-gate states.
4. Publish/tag/GitHub release actions require explicit approval inputs and fail closed by default.
5. Support docs explain which evidence is required and which visual evidence is optional context.
6. Beads, generated public roadmap, Git, and Dolt refs are synchronized before handoff.

## Abort and Hold Criteria

Hold the cutover if any of these are true:

- Package build, package validation, or package-size validation fails.
- Public API/package contract changes without reviewed contract updates.
- `scripts/Test-SnapshotExportScope.ps1` fails.
- SurfaceCharts consumer smoke fails or stops producing deterministic support artifacts.
- Docs imply ScottPlot compatibility, old chart controls, public direct `Source`, hidden fallback/downshift, PDF/vector export, or backend expansion.
- Dry-run output hides skipped publish/tag/release actions or treats manual-gated actions as complete.
- Manual publish/tag workflow inputs do not match the expected version and commit.
- Beads export, generated roadmap, Git push, or Dolt Beads push cannot be safely synchronized.

## Known Weak Evidence

- `artifacts/phase395-release-readiness-final/surfacecharts-consumer-smoke/consumer-smoke-result.json` lists optional `inspection-snapshot.png` and `inspection-bundle` support paths that were not present in the inspected folder. Treat this as a Phase 397/398 review item: either fix producer output, clarify optionality, or ensure the final dry-run evidence does not list missing support artifacts as if they exist.
- Optional Doctor and Performance Lab visual evidence are currently optional/non-blocking context. Do not convert them into hard gates without an explicit requirement change.

## Parallelization Map

- Phase 397 is blocked by Phase 396 and should run next.
- After Phase 397 closes, Phase 398 (automation/dry-run gates) and Phase 399 (release notes/docs/support cutover) can run in parallel with isolated worktrees and disjoint write sets.
- Phase 400 depends on both Phase 398 and Phase 399.
