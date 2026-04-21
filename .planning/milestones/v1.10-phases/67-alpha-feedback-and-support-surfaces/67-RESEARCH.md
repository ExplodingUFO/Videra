# Phase 67 Research

## Problem

Public alpha docs still lacked a single obvious place to tell adopters what data to capture when something goes wrong. Without that feedback loop, the repository could be internally strong while still collecting weak external bug reports.

## Findings

- The repo already had issue templates, troubleshooting docs, and a support matrix, but they did not yet consistently ask for diagnostics-rich reproduction data.
- The newly introduced minimal sample and consumer smoke path were the right reproduction anchors for alpha feedback.
- The supported Linux story needed to stay explicit about X11 and XWayland to avoid collecting unusable bug reports against deferred native Wayland work.

## Decision

Phase 67 should add a dedicated alpha-feedback doc, update reporting templates, align troubleshooting/support docs around the same reproduction anchors, and lock that vocabulary with repository tests.
