using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Scene;

/// <summary>
/// Retained scene-document truth for one first-version instance batch.
/// Runtime rendering and picking consumption are wired in a later phase.
/// </summary>
public sealed class InstanceBatchEntry
{
    internal InstanceBatchEntry(SceneEntryId entryId, InstanceBatchDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        EntryId = entryId;
        Name = descriptor.Name;
        Mesh = descriptor.Mesh;
        Material = descriptor.Material;
        Transforms = descriptor.Transforms;
        Colors = descriptor.Colors;
        ObjectIds = descriptor.ObjectIds;
        Pickable = descriptor.Pickable;
        Bounds = descriptor.Bounds;
    }

    public SceneEntryId EntryId { get; }

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
}
