# Phase 187 Context: Install Truth, Size Evidence, and Guardrails

## Why This Phase Exists

After Phase 186, the code path is honest:

- viewer-only installs can stay importer-free by default
- importer-backed loading requires explicit package/configuration

The remaining gap is repository truth:

- docs still need to teach the new viewer-only vs importer-enabled split consistently
- package-size/install-surface evidence should reflect the slimmer default path
- repository guardrails still contain older dependency-boundary assumptions

## Scope

- package/install docs and support wording
- package-size/install-surface evidence
- repository tests/guardrails that describe the Avalonia install boundary

## Out of Scope

- new runtime/importer/chart/platform work
- release tagging/publishing changes
- compatibility adapters or transitional dual-default stories
