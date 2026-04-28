using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Scene;

public sealed class SceneAuthoringBuilder
{
    private readonly List<AuthoredMesh> _meshes = [];
    private readonly List<InstanceBatchDescriptor> _instanceBatches = [];

    internal SceneAuthoringBuilder(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public SceneAuthoringBuilder AddMesh(
        string name,
        MeshData meshData,
        MaterialInstance material,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        ArgumentNullException.ThrowIfNull(meshData);
        ArgumentNullException.ThrowIfNull(material);

        _meshes.Add(new AuthoredMesh(
            name,
            meshData,
            material,
            transform ?? Matrix4x4.Identity,
            nodeId ?? SceneNodeId.New(),
            primitiveId ?? MeshPrimitiveId.New()));
        return this;
    }

    public SceneAuthoringBuilder AddMesh(
        string name,
        MeshData meshData,
        MaterialInstance material,
        SceneAuthoringPlacement placement,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, meshData, material, placement.ToMatrix(), nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddTriangle(
        string name,
        MaterialInstance material,
        float size = 1f,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.Triangle(size, material.BaseColorFactor), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddQuad(
        string name,
        MaterialInstance material,
        float width = 1f,
        float height = 1f,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.Quad(width, height, material.BaseColorFactor), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddPlane(
        string name,
        MaterialInstance material,
        float width = 1f,
        float depth = 1f,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.Plane(width, depth, material.BaseColorFactor), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddPlane(
        string name,
        MaterialInstance material,
        SceneAuthoringPlacement placement,
        float width = 1f,
        float depth = 1f,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddPlane(name, material, width, depth, placement.ToMatrix(), nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddPlane(
        string name,
        RgbaFloat color,
        SceneAuthoringPlacement placement,
        float width = 1f,
        float depth = 1f,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddPlane(name, SceneMaterials.Matte(name, color), placement, width, depth, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddGrid(
        string name,
        MaterialInstance material,
        float width = 1f,
        float depth = 1f,
        int divisions = 10,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.Grid(width, depth, divisions, material.BaseColorFactor), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddPolyline(
        string name,
        MaterialInstance material,
        ReadOnlySpan<Vector3> points,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.Polyline(points, material.BaseColorFactor), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddAxisTriad(
        string name,
        float length = 1f,
        Matrix4x4? transform = null)
    {
        AddMesh(
            $"{name}-x-axis",
            SceneGeometry.AxisLine(Vector3.UnitX, length, RgbaFloat.Red),
            SceneMaterials.Matte($"{name}-x-axis", RgbaFloat.Red),
            transform);
        AddMesh(
            $"{name}-y-axis",
            SceneGeometry.AxisLine(Vector3.UnitY, length, RgbaFloat.Green),
            SceneMaterials.Matte($"{name}-y-axis", RgbaFloat.Green),
            transform);
        AddMesh(
            $"{name}-z-axis",
            SceneGeometry.AxisLine(Vector3.UnitZ, length, RgbaFloat.Blue),
            SceneMaterials.Matte($"{name}-z-axis", RgbaFloat.Blue),
            transform);
        return this;
    }

    public SceneAuthoringBuilder AddScaleBar(
        string name,
        MaterialInstance material,
        float length,
        float tickHeight,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.ScaleBar(length, tickHeight, material.BaseColorFactor), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddPointCloud(
        string name,
        MaterialInstance material,
        ReadOnlySpan<Vector3> points,
        ReadOnlySpan<RgbaFloat> colors = default,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.PointCloud(points, colors), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddCube(
        string name,
        MaterialInstance material,
        float size = 1f,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.Cube(size, material.BaseColorFactor), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddCube(
        string name,
        MaterialInstance material,
        SceneAuthoringPlacement placement,
        float size = 1f,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddCube(name, material, size, placement.ToMatrix(), nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddCube(
        string name,
        RgbaFloat color,
        SceneAuthoringPlacement placement,
        float size = 1f,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddCube(name, SceneMaterials.Matte(name, color), placement, size, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddSphere(
        string name,
        MaterialInstance material,
        float radius = 0.5f,
        int segments = 16,
        int rings = 8,
        Matrix4x4? transform = null,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddMesh(name, SceneGeometry.Sphere(radius, segments, rings, material.BaseColorFactor), material, transform, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddSphere(
        string name,
        MaterialInstance material,
        SceneAuthoringPlacement placement,
        float radius = 0.5f,
        int segments = 16,
        int rings = 8,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddSphere(name, material, radius, segments, rings, placement.ToMatrix(), nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddSphere(
        string name,
        RgbaFloat color,
        SceneAuthoringPlacement placement,
        float radius = 0.5f,
        int segments = 16,
        int rings = 8,
        SceneNodeId? nodeId = null,
        MeshPrimitiveId? primitiveId = null)
    {
        return AddSphere(name, SceneMaterials.Matte(name, color), placement, radius, segments, rings, nodeId, primitiveId);
    }

    public SceneAuthoringBuilder AddInstances(
        string name,
        MeshData meshData,
        MaterialInstance material,
        ReadOnlyMemory<Matrix4x4> transforms,
        ReadOnlyMemory<RgbaFloat> colors = default,
        ReadOnlyMemory<Guid> objectIds = default,
        bool pickable = true,
        MeshPrimitiveId? primitiveId = null)
    {
        ArgumentNullException.ThrowIfNull(meshData);
        ArgumentNullException.ThrowIfNull(material);

        var mesh = new MeshPrimitive(primitiveId ?? MeshPrimitiveId.New(), name, meshData, material.Id);
        return AddInstanceBatch(new InstanceBatchDescriptor(name, mesh, material, transforms, colors, objectIds, pickable));
    }

    public SceneAuthoringBuilder AddInstances(
        string name,
        MeshData meshData,
        MaterialInstance material,
        ReadOnlyMemory<SceneAuthoringPlacement> placements,
        ReadOnlyMemory<RgbaFloat> colors = default,
        ReadOnlyMemory<Guid> objectIds = default,
        bool pickable = true,
        MeshPrimitiveId? primitiveId = null)
    {
        var transforms = new Matrix4x4[placements.Length];
        for (var i = 0; i < placements.Length; i++)
        {
            transforms[i] = placements.Span[i].ToMatrix();
        }

        return AddInstances(name, meshData, material, transforms, colors, objectIds, pickable, primitiveId);
    }

    public SceneAuthoringBuilder AddInstanceBatch(InstanceBatchDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        _instanceBatches.Add(descriptor);
        return this;
    }

    public IReadOnlyList<SceneAuthoringDiagnostic> Validate()
    {
        var diagnostics = new List<SceneAuthoringDiagnostic>();
        ValidateScene(diagnostics);
        ValidateMeshes(diagnostics);
        return diagnostics;
    }

    public SceneAuthoringResult TryBuild()
    {
        var diagnostics = Validate();
        if (diagnostics.Any(static diagnostic => diagnostic.Severity == SceneAuthoringDiagnosticSeverity.Error))
        {
            return new SceneAuthoringResult(document: null, diagnostics);
        }

        return new SceneAuthoringResult(BuildDocument(), diagnostics);
    }

    public SceneDocument Build()
    {
        var result = TryBuild();
        if (result.Document is null)
        {
            var messages = string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}"));
            throw new InvalidOperationException($"Scene authoring failed validation:{Environment.NewLine}{messages}");
        }

        return result.Document;
    }

    private void ValidateScene(ICollection<SceneAuthoringDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            diagnostics.Add(Error("scene.name", "Scene name is required."));
        }

        if (_meshes.Count == 0 && _instanceBatches.Count == 0)
        {
            diagnostics.Add(Error("scene.empty", "Scene must contain at least one mesh or instance batch."));
        }
    }

    private void ValidateMeshes(ICollection<SceneAuthoringDiagnostic> diagnostics)
    {
        var nodeIds = new HashSet<SceneNodeId>();
        var primitiveIds = new HashSet<MeshPrimitiveId>();
        foreach (var mesh in _meshes)
        {
            if (string.IsNullOrWhiteSpace(mesh.Name))
            {
                diagnostics.Add(Error("mesh.name", "Mesh name is required."));
            }

            if (!nodeIds.Add(mesh.NodeId))
            {
                diagnostics.Add(Error("node.id.duplicate", $"Node id '{mesh.NodeId.Value}' is used more than once.", mesh.Name));
            }

            if (!primitiveIds.Add(mesh.PrimitiveId))
            {
                diagnostics.Add(Error("mesh.id.duplicate", $"Mesh primitive id '{mesh.PrimitiveId.Value}' is used more than once.", mesh.Name));
            }

            ValidateMeshData(mesh, diagnostics);
        }
    }

    private static void ValidateMeshData(AuthoredMesh mesh, ICollection<SceneAuthoringDiagnostic> diagnostics)
    {
        if (mesh.MeshData.Vertices.Length == 0)
        {
            diagnostics.Add(Error("mesh.vertices.empty", "Mesh must contain at least one vertex.", mesh.Name));
            return;
        }

        if (mesh.MeshData.Topology == MeshTopology.Triangles && mesh.MeshData.Indices.Length % 3 != 0)
        {
            diagnostics.Add(Error("mesh.indices.triangles", "Triangle meshes must use an index count divisible by three.", mesh.Name));
        }

        if (mesh.MeshData.Topology == MeshTopology.Lines && mesh.MeshData.Indices.Length % 2 != 0)
        {
            diagnostics.Add(Error("mesh.indices.lines", "Line meshes must use an index count divisible by two.", mesh.Name));
        }

        for (var i = 0; i < mesh.MeshData.Vertices.Length; i++)
        {
            var vertex = mesh.MeshData.Vertices[i];
            if (!IsFinite(vertex.Position) || !IsFinite(vertex.Normal) || !IsFinite(vertex.Color))
            {
                diagnostics.Add(Error("mesh.vertex.nonfinite", $"Vertex {i} contains a non-finite position, normal, or color value.", mesh.Name));
                break;
            }
        }

        for (var i = 0; i < mesh.MeshData.Indices.Length; i++)
        {
            if (mesh.MeshData.Indices[i] >= mesh.MeshData.Vertices.Length)
            {
                diagnostics.Add(Error("mesh.index.range", $"Index {i} references vertex {mesh.MeshData.Indices[i]}, outside the vertex buffer.", mesh.Name));
                break;
            }
        }

        foreach (var textureCoordinateSet in mesh.MeshData.TextureCoordinateSets)
        {
            if (textureCoordinateSet.Coordinates.Length != mesh.MeshData.Vertices.Length)
            {
                diagnostics.Add(Error("mesh.uv.length", $"UV set {textureCoordinateSet.SetIndex} must match the vertex count.", mesh.Name));
                break;
            }

            foreach (var uv in textureCoordinateSet.Coordinates.Span)
            {
                if (!IsFinite(uv))
                {
                    diagnostics.Add(Error("mesh.uv.nonfinite", $"UV set {textureCoordinateSet.SetIndex} contains a non-finite value.", mesh.Name));
                    break;
                }
            }
        }

        if (!IsFinite(mesh.Transform))
        {
            diagnostics.Add(Error("node.transform.nonfinite", "Node transform must contain only finite matrix values.", mesh.Name));
        }
    }

    private SceneDocument BuildDocument()
    {
        var materials = _meshes
            .Select(static mesh => mesh.Material)
            .DistinctBy(static material => material.Id)
            .ToArray();
        var nodes = new SceneNode[_meshes.Count];
        var primitives = new MeshPrimitive[_meshes.Count];

        for (var i = 0; i < _meshes.Count; i++)
        {
            var mesh = _meshes[i];
            primitives[i] = new MeshPrimitive(mesh.PrimitiveId, mesh.Name, mesh.MeshData, mesh.Material.Id);
            nodes[i] = new SceneNode(mesh.NodeId, mesh.Name, mesh.Transform, parentId: null, [mesh.PrimitiveId]);
        }

        SceneDocument document;
        if (nodes.Length == 0)
        {
            document = SceneDocument.Empty;
        }
        else
        {
            var asset = new ImportedSceneAsset($"{Name}.authored", Name, nodes, primitives, materials);
            var runtimeObjects = SceneObjectFactory.CreateDeferredRuntimeObjects(asset);
            var entry = new SceneDocumentMutator().CreateImportedEntry(runtimeObjects, asset);
            document = new SceneDocument([entry]);
        }

        foreach (var instanceBatch in _instanceBatches)
        {
            document = document.AddInstanceBatch(instanceBatch);
        }

        return document;
    }

    private static SceneAuthoringDiagnostic Error(string code, string message, string? target = null)
    {
        return new SceneAuthoringDiagnostic(SceneAuthoringDiagnosticSeverity.Error, code, message, target);
    }

    private static bool IsFinite(Matrix4x4 matrix)
    {
        return float.IsFinite(matrix.M11)
            && float.IsFinite(matrix.M12)
            && float.IsFinite(matrix.M13)
            && float.IsFinite(matrix.M14)
            && float.IsFinite(matrix.M21)
            && float.IsFinite(matrix.M22)
            && float.IsFinite(matrix.M23)
            && float.IsFinite(matrix.M24)
            && float.IsFinite(matrix.M31)
            && float.IsFinite(matrix.M32)
            && float.IsFinite(matrix.M33)
            && float.IsFinite(matrix.M34)
            && float.IsFinite(matrix.M41)
            && float.IsFinite(matrix.M42)
            && float.IsFinite(matrix.M43)
            && float.IsFinite(matrix.M44);
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }

    private static bool IsFinite(Vector2 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y);
    }

    private static bool IsFinite(RgbaFloat value)
    {
        return float.IsFinite(value.R) && float.IsFinite(value.G) && float.IsFinite(value.B) && float.IsFinite(value.A);
    }

    private sealed record AuthoredMesh(
        string Name,
        MeshData MeshData,
        MaterialInstance Material,
        Matrix4x4 Transform,
        SceneNodeId NodeId,
        MeshPrimitiveId PrimitiveId);
}
