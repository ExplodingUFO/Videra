---
phase: 431-v264-final-verification
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - .planning/ROADMAP.md
  - .planning/STATE.md
autonomous: true
requirements:
  - VERIFY-01
  - VERIFY-02
must_haves:
  truths:
    - "All v2.64 focused tests pass or are explicitly reported"
    - "Beads state reflects all phases closed"
    - "Generated roadmap is synchronized"
    - "Git status is clean and pushed to remote"
  artifacts:
    - path: ".planning/ROADMAP.md"
      provides: "Updated roadmap with all phases complete"
      contains: "Complete"
    - path: ".planning/STATE.md"
      provides: "Final state with milestone complete"
      contains: "Complete"
  key_links:
    - from: ".planning/ROADMAP.md"
      to: "Beads state"
      via: "roadmap reflects bead completion"
      pattern: "Complete"
---

<objective>
Close v2.64 with synchronized verification and handoff.

Purpose: All implementation phases (426-430) are complete. Phase 431 runs
verification, closes beads, updates roadmap, and pushes to remote.

Output: Verified milestone, closed beads, synchronized roadmap, clean git state.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/phases/431-v264-final-verification/431-CONTEXT.md
</context>

<tasks>

<task type="auto">
  <name>Task 1: Run verification commands</name>
  <files></files>
  <read_first></read_first>
  <behavior>
    - All workspace tests pass
    - All linked interaction tests pass
    - All streaming evidence tests pass
    - All cookbook coverage tests pass
    - All CI truth tests pass
    - Build succeeds for all projects
  </behavior>
  <action>
Run verification commands and record results:

```bash
# Build verification
dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj --no-restore
dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj --no-restore

# Test verification
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/ --filter "FullyQualifiedName~Workspace" --no-restore
dotnet test tests/Videra.Core.Tests/ --filter "FullyQualifiedName~SurfaceChartsCiTruthTests" --no-restore
dotnet test tests/Videra.Core.Tests/ --filter "FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests" --no-restore
```

Record pass/fail for each command. If any fail, report and do not proceed.
  </action>
  <verify>
    <automated>echo "Verification complete"</automated>
  </verify>
  <acceptance_criteria>
    - All build commands succeed
    - All test commands pass
  </acceptance_criteria>
  <done>
    All verification commands pass. Build and test suites are green.
  </done>
</task>

<task type="auto">
  <name>Task 2: Close beads and update roadmap</name>
  <files>
    .planning/ROADMAP.md,
    .planning/STATE.md
  </files>
  <read_first>
    .planning/ROADMAP.md,
    .planning/STATE.md
  </read_first>
  <behavior>
    - All v2.64 beads are closed
    - Roadmap shows all phases complete
    - State shows milestone complete
  </behavior>
  <action>
1. Close remaining beads:
   ```bash
   bd close Videra-7tqx.4 --reason="Phase 428 complete: streaming evidence"
   bd close Videra-7tqx.5 --reason="Phase 429 complete: cookbook recipes"
   bd close Videra-7tqx.6 --reason="Phase 430 complete: CI truth tests"
   bd close Videra-7tqx.7 --reason="Phase 431 complete: final verification"
   bd close Videra-7tqx --reason="v2.64 milestone complete"
   ```

2. Update ROADMAP.md progress table — mark all phases Complete.

3. Update STATE.md — status: complete, all phases done.
  </action>
  <verify>
    <automated>bd stats 2>/dev/null | head -10</automated>
  </verify>
  <acceptance_criteria>
    - All beads are closed
    - Roadmap shows all phases complete
    - State shows milestone complete
  </acceptance_criteria>
  <done>
    All beads closed. Roadmap and state reflect milestone completion.
  </done>
</task>

<task type="auto">
  <name>Task 3: Commit, push, and verify clean state</name>
  <files></files>
  <read_first></read_first>
  <behavior>
    - All changes committed
    - Git pushed to remote
    - Git status clean
  </behavior>
  <action>
1. Stage and commit any remaining changes:
   ```bash
   git add -f .planning/
   git commit -m "docs(431): close v2.64 milestone"
   ```

2. Push to remote:
   ```bash
   git pull --rebase
   git push
   ```

3. Verify clean state:
   ```bash
   git status
   ```
  </action>
  <verify>
    <automated>git status --short</automated>
  </verify>
  <acceptance_criteria>
    - Git status is clean
    - All commits pushed to remote
  </acceptance_criteria>
  <done>
    v2.64 milestone is complete. All changes committed and pushed.
  </done>
</task>

</tasks>

<verification>
1. All focused tests pass
2. All beads are closed
3. Roadmap shows all phases complete
4. Git status is clean and pushed
</verification>

<success_criteria>
- All v2.64 verification commands pass
- All beads closed with proper reasons
- Roadmap synchronized with bead state
- Git pushed to remote
- Clean handoff for next milestone
</success_criteria>

<output>
After completion, create `.planning/phases/431-v264-final-verification/431-SUMMARY.md`
</output>
