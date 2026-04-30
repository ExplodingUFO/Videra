# Phase 21 Research

## Current gap shape

The latest audit report (`.planning/v1.2-MILESTONE-AUDIT.md`) no longer reports blocker gaps. It records three cleanup items:

1. Phase 19/20 summaries lack `requirements-completed`, so the audit had to manually close `VIEW-*` / `INT-*`
2. Historical verification wording in Phase 13/14/18/19 still references the pre-recovery state
3. No deterministic guard ensures future milestone-artifact consistency

## Relevant evidence

- Phase 19 verification maps:
  - `VIEW-01` -> `19-01`, `19-03`
  - `VIEW-02` -> `19-02`, `19-03`
  - `VIEW-03` -> `19-02`
- Phase 20 verification maps:
  - `INT-01` -> `20-01`, `20-03`
  - `INT-02` -> `20-01`, `20-02`, `20-03`
  - `INT-03` -> `20-02`, `20-03`
  - `INT-04` -> `20-03`

That means the summary frontmatter can be filled mechanically from the shipped verification truth instead of inventing a new interpretation.

## Existing guard seam

`SurfaceChartsRepositoryArchitectureTests` already reads repository files directly and asserts exact wording for chart truth. It is the cleanest place to add milestone-artifact guards because:

- the test project already depends on repository-file reads
- the audit-critical files are all under the same repo root
- the usual filtered command already targets this test class

## Cleanup strategy

1. Add `requirements-completed` frontmatter to the six Phase 19/20 summary files
2. Add explicit supersession notes to the four verification files that still mention the pre-recovery gap
3. Extend repository tests so future drift is caught automatically
