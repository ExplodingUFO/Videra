using FluentAssertions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneDeltaPlannerTests
{
    private readonly SceneDocumentMutator _mutator = new();
    private readonly SceneDeltaPlanner _planner = new();

    [Fact]
    public void Diff_tracks_added_removed_and_retained_entries_by_entry_id()
    {
        var first = new Object3D { Name = "first" };
        var second = new Object3D { Name = "second" };
        var third = new Object3D { Name = "third" };

        var previous = _mutator.Add(SceneDocument.Empty, first);
        previous = _mutator.Add(previous, second);

        var next = _mutator.ReplaceEntries(previous, [previous.Entries[1], _mutator.CreateExternalEntry(third)]);

        var delta = _planner.Diff(previous, next);

        delta.Added.Should().ContainSingle().Which.SceneObject.Should().BeSameAs(third);
        delta.Removed.Should().ContainSingle().Which.SceneObject.Should().BeSameAs(first);
        delta.Retained.Should().ContainSingle().Which.SceneObject.Should().BeSameAs(second);
    }
}
