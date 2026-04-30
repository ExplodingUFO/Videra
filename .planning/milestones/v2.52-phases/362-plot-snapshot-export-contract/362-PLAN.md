---
phase: 362-plot-snapshot-export-contract
plan: 01
type: execute
wave: 1
depends_on:
  - 361-01
files_modified:
  - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotFormat.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotBackground.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs
  - tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj
  - tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs
autonomous: true
requirements:
  - SNAP-01
  - SNAP-02
  - SNAP-03
  - SNAP-04
  - VER-01
  - VER-02
  - VER-03

must_haves:
  truths:
    - "PlotSnapshotRequest can be constructed with valid Width, Height, Scale, Background, Format"
    - "PlotSnapshotResult can represent success (with Path + Manifest) and failure (with Diagnostic)"
    - "PlotSnapshotManifest carries deterministic metadata: Width, Height, OutputEvidenceKind, DatasetEvidenceKind, ActiveSeriesIdentity, Format, Background, CreatedUtc"
    - "Invalid requests (zero dimensions, null path, unsupported format) produce PlotSnapshotDiagnostic with explicit diagnostic code"
    - "Types live in Videra.SurfaceCharts.Avalonia.Controls namespace with no dependency on Videra.Avalonia or Videra.Core"
  artifacts:
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotFormat.cs"
      provides: "Format enum with Png value"
      contains: "PlotSnapshotFormat"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotBackground.cs"
      provides: "Background enum with Transparent and Opaque values"
      contains: "PlotSnapshotBackground"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs"
      provides: "Diagnostic type for explicit error reporting"
      contains: "PlotSnapshotDiagnostic"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs"
      provides: "Request type capturing dimensions, scale, background, format"
      contains: "PlotSnapshotRequest"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs"
      provides: "Result type with Path, Manifest, Succeeded, Failure, Duration"
      contains: "PlotSnapshotResult"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs"
      provides: "Manifest type with deterministic metadata"
      contains: "PlotSnapshotManifest"
    - path: "tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs"
      provides: "Unit tests for contract construction, validation, diagnostics"
      min_lines: 80
  key_links:
    - from: "PlotSnapshotRequest"
      to: "PlotSnapshotFormat"
      via: "Format property"
      pattern: "PlotSnapshotFormat\\.Png"
    - from: "PlotSnapshotRequest"
      to: "PlotSnapshotBackground"
      via: "Background property"
      pattern: "PlotSnapshotBackground\\."
    - from: "PlotSnapshotResult"
      to: "PlotSnapshotManifest"
      via: "Manifest property"
      pattern: "PlotSnapshotManifest"
    - from: "PlotSnapshotResult"
      to: "PlotSnapshotDiagnostic"
      via: "Failure property on failed result"
      pattern: "PlotSnapshotDiagnostic"
---

<objective>
Add the Plot-owned snapshot request/result contract types for Phase 362.

Purpose: Define the bounded chart-local snapshot API surface that Phase 363 will implement. Contract types capture dimensions, scale/DPI, background behavior, and target semantics without exposing backend internals. Results include deterministic manifest metadata linked to output and dataset evidence. Unsupported formats and invalid requests return explicit diagnostics rather than fallback behavior.

Output: Six new contract types in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/` plus unit tests in `tests/Videra.SurfaceCharts.Core.Tests/`.
</objective>

<execution_context>
@$HOME/.config/opencode/get-shit-done/workflows/execute-plan.md
@$HOME/.config/opencode/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/phases/362-plot-snapshot-export-contract/362-CONTEXT.md
@.planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md

<interfaces>
<!-- Key types and patterns the executor must follow. Extracted from codebase. -->

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs — REFERENCE PATTERN:
```csharp
// Pattern: internal ctor, public properties, ArgumentNullException validation
public sealed class Plot3DOutputEvidence
{
    internal Plot3DOutputEvidence(
        int seriesCount,
        int activeSeriesIndex,
        string? activeSeriesName,
        Plot3DSeriesKind? activeSeriesKind,
        string? activeSeriesIdentity,
        Plot3DColorMapStatus colorMapStatus,
        SurfaceChartOutputEvidence? colorMapEvidence,
        string precisionProfile,
        Plot3DRenderingEvidence? renderingEvidence,
        IReadOnlyList<Plot3DOutputCapabilityDiagnostic> outputCapabilityDiagnostics)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(seriesCount);
        ArgumentException.ThrowIfNullOrWhiteSpace(precisionProfile);
        ArgumentNullException.ThrowIfNull(outputCapabilityDiagnostics);
        // ...
    }

    public string EvidenceKind { get; }
    // ... more public properties
}

