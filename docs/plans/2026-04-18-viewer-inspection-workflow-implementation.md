# Viewer Inspection Workflow Implementation Plan

**Goal:** Add clipping, measurement, view-state persistence, and snapshot export to `VideraView` without expanding the public engine-level extensibility surface.

**Architecture:** Keep `VideraView` as the public shell and reuse the existing scene residency/upload/runtime seams. Clipping is implemented as source-payload-derived active payloads, measurements reuse picking plus overlay projection, and snapshot export uses a software export path so the artifact matches the current inspection truth across backends.

**Tech Stack:** Avalonia, Videra.Core scene/runtime infrastructure, xUnit, FluentAssertions, existing render/runtime diagnostics shell.

---

### Task 1: Add clipping contract tests

**Files:**
- Create: `tests/Videra.Core.Tests/Inspection/VideraClipPlaneTests.cs`
- Create: `tests/Videra.Core.IntegrationTests/Rendering/VideraViewClippingIntegrationTests.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`

**Step 1: Write failing tests**
- verify a clip plane class/record exists and can be enabled/disabled
- verify applying clipping changes a scene object's active payload bounds without destroying source payload truth
- verify `VideraView` exposes a typed clipping property

**Step 2: Run tests to confirm failure**

Run:
`dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~VideraClipPlaneTests"`

**Step 3: Implement clipping primitives and runtime wiring**
- add core clip plane types and payload clipping service
- extend object/runtime state to preserve source payload and derived clipped payload
- route `VideraView.ClippingPlanes` through runtime and scene residency reupload

**Step 4: Run focused tests**

**Step 5: Refactor**
- keep `VideraViewRuntime` thin; move clipping mechanics into dedicated helpers/services

### Task 2: Add measurement workflow tests

**Files:**
- Create: `tests/Videra.Core.Tests/Inspection/VideraMeasurementTests.cs`
- Create: `tests/Videra.Avalonia.Tests/Controls/VideraMeasurementOverlayTests.cs`
- Modify: `src/Videra.Avalonia/Controls/Interaction/VideraInteractionController.cs`

**Step 1: Write failing tests**
- verify distance and height-delta measurement records
- verify measurement overlay labels/projected segments are created
- verify a new interaction mode can create measurements

**Step 2: Run tests to confirm failure**

**Step 3: Implement measurement models, overlay state, and runtime/public contract**

**Step 4: Run focused tests**

**Step 5: Refactor**

### Task 3: Add view-state and snapshot export tests

**Files:**
- Create: `tests/Videra.Core.Tests/Cameras/VideraViewStateTests.cs`
- Create: `tests/Videra.Core.IntegrationTests/Rendering/VideraSnapshotExportIntegrationTests.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.Camera.cs`
- Modify: `src/Videra.Avalonia/Rendering/RenderSession.cs`

**Step 1: Write failing tests**
- verify camera/view-state round-trips
- verify snapshot export produces an image artifact and reflects overlays/clipping truth

**Step 2: Run tests to confirm failure**

**Step 3: Implement view-state record, capture/restore APIs, and software export path**

**Step 4: Run focused tests**

**Step 5: Refactor**

### Task 4: Close samples, docs, diagnostics, and guards

**Files:**
- Modify: `samples/Videra.MinimalSample/**`
- Modify: `README.md`
- Modify: `src/Videra.Avalonia/README.md`
- Modify: `docs/alpha-feedback.md`
- Modify: repository/sample guard tests under `tests/Videra.Core.Tests/Repository`

**Step 1: Write failing repo/sample guard tests**
- enforce docs mention the inspection happy path
- enforce diagnostics snapshot includes inspection truth

**Step 2: Run tests to confirm failure**

**Step 3: Update docs, sample, diagnostics formatter, and consumer-facing validation**

**Step 4: Run focused tests**

**Step 5: Run full verification**

Run:
- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release`
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release`
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release`
- `pwsh -File ./scripts/verify.ps1 -Configuration Release`
