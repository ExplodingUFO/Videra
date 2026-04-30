using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// A grid container that arranges multiple <see cref="VideraChartView"/> instances
/// in an N×M subplot layout with optional shared camera/axis linking.
/// </summary>
public sealed class MultiPlot3D : Grid, IDisposable
{
    private readonly VideraChartView[,] _cells;
    private readonly SurfaceChartWorkspace _workspace;
    private readonly List<SurfaceChartLinkGroup> _linkGroups = [];
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiPlot3D"/> class with the specified grid dimensions.
    /// </summary>
    /// <param name="rows">The number of rows. Must be positive.</param>
    /// <param name="cols">The number of columns. Must be positive.</param>
    public MultiPlot3D(int rows, int cols)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cols);

        Rows = rows;
        Columns = cols;
        _cells = new VideraChartView[rows, cols];
        _workspace = new SurfaceChartWorkspace();

        // Define grid rows and columns
        for (var r = 0; r < rows; r++)
        {
            RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
        }

        for (var c = 0; c < cols; c++)
        {
            ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        }

        // Create and place chart views
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                var chart = new VideraChartView();
                SetRow(chart, r);
                SetColumn(chart, c);
                Children.Add(chart);
                _cells[r, c] = chart;

                var chartId = $"[{r},{c}]";
                _workspace.Register(chart, new SurfaceChartPanelInfo(chartId, $"Cell {chartId}", Plot3DSeriesKind.Surface));
            }
        }
    }

    /// <summary>
    /// Gets the number of rows in the grid.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Gets the number of columns in the grid.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets the total number of cells in the grid.
    /// </summary>
    public int CellCount => Rows * Columns;

    /// <summary>
    /// Gets the <see cref="VideraChartView"/> at the specified grid position.
    /// </summary>
    /// <param name="row">The zero-based row index.</param>
    /// <param name="col">The zero-based column index.</param>
    /// <returns>The chart view at the specified position.</returns>
    public VideraChartView this[int row, int col]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfNegative(col);
            if (row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
            if (col >= Columns) throw new ArgumentOutOfRangeException(nameof(col));
            return _cells[row, col];
        }
    }

    /// <summary>
    /// Gets the <see cref="Plot3D"/> model at the specified grid position.
    /// </summary>
    public Plot3D GetPlot(int row, int col) => this[row, col].Plot;

    /// <summary>
    /// Gets the workspace that tracks all cell charts.
    /// </summary>
    public SurfaceChartWorkspace Workspace => _workspace;

    /// <summary>
    /// Creates a link group across all cells with the specified policy.
    /// </summary>
    /// <param name="policy">The link policy (camera-only, axis-only, or full view state).</param>
    /// <returns>The created link group.</returns>
    public SurfaceChartLinkGroup LinkAll(SurfaceChartLinkPolicy policy = SurfaceChartLinkPolicy.CameraOnly)
    {
        var group = new SurfaceChartLinkGroup(policy);
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                group.Add(_cells[r, c]);
            }
        }

        _workspace.RegisterLinkGroup(group);
        _linkGroups.Add(group);
        return group;
    }

    /// <summary>
    /// Creates a link group across the specified row with the given policy.
    /// </summary>
    /// <param name="row">The zero-based row index.</param>
    /// <param name="policy">The link policy.</param>
    /// <returns>The created link group.</returns>
    public SurfaceChartLinkGroup LinkRow(int row, SurfaceChartLinkPolicy policy = SurfaceChartLinkPolicy.CameraOnly)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        if (row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));

        var group = new SurfaceChartLinkGroup(policy);
        for (var c = 0; c < Columns; c++)
        {
            group.Add(_cells[row, c]);
        }

        _workspace.RegisterLinkGroup(group);
        _linkGroups.Add(group);
        return group;
    }

    /// <summary>
    /// Creates a link group across the specified column with the given policy.
    /// </summary>
    /// <param name="col">The zero-based column index.</param>
    /// <param name="policy">The link policy.</param>
    /// <returns>The created link group.</returns>
    public SurfaceChartLinkGroup LinkColumn(int col, SurfaceChartLinkPolicy policy = SurfaceChartLinkPolicy.CameraOnly)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(col);
        if (col >= Columns) throw new ArgumentOutOfRangeException(nameof(col));

        var group = new SurfaceChartLinkGroup(policy);
        for (var r = 0; r < Rows; r++)
        {
            group.Add(_cells[r, col]);
        }

        _workspace.RegisterLinkGroup(group);
        _linkGroups.Add(group);
        return group;
    }

    /// <summary>
    /// Captures the entire grid as a single PNG image.
    /// </summary>
    /// <param name="width">The output image width in pixels. Defaults to 1920.</param>
    /// <param name="height">The output image height in pixels. Defaults to 1080.</param>
    /// <param name="scale">The DPI scale factor. Defaults to 1.0.</param>
    /// <returns>The path to the saved PNG file.</returns>
    public async Task<string> CaptureSnapshotAsync(int width = 1920, int height = 1080, double scale = 1.0)
    {
        var pixelWidth = (int)(width * scale);
        var pixelHeight = (int)(height * scale);

        var bitmap = new RenderTargetBitmap(new PixelSize(pixelWidth, pixelHeight), new Vector(96 * scale, 96 * scale));

        if (Dispatcher.UIThread.CheckAccess())
        {
            bitmap.Render(this);
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() => bitmap.Render(this));
        }

        var path = Path.Combine(Path.GetTempPath(), $"multiplot3d_{Guid.NewGuid():N}.png");
        await using var stream = File.Create(path);
        bitmap.Save(stream);
        return path;
    }

    /// <summary>
    /// Resets the camera for all cells to fit their data.
    /// </summary>
    public void FitAllToData()
    {
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                _cells[r, c].FitToData();
            }
        }
    }

    /// <summary>
    /// Resets the camera for all cells.
    /// </summary>
    public void ResetAllCameras()
    {
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                _cells[r, c].ResetCamera();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var group in _linkGroups)
        {
            group.Dispose();
        }

        _linkGroups.Clear();
        _workspace.Dispose();
    }
}
