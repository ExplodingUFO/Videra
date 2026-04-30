# Phase 214 Summary

## Completed

- Added `DemoSupportReportBuilder` for copyable diagnostics bundle, import report, and minimal reproduction metadata.
- Added Demo support UI with `Copy Diagnostics Bundle`, `Copy Repro Metadata`, and a read-only import report.
- Cached the latest backend diagnostics, render capabilities, and load result in `MainWindowViewModel`.
- Included package versions, OS/runtime information, render capabilities, backend diagnostics, demo settings, loaded model count, import diagnostics, timing, and asset metrics in support output.
- Added `ModelLoadFileResult.AssetMetrics` so batch imports can report metrics for imported files even when atomic scene replacement is skipped after a partial failure.
- Updated Demo README and focused Core tests for the support diagnostics contract.

## Commit

- `68aa8b4 demo: add support diagnostics bundle`

