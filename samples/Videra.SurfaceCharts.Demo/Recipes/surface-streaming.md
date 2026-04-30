# Surface Streaming

## Recipe: SurfaceDataLogger3D

Demonstrates live-streaming facade for surface matrices with append/replace/FIFO semantics.

```csharp
var matrix = new SurfaceMatrix(metadata, initialValues);
var logger = new SurfaceDataLogger3D(matrix, fifoRowCapacity: 100);

// Append new rows
logger.Append(newRows);
Console.WriteLine($"Rows: {logger.RowCount}, Appended: {logger.TotalAppendedRowCount}");

// Replace entire matrix
logger.Replace(newMatrix);

// FIFO: oldest rows auto-trimmed when capacity exceeded
logger.Append(moreRows);
Console.WriteLine($"Dropped: {logger.LastDroppedRowCount}");
```

## Features

- Append new rows to existing matrix
- Replace entire matrix with new data
- FIFO row-cap with automatic oldest-row trimming
- Live counters for batches, rows, and dropped rows
