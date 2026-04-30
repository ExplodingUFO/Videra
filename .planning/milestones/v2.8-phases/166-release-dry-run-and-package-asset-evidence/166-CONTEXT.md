# Phase 166: Release Dry Run and Package Asset Evidence - Context

**Gathered:** 2026-04-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Validate release-candidate package asset expectations without publishing to `nuget.org` or GitHub Packages.

This phase must preserve existing public and preview publishing behavior. The new path should be evidence-only and non-publishing.
</domain>

<decisions>
## Implementation Decisions

- Add a dedicated release dry-run script that packs the public package line and reuses the existing package validator.
- Drive the package set from the source-controlled public API contract manifest so release dry-run and API/package truth share one candidate package list.
- Add a dedicated GitHub workflow for release dry-run evidence rather than changing public/preview feed-push jobs.
- Keep the workflow read-only and artifact-only: no feed tokens, no `dotnet nuget push`, no GitHub release creation.
</decisions>

<code_context>
## Existing Code Insights

- `scripts/Validate-Packages.ps1` already validates package count, expected version, symbol packages, README/icon/license/repository metadata, dependencies, and size budgets.
- `publish-public.yml` and `publish-github-packages.yml` already call `Validate-Packages.ps1` immediately before pushing packages.
- `eng/public-api-contract.json` now lists the canonical public package projects and can be reused as the dry-run package list.
</code_context>

<specifics>
## Specific Ideas

- Add `scripts/Invoke-ReleaseDryRun.ps1`.
- Add `.github/workflows/release-dry-run.yml`.
- Add `ReleaseDryRunRepositoryTests` for workflow/script guardrails.
- Run the script locally once with a dry-run version and run focused repository tests.
</specifics>

<deferred>
## Deferred Ideas

- Actual package publication.
- Release tag creation.
- Feed-specific smoke against `nuget.org` or GitHub Packages.
- Signed package verification.
</deferred>
