namespace Videra.Core.Tests.Repository;

internal static class InteractionContractDocumentationTerms
{
    public static readonly string[] SharedApiSymbols =
    {
        "Videra.InteractionSample",
        "SelectionState",
        "Annotations",
        "SelectionRequested",
        "AnnotationRequested",
        "VideraNodeAnnotation",
        "VideraWorldPointAnnotation",
        "Navigate",
        "Select",
        "Annotate",
        "object-level"
    };

    public static readonly string[] SharedBehaviorMarkers =
    {
        "host owns",
        "annotation state",
        "object anchors",
        "world-point anchors",
        "3D highlight/render state",
        "2D label/feedback rendering"
    };

    public static readonly string[] ForbiddenNodeAnchorPhrases =
    {
        "object/node",
        "node anchor",
        "node-specific anchor"
    };
}
