# Releasing Videra

This runbook describes how public and preview package publication works.

## Tag format

- Public releases start from a git tag named `v<semver>`.
- Example: `v0.2.0-alpha.1`

## Public release flow

Public package publishing runs through `.github/workflows/publish-public.yml`.

That workflow is expected to:

1. Resolve the version from the tag.
2. Run matching-host native validation for Linux X11, Linux Wayland-session `XWayland`, macOS, and Windows.
3. Pack the public package set.
4. Validate package metadata and assets through `scripts/Validate-Packages.ps1`.
5. Push `.nupkg` and `.snupkg` assets to `nuget.org`.
6. Create or update the GitHub Release with generated notes and attached package assets.

## Preview flow

Preview and internal validation runs through `.github/workflows/publish-github-packages.yml`.

That workflow is manual (`workflow_dispatch`) and pushes preview artifacts to `GitHub Packages`.

## Release notes

- Release-page categories come from `.github/release.yml`.
- The release surface should communicate breaking changes, features, fixes, docs, and CI/build work.
- Public release assets should make it obvious which package IDs are part of the release.

## Package set

The public package set is limited to:

- `Videra.Core`
- `Videra.Avalonia`
- `Videra.Platform.Windows`
- `Videra.Platform.Linux`
- `Videra.Platform.macOS`

`Videra.SurfaceCharts.*` and the sample/demo applications remain repository-only until a later milestone explicitly promotes them.

## Maintainer checklist

- Confirm `CHANGELOG.md` and public docs match the shipped truth.
- Confirm release notes categories in `.github/release.yml` still match the current label taxonomy.
- Confirm `NUGET_API_KEY` is configured for the public workflow.
- Confirm preview/internal workflows do not override the public-feed truth.
