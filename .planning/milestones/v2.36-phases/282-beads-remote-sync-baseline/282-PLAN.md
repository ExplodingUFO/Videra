# Phase 282: Beads Remote Sync Baseline - Plan

## Goal

Verify the Docker Dolt Beads remote, credential path, push path, and clean state before PR execution resumes.

## Tasks

1. Capture Beads context, Dolt remote list, Docker Dolt remote/status, Git status, and Dolt status.
2. Push Docker-hosted Dolt state with `GIT_TERMINAL_PROMPT=0 dolt push origin main`.
3. Record evidence and limitations in phase summary and verification.
4. Close the phase bead with a concise handoff reason.
5. Export `.beads/issues.jsonl` and keep Git/Beads/Dolt state clean.

## Validation

- `bd context --json`
- `bd dolt remote list`
- `docker exec dolt-sql-server sh -lc "cd /var/lib/dolt/Videra && dolt remote -v && dolt status"`
- `bd sql "select * from dolt_status"`
- `docker exec dolt-sql-server sh -lc "cd /var/lib/dolt/Videra && GIT_TERMINAL_PROMPT=0 dolt push origin main"`
- `git status --short --branch`
