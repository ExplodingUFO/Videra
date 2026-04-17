using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FluentAssertions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneItemsAdapterTests
{
    private readonly SceneDocumentMutator _mutator = new();
    private readonly SceneItemsAdapter _adapter;

    public SceneItemsAdapterTests()
    {
        _adapter = new SceneItemsAdapter(_mutator);
    }

    [Fact]
    public void Add_mutation_updates_document_incrementally()
    {
        var first = new Object3D { Name = "first" };
        var second = new Object3D { Name = "second" };
        var document = _mutator.Add(SceneDocument.Empty, first);

        var change = new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            changedItems: new[] { second },
            startingIndex: 1);

        var next = _adapter.ApplyChange(document, change, new ObservableCollection<Object3D> { first, second });

        next.SceneObjects.Should().ContainInOrder(first, second);
        next.Entries[0].Id.Should().Be(document.Entries[0].Id);
    }

    [Fact]
    public void Move_mutation_only_reorders_entries()
    {
        var first = new Object3D { Name = "first" };
        var second = new Object3D { Name = "second" };
        var document = _mutator.Add(SceneDocument.Empty, first);
        document = _mutator.Add(document, second);

        var change = new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Move,
            changedItem: second,
            index: 0,
            oldIndex: 1);

        var next = _adapter.ApplyChange(document, change, new ObservableCollection<Object3D> { second, first });

        next.SceneObjects.Should().ContainInOrder(second, first);
        next.Entries.Select(static entry => entry.Id).Should().BeEquivalentTo(document.Entries.Select(static entry => entry.Id));
    }
}
