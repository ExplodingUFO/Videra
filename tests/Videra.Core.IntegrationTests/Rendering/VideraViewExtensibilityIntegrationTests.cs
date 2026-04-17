using System.Collections;
using System.Reflection;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Software;
using Videra.Core.Selection.Annotations;
using Videra.Core.Selection.Rendering;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraViewExtensibilityIntegrationTests
{
    [Fact]
    public void VideraView_ExposesPublicEngineExtensibilityAndCapabilityQuery()
    {
        var view = new VideraView();
        try
        {
            using var backend = new SoftwareBackend();
            backend.Initialize(IntPtr.Zero, 200, 200);

            var observed = new List<string>();
            view.Engine.RegisterFrameHook(RenderFrameHookPoint.FrameBegin, context => observed.Add(context.HookPoint.ToString()));
            view.Engine.RegisterPassContributor(
                RenderPassSlot.SolidGeometry,
                new RecordingContributor(context => observed.Add(context.Slot.ToString())));

            view.Engine.Initialize(backend);
            view.Engine.Resize(200, 200);
            view.Engine.Grid.IsVisible = false;
            view.Engine.ShowAxis = false;
            view.Engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

            view.Engine.Draw();

            observed.Should().Equal("FrameBegin", "SolidGeometry");
            view.RenderCapabilities.SupportsPassContributors.Should().BeTrue();
            view.RenderCapabilities.SupportsPassReplacement.Should().BeTrue();
            view.RenderCapabilities.SupportsFrameHooks.Should().BeTrue();
            view.RenderCapabilities.SupportsPipelineSnapshots.Should().BeTrue();
            view.RenderCapabilities.ActiveBackendPreference.Should().Be(GraphicsBackendPreference.Software);
            view.RenderCapabilities.LastPipelineSnapshot.Should().NotBeNull();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void BackendDiagnostics_ExposeCapabilityProjectionFields()
    {
        var view = new VideraView();
        try
        {
            view.BackendDiagnostics.SupportsPassContributors.Should().BeTrue();
            view.BackendDiagnostics.SupportsPassReplacement.Should().BeTrue();
            view.BackendDiagnostics.SupportsFrameHooks.Should().BeTrue();
            view.BackendDiagnostics.SupportsPipelineSnapshots.Should().BeTrue();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void RenderCapabilities_AndBackendDiagnostics_AreQueryableBeforeInitialization()
    {
        var view = new VideraView();
        try
        {
            view.RenderCapabilities.IsInitialized.Should().BeFalse();
            view.RenderCapabilities.SupportsPassContributors.Should().BeTrue();
            view.RenderCapabilities.SupportsPassReplacement.Should().BeTrue();
            view.RenderCapabilities.SupportsFrameHooks.Should().BeTrue();
            view.RenderCapabilities.SupportsPipelineSnapshots.Should().BeTrue();
            view.RenderCapabilities.LastPipelineSnapshot.Should().BeNull();

            view.BackendDiagnostics.IsReady.Should().BeFalse();
            view.BackendDiagnostics.IsUsingSoftwareFallback.Should().BeFalse();
            view.BackendDiagnostics.FallbackReason.Should().BeNull();
            view.BackendDiagnostics.SupportsPassContributors.Should().BeTrue();
            view.BackendDiagnostics.SupportsPassReplacement.Should().BeTrue();
            view.BackendDiagnostics.SupportsFrameHooks.Should().BeTrue();
            view.BackendDiagnostics.SupportsPipelineSnapshots.Should().BeTrue();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ControlledInteractionState_IsQueryableBeforeInitialization_AndAfterEngineDisposal()
    {
        var view = new VideraView();
        try
        {
            ReadSelectionObjectIds(view).Should().BeEmpty();
            ReadPrimaryObjectId(view).Should().BeNull();
            ReadAnnotations(view).Should().BeEmpty();
            ReadProperty(view, "InteractionMode").ToString().Should().Be("Navigate");
            ReadProperty(view, "InteractionOptions").Should().NotBeNull();

            var objectId = Guid.NewGuid();
            var selectionState = CreateSelectionState(objectId);
            var nodeAnnotation = CreateNodeAnnotation(objectId);
            var interactionOptions = Activator.CreateInstance(GetInteractionType("VideraInteractionOptions"));
            var navigateMode = Enum.Parse(GetInteractionType("VideraInteractionMode"), "Navigate");

            WriteProperty(view, "SelectionState", selectionState);
            WriteProperty(view, "Annotations", CreateAnnotations(nodeAnnotation));
            WriteProperty(view, "InteractionMode", navigateMode);
            WriteProperty(view, "InteractionOptions", interactionOptions);

            ReadSelectionObjectIds(view).Should().ContainSingle().Which.Should().Be(objectId);
            ReadPrimaryObjectId(view).Should().Be(objectId);
            ReadAnnotations(view).Should().ContainSingle().Which.Should().BeSameAs(nodeAnnotation);
            ReadProperty(view, "InteractionMode").Should().Be(navigateMode);
            ReadProperty(view, "InteractionOptions").Should().BeSameAs(interactionOptions);

            view.Engine.Dispose();

            ReadSelectionObjectIds(view).Should().ContainSingle().Which.Should().Be(objectId);
            ReadPrimaryObjectId(view).Should().Be(objectId);
            ReadAnnotations(view).Should().ContainSingle().Which.Should().BeSameAs(nodeAnnotation);
            ReadProperty(view, "InteractionMode").Should().Be(navigateMode);
            ReadProperty(view, "InteractionOptions").Should().BeSameAs(interactionOptions);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ControlledInteractionIntentEventArgs_ExposeHostFacingSelectionAndAnchorData()
    {
        var objectId = Guid.NewGuid();
        var selectionRequest = CreateSelectionRequest(objectId);
        var replacementObjectId = Guid.NewGuid();

        var selectionArgs = Activator.CreateInstance(GetInteractionType("SelectionRequestedEventArgs"), selectionRequest);
        selectionArgs.Should().NotBeNull();
        ReadRequestedObjectIds(selectionArgs!).Should().ContainSingle().Which.Should().Be(objectId);
        ReadRequestedPrimaryObjectId(selectionArgs!).Should().Be(objectId);
        ReadProperty(selectionArgs!, "Operation").ToString().Should().Be("Replace");
        ReadProperty(selectionArgs!, "EmptySpaceSelectionBehavior").ToString().Should().Be("ClearSelection");
        AssertObjectIdsAreReadOnly(ReadProperty(selectionArgs!, "ObjectIds"), replacementObjectId);

        var projectedRequest = ReadProperty(selectionArgs!, "Request");
        ReadRequestedObjectIds(projectedRequest).Should().ContainSingle().Which.Should().Be(objectId);
        ReadRequestedPrimaryObjectId(projectedRequest).Should().Be(objectId);
        ReadProperty(projectedRequest, "Operation").ToString().Should().Be("Replace");
        ReadProperty(projectedRequest, "EmptySpaceSelectionBehavior").ToString().Should().Be("ClearSelection");
        AssertObjectIdsAreReadOnly(ReadProperty(projectedRequest, "ObjectIds"), replacementObjectId);

        ReadRequestedObjectIds(selectionArgs!).Should().ContainSingle().Which.Should().Be(objectId);
        ReadRequestedPrimaryObjectId(selectionArgs!).Should().Be(objectId);
        ReadRequestedObjectIds(ReadProperty(selectionArgs!, "Request")).Should().ContainSingle().Which.Should().Be(objectId);
        ReadRequestedPrimaryObjectId(ReadProperty(selectionArgs!, "Request")).Should().Be(objectId);

        var anchor = AnnotationAnchorDescriptor.ForObject(objectId);
        var annotationArgs = Activator.CreateInstance(GetInteractionType("AnnotationRequestedEventArgs"), anchor);
        annotationArgs.Should().NotBeNull();
        ReadProperty(annotationArgs!, "Anchor").Should().Be(anchor);
    }

    [Fact]
    public void HostOwnedOverlayState_IsForwardedIntoEngineRenderFlow()
    {
        var view = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        try
        {
            var renderSession = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            renderSession.Attach(GraphicsBackendPreference.Software);
            renderSession.Resize(200, 200, 1f);

            var sceneObject = DemoMeshFactory.CreateWhiteQuad(renderSession.ResourceFactory!);
            view.AddObject(sceneObject);
            view.SelectionState = new VideraSelectionState
            {
                ObjectIds = [sceneObject.Id],
                PrimaryObjectId = sceneObject.Id
            };
            view.Annotations =
            [
                new VideraNodeAnnotation
                {
                    Id = Guid.NewGuid(),
                    Text = "Pinned",
                    ObjectId = sceneObject.Id
                }
            ];

            SelectionOverlayRenderState? observedSelection = null;
            AnnotationOverlayRenderState? observedAnnotation = null;
            view.Engine.RegisterPassContributor(
                RenderPassSlot.Wireframe,
                new RecordingContributor(context =>
                {
                    observedSelection = context.SelectionOverlay;
                    observedAnnotation = context.AnnotationOverlay;
                }));

            renderSession.RenderOnce();

            observedSelection.Should().NotBeNull();
            observedSelection!.SelectedObjectIds.Should().ContainSingle().Which.Should().Be(sceneObject.Id);
            observedSelection.HoverObjectId.Should().BeNull();
            observedAnnotation.Should().NotBeNull();
            observedAnnotation!.Anchors.Should().ContainSingle();
            observedAnnotation.Anchors[0].Anchor.Should().Be(AnnotationAnchorDescriptor.ForObject(sceneObject.Id));
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void HostOwnedOverlayState_InPlaceMutations_AreObservedOnNextFrame()
    {
        var view = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        try
        {
            var renderSession = VideraViewRuntimeTestAccess.ReadRenderSession(view);
            renderSession.Attach(GraphicsBackendPreference.Software);
            renderSession.Resize(200, 200, 1f);

            var sceneObject = DemoMeshFactory.CreateWhiteQuad(renderSession.ResourceFactory!);
            view.AddObject(sceneObject);

            var mutableSelectionIds = new List<Guid>();
            var selectionState = new VideraSelectionState
            {
                ObjectIds = mutableSelectionIds,
                PrimaryObjectId = sceneObject.Id
            };
            var mutableAnnotations = new List<VideraAnnotation>();
            view.SelectionState = selectionState;
            view.Annotations = mutableAnnotations;

            mutableSelectionIds.Add(sceneObject.Id);
            mutableAnnotations.Add(new VideraNodeAnnotation
            {
                Id = Guid.NewGuid(),
                Text = "Pinned",
                ObjectId = sceneObject.Id
            });

            SelectionOverlayRenderState? observedSelection = null;
            AnnotationOverlayRenderState? observedAnnotation = null;
            view.Engine.RegisterPassContributor(
                RenderPassSlot.Wireframe,
                new RecordingContributor(context =>
                {
                    observedSelection = context.SelectionOverlay;
                    observedAnnotation = context.AnnotationOverlay;
                }));

            renderSession.RenderOnce();

            observedSelection.Should().NotBeNull();
            observedSelection!.SelectedObjectIds.Should().ContainSingle().Which.Should().Be(sceneObject.Id);
            observedAnnotation.Should().NotBeNull();
            observedAnnotation!.Anchors.Should().ContainSingle();
            observedAnnotation.Anchors[0].Anchor.Should().Be(AnnotationAnchorDescriptor.ForObject(sceneObject.Id));
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    private sealed class RecordingContributor(Action<RenderPassContributionContext> onContribute) : IRenderPassContributor
    {
        public void Contribute(RenderPassContributionContext context)
        {
            onContribute(context);
        }
    }

    private static object CreateSelectionState(Guid objectId)
    {
        var selectionState = Activator.CreateInstance(GetInteractionType("VideraSelectionState"));
        selectionState.Should().NotBeNull();
        WriteProperty(selectionState!, "ObjectIds", new[] { objectId });
        WriteProperty(selectionState!, "PrimaryObjectId", objectId);
        return selectionState!;
    }

    private static object CreateSelectionRequest(Guid objectId)
    {
        var operation = Enum.Parse(GetInteractionType("VideraSelectionOperation"), "Replace");
        var emptySpaceBehavior = Enum.Parse(GetInteractionType("VideraEmptySpaceSelectionBehavior"), "ClearSelection");
        var selectionRequest = Activator.CreateInstance(GetInteractionType("VideraSelectionRequest"), operation, new[] { objectId }, objectId, emptySpaceBehavior);
        selectionRequest.Should().NotBeNull();
        return selectionRequest!;
    }

    private static object CreateNodeAnnotation(Guid objectId)
    {
        var annotation = Activator.CreateInstance(GetInteractionType("VideraNodeAnnotation"));
        annotation.Should().NotBeNull();
        WriteProperty(annotation!, "Id", Guid.NewGuid());
        WriteProperty(annotation!, "Text", "Pinned");
        WriteProperty(annotation!, "ObjectId", objectId);
        return annotation!;
    }

    private static object CreateAnnotations(object annotation)
    {
        var listType = typeof(List<>).MakeGenericType(GetInteractionType("VideraAnnotation"));
        var list = Activator.CreateInstance(listType);
        list.Should().BeAssignableTo<IList>();
        ((IList)list!).Add(annotation);
        return list!;
    }

    private static Type GetInteractionType(string name)
    {
        var type = typeof(VideraView).Assembly.GetType($"Videra.Avalonia.Controls.Interaction.{name}");
        type.Should().NotBeNull($"interaction type {name} should exist on the public shell");
        return type!;
    }

    private static IReadOnlyList<Guid> ReadSelectionObjectIds(object source)
    {
        var selection = source.GetType().Name == "VideraSelectionState"
            ? source
            : ReadProperty(source, "SelectionState");
        var objectIds = ReadProperty(selection, "ObjectIds");
        objectIds.Should().BeAssignableTo<IEnumerable<Guid>>();
        return ((IEnumerable<Guid>)objectIds).ToArray();
    }

    private static Guid? ReadPrimaryObjectId(object source)
    {
        var selection = source.GetType().Name == "VideraSelectionState"
            ? source
            : ReadProperty(source, "SelectionState");
        return (Guid?)ReadNullableProperty(selection, "PrimaryObjectId");
    }

    private static IReadOnlyList<Guid> ReadRequestedObjectIds(object source)
    {
        var objectIds = ReadProperty(source, "ObjectIds");
        objectIds.Should().BeAssignableTo<IEnumerable<Guid>>();
        return ((IEnumerable<Guid>)objectIds).ToArray();
    }

    private static Guid? ReadRequestedPrimaryObjectId(object source)
    {
        return (Guid?)ReadNullableProperty(source, "PrimaryObjectId");
    }

    private static void AssertObjectIdsAreReadOnly(object objectIds, Guid replacementObjectId)
    {
        objectIds.Should().BeAssignableTo<IList<Guid>>("the request payload should expose a read-only list contract");
        var list = (IList<Guid>)objectIds;
        FluentActions.Invoking(() => list[0] = replacementObjectId)
            .Should()
            .Throw<NotSupportedException>();
    }

    private static IReadOnlyList<object> ReadAnnotations(VideraView view)
    {
        var annotations = ReadProperty(view, "Annotations");
        annotations.Should().BeAssignableTo<IEnumerable>();
        return ((IEnumerable)annotations).Cast<object>().ToArray();
    }

    private static object ReadProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        property.Should().NotBeNull($"property {propertyName} should exist on {instance.GetType().FullName}");

        var value = property!.GetValue(instance);
        value.Should().NotBeNull($"property {propertyName} should be queryable on {instance.GetType().FullName}");
        return value!;
    }

    private static object? ReadNullableProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        property.Should().NotBeNull($"property {propertyName} should exist on {instance.GetType().FullName}");
        return property!.GetValue(instance);
    }

    private static void WriteProperty(object instance, string propertyName, object? value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        property.Should().NotBeNull($"property {propertyName} should exist on {instance.GetType().FullName}");
        property!.CanWrite.Should().BeTrue($"property {propertyName} should be writable on {instance.GetType().FullName}");
        property.SetValue(instance, value);
    }
}
