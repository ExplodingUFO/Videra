# Native CI Gating Implementation Plan

> **Execution Note:** Implement this plan task-by-task with verification between tasks.

**Goal:** Turn cross-platform native validation into an automatic PR and release gate, then align project status and public docs with that CI-driven validation policy.

**Architecture:** Reuse the existing Windows verification workflow, upgrade the dedicated native-validation workflow into an automatic multi-trigger gate, and make the publish workflow depend on fresh Linux/macOS/Windows native validation jobs. Enforce the contract with repository-level tests before updating workflows and docs.

**Tech Stack:** GitHub Actions, PowerShell, Bash, .NET 8, xUnit, FluentAssertions

---

### Task 1: Add Failing Repository Tests For CI Gating

**Files:**
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`

**Step 1: Write the failing test**

Add repository tests that assert:

- `.github/workflows/native-validation.yml` contains `pull_request:`, `push:`, and `workflow_dispatch:`
- the native validation jobs run automatically outside manual dispatch
- `.github/workflows/publish-nuget.yml` includes Linux, macOS, and Windows native validation jobs and a publish job that depends on them

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: FAIL because the current workflows are still manual-only for native validation and publish is not gated by native validation jobs.

**Step 3: Write minimal implementation**

Do not change production workflows yet. Only commit the failing tests in local working state and confirm they fail for the expected reason.

**Step 4: Run test to verify it fails**

Run the same command again and confirm the failure is stable and meaningful.

**Step 5: Commit**

```bash
git add tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
git commit -m "test: codify native CI gating expectations"
```

### Task 2: Upgrade Native Validation Workflow To An Automatic Gate

**Files:**
- Modify: `.github/workflows/native-validation.yml`

**Step 1: Write the failing test**

Use the tests from Task 1 as the active failing specification. Do not add new tests unless the existing assertions are too weak.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryNativeValidationTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Update `.github/workflows/native-validation.yml` so that:

- `pull_request` and `push` to `master` trigger the workflow automatically
- `workflow_dispatch` remains supported with target selection
- Linux, macOS, and Windows jobs still reuse the existing native validation scripts
- job `if` expressions treat `pull_request` and `push` as "run all targets"

**Step 4: Run test to verify it passes**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryNativeValidationTests"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add .github/workflows/native-validation.yml tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs
git commit -m "ci: run native validation on pull requests"
```

### Task 3: Gate Tag Publishing On Fresh Native Validation

**Files:**
- Modify: `.github/workflows/publish-nuget.yml`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`

**Step 1: Write the failing test**

Extend the repository release-readiness test expectations so they require:

- Linux/macOS/Windows native validation jobs in `publish-nuget.yml`
- a publish job with `needs` on those validation jobs
- package push happening only after those jobs complete

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Refactor `.github/workflows/publish-nuget.yml` into:

- `linux-native-validation`
- `macos-native-validation`
- `windows-native-validation`
- `publish`

Reuse the same commands and prerequisites already used by `.github/workflows/native-validation.yml`.

Keep the existing package version detection, verification, packing, artifact validation, and package push steps inside the final `publish` job.

**Step 4: Run test to verify it passes**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add .github/workflows/publish-nuget.yml tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
git commit -m "ci: gate publishing on native validation"
```

### Task 4: Update Public Docs And Planning State

**Files:**
- Modify: `README.md`
- Modify: `docs/native-validation.md`
- Modify: `.planning/STATE.md`
- Modify: `.planning/ROADMAP.md`
- Optional if wording must stay aligned: `docs/zh-CN/native-validation.md`, `docs/zh-CN/README.md`

**Step 1: Write the failing test**

Add or extend repository tests that require the docs to state:

- native validation runs in GitHub Actions for pull requests
- manual dispatch remains available
- local matching-host scripts remain for troubleshooting
- publishing is gated by native validation

Do not assert that Phase 1 is complete unless the wording explicitly says only that CI gating exists.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Update docs and planning text so they consistently say:

- GitHub-hosted matching-host CI is now the primary closure path for native validation
- manual dispatch and local execution are fallback/reproduction tools
- Linux is still X11-first and Wayland is still open
- the first successful hosted run is still the evidence needed to fully close the prior blocker

**Step 4: Run test to verify it passes**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add README.md docs/native-validation.md .planning/STATE.md .planning/ROADMAP.md tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
git commit -m "docs: align native validation status with CI gating"
```

### Task 5: Run Full Verification

**Files:**
- Modify only files already touched if regressions appear

**Step 1: Run repository verification**

Run:

```powershell
pwsh -File ./verify.ps1 -Configuration Release
```

Expected: PASS.

**Step 2: Run focused repository tests for workflow/docs assertions**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: PASS.

**Step 3: Review for consistency**

Confirm that:

- workflow triggers match the docs
- publish is gated by native validation jobs
- planning docs describe CI gating honestly without claiming unverified closure

**Step 4: Commit**

```bash
git add .
git commit -m "ci: enforce native validation gates"
```
