namespace Videra.SurfaceCharts.Processing;

/// <summary>
/// Describes the validated header for a surface cache file.
/// </summary>
public sealed class SurfaceCacheHeader
{
    /// <summary>
    /// Gets the expected cache magic identifier.
    /// </summary>
    public const string ExpectedMagic = "VIDERA_SURFACE_CACHE";

    /// <summary>
    /// Gets the supported cache format version.
    /// </summary>
    public const int CurrentVersion = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceCacheHeader"/> class.
    /// </summary>
    /// <param name="magic">The cache magic identifier.</param>
    /// <param name="version">The cache format version.</param>
    /// <param name="tileCount">The number of tiles in the cache file.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="magic"/> is blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="version"/> or <paramref name="tileCount"/> is negative.</exception>
    public SurfaceCacheHeader(string magic, int version, int tileCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(magic);
        ArgumentOutOfRangeException.ThrowIfNegative(version);
        ArgumentOutOfRangeException.ThrowIfNegative(tileCount);

        Magic = magic;
        Version = version;
        TileCount = tileCount;
    }

    /// <summary>
    /// Gets the cache magic identifier.
    /// </summary>
    public string Magic { get; }

    /// <summary>
    /// Gets the cache format version.
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Gets the number of tile records in the file.
    /// </summary>
    public int TileCount { get; }

    /// <summary>
    /// Validates that the header matches the supported cache format.
    /// </summary>
    /// <exception cref="InvalidDataException">Thrown when the header does not describe a supported cache file.</exception>
    public void Validate()
    {
        if (!string.Equals(Magic, ExpectedMagic, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Surface cache header has an invalid magic value.");
        }

        if (Version != CurrentVersion)
        {
            throw new InvalidDataException(
                $"Surface cache header version '{Version}' is not supported. Expected version '{CurrentVersion}'.");
        }
    }
}
