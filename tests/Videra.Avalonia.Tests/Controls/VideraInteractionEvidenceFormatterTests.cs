using System.Numerics;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Inspection;
using Xunit;

namespace Videra.Avalonia.Tests.Controls;

public sealed class VideraInteractionEvidenceFormatterTests
{
    [Fact]
    public void Create_ShouldSummarizeInspectionStateWithoutMutatingIt()
    {
        var selectedId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondSelectedId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var state = new VideraInspectionState
        {
            CameraTarget = new Vector3(1.25f, 2.5f, 3.75f),
            CameraRadius = 8.5f,
            CameraYaw = 35f,
            CameraPitch = -12.5f,
            SelectedObjectIds = [selectedId, secondSelectedId],
            PrimarySelectedObjectId = selectedId,
            Annotations =
            [
                new VideraNodeAnnotation
                {
                    ObjectId = selectedId,
                    Text = "Primary node"
                },
                new VideraWorldPointAnnotation
                {
                    WorldPoint = new Vector3(4f, 5f, 6f),
                    Text = "World note"
                }
            ],
            MeasurementSnapMode = VideraMeasurementSnapMode.Vertex,
            ClippingPlanes =
            [
                VideraClipPlane.FromPointNormal(Vector3.Zero, Vector3.UnitY),
                VideraClipPlane.FromPointNormal(Vector3.One, Vector3.UnitX, isEnabled: false)
            ],
            Measurements =
            [
                new VideraMeasurement
                {
                    Label = "span",
                    Start = VideraMeasurementAnchor.ForWorldPoint(Vector3.Zero),
                    End = VideraMeasurementAnchor.ForWorldPoint(Vector3.One)
                },
                new VideraMeasurement
                {
                    Start = VideraMeasurementAnchor.ForWorldPoint(Vector3.UnitX),
                    End = VideraMeasurementAnchor.ForWorldPoint(Vector3.UnitY)
                }
            ]
        };

        var evidence = VideraInteractionEvidenceFormatter.Create(
            state,
            new VideraInteractionDiagnostics
            {
                SupportsControlledSelection = true,
                SupportsControlledAnnotations = false,
                SupportsIntentEvents = true
            });

        evidence.EvidenceKind.Should().Be("ViewerInteraction");
        evidence.SelectedObjectCount.Should().Be(2);
        evidence.PrimarySelectedObjectId.Should().Be(selectedId);
        evidence.AnnotationCount.Should().Be(2);
        evidence.AnnotationKinds.Should().Equal("VideraNodeAnnotation", "VideraWorldPointAnnotation");
        evidence.MeasurementCount.Should().Be(2);
        evidence.MeasurementLabels.Should().Equal("span", "Unlabeled");
        evidence.ClippingPlaneCount.Should().Be(2);
        evidence.MeasurementSnapMode.Should().Be(VideraMeasurementSnapMode.Vertex);
        evidence.CameraTarget.Should().Be(new Vector3(1.25f, 2.5f, 3.75f));
        evidence.CameraRadius.Should().Be(8.5f);
        evidence.CameraYaw.Should().Be(35f);
        evidence.CameraPitch.Should().Be(-12.5f);
        evidence.SupportsControlledSelection.Should().BeTrue();
        evidence.SupportsControlledAnnotations.Should().BeFalse();
        evidence.SupportsIntentEvents.Should().BeTrue();

        state.SelectedObjectIds.Should().Equal(selectedId, secondSelectedId);
        state.Measurements.Should().HaveCount(2);
    }

    [Fact]
    public void Format_ShouldProduceDeterministicReportWithoutDiagnostics()
    {
        var state = new VideraInspectionState
        {
            CameraTarget = new Vector3(1.25f, 2.5f, 3.75f),
            CameraRadius = 8.5f,
            CameraYaw = 35f,
            CameraPitch = -12.5f,
            PrimarySelectedObjectId = null,
            MeasurementSnapMode = VideraMeasurementSnapMode.Free
        };

        var report = VideraInteractionEvidenceFormatter.Format(state);

        report.Should().Be(string.Join(
            Environment.NewLine,
            "Videra interaction evidence",
            "EvidenceKind: ViewerInteraction",
            "SelectedObjectCount: 0",
            "PrimarySelectedObjectId: Unavailable",
            "AnnotationCount: 0",
            "AnnotationKinds: Unavailable",
            "MeasurementCount: 0",
            "MeasurementLabels: Unavailable",
            "ClippingPlaneCount: 0",
            "MeasurementSnapMode: Free",
            "CameraTarget: 1.25, 2.5, 3.75",
            "CameraRadius: 8.5",
            "CameraYaw: 35",
            "CameraPitch: -12.5",
            "SupportsControlledSelection: Unavailable",
            "SupportsControlledAnnotations: Unavailable",
            "SupportsIntentEvents: Unavailable") + Environment.NewLine);
    }
}
