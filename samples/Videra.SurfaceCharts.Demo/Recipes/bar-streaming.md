# Bar Streaming

## Recipe: BarDataLogger3D

Demonstrates BarDataLogger3D with append/replace semantics for bar chart data.

```csharp
var data = new BarChartData(initialSeries);
var logger = new BarDataLogger3D(data);

// Append new series
logger.Append(newSeries);
Console.WriteLine($"Series: {logger.SeriesCount}, Appended: {logger.TotalAppendedSeriesCount}");

// Replace entire data
logger.Replace(newData);
Console.WriteLine($"Replaced: {logger.ReplaceBatchCount}");
```

## Features

- Append new bar series to existing data
- Replace entire bar chart data
- Live counters for batches and series
- Category count tracking
