# Analyzer Policy

This document defines how Videra treats .NET analyzer major-version upgrades. It exists so analyzer changes remain deliberate maintenance work rather than broad architecture rewrites triggered by robot PRs.

## Current Boundary

- Analyzer package: `Microsoft.CodeAnalysis.NetAnalyzers`
- Current repository baseline before analyzer 10 adoption: `9.0.0`
- Target milestone: evaluate analyzer 10 under the policy below before changing the shared package version
- Quality posture: keep `TreatWarningsAsErrors` meaningful on release and quality-gate paths

Analyzer major updates must be handled as repo-owned work when they introduce broad warning-as-error failures. Do not merge a robot analyzer major PR directly when it changes the meaning of the quality gate.

## Rule Decisions for Analyzer 10

| Rule | Decision | Reason |
|---|---|---|
| `CA1051` | Policy-gated suppression by contract area | Videra intentionally uses visible fields in selected value-like graphics/runtime structs where property conversion would create churn or weaken the low-level shape. Fix ordinary reference-type design issues case by case. |
| `CA1720` | Suppress for established vocabulary | Names such as `Object` can be part of existing scene, selection, and hit-test vocabulary. Rename only when it improves the public concept model and is not churn for analyzer compliance alone. |
| `CA1822` | Disable as an error by default | Marking instance members static is low-value churn for service-shaped APIs and can reduce future extensibility. Apply manually only in narrow private helpers when it improves clarity. |
| `CA1859` | Disable as an error by default | Concrete-type suggestions can leak implementation details into contracts and fight interface-based boundaries. Apply only inside private hot paths with measured benefit. |

## Actionable Analyzer Failures

Keep analyzer failures actionable when they identify:

- correctness or resource-lifetime issues
- security or input-validation risks
- package/release contract drift
- warnings that point to a small local fix without public API churn
- warnings that preserve existing architecture boundaries

## Excluded Analyzer Churn

Do not accept analyzer-driven work that requires:

- broad Core, Import, Backend, Avalonia, or SurfaceCharts redesign
- compatibility layers, fallback layers, or downgrade paths
- public API churn solely for style or micro-performance suggestions
- central package management migration
- renderer, backend, chart, importer, or UI-adapter product breadth

## Triage Flow

1. Rebase or recreate the dependency PR only after confirming it is still relevant.
2. Run the release/quality path locally enough to identify real analyzer failures.
3. Classify each new rule as fix, suppress, disable, or defer in this document.
4. Prefer focused fixes for correctness issues and explicit configuration for low-value churn rules.
5. Update repository tests when the policy changes.
6. Merge only after the quality-gate path is green under the documented policy.

## Expected Outcome for Analyzer 10

The analyzer 10 milestone should end with:

- `Microsoft.CodeAnalysis.NetAnalyzers` upgraded deliberately
- rule policy documented and tested
- no broad public API churn for `CA1051`, `CA1720`, `CA1822`, or `CA1859`
- `TreatWarningsAsErrors` still active where release and quality evidence depend on it
