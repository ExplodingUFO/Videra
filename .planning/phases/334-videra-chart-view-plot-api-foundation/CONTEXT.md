# Phase 334 Context: VideraChartView Plot API Foundation

## Bead

- `Videra-hen`

## Scope

Introduce the single public chart View and the first `Plot.Add.Surface(...)`, `Plot.Add.Waterfall(...)`, and `Plot.Add.Scatter(...)` authoring API.

## Constraints

- No compatibility wrapper around old chart View components.
- No internalized old View implementation as the new architecture.
- No hidden fallback/downshift path.
- No backend expansion or new chart family.
- Keep the implementation split by responsibility.

## Handoff From Phase 333

Phase 333 found no existing `VideraChartView` implementation and confirmed the old public chart Views are broad source/test/sample/doc references that should be deleted or migrated in later phases.
