# Phase 12: Developer-Facing Samples, Docs, and Compatibility Guards - Research

**Researched:** 2026-04-08
**Domain:** .NET library developer experience, public API contract documentation, and compatibility guarding
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

No phase-specific `12-CONTEXT.md` exists. Effective constraints come from `ROADMAP.md`, `REQUIREMENTS.md`, `PROJECT.md`, and Phase 11 artifacts.

### Locked Decisions
- **Phase goal:** 用 sample、文档和 contract tests 固定新的扩展模型，防止它再次漂移成内部实现细节。
- **Must address `MAIN-02`:** 新的开发者扩展点有最小 sample/reference usage 和面向库使用者的文档
- **Must address `MAIN-03`:** 新的公开接口在 unsupported / disposed / unavailable 场景下有明确 contract，而不是隐式失败或行为漂移
- **Phase 12 success criteria:**
  - 新的扩展点有最小 sample/reference usage，外部开发者可以照着使用
  - 文档明确说明新的 pipeline contract、扩展点和 unsupported/disposed/unavailable 语义
  - repository tests 会阻止公开扩展接口、文档和实现再次分叉
- **Depends on:** Phase 11 public extensibility APIs

### Claude's Discretion
- Choose the concrete sample shape as long as it is minimal, developer-facing, and uses public APIs only.
- Choose the exact mix of docs, contract tests, and compatibility guards as long as docs, sample usage, and implementation cannot drift apart again.

### Deferred Ideas (OUT OF SCOPE)
- compositor-native Wayland embedding
- higher-level macOS safer binding replacement
- package discovery / plugin loading
- new rendering features beyond stabilizing the extension contract
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MAIN-02 | 新的开发者扩展点有最小 sample/reference usage 和面向库使用者的文档 | Add a narrow public sample, extend package README/architecture docs, and keep English/Chinese mirrors under repository tests. |
| MAIN-03 | 新的公开接口在 unsupported / disposed / unavailable 场景下有明确 contract，而不是隐式失败或行为漂移 | Add explicit behavior docs plus integration/repository tests for disposed/no-op, capability snapshot, and unavailable/fallback semantics; add pack-time API compatibility guard where feasible. |
</phase_requirements>

## Summary

Phase 11 already shipped the public extensibility surface and added first-pass docs/tests, but the current repo only pins symbol presence and broad boundary statements. The codebase still lacks a minimal developer-facing usage reference for contributors/hooks, and `src/Videra.Core/README.md` explicitly says sample/reference onboarding is deferred to the next phase. The existing tests prove that the APIs work, not that developers can discover and use them correctly.

The current runtime behavior is also more specific than the docs say. `RegisterPassContributor(...)`, `ReplacePassContributor(...)`, and `RegisterFrameHook(...)` silently return after `VideraEngine` is disposed. `GetRenderCapabilities()` remains callable and reports a non-initialized snapshot. Backend unavailability already has a stable path through `GraphicsBackendFactory.ResolveBackend(...)` fallback reasons and `VideraView.BackendDiagnostics`, but that contract is only partially documented and not tied back to the new extensibility APIs. Phase 12 should lock these semantics down explicitly instead of letting them remain accidental behavior.

The repo already has the right enforcement pattern to extend: xUnit + FluentAssertions for integration behavior, repository tests that read docs/sample files as contract, bilingual doc mirrors, and pack/publish workflows that already run `dotnet pack`. The missing piece is a dedicated developer-facing sample plus one stronger compatibility layer. Official .NET guidance supports SDK-based package validation on `dotnet pack`, but Videra currently has no repo `NuGet.config` or workflow step that authenticates GitHub Packages for baseline-package restore, so that part needs explicit planning.

**Primary recommendation:** Add one narrow public extensibility sample, document a ready/unavailable/disposed behavior matrix in the package/docs entrypoints, extend integration and repository tests to pin those semantics, and wire SDK package validation into `dotnet pack` only after GitHub Packages baseline restore/auth is configured.

