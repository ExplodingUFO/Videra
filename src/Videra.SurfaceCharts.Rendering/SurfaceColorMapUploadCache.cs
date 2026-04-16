using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

internal sealed class SurfaceColorMapUploadCache
{
    private SurfaceColorMapLut? _current;

    public SurfaceColorMapLut Resolve(SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(colorMap);

        if (_current is not null && _current.Matches(colorMap))
        {
            return _current;
        }

        _current = new SurfaceColorMapLut(colorMap);
        return _current;
    }
}
