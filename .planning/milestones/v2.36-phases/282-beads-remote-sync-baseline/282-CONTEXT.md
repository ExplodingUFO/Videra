# Phase 282: Beads Remote Sync Baseline - Context

**Gathered:** 2026-04-28  
**Status:** Ready for planning  
**Bead:** Videra-s5u

<domain>
## Phase Boundary

Verify the Docker Dolt Beads remote, credential path, push path, and clean state before PR execution resumes.
</domain>

<decisions>
## Implementation Decisions

- Treat Docker `dolt-sql-server` as the source of Beads Dolt truth for remote push.
- Use `git+https://github.com/ExplodingUFO/Videra.git` as the Dolt remote.
- Treat the current `bd doctor` remotesapi warning as diagnostic-only unless it blocks an explicit sync command.
- Do not change product code, CI workflows, release behavior, packages, feeds, or tags in this phase.
</decisions>

<code_context>
## Existing Context

- `bd context --json` reports backend `dolt`, mode `server`, database `Videra`, host `127.0.0.1`, port `3306`, and project id `cf27bb80-40f6-4ba7-95f7-bc455a484d7b`.
- `bd dolt remote list` reports `origin git+https://github.com/ExplodingUFO/Videra.git`.
- Docker repo path is `/var/lib/dolt/Videra`.
- GitHub HTTPS credentials are configured inside the Docker container for non-interactive Dolt push.
</code_context>

<specifics>
## Specific Proof Needed

1. Capture Beads context and remote identity.
2. Confirm Docker Dolt working tree is clean.
3. Confirm `bd sql "select * from dolt_status"` is empty.
4. Execute Docker-hosted `GIT_TERMINAL_PROMPT=0 dolt push origin main`.
5. Confirm Git working tree and Beads state are clean after push.
</specifics>

<deferred>
## Deferred Ideas

- Full remotesapi peer-sync diagnosis remains deferred unless it blocks the selected Docker-hosted Dolt push path.
</deferred>