// Diagnostic pattern:
public sealed class Plot3DOutputCapabilityDiagnostic
{
    internal Plot3DOutputCapabilityDiagnostic(
        string capability,
        bool isSupported,
        string diagnosticCode,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(capability);
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnosticCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        // ...
    }

    public string Capability { get; }
    public bool IsSupported { get; }
    public string DiagnosticCode { get; }
    public string Message { get; }
}

// Enum pattern:
public enum Plot3DSeriesKind { Surface, Waterfall, Scatter }
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs:
```csharp
// EvidenceKind pattern:
public string EvidenceKind { get; } = "Plot3DDatasetEvidence";

// Create factory pattern:
internal static Plot3DDatasetEvidence Create(
    int plotRevision,
    IReadOnlyList<Plot3DSeries> series,
    SurfaceChartOverlayOptions overlayOptions)
{
    ArgumentNullException.ThrowIfNull(series);
    ArgumentNullException.ThrowIfNull(overlayOptions);
    // ...
}
```

From tests/Videra.SurfaceCharts.Core.Tests/SurfaceChartOutputEvidenceTests.cs — TEST PATTERN:
```csharp
using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceChartOutputEvidenceTests
{
    [Fact]
    public void Create_ReportsPaletteStopsAndPrecisionProfile()
    {
        // arrange + act + assert with FluentAssertions
        evidence.EvidenceKind.Should().Be("SurfaceChartOutputEvidence");
    }
}
```

</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create snapshot contract types</name>
  <files>
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotFormat.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotBackground.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs
  </files>
  <read_first>
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs
  </read_first>
  <action>
Create six new files in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/` using the existing `Plot3DOutputEvidence` pattern (internal ctor, public properties, `ArgumentNullException` validation). All types in namespace `Videra.SurfaceCharts.Avalonia.Controls`. No dependency on Videra.Avalonia or Videra.Core — chart-local only. Per D-01 through D-07 from CONTEXT.md.

**1. PlotSnapshotFormat.cs** — enum, single value:
```csharp
namespace Videra.SurfaceCharts.Avalonia.Controls;

public enum PlotSnapshotFormat
{
    Png,
}
```

**2. PlotSnapshotBackground.cs** — enum, two values:
```csharp
namespace Videra.SurfaceCharts.Avalonia.Controls;