## Standard Stack

### Core
| Library / Tool | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET SDK package validation | SDK feature; CI uses `8.0.x`, local machine has `10.0.201` | Binary/public API compatibility checks during `dotnet pack` | Official .NET-supported way to catch public API drift and review intentional suppressions |
| `xUnit` | `2.9.3` | Repository, contract, and integration tests | Already the repo standard across test projects |
| `FluentAssertions` | `7.0.0` | Expressive contract assertions | Already used for repo docs/tests and integration behavior |
| XML documentation output | `<GenerateDocumentationFile>true>` in `Videra.Core`, `Videra.Avalonia`, and sample projects | IntelliSense-facing API docs and downstream doc generation | Official C# doc surface; already enabled in the repo |
| NuGet/GitHub Packages README packaging | `<PackageReadmeFile>README.md</PackageReadmeFile>` | Package-facing developer docs | Official NuGet package-entrypoint pattern; already enabled in all shipped packages |

### Supporting
| Library / Tool | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `Microsoft.NET.Test.Sdk` | `18.3.0` | Test runner infrastructure | Keep using existing repo test projects; no change needed |
| `coverlet.collector` | `6.0.2` | Coverage collection | Keep existing coverage path; Phase 12 does not need a new coverage tool |
| GitHub Actions `dotnet pack` workflows | `actions/setup-dotnet@v5` with `8.0.x` | CI compatibility/package evidence lane | Use for pack-time contract checks after baseline restore is wired |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Dedicated minimal extensibility sample | Demo-only documentation | Lower maintenance, but the demo is too broad to serve as copy-paste reference for the extension model |
| SDK package validation on pack | Repository string tests only | Simpler, but cannot catch binary/public API drift |
| Preserve current disposed semantics and document/test them | Change disposed behavior to throw now | Throwing may be cleaner long-term, but it expands scope and changes already-implemented engine conventions |

**Installation:**

No new package is required for the recommended baseline path. The main new guard is SDK-built-in:

```xml
<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <PackageValidationBaselineVersion>0.1.0-alpha.1</PackageValidationBaselineVersion>
</PropertyGroup>
```

**Version verification:**
- Test stack verified from repo project files on 2026-04-08:
  - `xUnit 2.9.3`
  - `FluentAssertions 7.0.0`
  - `Microsoft.NET.Test.Sdk 18.3.0`
  - `coverlet.collector 6.0.2`
- Tooling verified from repo/workflow state on 2026-04-08:
  - local `dotnet --version` = `10.0.201`
  - CI workflows pin `actions/setup-dotnet@v5` with `8.0.x`
  - package READMEs already pack via `<PackageReadmeFile>`
  - XML docs already emit via `<GenerateDocumentationFile>true>`

## Architecture Patterns

### Recommended Project Structure
```text
samples/
├── Videra.Demo/                  # broad end-to-end demo (keep, do not overload further)
└── Videra.ExtensibilitySample/   # new minimal reference sample for hooks/contributors/capabilities

docs/
├── index.md
├── extensibility.md              # new long-lived developer-facing contract page
└── zh-CN/
    ├── index.md
    └── extensibility.md          # Chinese mirror

tests/
├── Videra.Core.IntegrationTests/
│   └── Rendering/                # lifecycle/behavior contract tests
└── Videra.Core.Tests/
    ├── Repository/               # doc/sample parity guards
    └── Samples/                  # sample-structure and usage guards
```

### Pattern 1: Narrow Public Sample, Not Demo Archaeology
**What:** Add one minimal sample focused on the shipped extension API: register a contributor, register a frame hook, and inspect capabilities using public APIs only.
**When to use:** When a new public surface needs a copyable reference path for external developers.
**Example:**

```csharp
// Source: repository pattern from src/Videra.Avalonia/README.md
View3D.Engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, context =>
{
    Console.WriteLine(context.HookPoint);
});

var capabilities = View3D.RenderCapabilities;
```

