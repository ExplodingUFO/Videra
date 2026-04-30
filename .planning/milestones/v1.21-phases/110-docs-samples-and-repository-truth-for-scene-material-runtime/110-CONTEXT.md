# Phase 110 Context

## Goal

Make the repository teach the same scene/material runtime model that the shipped code already exposes.

## Constraints

- Keep the explanation viewer-first instead of backend-first.
- Do not add compatibility shims, migration layers, or speculative abstractions.
- Keep samples on public APIs only; no internal seam leakage.
- Make repository guards prove the same truth that docs and samples claim.

## Relevant Surfaces

- `README.md`
- `ARCHITECTURE.md`
- `docs/package-matrix.md`
- `docs/hosting-boundary.md`
- `src/Videra.Core/README.md`
- `src/Videra.Avalonia/README.md`
- `samples/Videra.Demo/*`
- `samples/Videra.ExtensibilitySample/*`
- `tests/Videra.Core.Tests/Repository/*`
- `tests/Videra.Core.Tests/Samples/*`

## Definition of Done

- Public docs consistently describe the backend-neutral scene/material runtime model.
- Demo/sample surfaces expose the public render-feature diagnostics now shipped by the runtime.
- Repository guards fail if those docs or samples drift away from the shipped runtime truth.
