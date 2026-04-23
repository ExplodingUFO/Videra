# Release Candidate Abort and Cutover Runbook

This runbook starts after `Release Dry Run` produces `release-dry-run-evidence` and the reviewer has opened `release-candidate-evidence-index.txt`.

It does not publish packages and does not create release tags. Public publishing remains a separate human-approved cutover.

## Review Inputs

- `release-candidate-evidence-index.txt`
- `release-candidate-evidence-index.json`
- `release-dry-run-summary.json`
- `package-size-evaluation.json`
- `package-size-summary.txt`
- Pull-request `verify` / `quality-gate-evidence`
- `Benchmark Gates`
- `Native Validation`
- `Consumer Smoke`
- `docs/releasing.md`
- `docs/release-policy.md`
- `CHANGELOG.md`

## Abort Criteria

Abort the candidate before cutover if any of these are true:

- A required check in `release-candidate-evidence-index.txt` is failing, missing, stale, or from the wrong commit.
- `scripts/Test-ReleaseCandidateVersion.ps1` reports a candidate tag/version mismatch.
- `release-dry-run-summary.json` does not match the intended package version.
- `scripts/Validate-Packages.ps1` reports package metadata, package asset, dependency, symbol, or package-size budget failures.
- `Benchmark Gates` reports a hard-threshold regression.
- Native validation fails on Windows, Linux X11, Linux XWayland, or macOS.
- Consumer smoke fails for the packaged viewer path or the packaged SurfaceCharts path.
- Release docs, support docs, changelog, or package matrix contradict the candidate boundary.
- The dry-run path attempts to use feed credentials, create a GitHub release, create a release tag, or push packages.

## Abort Steps

1. Stop the cutover.
2. Do not create a release tag.
3. Do not publish packages.
4. Record the failed evidence item and commit SHA in the candidate review notes.
5. Fix the issue through a normal pull request.
6. Rerun `Release Dry Run`, `Benchmark Gates`, `Native Validation`, and `Consumer Smoke`.
7. Restart review from the new `release-candidate-evidence-index.txt`.

## Human Cutover Preconditions

Before public publishing, a maintainer must confirm:

- The candidate review notes link the final `release-candidate-evidence-index.txt`.
- Every required check and artifact listed in the evidence index is green and from the intended commit.
- The package version matches the version-aligned tag that will be created as `v<package-version>`.
- `docs/releasing.md`, `docs/release-policy.md`, `docs/support-matrix.md`, `docs/package-matrix.md`, and `CHANGELOG.md` describe the same release boundary.
- Feed credentials are only used by the public publish workflow after human approval.

## Human Cutover Sequence

1. Create the version-aligned repository tag `v<package-version>` from the approved commit.
2. Let the existing `Publish Public Packages` workflow run from that tag.
3. Confirm package validation passes in the publish workflow.
4. Confirm packages and symbols are attached to the generated GitHub release.
5. Confirm published packages match the approved package IDs and version.
6. Record the public release URL in the release notes.

## Non-Goals

- No fallback publishing path.
- No compatibility or migration release path.
- No manual package push outside the documented publish workflow.
- No release tag creation before human approval.
