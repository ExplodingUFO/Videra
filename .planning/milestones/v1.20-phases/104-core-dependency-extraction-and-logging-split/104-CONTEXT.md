# Phase 104 Context: Core Dependency Extraction and Logging Split

## Goal

Slim `Videra.Core` into a true runtime kernel by removing concrete importer and logging-provider dependencies while keeping the existing viewer/runtime and packaged consumer paths intact.

## Why This Phase Exists

Phase 103 fixed the public product boundary story, but the package graph still had to match that story in code. `Videra.Core` could not keep concrete importer or Serilog provider dependencies without undermining the new `Core` / `Import` / `Backend` / `UI adapter` split.

## Inputs

- `.planning/ROADMAP.md` Phase 104 definition
- `.planning/REQUIREMENTS.md` requirements `CORE-01`, `CORE-02`, `PKG-01`, `PKG-02`
- `README.md`
- `docs/capability-matrix.md`
- `src/Videra.Core/Videra.Core.csproj`
- `src/Videra.Avalonia/Videra.Avalonia.csproj`
- `src/Videra.Import.Gltf/`
- `src/Videra.Import.Obj/`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `scripts/Validate-Packages.ps1`
- importer and consumer-smoke repository tests

## Done When

1. `Videra.Core` no longer directly references concrete importer packages.
2. `Videra.Core` no longer directly references concrete logging-provider packages.
3. glTF and OBJ import live in dedicated `Videra.Import.*` packages.
4. The existing viewer/runtime and packaged consumer paths still compose through the explicit package split.
