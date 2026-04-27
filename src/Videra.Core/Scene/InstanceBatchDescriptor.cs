using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Scene;

/// <summary>
/// Describes a first-version instance batch: one mesh primitive, one material, and per-instance transforms.
/// Multi-geometry batching, per-instance materials, transparent Blend sorting, GPU-driven culling, indirect draw,
/// and ECS-style ownership are intentionally outside this contract.
/// </summary>
public sealed class InstanceBatchDescriptor
{
    public InstanceBatchDescriptor(
        string name,
        MeshPrimitive mesh,
        MaterialInstance material,
        ReadOnlyMemory<Matrix4x4> transforms,
        ReadOnlyMemory<RgbaFloat> colors = default,
        ReadOnlyMemory<Guid> objectIds = default,
        bool pickable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(material);

        ValidateMeshMaterial(mesh, material);
        ValidateInstanceData(mesh, transforms, colors, objectIds);

        Name = name;
        Mesh = mesh;
        Material = material;
        Transforms = transforms;
        Colors = colors;
        ObjectIds = objectIds;
        Pickable = pickable;
        Bounds = ComputeBounds(mesh, transforms.Span);
    }

    public string Name { get; }

    public MeshPrimitive Mesh { get; }

    public MaterialInstance Material { get; }

    public ReadOnlyMemory<Matrix4x4> Transforms { get; }

    public ReadOnlyMemory<RgbaFloat> Colors { get; }

    public ReadOnlyMemory<Guid> ObjectIds { get; }

    public bool Pickable { get; }

    public int InstanceCount => Transforms.Length;

    public BoundingBox3 Bounds { get; }

    public bool HasPerInstanceColors => !Colors.IsEmpty;

    public bool HasObjectIds => !ObjectIds.IsEmpty;

    private static void ValidateMeshMaterial(MeshPrimitive mesh, MaterialInstance material)
    {
        if (mesh.MaterialId is { } meshMaterialId && meshMaterialId != material.Id)
        {
            throw new ArgumentException("Instance batch material must match the mesh primitive material id.", nameof(material));
        }

        if (material.Alpha.Mode == MaterialAlphaMode.Blend)
        {
            throw new ArgumentException("Instance batches do not support transparent Blend materials in the first version.", nameof(material));
        }
    }

    private static void ValidateInstanceData(
        MeshPrimitive mesh,
        ReadOnlyMemory<Matrix4x4> transforms,
        ReadOnlyMemory<RgbaFloat> colors,
        ReadOnlyMemory<Guid> objectIds)
    {
        if (mesh.MeshData.Vertices.Length == 0)
        {
            throw new ArgumentException("Instance batch mesh must contain at least one vertex.", nameof(mesh));
        }

        if (transforms.IsEmpty)
        {
            throw new ArgumentException("Instance batch transforms must contain at least one transform.", nameof(transforms));
        }

        if (!colors.IsEmpty && colors.Length != transforms.Length)
        {
            throw new ArgumentException("Per-instance colors must be empty or match the transform count.", nameof(colors));
        }

        if (!objectIds.IsEmpty && objectIds.Length != transforms.Length)
        {
            throw new ArgumentException("Per-instance object ids must be empty or match the transform count.", nameof(objectIds));
        }

        foreach (var transform in transforms.Span)
        {
            if (!IsFinite(transform))
            {
                throw new ArgumentException("Instance batch transforms must contain only finite matrix values.", nameof(transforms));
            }
        }

        foreach (var color in colors.Span)
        {
            if (!IsFinite(color))
            {
                throw new ArgumentException("Per-instance colors must contain only finite values.", nameof(colors));
            }
        }

        foreach (var objectId in objectIds.Span)
        {
            if (objectId == Guid.Empty)
            {
                throw new ArgumentException("Per-instance object ids must not contain Guid.Empty.", nameof(objectIds));
            }
        }
    }

    private static BoundingBox3 ComputeBounds(MeshPrimitive mesh, ReadOnlySpan<Matrix4x4> transforms)
    {
        var localBounds = BoundingBox3.FromVertices(mesh.MeshData.Vertices);
        var aggregate = localBounds.Transform(transforms[0]);

        for (var i = 1; i < transforms.Length; i++)
        {
            aggregate = aggregate.Encapsulate(localBounds.Transform(transforms[i]));
        }

        return aggregate;
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

    private static bool IsFinite(RgbaFloat color)
    {
        return float.IsFinite(color.R)
            && float.IsFinite(color.G)
            && float.IsFinite(color.B)
            && float.IsFinite(color.A);
    }
}
