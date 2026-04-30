# Waterfall Streaming

## Recipe: WaterfallDataLogger3D

Demonstrates WaterfallDataLogger3D delegating to SurfaceDataLogger3D for row streaming.

```csharp
var matrix = new SurfaceMatrix(metadata, initialValues);
var logger = new WaterfallDataLogger3D(matrix, fifoRowCapacity: 50);

// Same API as SurfaceDataLogger3D
logger.Append(newRows);
logger.Replace(newMatrix);
Console.WriteLine($"Rows: {logger.RowCount}");
```

## Features

- Same streaming API as SurfaceDataLogger3D
- Delegates internally for matrix operations
- FIFO row-cap support
- Compatible with waterfall chart rendering
