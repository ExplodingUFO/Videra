# Release Candidate Abort and Cutover Runbook

This runbook starts after `Release Dry Run` produces `release-dry-run-evidence`, `Public release preflight` produces `public-release-preflight-summary.json`, and the reviewer has opened `release-candidate-evidence-index.txt`.

It does not publish packages and does not create release tags. Public publishing remains a separate human-approved cutover.

## Review Inputs

- `release-candidate-evidence-index.txt`
- `release-candidate-evidence-index.json`
- `release-dry-run-summary.json`
- `package-size-evaluation.json`
- `package-size-summary.txt`
- `public-release-preflight-summary.json`
- `public-publish-before-summary.json`
- `public-publish-after-summary.json`
- `public-release-notes.md`
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

## Finding Classification

Classify every dry-run or candidate-review finding before changing code:

| Classification | Meaning | Action |
| --- | --- | --- |
| Release blocker | Required evidence is failing, missing, stale, from the wrong commit, or contradicts the package/release boundary. | Stop the cutover, fix the blocker with a targeted change, rerun the affected validation, and link the evidence. |
| Environment residual | The finding depends on local machine state, unavailable host capability, missing optional hosted evidence, or a known warning that does not affect the package candidate. | Record the residual and evidence link; do not change product code unless it becomes reproducible release evidence. |
| Deferred enhancement | The finding is useful but outside the alpha candidate boundary, such as new renderer features, broader demos, extra adapters, or polish not required by the evidence contract. | Record it as deferred work; do not fold it into closeout. |

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
- `public-release-preflight-summary.json` reports the intended `tag`, `version`, and `expected_commit` truth.
- Every required check and artifact listed in the evidence index is green and from the intended commit.
- The package version matches the version-aligned tag that will be dispatched as `v<package-version>`.
- `docs/releasing.md`, `docs/release-policy.md`, `docs/support-matrix.md`, `docs/package-matrix.md`, and `CHANGELOG.md` describe the same release boundary.
- Feed credentials are only used by the public publish workflow after human approval.

## Before-publish state

This is the state before any approved feed mutation. Required action:

1. Confirm the approved commit SHA.
2. Confirm the version-aligned tag exists at that commit.
3. Run public release preflight with the explicit `tag`, `version`, and `expected_commit` values.
4. Dispatch `Publish Public Packages` with the same `tag`, `version`, and `expected_commit`.
5. Wait for the `public-release` environment approval.
6. Confirm `public-publish-before-summary.json` exists before package push begins.

Evidence to collect: `public-release-preflight-summary.json`, `public-publish-before-summary.json`, workflow run URL, approved commit SHA, and package version.

## Partial-publish state

This is the state where one or more package assets may have reached `nuget.org`, but the workflow did not finish GitHub release assets or post-publish evidence. Required action:

1. Stop retrying until the current workflow result is classified.
2. Record which package IDs and symbol packages are visible on `nuget.org`.
3. Attach `public-publish-before-summary.json`, workflow logs, and any failed step output.
4. Do not use manual package push outside the documented workflow.
5. Fix the blocking workflow or package issue through a normal pull request before retrying.
6. When retrying, dispatch the same version-aligned tag and expected commit; rely on the workflow's duplicate handling only after the partial state is recorded.

Evidence to collect: package IDs visible on `nuget.org`, missing package IDs, workflow run URL, failed step, `public-publish-before-summary.json`, and release notes draft status.

## After-publish state

This is the state after package push, GitHub release assets, and post-publish evidence complete. Required action:

1. Confirm `public-publish-after-summary.json` exists and lists the approved package IDs, version, source commit, validation artifacts, and publish target.
2. Generate `public-release-notes.md` with `scripts/New-PublicReleaseNotes.ps1`.
3. Confirm attached GitHub release assets match the package IDs in `public-publish-after-summary.json`.
4. Confirm support docs link the package matrix, known alpha limitations, and release evidence to attach for release-related reports.
5. Record the public release URL in the closeout notes.

Evidence to collect: `public-publish-after-summary.json`, `public-release-notes.md`, GitHub release URL, `nuget.org` package URLs, and support issue routing notes.

## Human Cutover Sequence

1. Create or confirm the version-aligned repository tag `v<package-version>` from the approved commit.
2. Run public release preflight for the intended `tag`, `version`, and `expected_commit`.
3. Manually dispatch `Publish Public Packages` with that `tag`, `version`, and `expected_commit`.
4. Approve the `public-release` environment only after reviewing preflight evidence.
5. Confirm package validation passes in the publish workflow before feed mutation.
6. Confirm packages and symbols are attached to the generated GitHub release.
7. Confirm published packages match the approved package IDs and version.
8. Generate `public-release-notes.md` from `public-publish-after-summary.json`.
9. Record the public release URL in the release notes.

## Non-Goals

- No fallback publishing path.
- No compatibility or migration release path.
- No manual package push outside the documented publish workflow.
- No release tag creation before human approval.
