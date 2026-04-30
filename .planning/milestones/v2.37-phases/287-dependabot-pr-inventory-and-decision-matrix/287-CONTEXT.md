# Phase 287: Dependabot PR Inventory and Decision Matrix - Context

**Gathered:** 2026-04-28  
**Status:** Ready for planning  
**Bead:** Videra-lm9

## Phase Boundary

Capture the open Dependabot PR set, package overlap, mergeability, and merge/replace/close decisions before modifying branches.

## Current Open Dependabot PRs

| PR | Branch | Package family | Files | Current checks | Initial decision |
|----|--------|----------------|-------|----------------|------------------|
| #84 | `dependabot/nuget/tests/Tests.Common/shared-test-tooling-72d21994e0` | FluentAssertions 7.0.0 -> 8.9.0, Microsoft.NET.Test.Sdk 18.3.0 -> 18.4.0, xunit.runner.visualstudio 2.8.2 -> 3.1.5 | 11 test `.csproj` files | mixed failure/cancelled/success | Accept only after targeted verification/remediation; likely highest test-source blast radius |
| #85 | `dependabot/nuget/analyzers-fd6f48b923` | SonarAnalyzer.CSharp 10.6.0.109712 -> 10.24.0.138807 | `Directory.Build.props` | one quality-gate failure; other checks passed | Accept with quality-gate remediation if failure is analyzer-induced |
| #86 | `dependabot/nuget/src/Videra.Core/multi-cf9afaf959` | Microsoft.Extensions.Logging 9.0.11 -> 10.0.7 and Abstractions 9.0.11 -> 10.0.7 | `src/Videra.Core/Videra.Core.csproj` | broad failures | Prefer this over #87; verify package/runtime compatibility before merge |
| #87 | `dependabot/nuget/src/Videra.Core/Microsoft.Extensions.Logging.Abstractions-10.0.7` | Microsoft.Extensions.Logging.Abstractions 9.0.11 -> 10.0.7 | `src/Videra.Core/Videra.Core.csproj` | broad failures | Close as superseded by #86 after #86 path is confirmed |
| #88 | `dependabot/nuget/Microsoft.SourceLink.GitHub-10.0.203` | Microsoft.SourceLink.GitHub 8.0.0 -> 10.0.203 | `Directory.Build.props` | all checks passed on PR branch | Accept as clean merge candidate; recheck against current `master` before merge |

## Overlap

- #86 and #87 both touch `src/Videra.Core/Videra.Core.csproj`; #86 includes the #87 abstraction update plus `Microsoft.Extensions.Logging`, so #87 should not be merged separately.
- #85 and #88 both touch `Directory.Build.props` but update different analyzer/source-link references. They may require serial merge/rebase but do not semantically supersede each other.
- #84 only touches test project files. It does not overlap file-wise with #85/#86/#87/#88, but may expose analyzer/test source compatibility failures.

## Decisions

- Continue with four verification tracks: SourceLink (#88), Analyzer (#85), Logging (#86 replacing #87), and Test Tooling (#84).
- Do not modify product or package files during inventory.
- Do not merge #87; close it as superseded once #86 is either accepted or explicitly rejected.
- Use Beads child tasks under Phase 288 for independent verification tracks.