**Recommendation:** Prefer an Avalonia-host-app sample rooted at `VideraView.Engine` for the main developer-facing reference. If a Core-only sample is added, it must use public factory paths such as `GraphicsBackendFactory`, not internal `SoftwareBackend`.

### Pattern 2: Behavior Matrix as Public Contract
**What:** Document the exact semantics for these states:
- before initialization
- backend unavailable with software fallback
- backend unavailable without software fallback
- disposed engine/view
- capability query after render vs before render

**When to use:** Any time a public method can no-op, fallback, or report capability truth without throwing.
**Example:**

```markdown
| Scenario | RegisterPassContributor | RegisterFrameHook | GetRenderCapabilities / RenderCapabilities | Diagnostics truth |
|----------|-------------------------|-------------------|--------------------------------------------|------------------|
| Ready | registers normally | registers normally | initialized snapshot | live backend/profile |
| Disposed | no-op (or throw, if contract is changed) | no-op (or throw) | non-initialized snapshot | last known diagnostics only |
| Native unavailable + fallback allowed | registers normally | registers normally | software capability snapshot | fallback reason populated |
| Native unavailable + fallback disabled | n/a until caller resolves backend | n/a | n/a | caller gets explicit failure |
```

### Pattern 3: Pack-Time Compatibility Guard for Public Assemblies
**What:** Use official SDK package validation on `Videra.Core` and `Videra.Avalonia` so signature-level API drift fails on `dotnet pack`.
**When to use:** Once the repo can restore a baseline package version from GitHub Packages in CI and local contributor docs.
**Example:**

```xml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator -->
<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <PackageValidationBaselineVersion>0.1.0-alpha.1</PackageValidationBaselineVersion>
</PropertyGroup>
```

### Anti-Patterns to Avoid
- **Demo-as-only-reference:** The current demo proves app behavior, but it is too broad to teach the extension model cleanly.
- **Public docs that rely on internal types:** `SoftwareBackend` is internal. Public docs must not show `new SoftwareBackend()`.
- **String-only contract enforcement:** Current repository tests already pin names; Phase 12 must also pin lifecycle behavior.
- **Implicit lifecycle semantics:** If disposed/unavailable behavior stays as-is, document and test it explicitly. If it changes, do that deliberately and review it as an API contract change.
- **Plugin-model overclaiming:** Phase 11 intentionally kept package discovery/plugin loading out of scope. Phase 12 must keep that boundary.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Public API compatibility diffing | Custom reflection or text diff script | SDK package validation (`EnablePackageValidation`) | Official CP/PKV diagnostics, suppression workflow, and `dotnet pack` integration already exist |
| Package entrypoint docs | Custom packaging convention | `<PackageReadmeFile>` + packed `README.md` | Official NuGet path that already matches this repo |
| Docs/sample drift detection | Manual review only | Existing xUnit repository tests that read docs/sample files | The repo already uses this successfully for docs, workflows, and demo contracts |
| Lifecycle contract verification | Ad hoc manual smoke checks | Existing integration tests in `tests/Videra.Core.IntegrationTests` | Deterministic and already part of the repo’s verification style |

**Key insight:** Videra already treats docs and sample behavior as testable contract. Phase 12 should extend that existing pattern, not introduce a parallel documentation system.

## Common Pitfalls

### Pitfall 1: Symbol Lists Without Usable Reference Flow
**What goes wrong:** Docs mention API names but developers still do not know the minimum working sequence.
**Why it happens:** Current docs list extensibility symbols and boundaries, but the only “real” usage surface is a broad demo plus one short README snippet.
**How to avoid:** Add one narrow reference sample and link it from package READMEs, architecture docs, and docs index.
**Warning signs:** Docs contain symbol bullets but no single end-to-end hook/contributor example.

