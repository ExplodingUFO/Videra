# Phase 396 Plan: Release Cutover Inventory and Approval Packet

## Goal

Convert v2.57 release-readiness evidence into an approval packet, abort/hold criteria, and Beads/worktree coordination map for the rest of v2.58.

## Tasks

1. Claim `Videra-v258.1` and split read-only inventory into child beads:
   - `Videra-v258.1a` - v2.57 evidence inventory
   - `Videra-v258.1b` - package and release script inventory
   - `Videra-v258.1c` - Beads/docs/handoff inventory
2. Run the three child beads in isolated worktrees/branches and collect handoff notes.
3. Synthesize the approval packet from v2.57 archived planning, final validation evidence, package/release surfaces, and Beads/docs state.
4. Define abort/hold criteria for package, docs, validation, Beads, Git, Dolt, and remote-state failures.
5. Update `.planning/STATE.md` and `.planning/ROADMAP.md` with Phase 396 completion and Phase 397 as next ready.
6. Close child beads and `Videra-v258.1`, export Beads, regenerate the public roadmap, remove Phase 396 worktrees/branches, commit, and push Git plus Dolt Beads state.

## Validation

- `bd show Videra-v258 --json`
- `bd ready --json`
- `bd lint --json`
- `pwsh -NoProfile -File scripts\Export-BeadsRoadmap.ps1`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests`
- `git worktree list | Select-String "v258-phase396|v2.58-phase396"`
- `git status --short --branch`
