namespace Videra.Core.Graphics.RenderPipeline;

[Flags]
public enum RenderFeatureSet
{
    None = 0,
    Opaque = 1 << 0,
    Transparent = 1 << 1,
    Overlay = 1 << 2,
    Picking = 1 << 3,
    Screenshot = 1 << 4
}

public static class RenderFeatureSetExtensions
{
    public static IReadOnlyList<string> ToFeatureNames(this RenderFeatureSet features)
    {
        var names = new List<string>(capacity: 5);

        AppendIfPresent(names, features, RenderFeatureSet.Opaque);
        AppendIfPresent(names, features, RenderFeatureSet.Transparent);
        AppendIfPresent(names, features, RenderFeatureSet.Overlay);
        AppendIfPresent(names, features, RenderFeatureSet.Picking);
        AppendIfPresent(names, features, RenderFeatureSet.Screenshot);

        return names;
    }

    private static void AppendIfPresent(List<string> names, RenderFeatureSet features, RenderFeatureSet candidate)
    {
        if ((features & candidate) != 0)
        {
            names.Add(candidate.ToString());
        }
    }
}
