# Phase 65 Research

## Problem

The repository already had strong internal tests, but the public package-consumer path from install to first scene was still unproven. That left the alpha story vulnerable to package-install or package-surface regressions that repo-local project references would not catch.

## Findings

- The current public packages are published separately from the repo-local project graph, so a real consumer smoke path needed package references instead of project references.
- The already published `0.1.0-alpha.1` packages lagged the current happy-path APIs, so consumer smoke needed to validate the current package surface by packing current public packages locally.
- Host validation needed explicit workflow entrypoints across Windows, Linux X11, Linux XWayland, and macOS so the supported alpha paths stayed visible.

## Decision

Phase 65 should add a package-based smoke app plus a dedicated script/workflow that pack current public packages locally, restore a true consumer app from those nupkgs, and validate initialize → load → frame → diagnostics across host-specific entrypoints.
