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

## Explicit Validation

Run the coordination validation only when you are intentionally checking the local Beads service. Normal product builds, package validation, release dry runs, and CI workflows do not start or require the Beads Docker service.

```powershell
pwsh -File ./scripts/Test-BeadsCoordination.ps1
```

The script checks `bd context --json`, `bd doctor`, `bd worktree list`, and Docker-backed Dolt metadata for the configured `Videra` database. It does not build product code, publish packages, create tags, mutate feeds, merge branches, or replace GSD phase planning.

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

If Docker-hosted Dolt can write local Beads commits but the container cannot complete GitHub HTTPS TLS handshakes, use the host-relay push script. The script fetches the current `refs/dolt/data` on the host, lets Docker Dolt push into a temporary local Git remote, then fast-forwards `refs/dolt/data` to GitHub with the host Git client:

```powershell
pwsh -File ./scripts/Push-BeadsDoltViaHost.ps1
```

The tracked `.beads/issues.jsonl` file is an interoperability export of the current issue state. Regenerate it after meaningful issue lifecycle changes:

```powershell
bd export -o .beads/issues.jsonl
```

Do not commit runtime files such as `.beads/export-state.json`, local locks, embedded Dolt data, credentials, logs, or per-machine redirect files.

## Public Roadmap Export

`docs/ROADMAP.generated.md` is a deterministic, read-only public summary generated from the checked-in `.beads/issues.jsonl` snapshot. It exists so external readers can see active and ready roadmap work without a local Beads service.

Regenerate it after exporting Beads state:

```powershell
pwsh -File ./scripts/Export-BeadsRoadmap.ps1
```

Do not edit the generated roadmap by hand, and do not use it as a parallel tracker. Beads remains the single task authority.
