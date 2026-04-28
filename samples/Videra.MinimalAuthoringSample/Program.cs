using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Scene;

var focusPlacement = SceneAuthoringPlacement.From(
    new Vector3(0f, 0.35f, 0f),
    Quaternion.Identity,
    new Vector3(1f, 1.2f, 1f));
var outlineColor = new RgbaFloat(1f, 0.85f, 0.2f, 1f);
var focus = SceneMaterials.Metal("focus", new RgbaFloat(0.2f, 0.45f, 0.9f, 1f));
var marker = SceneMaterials.Emissive("marker-glow", RgbaFloat.White, intensity: 1.25f);

var markerPlacements = new[]
{
    SceneAuthoringPlacement.At(-1.2f, 0.2f, -0.6f),
    SceneAuthoringPlacement.At(0f, 0.2f, 0.6f),
    SceneAuthoringPlacement.At(1.2f, 0.2f, -0.6f)
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

var result = SceneAuthoring.Create("minimal-authoring")
    .AddPlane("ground", RgbaFloat.LightGrey, SceneAuthoringPlacement.Identity, width: 4f, depth: 3f)
    .AddGrid("grid", SceneMaterials.Matte("grid", RgbaFloat.DarkGrey), width: 4f, depth: 3f, divisions: 4)
    .AddAxisTriad("world", length: 0.75f)
    .AddScaleBar(
        "one-meter",
        SceneMaterials.Matte("measure", RgbaFloat.White),
        length: 1f,
        tickHeight: 0.1f,
        transform: Matrix4x4.CreateTranslation(-1.8f, 0.03f, 1.1f))
    .AddSphere("focus", focus, focusPlacement, radius: 0.35f)
    .AddMesh(
        "focus-bounds",
        SceneGeometry.BoxOutline(width: 0.9f, height: 0.9f, depth: 0.9f, outlineColor),
        SceneMaterials.Emissive("focus-bounds", outlineColor, intensity: 0.8f),
        SceneAuthoringPlacement.At(0f, 0.35f, 0f))
    .AddInstances(
        "marker-spheres",
        SceneGeometry.Sphere(radius: 0.12f, segments: 12, rings: 6, color: marker.BaseColorFactor),
        marker,
        markerPlacements,
        markerColors,
        objectIds: markerObjectIds,
        pickable: true)
    .TryBuild();

IReadOnlyList<SceneAuthoringDiagnostic> diagnostics = result.Diagnostics;

foreach (var diagnostic in diagnostics)
{
    Console.WriteLine($"Diagnostic {diagnostic.Severity}: {diagnostic.Code} {diagnostic.Target} {diagnostic.Message}");
}

var document = result.Document
    ?? throw new InvalidOperationException("The authored scene did not pass explicit diagnostics.");

var retainedAsset = document.Entries.Single().ImportedAsset
    ?? throw new InvalidOperationException("The authored scene did not produce retained scene truth.");
var markerBatch = document.InstanceBatches.Single();

Console.WriteLine($"Scene: {retainedAsset.Name}");
Console.WriteLine($"Document entries: {document.Entries.Count}");
Console.WriteLine($"Nodes: {retainedAsset.Nodes.Count}");
Console.WriteLine($"Mesh primitives: {retainedAsset.Primitives.Count}");
Console.WriteLine($"Materials: {retainedAsset.Materials.Count}");
Console.WriteLine($"Instance batches: {document.InstanceBatches.Count}");
Console.WriteLine($"Marker instances: {markerBatch.InstanceCount}");
Console.WriteLine($"Marker object ids: {string.Join(", ", markerBatch.ObjectIds.ToArray())}");
