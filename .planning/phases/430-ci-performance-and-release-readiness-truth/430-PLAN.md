---
phase: 430-ci-performance-and-release-readiness-truth
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs
  - tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
autonomous: true
requirements:
  - TRUTH-01
  - TRUTH-02
  - TRUTH-03
  - VERIFY-01
must_haves:
  truths:
    - "CI truth tests verify workspace, linked interaction, and streaming test filters exist"
    - "CI truth tests verify cookbook coverage tests are in the sample evidence step"
    - "Release readiness tests verify v2.64 scope guardrails"
    - "No fake green patterns on validation steps"
  artifacts:
    - path: "tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs"
      provides: "Extended CI truth tests for v2.64 surfaces"
      contains: "Workspace"
    - path: "tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs"
      provides: "Extended release readiness tests"
      contains: "Workspace"
  key_links:
    - from: "SurfaceChartsCiTruthTests.cs"
      to: ".github/workflows/ci.yml"
      via: "tests validate CI workflow contains expected filters"
      pattern: "Run SurfaceCharts"
---

<objective>
Extend CI truth and release-readiness tests for v2.64 surfaces.

Purpose: Phases 426-429 added workspace, linked interaction, streaming, and
cookbook features. Phase 430 ensures CI and release-readiness validation covers
these new surfaces without fake green patterns.

Output: Extended CI truth tests and release-readiness tests.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/430-ci-performance-and-release-readiness-truth/430-CONTEXT.md

<interfaces>
<!-- Key test patterns. -->

From tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs:
```csharp
// Pattern: read CI workflow YAML, find step by name, assert contains test filters
var workflow = ReadWorkflow();
var stepIndex = workflow.IndexOf("Run SurfaceCharts sample evidence", StringComparison.Ordinal);
var step = GetWorkflowStep(workflow, stepIndex);
AssertContainsAll(step, "TestFilter1", "TestFilter2");
AssertDoesNotMaskValidationFailure(step);
```

From .github/workflows/ci.yml (relevant steps):
- "Run SurfaceCharts sample evidence" — runs cookbook/demo/streaming tests
- "Run SurfaceCharts runtime evidence" — runs integration tests
- "Run SurfaceCharts generated roadmap and scope evidence" — runs roadmap/scope tests
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Extend CI truth tests for v2.64 surfaces</name>
  <files>
    tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs
  </files>
  <read_first>
    tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs,
    .github/workflows/ci.yml
  </read_first>
  <behavior>
    - CI sample evidence step includes workspace test filter
    - CI sample evidence step includes linked interaction test filter
    - CI sample evidence step includes streaming evidence test filter
    - CI runtime evidence step includes workspace integration test filter
    - No fake green patterns on new filters
  </behavior>
  <action>
Modify `SurfaceChartsCiTruthTests.cs`:

1. In `CiWorkflow_ShouldRunFocusedSurfaceChartsSampleEvidenceWithoutFakeGreen`, add to the `AssertContainsAll` call:
   - `"SurfaceChartWorkspaceTests"` — workspace contract tests
   - `"SurfaceChartLinkGroupTests"` — link group tests
   - `"SurfaceChartInteractionPropagatorTests"` — propagator tests
   - `"SurfaceChartStreamingEvidenceTests"` — streaming evidence tests

2. In `CiWorkflow_ShouldRunSurfaceChartsRuntimeEvidenceWithoutFakeGreen`, add to the `AssertContainsAll` call:
   - `"SurfaceChartWorkspaceTests"` — workspace integration tests
   - `"SurfaceChartLinkGroupTests"` — link group integration tests

These additions verify that CI actually runs the v2.64 test suites.
  </action>
  <verify>
    <automated>dotnet test tests/Videra.Core.Tests/ --filter "FullyQualifiedName~SurfaceChartsCiTruthTests" --no-restore 2>&amp;1 | tail -10</automated>
  </verify>
  <acceptance_criteria>
    - grep -q "SurfaceChartWorkspaceTests" tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs
    - grep -q "SurfaceChartLinkGroupTests" tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs
    - grep -q "SurfaceChartInteractionPropagatorTests" tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs
    - All CI truth tests pass
  </acceptance_criteria>
  <done>
    CI truth tests verify v2.64 workspace, linked interaction, and streaming tests are in CI.
    No fake green patterns on validation steps.
  </done>
</task>

<task type="auto" tdd="true">
  <name>Task 2: Extend release-readiness tests for v2.64 scope</name>
  <files>
    tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
  </files>
  <read_first>
    tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs,
    scripts/Test-SnapshotExportScope.ps1
  </read_first>
  <behavior>
    - Release readiness tests verify no generic workbench claims
    - Release readiness tests verify no compatibility adapter claims
    - Release readiness tests verify no hidden propagation state claims
  </behavior>
  <action>
Read `RepositoryReleaseReadinessTests.cs` to understand the existing pattern.

If the file already has scope guardrail tests, add assertions for:
- No "workbench" claims in support documentation
- No "compatibility" or "adapter" claims
- No "hidden" propagation or fallback claims

If the file doesn't have scope tests, add a new test method:
```csharp
[Fact]
public void Documentation_ShouldNotClaimGenericWorkbenchOrCompatibilityAdapters()
{
    var readme = File.ReadAllText("README.md");
    var releaseCutover = File.ReadAllText("docs/surfacecharts-release-cutover.md");

    readme.Should().NotContain("generic workbench");
    readme.Should().NotContain("compatibility adapter");
    readme.Should().NotContain("hidden fallback");

    releaseCutover.Should().NotContain("generic workbench");
    releaseCutover.Should().NotContain("compatibility adapter");
}
```

The exact implementation depends on the existing file structure.
  </action>
  <verify>
    <automated>dotnet test tests/Videra.Core.Tests/ --filter "FullyQualifiedName~RepositoryReleaseReadinessTests" --no-restore 2>&amp;1 | tail -10</automated>
  </verify>
  <acceptance_criteria>
    - Release readiness tests include v2.64 scope guardrails
    - All release readiness tests pass
  </acceptance_criteria>
  <done>
    Release-readiness tests verify v2.64 scope boundaries.
    No generic workbench, compatibility, or hidden fallback claims.
  </done>
</task>

</tasks>

<threat_model>
## Trust Boundaries

| Boundary | Description |
|----------|-------------|
| tests→CI | CI truth tests validate workflow YAML content |

## STRIDE Threat Register

| Threat ID | Category | Component | Disposition | Mitigation Plan |
|-----------|----------|-----------|-------------|-----------------|
| T-430-01 | Tampering | CI workflow | accept | Tests read workflow file; cannot modify it |
| T-430-02 | Elevation of Privilege | Release readiness | accept | Tests are read-only validation |
</threat_model>

<verification>
1. `dotnet test tests/Videra.Core.Tests/ --filter "FullyQualifiedName~SurfaceChartsCiTruthTests"` — all pass
2. `dotnet test tests/Videra.Core.Tests/ --filter "FullyQualifiedName~RepositoryReleaseReadinessTests"` — all pass
3. CI truth tests include v2.64 test filters
</verification>

<success_criteria>
- CI truth tests verify workspace, linked interaction, and streaming tests are in CI
- Release-readiness tests verify v2.64 scope boundaries
- No fake green patterns on validation steps
- All tests pass
</success_criteria>

<output>
After completion, create `.planning/phases/430-ci-performance-and-release-readiness-truth/430-SUMMARY.md`
</output>
