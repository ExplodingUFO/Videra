# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this repository is currently in an early pre-release stage.

## [Unreleased]

## [0.1.0-alpha.2] - 2026-04-17

### Added

- `Videra.MinimalSample` as the shortest public first-scene reference
- package-based consumer smoke validation from install to first scene
- viewer and surface-chart benchmark workflow gates with published artifacts
- alpha feedback and support surfaces centered on diagnostics-rich reproduction data

### Changed

- Simplified the default `VideraView` onboarding path and moved advanced extensibility behind explicit opt-in docs
- Bumped the default package version baseline to `0.1.0-alpha.2`

### Fixed

- Repository truth drift between happy-path docs and `LoadModelsAsync(...)` batch-replace semantics
- Missing workflow-visible consumer and benchmark validation for the public alpha path

## [0.1.0-alpha.1] - 2026-04-06

### Added

- Baseline open-source governance files
- CI verification workflow
- Initial GitHub Packages alpha distribution path

### Changed

- Improved repository documentation and package metadata
- Improved open-source readiness and verification baseline
- Set the default package version baseline to `0.1.0-alpha.1`

### Fixed

- Cross-platform Avalonia build failure caused by platform-specific backend type leakage
- Repository-specific local path assumptions in the Demo project
