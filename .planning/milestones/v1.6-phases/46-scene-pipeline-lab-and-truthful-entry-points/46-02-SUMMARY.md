---
phase: 46-scene-pipeline-lab-and-truthful-entry-points
plan: 02
subsystem: scene-pipeline-docs
tags: [docs, architecture, localization]
provides:
  - aligned README/architecture/extensibility truth
  - localized scene-pipeline wording
  - narrow lab positioning across entrypoints
key-files:
  modified:
    - README.md
    - ARCHITECTURE.md
    - docs/extensibility.md
    - docs/zh-CN/ARCHITECTURE.md
    - docs/zh-CN/extensibility.md
    - docs/zh-CN/modules/demo.md
    - docs/zh-CN/modules/videra-avalonia.md
    - docs/zh-CN/modules/videra-core.md
    - src/Videra.Avalonia/README.md
    - src/Videra.Core/README.md
requirements-completed: [DOC-01]
completed: 2026-04-17
---

# Phase 46 Plan 02 Summary

## Accomplishments
- Updated README and architecture/extensibility docs so they all describe the new authoritative scene contract and direct device/surface migration truth.
- Updated Chinese entrypoints and module docs so localization stays aligned with the English story.
- Repositioned the demo surface as a narrow Scene Pipeline Lab instead of a broader viewer showcase.

## Verification
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryArchitectureTests|FullyQualifiedName~DemoConfigurationTests"`
- `pwsh -File ./scripts/verify.ps1 -Configuration Release`

## Notes
The docs moved with the code so the milestone closed on one consistent story instead of implementation-only truth.
