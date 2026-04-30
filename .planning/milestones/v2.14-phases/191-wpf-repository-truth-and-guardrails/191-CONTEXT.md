# Phase 191 Context: WPF Repository Truth and Guardrails

## Why now

`Phase 190` already proved one repository-only WPF golden path and generated a richer diagnostics artifact. The remaining gap is not runtime behavior; it is repository truth. Docs and guard tests must describe that proof honestly without implying a second public UI package or release path.

## Scope

- tighten WPF wording in the repository-level docs that describe package, support, and native-validation truth
- update repository tests so the stronger WPF boundary stays locked
- keep the change bounded to docs/guardrails only

## Non-goals

- no runtime/backend/import/chart changes
- no WPF public package line
- no workflow expansion beyond the existing repository-native validation lane
- no compatibility shims or transitional wording
