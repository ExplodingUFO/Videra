# Analyzer Policy

This document defines how Videra treats .NET analyzer major-version upgrades. It exists so analyzer changes remain deliberate maintenance work rather than broad architecture rewrites triggered by robot PRs.

## Current Boundary

- Analyzer package: `Microsoft.CodeAnalysis.NetAnalyzers`
- Current repository baseline after analyzer 10 adoption: `10.0.203`
- Target milestone: keep analyzer 10 active under the policy below
- Quality posture: keep `TreatWarningsAsErrors` meaningful on release and quality-gate paths

Analyzer major updates must be handled as repo-owned work when they introduce broad warning-as-error failures. Do not merge a robot analyzer major PR directly when it changes the meaning of the quality gate.

## Rule Decisions for Analyzer 10

| Rule | Decision | Reason |
|---|---|---|
| `CA1051` | Policy-gated suppression by contract area | Videra intentionally uses visible fields in selected value-like graphics/runtime structs where property conversion would create churn or weaken the low-level shape. Fix ordinary reference-type design issues case by case. |
| `CA1707` | Disable as an error by default | BDD-style test names intentionally use underscores for readable failure output. Rename tests only when the name is unclear, not for analyzer compliance alone. |
| `CA1720` | Suppress for established vocabulary | Names such as `Object` can be part of existing scene, selection, and hit-test vocabulary. Rename only when it improves the public concept model and is not churn for analyzer compliance alone. |
| `CA1822` | Disable as an error by default | Marking instance members static is low-value churn for service-shaped APIs and can reduce future extensibility. Apply manually only in narrow private helpers when it improves clarity. |
| `CA1859` | Disable as an error by default | Concrete-type suggestions can leak implementation details into contracts and fight interface-based boundaries. Apply only inside private hot paths with measured benefit. |
| `CA1861` | Disable as an error by default | Test assertion data often reads best inline. Extract arrays only when it removes duplication or a measured hot path needs it. |
| `CA2201` | Disable as an error by default | Generic exceptions are acceptable in test-only failure injection helpers. Production code should still use specific exception types. |
| `CA2263` | Disable as an error by default | Existing FluentAssertions type assertions remain on the current project idiom until a deliberate assertion-style cleanup is planned. |
| `CA1305` | Disable as an error by default | Diagnostics and support snapshot strings are invariant evidence artifacts, not localized UI. Requiring provider overloads everywhere creates low-value formatting churn. |
| `CA1848` | Disable as an error by default | LoggerMessage conversion is useful only for measured logging hot paths. Applying it repository-wide would be a mechanical rewrite. |
| `CA1873` | Disable as an error by default | Logging argument micro-optimization should follow profiling evidence, not block broad maintenance upgrades. |

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
- no broad test renaming, test data extraction, or assertion-style rewrites for `CA1707`, `CA1861`, `CA2201`, or `CA2263`
- no broad diagnostics/logging rewrites for `CA1305`, `CA1848`, or `CA1873`
- `TreatWarningsAsErrors` still active where release and quality evidence depend on it
