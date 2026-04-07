# Native CI Gating Design

**Date:** 2026-04-08

## Context

Videra is already in public alpha development, and the repository now includes:

- a default Windows-based repository verification workflow in `.github/workflows/ci.yml`
- a manual cross-platform native validation workflow in `.github/workflows/native-validation.yml`
- a tag-triggered package publishing workflow in `.github/workflows/publish-nuget.yml`

The remaining mismatch is procedural rather than architectural. The project state and public docs still describe Linux/macOS native validation as a manually closed gap, while the repository is already capable of shifting that responsibility into GitHub-hosted CI.

## Goals

- Make Linux, macOS, and Windows native validation run automatically on pull requests
- Keep manual reruns available for targeted investigation
- Make tag-based package publishing depend on fresh cross-platform native validation
- Update planning and public documentation so the quality bar matches the new CI-driven policy
- Preserve honest wording about Linux being X11-first and Wayland remaining unsupported

## Non-Goals

- No attempt to add Wayland support in this change
- No attempt to redesign package layout or backend discovery in this change
- No attempt to mark Phase 1 or Phase 3 complete without fresh hosted-run evidence
- No replacement of the existing verification scripts

## Approaches Considered

### 1. Separate Native Gate Workflow

Keep `.github/workflows/ci.yml` as the fast default repository verification workflow and upgrade `.github/workflows/native-validation.yml` into an automatic pull-request and post-merge gate.

**Pros**

- Minimal structural change
- Reuses the existing native validation scripts and documentation
- Keeps native failures isolated from the general repository verification job
- Easy to make required in branch protection

**Cons**

- PRs will show multiple workflows instead of one consolidated pipeline

### 2. Fold Native Validation Into `ci.yml`

Convert `ci.yml` into a multi-OS matrix that runs both standard verification and native validation.

**Pros**

- Single entrypoint for contributors

**Cons**

- Native validation concerns become mixed with baseline verification
- Slower feedback loop for standard PR failures
- Harder to reason about which jobs are optional versus native-host specific

### 3. Split Native Validation Into Three Separate Workflows

Create one workflow per host OS.

**Pros**

- Very fine-grained status checks

**Cons**

- Repetition
- More maintenance overhead
- More branch-protection setup burden

## Recommendation

Use approach 1.

It fits the repository as it exists today, requires the least churn, and makes the new policy obvious:

- `CI` remains the fast repository verification workflow
- `Native Validation` becomes the cross-platform native gate
- `Publish NuGet Packages` re-runs that same native gate before packaging

## Design

## Workflow Structure

### `ci.yml`

Keep this workflow narrow:

- trigger on `push` to `master`
- trigger on `pull_request`
- run `./verify.ps1 -Configuration Release` on `windows-latest`

This preserves fast baseline validation and avoids duplicating native-host setup in the default path.

### `native-validation.yml`

Expand triggers from manual-only to:

- `pull_request`
- `push` to `master`
- `workflow_dispatch`

Job behavior:

- for `pull_request` and `push`, run all three OS jobs automatically
- for `workflow_dispatch`, continue supporting targeted `all` / `linux` / `macos` / `windows` selection

This keeps manual diagnostics while making native validation part of normal repository flow.

### `publish-nuget.yml`

Add explicit native validation jobs for:

- Linux
- macOS
- Windows

Then make the publish job depend on those jobs through `needs`.

This avoids relying on cross-workflow orchestration and guarantees tag publishing cannot proceed without fresh native validation evidence in the same workflow run.

## Documentation And Planning Updates

### Public Docs

Update:

- `README.md`
- `docs/native-validation.md`

New message:

- cross-platform native validation runs automatically in GitHub Actions for pull requests
- manual workflow dispatch and local scripts remain available for targeted reruns and troubleshooting
- Linux remains X11-first
- Wayland remains an open gap

### Planning Docs

Update:

- `.planning/STATE.md`
- `.planning/ROADMAP.md`

New message:

- the native validation closure path is now GitHub-hosted matching-host CI rather than local-only manual execution
- the blocker is reduced to "first successful hosted native-validation evidence" rather than "local manual execution only"
- Phase 6, 7, and 8 remain pending

## Test Strategy

Use repository-level tests first.

Add or update tests in:

- `tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`

The tests should assert:

- `native-validation.yml` is triggered by `pull_request`, `push`, and `workflow_dispatch`
- automatic runs cover Linux, macOS, and Windows
- tag publishing is gated by native validation jobs before package push
- public docs describe CI-hosted native validation accurately

## Risks

- Hosted GitHub runners may expose flaky native behavior, especially for Linux X11/Vulkan paths under `xvfb`
- A stricter release gate increases tag-to-publish latency
- Planning docs can become misleading if they claim closure before the first successful hosted run

## Mitigations

- Keep manual rerun entrypoints for focused reruns
- Preserve local scripts for true host debugging
- Update wording carefully so "CI-gated" does not become "already validated" without evidence

## Success Criteria

- Pull requests automatically run Linux, macOS, and Windows native validation
- Tag publishing depends on fresh three-platform native validation
- Repository tests enforce the new workflow and documentation contract
- README and native validation runbook match actual workflow behavior
- Planning documents describe GitHub CI as the native validation path without overstating completion