public enum PlotSnapshotBackground
{
    Transparent,
    Opaque,
}
```

**3. PlotSnapshotDiagnostic.cs** — diagnostic type following `Plot3DOutputCapabilityDiagnostic` pattern:
- `internal` ctor with `(string diagnosticCode, string message)`
- Validation: `ArgumentException.ThrowIfNullOrWhiteSpace` for both params
- Public properties: `DiagnosticCode` (string), `Message` (string)
- Static factory: `internal static PlotSnapshotDiagnostic Create(string diagnosticCode, string message)` — returns new instance

**4. PlotSnapshotRequest.cs** — request type:
- `public` ctor (not internal — user-facing API per SNAP-01) with `(int width, int height, double scale, PlotSnapshotBackground background, PlotSnapshotFormat format)`
- Validation:
  - `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width)`
  - `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height)`
  - `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(scale)`
- Public properties: `Width` (int), `Height` (int), `Scale` (double), `Background` (PlotSnapshotBackground), `Format` (PlotSnapshotFormat)
- No output path — result carries path (per CONTEXT.md: result includes Path)

**5. PlotSnapshotManifest.cs** — manifest type following `Plot3DOutputEvidence` pattern:
- `internal` ctor with `(int width, int height, string outputEvidenceKind, string datasetEvidenceKind, string activeSeriesIdentity, PlotSnapshotFormat format, PlotSnapshotBackground background, DateTime createdUtc)`
- Validation:
  - `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width)`
  - `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height)`
  - `ArgumentException.ThrowIfNullOrWhiteSpace(outputEvidenceKind)`
  - `ArgumentException.ThrowIfNullOrWhiteSpace(datasetEvidenceKind)`
  - `ArgumentException.ThrowIfNullOrWhiteSpace(activeSeriesIdentity)`
- Public properties (all read-only): `Width`, `Height`, `OutputEvidenceKind`, `DatasetEvidenceKind`, `ActiveSeriesIdentity`, `Format`, `Background`, `CreatedUtc`

**6. PlotSnapshotResult.cs** — result type:
- `internal` ctor with `(bool succeeded, string? path, PlotSnapshotManifest? manifest, PlotSnapshotDiagnostic? failure, TimeSpan duration)`
- Validation:
  - When `succeeded == true`: `path` must not be null/whitespace, `manifest` must not be null
  - When `succeeded == false`: `failure` must not be null
  - Throw `ArgumentException` if success invariants violated (message: "Successful result requires path and manifest." / "Failed result requires diagnostic.")
- Public properties (all read-only): `Succeeded` (bool), `Path` (string?), `Manifest` (PlotSnapshotManifest?), `Failure` (PlotSnapshotDiagnostic?), `Duration` (TimeSpan)
- Static factories:
  - `internal static PlotSnapshotResult Success(string path, PlotSnapshotManifest manifest, TimeSpan duration)`
  - `internal static PlotSnapshotResult Failed(PlotSnapshotDiagnostic failure, TimeSpan duration)`

**Key conventions to follow:**
- Use file-scoped namespaces (`namespace X;`)
- XML doc comments on all public types and members
- Use `ArgumentNullException.ThrowIfNull` / `ArgumentOutOfRangeException.ThrowIfNegativeOrZero` / `ArgumentException.ThrowIfNullOrWhiteSpace` for validation (matching existing patterns)
- Use expression-bodied members for simple property getters where the existing code does so
- Private backing fields → public get-only properties pattern (matching Plot3DOutputEvidence)
  </action>
  <verify>
    <automated>dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj --no-restore 2>&amp;1 | Select-String -Pattern "error|Build succeeded"</automated>
  </verify>
  <acceptance_criteria>
    - `grep -r "PlotSnapshotFormat" src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotFormat.cs` returns the enum with `Png` value
    - `grep -r "PlotSnapshotBackground" src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotBackground.cs` returns the enum with `Transparent` and `Opaque` values
    - `grep -r "PlotSnapshotDiagnostic" src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs` returns the diagnostic class with `DiagnosticCode` and `Message` properties
    - `grep -r "PlotSnapshotRequest" src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs` returns the request class with `Width`, `Height`, `Scale`, `Background`, `Format` properties
    - `grep -r "PlotSnapshotResult" src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs` returns the result class with `Succeeded`, `Path`, `Manifest`, `Failure`, `Duration` properties and `Success`/`Failed` factories
    - `grep -r "PlotSnapshotManifest" src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs` returns the manifest class with `Width`, `Height`, `OutputEvidenceKind`, `DatasetEvidenceKind`, `ActiveSeriesIdentity`, `Format`, `Background`, `CreatedUtc` properties
    - `grep -r "namespace Videra.SurfaceCharts.Avalonia.Controls;" src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshot*.cs` confirms all files use chart-local namespace
    - Build succeeds with zero errors
  </acceptance_criteria>
  <done>
    Six contract types exist in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/` with correct namespace, internal constructors (except PlotSnapshotRequest which is public), validation, and XML doc comments. Build compiles cleanly.
  </done>
</task>

<task type="auto">
  <name>Task 2: Write contract unit tests</name>
  <files>
    tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj,
    tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs
  </files>
  <read_first>
    tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj,
    tests/Videra.SurfaceCharts.Core.Tests/SurfaceChartOutputEvidenceTests.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs
  </read_first>
  <action>
First, add a `<ProjectReference>` to `Videra.SurfaceCharts.Core.Tests.csproj`:
```xml
<ProjectReference Include="..\..\src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj" />
```

Then create `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs` with these test methods using xunit + FluentAssertions (matching existing test patterns):

**Request construction tests:**
- `Request_ConstructsWithValidParameters` — verify all properties round-trip correctly: `new PlotSnapshotRequest(1920, 1080, 2.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png)` → Width=1920, Height=1080, Scale=2.0, Background=Transparent, Format=Png
- `Request_RejectsZeroWidth` — `new PlotSnapshotRequest(0, 1080, 2.0, ...)` → throws `ArgumentOutOfRangeException`
- `Request_RejectsZeroHeight` — `new PlotSnapshotRequest(1920, 0, 2.0, ...)` → throws `ArgumentOutOfRangeException`
- `Request_RejectsZeroScale` — `new PlotSnapshotRequest(1920, 1080, 0.0, ...)` → throws `ArgumentOutOfRangeException`
- `Request_RejectsNegativeWidth` — `new PlotSnapshotRequest(-1, 1080, 2.0, ...)` → throws `ArgumentOutOfRangeException`

