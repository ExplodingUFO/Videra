namespace Videra.Core.Scene;

public static class SceneAuthoring
{
    public static SceneAuthoringBuilder Create(string name)
    {
        return new SceneAuthoringBuilder(name);
    }
}
