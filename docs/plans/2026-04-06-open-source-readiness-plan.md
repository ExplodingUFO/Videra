# Open-Source Readiness Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Remove the main blockers that prevent Videra from being published as a credible public open-source repository.

**Architecture:** Fix the build break on the critical path first, then remove machine-specific assumptions, then tighten repository metadata and governance. Keep the implementation minimal and verifiable so the repo can reach a clean "clone, build, verify" baseline.

**Tech Stack:** .NET 8, Avalonia, PowerShell/Bash verification scripts, GitHub Actions, NuGet packaging metadata.

---

### Task 1: Fix the Avalonia cross-platform build break

**Files:**
- Modify: `src/Videra.Avalonia/Composition/AvaloniaGraphicsBackendResolver.cs`
- Verify: `verify.ps1`, `verify.sh`

**Step 1: Reproduce the failing build**

Run: `dotnet build src/Videra.Avalonia/Videra.Avalonia.csproj -c Release`

Expected: FAIL with `CS0246` referencing `MetalBackend`.

**Step 2: Write the minimal fix**

Change the Metal resolver helper signature so it does not reference a platform-specific concrete type when the macOS backend assembly is not compiled into the current build.

**Step 3: Re-run the targeted build**

Run: `dotnet build src/Videra.Avalonia/Videra.Avalonia.csproj -c Release`

Expected: PASS.

### Task 2: Remove machine-specific Demo assumptions

**Files:**
- Modify: `samples/Videra.Demo/App.axaml.cs`
- Modify: `samples/Videra.Demo/Videra.Demo.csproj`

**Step 1: Reproduce the current assumptions**

Inspect the hard-coded `libassimp.dylib` paths in both files.

**Step 2: Replace absolute local paths with a publish-safe fallback**

Keep macOS support, but avoid encoding Homebrew-specific absolute paths in the project file or treating local workstation layout as a repository requirement.

**Step 3: Re-run the Demo build**

Run: `dotnet build samples/Videra.Demo/Videra.Demo.csproj -c Release`

Expected: PASS.

### Task 3: Tighten repository hygiene

**Files:**
- Delete: `src/.DS_Store`
- Delete: `src/Videra.Avalonia/.DS_Store`
- Delete: `src/Videra.Core/.DS_Store`

**Step 1: Remove tracked OS metadata files**

Delete the `.DS_Store` files that should never be versioned.

**Step 2: Verify they remain ignored**

Run: `git status --short`

Expected: deleted tracked files only; no new `.DS_Store` additions afterward.

### Task 4: Add public package and governance metadata

**Files:**
- Modify: `Directory.Build.props`
- Modify: `src/Videra.Core/Videra.Core.csproj`
- Modify: `src/Videra.Avalonia/Videra.Avalonia.csproj`
- Modify: `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj`
- Modify: `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`
- Modify: `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`
- Modify: `LICENSE.txt`
- Modify: `README.md`
- Modify: `CONTRIBUTING.md`
- Create: `SECURITY.md`
- Create: `CODE_OF_CONDUCT.md`

**Step 1: Add minimum publishable NuGet metadata**

Set repository URL, package license expression, readme, tags, authors, and descriptions for the packable libraries without turning tests or samples into packages.

**Step 2: Replace placeholder public URLs and license placeholders**

Replace placeholder clone URLs with the real repository URL and finalize the MIT license text.

**Step 3: Add basic governance docs**

Create concise `SECURITY.md` and `CODE_OF_CONDUCT.md`.

### Task 5: Add baseline CI and verify repository health

**Files:**
- Create or modify: `.github/workflows/ci.yml`
- Verify: `verify.ps1`

**Step 1: Add a pull-request/push verification workflow**

Run the standard repository verification entrypoint on GitHub Actions.

**Step 2: Run end-to-end verification locally**

Run: `pwsh -File ./verify.ps1 -Configuration Release`

Expected: PASS.

**Step 3: Check repo state**

Run: `git status --short`

Expected: only intended changes.