**Result construction tests:**
- `Result_Success_FactoriesCreatesSuccessResult` — `PlotSnapshotResult.Success(path, manifest, duration)` → Succeeded=true, Path set, Manifest set, Failure=null
- `Result_Failed_FactoryCreatesFailedResult` — `PlotSnapshotResult.Failed(diagnostic, duration)` → Succeeded=false, Path=null, Manifest=null, Failure set
- `Result_Success_RequiresPath` — `PlotSnapshotResult.Success(null!, manifest, duration)` → throws `ArgumentException` with message "Successful result requires path and manifest."
- `Result_Success_RequiresManifest` — `PlotSnapshotResult.Success(path, null!, duration)` → throws `ArgumentException`
- `Result_Failed_RequiresDiagnostic` — `PlotSnapshotResult.Failed(null!, duration)` → throws `ArgumentException`

**Manifest construction tests:**
- `Manifest_ConstructsWithValidParameters` — verify all 8 properties: Width, Height, OutputEvidenceKind, DatasetEvidenceKind, ActiveSeriesIdentity, Format, Background, CreatedUtc
- `Manifest_RejectsZeroWidth` — throws `ArgumentOutOfRangeException`
- `Manifest_RejectsWhitespaceOutputEvidenceKind` — throws `ArgumentException`
- `Manifest_RejectsWhitespaceDatasetEvidenceKind` — throws `ArgumentException`
- `Manifest_RejectsWhitespaceActiveSeriesIdentity` — throws `ArgumentException`

**Diagnostic construction tests:**
- `Diagnostic_ConstructsWithValidParameters` — verify DiagnosticCode and Message round-trip
- `Diagnostic_RejectsWhitespaceCode` — throws `ArgumentException`
- `Diagnostic_RejectsWhitespaceMessage` — throws `ArgumentException`

**Evidence kind linkage tests:**
- `Manifest_OutputEvidenceKind_MatchesPlot3DOutputEvidenceConvention` — verify `OutputEvidenceKind` value `"plot-3d-output"` matches the established evidence kind
- `Manifest_DatasetEvidenceKind_MatchesPlot3DDatasetEvidenceConvention` — verify `DatasetEvidenceKind` value `"Plot3DDatasetEvidence"` matches the established evidence kind
  </action>
  <verify>
    <automated>dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~PlotSnapshotContract" --no-restore 2>&amp;1 | Select-String -Pattern "Passed|Failed|error|Build succeeded"</automated>
  </verify>
  <acceptance_criteria>
    - `grep -c "\[Fact\]" tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs` returns count >= 15
    - `grep "PlotSnapshotContractTests" tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs` confirms class exists
    - `grep "ProjectReference.*SurfaceCharts.Avalonia" tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj` confirms project reference added
    - All tests pass with zero failures
    - Build succeeds with zero errors
  </acceptance_criteria>
  <done>
    Unit tests cover request construction/validation, result success/failure factory behavior, manifest construction/validation, diagnostic construction/validation, and evidence kind linkage. All tests pass. Test project references SurfaceCharts.Avalonia.
  </done>
</task>

</tasks>

<verification>
1. All six contract type files exist in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/`
2. All types use namespace `Videra.SurfaceCharts.Avalonia.Controls`
3. `PlotSnapshotRequest` has public ctor; all other types have internal ctors
4. Validation uses `ArgumentNullException.ThrowIfNull`, `ArgumentOutOfRangeException.ThrowIfNegativeOrZero`, `ArgumentException.ThrowIfNullOrWhiteSpace`
5. `PlotSnapshotResult` has static factories `Success()` and `Failed()` with invariant enforcement
6. `PlotSnapshotManifest` has all 8 required properties: Width, Height, OutputEvidenceKind, DatasetEvidenceKind, ActiveSeriesIdentity, Format, Background, CreatedUtc
7. Unit tests pass: `dotnet test tests/Videra.SurfaceCharts.Core.Tests/ --filter "PlotSnapshotContract" --no-restore`
8. Build succeeds: `dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj --no-restore`
</verification>

<success_criteria>
- SNAP-01: Users can construct `PlotSnapshotRequest` and `PlotSnapshotResult` through the Plot-owned contract types
- SNAP-02: `PlotSnapshotRequest` captures Width, Height, Scale, Background, Format without backend internals
- SNAP-03: `PlotSnapshotManifest` includes deterministic metadata: Width, Height, OutputEvidenceKind, DatasetEvidenceKind, ActiveSeriesIdentity, Format, Background, CreatedUtc
- SNAP-04: Invalid requests (zero/negative dimensions, null paths) throw explicit exceptions; failed results carry `PlotSnapshotDiagnostic` with diagnostic code
- VER-01: Beads ownership recorded (Videra-lu9.2)
- VER-02: Implementation runs on isolated branch/worktree
- VER-03: Clean Beads status after completion
</success_criteria>

<output>
After completion, create `.planning/phases/362-plot-snapshot-export-contract/SUMMARY.md`
</output>
