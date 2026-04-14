# Verify Script Relocation Implementation Plan

> **Execution note:** Implement this plan task-by-task in an isolated worktree with review checkpoints between tasks.

**Goal:** Move the PowerShell repository verification entrypoint to `scripts/verify.ps1`, remove the root entrypoint, update all first-party callers, and improve failure diagnostics for CI test failures.

**Architecture:** Keep `verify.sh` and `scripts/run-native-validation.ps1` as thin wrappers around repository verification while making `scripts/verify.ps1` the sole PowerShell entrypoint. Repository tests and docs become the contract for the new location, and the script writes deterministic test result artifacts for failure analysis.

**Tech Stack:** PowerShell 7, .NET 8 CLI, xUnit, FluentAssertions, GitHub Actions YAML, Markdown docs

---

### Task 1: Move The PowerShell Verify Entrypoint And Harden Diagnostics

**Files:**
- Create: `scripts/verify.ps1`
- Delete: `verify.ps1`
- Modify: `scripts/run-native-validation.ps1`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs`
- Modify: `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs`

**Step 1: Write the failing tests**

Update repository tests so they fail against the old layout:

- `RepositoryNativeValidationTests` should read `scripts/verify.ps1` instead of root `verify.ps1`.
- `SurfaceChartsRepositoryArchitectureTests` should assert `scripts/verify.ps1` contains the expected demo verification targets.
- Add assertions that `scripts/run-native-validation.ps1` invokes `scripts/verify.ps1`.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test .\tests\Videra.Core.Tests\Videra.Core.Tests.csproj --configuration Release --filter "FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests" -v m
```

Expected: FAIL because `scripts/verify.ps1` does not exist yet and repository guards still point at the root file.

**Step 3: Write minimal implementation**

- Move the PowerShell verification logic from `verify.ps1` to `scripts/verify.ps1`.
- Recompute the repository root with `Split-Path -Parent $PSScriptRoot`.
- Keep existing parameters and step structure.
- Add deterministic test results output, for example under `artifacts/test-results/verify`.
- Ensure the test step emits enough detail for failing test names and prints the result directory on failure.
- Update `scripts/run-native-validation.ps1` to invoke `scripts/verify.ps1`.
- Delete the root `verify.ps1`.

**Step 4: Run targeted tests to verify it passes**

Run:

```powershell
dotnet test .\tests\Videra.Core.Tests\Videra.Core.Tests.csproj --configuration Release --filter "FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests" -v m
```

Expected: PASS

**Step 5: Commit**

```powershell
git add scripts/verify.ps1 scripts/run-native-validation.ps1 tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs verify.ps1
git commit -m "build: relocate powershell verify entrypoint"
```

### Task 2: Update Workflows, Docs, And Repository Contracts To The New Path

**Files:**
- Modify: `.github/workflows/ci.yml`
- Modify: `.github/workflows/publish-nuget.yml`
- Modify: `.github/pull_request_template.md`
- Modify: `ARCHITECTURE.md`
- Modify: `CONTRIBUTING.md`
- Modify: `README.md`
- Modify: `docs/troubleshooting.md`
- Modify: `docs/native-validation.md`
- Modify: `docs/zh-CN/ARCHITECTURE.md`
- Modify: `docs/zh-CN/CONTRIBUTING.md`
- Modify: `docs/zh-CN/README.md`
- Modify: `docs/zh-CN/native-validation.md`
- Modify: `docs/zh-CN/troubleshooting.md`
- Modify: `docs/zh-CN/modules/videra-core.md`
- Modify: `docs/zh-CN/modules/platform-linux.md`
- Modify: `docs/zh-CN/modules/platform-macos.md`
- Modify: `docs/zh-CN/modules/platform-windows.md`
- Modify: `docs/zh-CN/modules/demo.md`
- Modify: `samples/Videra.Demo/README.md`
- Modify: `src/Videra.Avalonia/README.md`
- Modify: `src/Videra.Core/README.md`
- Modify: `src/Videra.Platform.Linux/README.md`
- Modify: `src/Videra.Platform.Windows/README.md`
- Modify: `src/Videra.Platform.macOS/README.md`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`

**Step 1: Write the failing tests**

Update repository release-readiness assertions so they require `pwsh -File ./scripts/verify.ps1 -Configuration Release` where appropriate.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test .\tests\Videra.Core.Tests\Videra.Core.Tests.csproj --configuration Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests" -v m
```

Expected: FAIL because workflows and docs still point to the root path.

**Step 3: Write minimal implementation**

- Replace root `verify.ps1` references with `scripts/verify.ps1` across workflows, docs, templates, and package/sample READMEs.
- Keep `verify.sh` at the repository root, but make any PowerShell references it documents point at `scripts/verify.ps1`.
- Update repository tests to assert the new strings.

**Step 4: Run targeted tests to verify it passes**

Run:

```powershell
dotnet test .\tests\Videra.Core.Tests\Videra.Core.Tests.csproj --configuration Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests" -v m
```

Expected: PASS

**Step 5: Commit**

```powershell
git add .github/workflows/ci.yml .github/workflows/publish-nuget.yml .github/pull_request_template.md ARCHITECTURE.md CONTRIBUTING.md README.md docs docs/zh-CN samples/Videra.Demo/README.md src/Videra.Avalonia/README.md src/Videra.Core/README.md src/Videra.Platform.Linux/README.md src/Videra.Platform.Windows/README.md src/Videra.Platform.macOS/README.md tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
git commit -m "docs: point verification guidance at scripts entrypoint"
```

### Task 3: Run Final Verification And Inspect Deliverable State

**Files:**
- Verify only: working tree

**Step 1: Run targeted repository guards**

Run:

```powershell
dotnet test .\tests\Videra.Core.Tests\Videra.Core.Tests.csproj --configuration Release --filter "FullyQualifiedName~Repository" -v m
```

Expected: PASS

**Step 2: Run full repository verification from the new entrypoint**

Run:

```powershell
pwsh -File .\scripts\verify.ps1 -Configuration Release
```

Expected: PASS

**Step 3: Inspect diff and branch state**

Run:

```powershell
git status --short
git diff --stat
```

Expected: only intended relocation, diagnostics, workflow, test, and doc changes

**Step 4: Commit**

```powershell
git add docs/plans/2026-04-14-verify-script-relocation-design.md docs/plans/2026-04-14-verify-script-relocation-implementation.md
git commit -m "docs: record verify script relocation plan"
```