### Pitfall 2: Public Sample Accidentally Uses Internal/Test-Only Types
**What goes wrong:** A sample compiles only in the repo test world, not for package consumers.
**Why it happens:** Existing extensibility tests use internal `SoftwareBackend`.
**How to avoid:** Keep public examples on `VideraView.Engine` or `GraphicsBackendFactory`/`IGraphicsBackend` only.
**Warning signs:** Sample code references `SoftwareBackend`, `RenderSession`, `RenderSessionOrchestrator`, or `VideraViewSessionBridge`.

### Pitfall 3: Behavior Contract Remains Implicit
**What goes wrong:** Future changes keep docs green but change ready/disposed/unavailable semantics.
**Why it happens:** Current tests cover successful registration and diagnostics projection, but not disposed/no-op or unavailable/fallback behavior for the public extension model.
**How to avoid:** Add an explicit behavior matrix to docs and mirror it with integration tests.
**Warning signs:** Reviewers debate “should this throw or no-op?” because nothing public defines the contract.

### Pitfall 4: Package Validation Fails Only in CI
**What goes wrong:** SDK package validation is added, but `dotnet pack` cannot restore the baseline package.
**Why it happens:** Videra packages are published to GitHub Packages, but the repo currently has no committed `NuGet.config` or workflow restore-source/auth step for baseline fetch.
**How to avoid:** Either add authenticated GitHub Packages restore wiring in CI/local docs, or stage package validation after a source-controlled API baseline guard.
**Warning signs:** `dotnet pack` starts failing with package restore/source errors instead of CP/PKV compatibility diagnostics.

### Pitfall 5: README Parity Breaks Between English and Chinese Docs
**What goes wrong:** One locale documents the new contract semantics or sample, the other does not.
**Why it happens:** Videra already maintains mirrored long-lived docs, and Phase 12 adds another contract-heavy surface.
**How to avoid:** Extend repository localization tests alongside English doc changes in the same plan.
**Warning signs:** English docs mention extensibility lifecycle semantics that the Chinese mirrors do not.

## Code Examples

Verified patterns from official sources and current repo usage:

### Emit XML documentation for public APIs
```xml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/xml-documentation -->
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

### Use XML tags that matter for contract semantics
```csharp
/// <summary>Registers a contributor for a stable render pass slot.</summary>
/// <param name="slot">The stable pass slot to extend.</param>
/// <param name="contributor">The contributor invoked after the built-in pass.</param>
/// <remarks>Calls made after disposal are ignored.</remarks>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="contributor"/> is null.
/// </exception>
public void RegisterPassContributor(RenderPassSlot slot, IRenderPassContributor contributor)
{
    // ...
}
```

### Include README content in packed packages
```xml
<!-- Source: https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu5039 -->
<PropertyGroup>
  <PackageReadmeFile>README.md</PackageReadmeFile>
</PropertyGroup>

<ItemGroup>
  <None Include="README.md" Pack="true" PackagePath="\" />
</ItemGroup>
```

### Enable public API baseline validation on pack
```xml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator -->
<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <PackageValidationBaselineVersion>0.1.0-alpha.1</PackageValidationBaselineVersion>
</PropertyGroup>
```

### Generate compatibility suppressions for intentional breaks
```bash
# Source: https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/diagnostic-ids
dotnet pack /p:GenerateCompatibilitySuppressionFile=true
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| “README mentions the API names” | Package README should include code snippets/samples and links to fuller docs | Official NuGet guidance current as of 2025-10-14 | Package-consumer docs must teach usage, not just list symbols |
| Manual or custom API drift review | SDK package validation with baseline version and reviewed suppressions | Official .NET guidance current as of 2025-08-21 | Public API changes can fail automatically during `dotnet pack` |
| Implicit lifecycle/fallback semantics | Explicit documented behavior matrix + integration tests | Recommended for this phase | Prevents extension model drift into undocumented implementation detail |

**Deprecated/outdated:**
- Treating the demo as the only usage example for a public library surface.
- Leaving disposed/unavailable semantics as “whatever the current implementation happens to do.”
- Relying only on repository string tests for public API stability.

## Open Questions

