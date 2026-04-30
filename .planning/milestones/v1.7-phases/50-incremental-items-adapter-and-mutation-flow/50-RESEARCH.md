# Phase 50 Research

## Key Context

1. The old `Items` binding path still rebuilt the entire scene document too often, even after document truth became authoritative.
2. Avalonia collection events belong behind a dedicated adapter so runtime scene code does not grow another batch of event-parsing logic.
3. Incremental mutation had to preserve entry identity so later delta planning would stay meaningful.
