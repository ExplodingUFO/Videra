namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Stores one mutable scatter series through columnar point buffers.
/// </summary>
public sealed class ScatterColumnarSeries
{
    private float[] _x = [];
    private float[] _y = [];
    private float[] _z = [];
    private float[] _size = [];
    private uint[] _color = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterColumnarSeries"/> class.
    /// </summary>
    /// <param name="color">The default ARGB series color.</param>
    /// <param name="label">The optional series label.</param>
    /// <param name="isSortedX">Whether appended data is known to be sorted by X.</param>
    /// <param name="containsNaN">Whether the series accepts NaN gaps in coordinate columns.</param>
    /// <param name="pickable">Whether points in this high-volume path participate in picking.</param>
    /// <param name="fifoCapacity">The optional maximum retained point count.</param>
    public ScatterColumnarSeries(
        uint color,
        string? label = null,
        bool isSortedX = false,
        bool containsNaN = false,
        bool pickable = false,
        int? fifoCapacity = null)
    {
        if (fifoCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fifoCapacity), "FIFO capacity must be positive when specified.");
        }

        Color = color;
        Label = label;
        IsSortedX = isSortedX;
        ContainsNaN = containsNaN;
        Pickable = pickable;
        FifoCapacity = fifoCapacity;
    }

    /// <summary>
    /// Gets the optional series label.
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Gets the default ARGB series color.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets whether appended data is known to be sorted by X.
    /// </summary>
    public bool IsSortedX { get; }

    /// <summary>
    /// Gets whether coordinate columns may contain NaN gaps.
    /// </summary>
    public bool ContainsNaN { get; }

    /// <summary>
    /// Gets whether points in this high-volume path participate in picking.
    /// </summary>
    public bool Pickable { get; }

    /// <summary>
    /// Gets the optional maximum retained point count.
    /// </summary>
    public int? FifoCapacity { get; }

    /// <summary>
    /// Gets the retained point count.
    /// </summary>
    public int Count => _x.Length;

    /// <summary>
    /// Gets the horizontal-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> X => _x;

    /// <summary>
    /// Gets the value-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> Y => _y;

    /// <summary>
    /// Gets the depth-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> Z => _z;

    /// <summary>
    /// Gets the optional per-point marker sizes.
    /// </summary>
    public ReadOnlyMemory<float> Size => _size;

    /// <summary>
    /// Gets the optional per-point ARGB colors.
    /// </summary>
    public ReadOnlyMemory<uint> PointColor => _color;

    /// <summary>
    /// Replaces the retained columns with a new batch.
    /// </summary>
    /// <param name="data">The replacement data.</param>
    public void ReplaceRange(ScatterColumnarData data)
    {
        ValidateData(data);
        var retainedStart = GetRetainedStart(data.Count);
        var retainedCount = data.Count - retainedStart;
        ReplaceFrom(data, retainedStart, retainedCount);
    }

    /// <summary>
    /// Appends a batch and trims the oldest points when <see cref="FifoCapacity"/> is configured.
    /// </summary>
    /// <param name="data">The data to append.</param>
    public void AppendRange(ScatterColumnarData data)
    {
        ValidateData(data);

        var capacity = FifoCapacity;
        if (capacity is null)
        {
            AppendWithoutTrim(data);
            return;
        }

        var retainedCount = Math.Min(_x.Length + data.Count, capacity.Value);
        var nextX = new float[retainedCount];
        var nextY = new float[retainedCount];
        var nextZ = new float[retainedCount];
        var nextSize = ShouldKeepSize(data) ? new float[retainedCount] : [];
        var nextColor = ShouldKeepColor(data) ? new uint[retainedCount] : [];
        InitializeOptionalDefaults(nextSize, nextColor);
        var existingCount = Math.Min(_x.Length, Math.Max(retainedCount - data.Count, 0));
        var existingStart = _x.Length - existingCount;

        if (existingCount > 0)
        {
            Array.Copy(_x, existingStart, nextX, 0, existingCount);
            Array.Copy(_y, existingStart, nextY, 0, existingCount);
            Array.Copy(_z, existingStart, nextZ, 0, existingCount);
            CopyExistingOptional(_size, existingStart, nextSize, existingCount);
            CopyExistingOptional(_color, existingStart, nextColor, existingCount);
        }

        var appendStart = Math.Max(data.Count - (retainedCount - existingCount), 0);
        CopyData(data, appendStart, nextX, nextY, nextZ, nextSize, nextColor, existingCount, retainedCount - existingCount);
        _x = nextX;
        _y = nextY;
        _z = nextZ;
        _size = nextSize;
        _color = nextColor;
    }

    internal float GetSize(int index)
    {
        return _size.Length == 0 ? 1f : _size[index];
    }

    internal uint GetColor(int index)
    {
        return _color.Length == 0 ? Color : _color[index];
    }

    private void AppendWithoutTrim(ScatterColumnarData data)
    {
        var oldCount = _x.Length;
        var nextCount = oldCount + data.Count;
        var nextX = new float[nextCount];
        var nextY = new float[nextCount];
        var nextZ = new float[nextCount];
        var nextSize = ShouldKeepSize(data) ? new float[nextCount] : [];
        var nextColor = ShouldKeepColor(data) ? new uint[nextCount] : [];
        InitializeOptionalDefaults(nextSize, nextColor);

        Array.Copy(_x, nextX, oldCount);
        Array.Copy(_y, nextY, oldCount);
        Array.Copy(_z, nextZ, oldCount);
        CopyExistingOptional(_size, 0, nextSize, oldCount);
        CopyExistingOptional(_color, 0, nextColor, oldCount);
        CopyData(data, 0, nextX, nextY, nextZ, nextSize, nextColor, oldCount, data.Count);

        _x = nextX;
        _y = nextY;
        _z = nextZ;
        _size = nextSize;
        _color = nextColor;
    }

    private void ReplaceFrom(ScatterColumnarData data, int start, int count)
    {
        _x = new float[count];
        _y = new float[count];
        _z = new float[count];
        _size = data.Size.IsEmpty ? [] : new float[count];
        _color = data.Color.IsEmpty ? [] : new uint[count];
        InitializeOptionalDefaults(_size, _color);
        CopyData(data, start, _x, _y, _z, _size, _color, 0, count);
    }

    private void ValidateData(ScatterColumnarData data)
    {
        var x = data.X.Span;
        var y = data.Y.Span;
        var z = data.Z.Span;
        var size = data.Size.Span;

        for (var index = 0; index < data.Count; index++)
        {
            ValidateCoordinate(x[index], nameof(data.X));
            ValidateCoordinate(y[index], nameof(data.Y));
            ValidateCoordinate(z[index], nameof(data.Z));
            if (!data.Size.IsEmpty && (!float.IsFinite(size[index]) || size[index] <= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Point sizes must be positive finite values.");
            }
        }
    }

    private void ValidateCoordinate(float value, string paramName)
    {
        if (float.IsNaN(value) && ContainsNaN)
        {
            return;
        }

        if (!float.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "Columnar scatter coordinates must be finite unless ContainsNaN is enabled.");
        }
    }

    private int GetRetainedStart(int incomingCount)
    {
        return FifoCapacity is { } capacity && incomingCount > capacity
            ? incomingCount - capacity
            : 0;
    }

    private bool ShouldKeepSize(ScatterColumnarData data)
    {
        return _size.Length > 0 || !data.Size.IsEmpty;
    }

    private bool ShouldKeepColor(ScatterColumnarData data)
    {
        return _color.Length > 0 || !data.Color.IsEmpty;
    }

    private void InitializeOptionalDefaults(float[] size, uint[] color)
    {
        if (size.Length > 0)
        {
            Array.Fill(size, 1f);
        }

        if (color.Length > 0)
        {
            Array.Fill(color, Color);
        }
    }

    private static void CopyData(
        ScatterColumnarData data,
        int sourceStart,
        float[] x,
        float[] y,
        float[] z,
        float[] size,
        uint[] color,
        int targetStart,
        int count)
    {
        data.X.Span.Slice(sourceStart, count).CopyTo(x.AsSpan(targetStart, count));
        data.Y.Span.Slice(sourceStart, count).CopyTo(y.AsSpan(targetStart, count));
        data.Z.Span.Slice(sourceStart, count).CopyTo(z.AsSpan(targetStart, count));
        if (!data.Size.IsEmpty && size.Length > 0)
        {
            data.Size.Span.Slice(sourceStart, count).CopyTo(size.AsSpan(targetStart, count));
        }

        if (!data.Color.IsEmpty && color.Length > 0)
        {
            data.Color.Span.Slice(sourceStart, count).CopyTo(color.AsSpan(targetStart, count));
        }
    }

    private static void CopyExistingOptional<T>(T[] source, int sourceStart, T[] target, int count)
    {
        if (source.Length == 0 || target.Length == 0 || count == 0)
        {
            return;
        }

        Array.Copy(source, sourceStart, target, 0, count);
    }
}
