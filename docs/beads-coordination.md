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

