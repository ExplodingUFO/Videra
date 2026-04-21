---
phase: 67-alpha-feedback-and-support-surfaces
plan: 02
subsystem: templates-and-support-docs
tags: [alpha, templates, support]
provides:
  - richer issue templates
  - aligned troubleshooting and support docs
key-files:
  modified:
    - .github/ISSUE_TEMPLATE/bug_report.yml
    - .github/ISSUE_TEMPLATE/feature_request.yml
    - CONTRIBUTING.md
    - docs/support-matrix.md
    - docs/troubleshooting.md
requirements-completed: [FB-01, DOC-04, DOC-05]
completed: 2026-04-17
---

# Phase 67 Plan 02 Summary

## Accomplishments

- Expanded bug and feature templates to ask for install path, version, host environment, diagnostics, and consumer-path evidence.
- Updated troubleshooting and support docs to route reports through the same alpha consumer story.
- Kept Linux support wording explicit about X11 and XWayland compatibility.

## Verification

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~AlphaConsumerIntegrationTests"`
