# Beads Coordination

This repository uses `bd` (Beads) for agent task coordination. Beads is a coordination surface for work items and handoffs; it does not replace GSD requirements, roadmap phases, CI quality gates, release approvals, or product architecture decisions.

## Service Contract

Videra Beads state is stored in the Docker-backed Dolt SQL service:

| Setting | Value |
|---------|-------|
| Container | `dolt-sql-server` |
| Host | `127.0.0.1` |
| Port | `3306` |
| Database | `Videra` |
| User | `root` |
| Project id | `cf27bb80-40f6-4ba7-95f7-bc455a484d7b` |

The repository-owned Beads files under `.beads/` should resolve to that service. Verify the active context before assigning multi-agent work:

```powershell
bd context --json
bd doctor
```

`bd context --json` should report `backend: "dolt"`, `dolt_mode: "server"`, `server_host: "127.0.0.1"`, `server_port: 3306`, `database: "Videra"`, and the project id above.

Do not point this checkout at unrelated Dolt databases such as `AgentDialog`. Beads project identity is part of the safety contract; a database with a different `_project_id` is not Videra issue state.

## Agent Workflow

Agents must use Beads for task tracking instead of markdown task lists:

```powershell
bd ready --json
bd show <id> --json
bd update <id> --claim --json
bd create "Found issue" --description="Context and reproduction" -t bug -p 1 --deps discovered-from:<parent-id> --json
bd close <id> --reason "Completed" --json
```

Use `discovered-from` links when implementation reveals follow-up work. Close issues with a concrete reason that explains the delivered result or why work was intentionally deferred.

## Worktree Coordination

Parallel implementation work should keep Git branches isolated while sharing one Beads issue truth. Worktrees use `.beads/redirect` to point back to the main checkout's `.beads` directory; the redirect file is local-only and must not be committed.

Validate the current worktree map before dispatching parallel agents:

```powershell
bd worktree list
```

Expected shape:

- the main checkout reports `shared`
- phase worktrees report `redirect -> Videra`
- `bd context --json` from a worktree reports `is_redirected: true`, `is_worktree: true`, database `Videra`, and the same project id as the main checkout

Use this minimal check from any candidate worktree:

```powershell
bd context --json
bd ready --json
```

Branch/worktree ownership is still Git-local: each agent owns its assigned files and branch, avoids reverting unrelated edits, and coordinates task state through Beads. Beads does not merge branches, resolve conflicts, or decide release readiness.

## Issue Lifecycle and Handoff

Use one Beads issue per bounded task or phase. The normal lifecycle is:

1. `bd ready --json` to find unblocked work.
2. `bd update <id> --claim --json` before editing files.
3. `bd create ... --deps discovered-from:<parent-id> --json` for follow-up work found during implementation.
4. `bd close <id> --reason "..." --json` after verification or intentional deferral.
5. `bd export -o .beads/issues.jsonl` when issue state should be visible in the Git checkout.

Session handoff has two state channels:

- Git stores source, docs, scripts, and the `.beads/issues.jsonl` export.
- Dolt stores live Beads issue state. Use `bd vc status` and `bd vc commit -m "..."` for local Dolt history, and `bd dolt push` only after a Beads remote is configured.

Phase 276 recorded a lifecycle proof in `eng/beads-lifecycle-proof.json`: issue `Videra-mnx` was created and claimed, follow-up `Videra-4yl` was created with `discovered-from:Videra-mnx`, and the follow-up was closed with a concrete reason after Docker-backed database observation.

## Sync and Export

Beads writes issue state to Dolt. Use Dolt sync only when a remote is configured for the Beads database:

```powershell
bd vc status
bd vc commit -m "record coordination state"
bd dolt push
```

The tracked `.beads/issues.jsonl` file is an interoperability export of the current issue state. Regenerate it after meaningful issue lifecycle changes:

```powershell
bd export -o .beads/issues.jsonl
```

Do not commit runtime files such as `.beads/export-state.json`, local locks, embedded Dolt data, credentials, logs, or per-machine redirect files.
