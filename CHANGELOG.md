# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this repository is currently in an early pre-release stage.

## [Unreleased]

### Added

- GitHub Packages installation guidance, including feed setup and package install examples
- Release workflow package validation before push

### Changed

- Clarified alpha distribution boundaries and default verification scope
- Tightened release workflow to publish from tags only

### Fixed

- Strengthened security reporting and PR review templates for public collaboration

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
