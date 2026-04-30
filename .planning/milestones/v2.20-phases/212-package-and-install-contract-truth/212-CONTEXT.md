# Phase 212: Package and Install Contract Truth - Context

**Gathered:** 2026-04-26
**Status:** Ready for planning
**Mode:** Auto-generated from v2.20 milestone review

<domain>
## Phase Boundary

Fix package/documentation drift around explicit importer installation and add a small guardrail so README/package matrix/project-reference truth cannot silently diverge again.

</domain>

<decisions>
## Implementation Decisions

- Keep the change documentation/test-only.
- Preserve the existing package boundary: `Videra.Avalonia` depends on `Videra.Core` only and does not carry glTF/OBJ import packages transitively.
- Add a repository test rather than a standalone script because existing repo truth checks already live under `tests/Videra.Core.Tests/Repository`.

</decisions>

<code_context>
## Existing Code Insights

- `src/Videra.Avalonia/Videra.Avalonia.csproj` already references only `Videra.Core`.
- `docs/package-matrix.md` already describes explicit `Videra.Import.*` installation for importer-backed loading.
- `src/Videra.Import.Gltf/README.md` and `src/Videra.Import.Obj/README.md` still claimed `Videra.Avalonia` brings import packages transitively.
- `ARCHITECTURE.md` had the same stale transitive-import wording.

</code_context>

<specifics>
## Specific Ideas

- Add a root README install-by-scenario table.
- Correct import package READMEs and architecture wording.
- Add repository tests that fail if import READMEs claim transitive Avalonia import dependencies again.

</specifics>

<deferred>
## Deferred Ideas

- Broader importer API changes are Phase 213.
- Demo support bundle work is Phase 214.

</deferred>
