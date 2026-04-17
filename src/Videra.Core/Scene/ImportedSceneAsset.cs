using Videra.Core.Geometry;
using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed record ImportedSceneAsset(
    string FilePath,
    string Name,
    MeshData MeshData);
