# Phase 6: 分发与平台打包正确性 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-07
**Phase:** 06-distribution-and-platform-packaging-correctness
**Areas discussed:** distribution shape, backend discovery model, release validation strategy, documentation truthfulness

---

## Distribution Shape

| Option | Description | Selected |
|--------|-------------|----------|
| Unified package | Continue presenting `Videra.Avalonia` as one package that implicitly carries all native backend capability | |
| Base + platform packages | Keep `Videra.Avalonia` as UI/control entry, with explicit platform backend packages and truthful install semantics | ✓ |
| Per-platform entry packages | Introduce separate top-level platform entry packages immediately | |

**User's choice:** Auto-selected recommended option: `Base + platform packages`
**Notes:** Current host-conditional references already behave like split packages; the planning decision is to make that truth explicit rather than preserve a misleading pseudo-unified package.

---

## Backend Discovery Model

| Option | Description | Selected |
|--------|-------------|----------|
| Compile-time gated | Keep `VIDERA_*_BACKEND` build constants as the primary capability switch | |
| Runtime-discovered optional backends | Let Avalonia-side resolver discover installed platform backends and report missing capability explicitly | ✓ |
| Hard fail on missing platform package | Treat missing backend package as fatal even for unrelated platforms | |

**User's choice:** Auto-selected recommended option: `Runtime-discovered optional backends`
**Notes:** This best matches the alpha requirement to be truthful without forcing every installation to carry every native backend.

---

## Release Validation Strategy

| Option | Description | Selected |
|--------|-------------|----------|
| Windows-only packaging proof | Continue using one Windows publish job as the release proof for all packages | |
| Matching-host validation | Validate platform-specific packages on corresponding host OS and promote dependency/metadata checks to required gate | ✓ |
| Docs-only mitigation | Leave package structure as-is and rely on README clarifications | |

**User's choice:** Auto-selected recommended option: `Matching-host validation`
**Notes:** Phase 6 is specifically about packaging correctness, so documentation alone is not an acceptable substitute for release evidence.

---

## Distribution Channel

| Option | Description | Selected |
|--------|-------------|----------|
| Move to NuGet.org now | Use Phase 6 to switch public distribution to NuGet.org | |
| Stay on GitHub Packages | Keep GitHub Packages as the only alpha channel while package semantics are still settling | ✓ |
| Dual publish | Publish to both GitHub Packages and NuGet.org in alpha | |

**User's choice:** Auto-selected recommended option: `Stay on GitHub Packages`
**Notes:** This preserves rollback flexibility while the package/install model is still being corrected.

---

## Documentation Truthfulness

| Option | Description | Selected |
|--------|-------------|----------|
| Root README only | Keep source configuration and alpha caveats only in the root README | |
| Shared package-level prerequisites | Repeat source configuration, alpha status, and platform limits in each package README | ✓ |
| Minimal docs | Reduce package READMEs and point users to a central doc site only | |

**User's choice:** Auto-selected recommended option: `Shared package-level prerequisites`
**Notes:** Package pages are often read in isolation, so each one must carry enough installation truth to avoid misleading users.

---

## the agent's Discretion

- Technical implementation of runtime backend discovery remains open.
- Plan decomposition can separate packaging mechanics, release gates, and docs cleanup into separate waves.

## Deferred Ideas

- NuGet.org migration
- Meta-package strategy after alpha
- Broader distribution enhancements beyond Phase 6 scope
