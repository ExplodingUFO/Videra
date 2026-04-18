using System.Numerics;
using FluentAssertions;
using Videra.Core.Inspection;
using Xunit;

namespace Videra.Core.Tests.Inspection;

public sealed class VideraMeasurementTests
{
    [Fact]
    public void Measurement_ShouldExposeDistanceHeightDeltaAndHorizontalDistance()
    {
        var measurement = new VideraMeasurement
        {
            Start = VideraMeasurementAnchor.ForWorldPoint(new Vector3(1f, 2f, 3f)),
            End = VideraMeasurementAnchor.ForWorldPoint(new Vector3(4f, 6f, 7f))
        };

        measurement.Distance.Should().BeApproximately(MathF.Sqrt(41f), 0.0001f);
        measurement.HeightDelta.Should().BeApproximately(4f, 0.0001f);
        measurement.HorizontalDistance.Should().BeApproximately(5f, 0.0001f);
    }
}
