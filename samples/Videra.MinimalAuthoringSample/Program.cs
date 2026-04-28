using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Scene;

var ground = SceneMaterials.Matte("ground", RgbaFloat.LightGrey);
var focus = SceneMaterials.Metal("focus", new RgbaFloat(0.2f, 0.45f, 0.9f, 1f));
var marker = SceneMaterials.Matte("marker", RgbaFloat.White);

var markerTransforms = new[]
{
    Matrix4x4.CreateTranslation(-1.2f, 0.2f, -0.6f),
    Matrix4x4.CreateTranslation(0f, 0.2f, 0.6f),
    Matrix4x4.CreateTranslation(1.2f, 0.2f, -0.6f)
};

var markerColors = new[]
{
    RgbaFloat.Red,
    RgbaFloat.Green,
    RgbaFloat.Blue
};

var markerObjectIds = new[]
{
    Guid.Parse("7c824263-a9ed-46c0-8b91-e0bf9bb3f00a"),
    Guid.Parse("2b31ebc8-9355-43e7-9ac5-e1f04f7040d8"),
    Guid.Parse("e8a06050-33b4-49d9-8ac3-6189e0f9eaf6")
};

var document = SceneAuthoring.Create("minimal-authoring")
    .AddPlane("ground", ground, width: 4f, depth: 3f)
    .AddGrid("grid", SceneMaterials.Matte("grid", RgbaFloat.DarkGrey), width: 4f, depth: 3f, divisions: 4)
    .AddSphere("focus", focus, radius: 0.35f, transform: Matrix4x4.CreateTranslation(0f, 0.35f, 0f))
    .AddInstances(
        "marker-spheres",
        SceneGeometry.Sphere(radius: 0.12f, segments: 12, rings: 6, color: marker.BaseColorFactor),
        marker,
        markerTransforms,
        markerColors,
        objectIds: markerObjectIds,
        pickable: true)
    .Build();

var importedAsset = document.Entries.Single().ImportedAsset
    ?? throw new InvalidOperationException("The authored scene did not produce imported asset truth.");
var markerBatch = document.InstanceBatches.Single();

Console.WriteLine($"Scene: {importedAsset.Name}");
Console.WriteLine($"Document entries: {document.Entries.Count}");
Console.WriteLine($"Nodes: {importedAsset.Nodes.Count}");
Console.WriteLine($"Mesh primitives: {importedAsset.Primitives.Count}");
Console.WriteLine($"Materials: {importedAsset.Materials.Count}");
Console.WriteLine($"Instance batches: {document.InstanceBatches.Count}");
Console.WriteLine($"Marker instances: {markerBatch.InstanceCount}");
Console.WriteLine($"Marker object ids: {string.Join(", ", markerBatch.ObjectIds.ToArray())}");