1. **Should disposed extensibility calls stay as silent no-ops?**
   - What we know: current `VideraEngine` behavior returns silently after disposal for contributor/hook registration and many other engine entrypoints.
   - What's unclear: whether the project wants to keep that convention or tighten it into explicit exceptions before `1.0`.
   - Recommendation: default to documenting/testing the current no-op contract in Phase 12 unless the user explicitly wants a deliberate API behavior change.

2. **Should the main developer-facing sample be Core-first or Avalonia-first?**
   - What we know: `VideraEngine` is the public extensibility root, but most app consumers start from `VideraView`, and the current README example already uses `VideraView.Engine`.
   - What's unclear: whether the project wants one sample centered on `VideraView.Engine` only, or a second Core-only sample path as well.
   - Recommendation: make the primary sample Avalonia-first; add a Core-only snippet only if it uses public factory APIs and stays minimal.

3. **Should SDK package validation ship in this phase or be staged behind source/auth work?**
   - What we know: official .NET support exists, `dotnet pack` already runs in CI, and GitHub Packages is the publication target.
   - What's unclear: baseline package restore/auth is not wired in the repo today.
   - Recommendation: plan it in this phase only if the plan includes authenticated GitHub Packages restore for CI and contributor docs; otherwise keep a repo/test-level public contract guard now and stage pack-time validation immediately after.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET SDK | build/test/pack, XML docs, package validation | ✓ | local `10.0.201`; CI `8.0.x` | — |
| Existing xUnit test infrastructure | contract/repository/integration guards | ✓ | `xUnit 2.9.3`, `FluentAssertions 7.0.0`, `Microsoft.NET.Test.Sdk 18.3.0` | — |
| GitHub Actions pack workflows | pack-time compatibility guard | ✓ | `ci.yml` + `publish-nuget.yml` present | local `dotnet pack` |
| GitHub Packages authenticated restore source for baseline validation | SDK package validation against previous Videra package | ✗ in repo config | no committed `NuGet.config`; no restore-source/auth step found | use repo-level API/doc contract tests first, or add CI/local source auth |

**Missing dependencies with no fallback:**
- None for sample/docs/test work itself.

**Missing dependencies with fallback:**
- Baseline-package restore/auth for SDK package validation. Fallback: repo-level public contract tests and source-controlled sample/docs parity guards.

## Sources

### Primary (HIGH confidence)
- Repository source and tests:
  - `src/Videra.Core/Graphics/VideraEngine.cs`
  - `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
  - `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
  - `src/Videra.Core/README.md`
  - `src/Videra.Avalonia/README.md`
  - `ARCHITECTURE.md`
  - `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineExtensibilityIntegrationTests.cs`
  - `tests/Videra.Core.IntegrationTests/Rendering/VideraViewExtensibilityIntegrationTests.cs`
  - `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`
  - `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`
  - `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`
  - `tests/Videra.Core.Tests/Samples/DemoConfigurationTests.cs`
- Microsoft Learn: XML documentation tutorial
  - https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/xml-documentation
- Microsoft Learn: Package readme on NuGet.org
  - https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org
- Microsoft Learn: NuGet Error `NU5039`
  - https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu5039
- Microsoft Learn: Validate against a baseline package version
  - https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator
- Microsoft Learn: Package/assembly validation diagnostic IDs
  - https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/diagnostic-ids

### Secondary (MEDIUM confidence)
- NuGet Gallery listing for `Microsoft.DotNet.ApiCompat.Task` current versions
  - https://www.nuget.org/packages/Microsoft.DotNet.ApiCompat.Task/

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - mostly derived from existing repo tooling plus official Microsoft/NuGet documentation.
- Architecture: HIGH - grounded in live repo structure, shipped Phase 11 artifacts, and currently passing targeted tests.
- Pitfalls: MEDIUM - based on repo evidence plus one inference: SDK package validation will need explicit GitHub Packages restore/auth wiring.

**Research date:** 2026-04-08
**Valid until:** 2026-05-08
