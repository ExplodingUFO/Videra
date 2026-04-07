using System.Numerics;
using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Xunit;

namespace Videra.Core.Tests.Cameras;

public class OrbitCameraTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var camera = new OrbitCamera();

        // Assert
        camera.Yaw.Should().Be(0.5f);
        camera.Pitch.Should().Be(0.5f);
        camera.Target.Should().Be(Vector3.Zero);
        camera.Up.Should().Be(Vector3.UnitY);
        camera.FieldOfView.Should().BeApproximately(MathF.PI / 4.0f, 0.0001f);
        camera.RotationSpeed.Should().Be(0.01f);
        camera.ZoomSpeed.Should().Be(1.0f);
        camera.PanSpeed.Should().Be(0.02f);
        camera.InvertX.Should().BeFalse();
        camera.InvertY.Should().BeFalse();
    }

    [Fact]
    public void ViewMatrix_AfterConstruction_IsNotIdentity()
    {
        // Arrange
        var camera = new OrbitCamera();

        // Act
        var view = camera.ViewMatrix;

        // Assert
        view.Should().NotBe(default(Matrix4x4));
        // The view matrix should not be identity since the camera is positioned away from origin
        view.Should().NotBe(Matrix4x4.Identity);
    }

    [Fact]
    public void ProjectionMatrix_AfterUpdateProjection_IsValidPerspectiveMatrix()
    {
        // Arrange
        var camera = new OrbitCamera();

        // Act
        camera.UpdateProjection(800, 600);
        var proj = camera.ProjectionMatrix;

        // Assert
        proj.Should().NotBe(default(Matrix4x4));
        // A valid perspective projection should have M34 = -1
        proj.M34.Should().Be(-1f);
    }

    [Fact]
    public void UpdateProjection_WithZeroWidth_UsesWidthOfOne()
    {
        // Arrange
        var camera = new OrbitCamera();

        // Act
        camera.UpdateProjection(0, 600);
        var proj = camera.ProjectionMatrix;

        // Assert
        proj.Should().NotBe(default(Matrix4x4));
        proj.M34.Should().Be(-1f);
    }

    [Fact]
    public void UpdateProjection_WithZeroHeight_UsesHeightOfOne()
    {
        // Arrange
        var camera = new OrbitCamera();

        // Act
        camera.UpdateProjection(800, 0);
        var proj = camera.ProjectionMatrix;

        // Assert
        proj.Should().NotBe(default(Matrix4x4));
        proj.M34.Should().Be(-1f);
    }

    [Fact]
    public void UpdateAspectRatio_UpdatesProjectionMatrix()
    {
        // Arrange
        var camera = new OrbitCamera();
        camera.UpdateProjection(800, 600);
        var projBefore = camera.ProjectionMatrix;

        // Act
        camera.UpdateAspectRatio(2.0f);
        var projAfter = camera.ProjectionMatrix;

        // Assert
        projAfter.Should().NotBe(projBefore);
    }

    [Fact]
    public void FieldOfView_ChangeAffects_ProjectionMatrix()
    {
        // Arrange
        var camera = new OrbitCamera();
        camera.UpdateProjection(800, 600);
        var projBefore = camera.ProjectionMatrix;

        // Act
        camera.FieldOfView = MathF.PI / 2.0f; // 90 degrees
        camera.UpdateProjection(800, 600);
        var projAfter = camera.ProjectionMatrix;

        // Assert
        projAfter.Should().NotBe(projBefore);
    }

    [Fact]
    public void Rotate_ChangesYawAndPitch()
    {
        // Arrange
        var camera = new OrbitCamera();
        var yawBefore = camera.Yaw;
        var pitchBefore = camera.Pitch;

        // Act
        camera.Rotate(10f, 5f);

        // Assert
        camera.Yaw.Should().NotBe(yawBefore);
        camera.Pitch.Should().NotBe(pitchBefore);
    }

    [Fact]
    public void Rotate_UpdatesPositionAndViewMatrix()
    {
        // Arrange
        var camera = new OrbitCamera();
        var posBefore = camera.Position;
        var viewBefore = camera.ViewMatrix;

        // Act
        camera.Rotate(10f, 5f);

        // Assert
        camera.Position.Should().NotBe(posBefore);
        camera.ViewMatrix.Should().NotBe(viewBefore);
    }

    [Fact]
    public void Rotate_ClampsPitch_ToPreventGimbalLock()
    {
        // Arrange
        var camera = new OrbitCamera();

        // Act - try to rotate past the clamp limit
        camera.Rotate(0f, 500f);

        // Assert
        camera.Pitch.Should().BeInRange(-1.5f, 1.5f);
    }

    [Fact]
    public void Rotate_WithInvertX_ReversesYawDirection()
    {
        // Arrange
        var camera1 = new OrbitCamera();
        var camera2 = new OrbitCamera { InvertX = true };

        // Act
        camera1.Rotate(10f, 0f);
        camera2.Rotate(10f, 0f);

        // Assert
        camera2.Yaw.Should().BeLessThan(camera1.Yaw);
    }

    [Fact]
    public void Rotate_WithInvertY_ReversesPitchDirection()
    {
        // Arrange
        var camera1 = new OrbitCamera();
        var camera2 = new OrbitCamera { InvertY = true };

        // Act
        camera1.Rotate(0f, 10f);
        camera2.Rotate(0f, 10f);

        // Assert
        camera2.Pitch.Should().BeGreaterThan(camera1.Pitch);
    }

    [Fact]
    public void Zoom_ChangesRadius()
    {
        // Arrange
        var camera = new OrbitCamera();
        var posBefore = camera.Position;

        // Act
        camera.Zoom(5f);

        // Assert
        camera.Position.Should().NotBe(posBefore);
    }

    [Fact]
    public void Zoom_ClampsRadius_ToMinimumValue()
    {
        // Arrange
        var camera = new OrbitCamera();

        // Act - try to zoom in very close
        camera.Zoom(1000f);

        // Assert
        // Position magnitude should not be zero
        camera.Position.Length().Should().BeGreaterThan(0f);
    }

    [Fact]
    public void Zoom_ClampsRadius_ToMaximumValue()
    {
        // Arrange
        var camera = new OrbitCamera();

        // Act - try to zoom out very far
        camera.Zoom(-10000f);

        // Assert
        camera.Position.Length().Should().BeLessThan(250f);
    }

    [Fact]
    public void Pan_ChangesTarget()
    {
        // Arrange
        var camera = new OrbitCamera();
        var targetBefore = camera.Target;

        // Act
        camera.Pan(10f, 5f);

        // Assert
        camera.Target.Should().NotBe(targetBefore);
    }

    [Fact]
    public void Pan_ChangesPositionRelativeToTarget()
    {
        // Arrange
        var camera = new OrbitCamera();
        var posBefore = camera.Position;

        // Act
        camera.Pan(10f, 0f);

        // Assert - position should change since it is relative to target
        camera.Position.Should().NotBe(posBefore);
    }

    [Fact]
    public void Position_AfterConstruction_IsNotAtOrigin()
    {
        // Arrange & Act
        var camera = new OrbitCamera();

        // Assert - default radius is 15 with yaw=0.5 and pitch=0.5, so position should not be origin
        camera.Position.Should().NotBe(Vector3.Zero);
    }

    [Fact]
    public void Position_HasExpectedMagnitude_AfterConstruction()
    {
        // Arrange & Act
        var camera = new OrbitCamera();

        // Assert - default radius is 15
        camera.Position.Length().Should().BeApproximately(15.0f, 0.01f);
    }

    [Fact]
    public void Reset_RestoresDefaultCameraState()
    {
        var camera = new OrbitCamera();
        var defaultPosition = camera.Position;

        camera.Rotate(20f, 10f);
        camera.Zoom(4f);
        camera.Pan(2f, -1f);

        camera.Reset();

        camera.Yaw.Should().BeApproximately(0.5f, 0.0001f);
        camera.Pitch.Should().BeApproximately(0.5f, 0.0001f);
        camera.Target.Should().Be(Vector3.Zero);
        Vector3.Distance(camera.Position, defaultPosition).Should().BeLessThan(0.0001f);
    }

    [Fact]
    public void FrameBounds_CentersCameraOnBounds()
    {
        var camera = new OrbitCamera();
        var bounds = new BoundingBox3(new Vector3(-2f, -1f, -3f), new Vector3(6f, 5f, 9f));

        var framed = camera.FrameBounds(bounds);

        framed.Should().BeTrue();
        camera.Target.Should().Be(new Vector3(2f, 2f, 3f));
        Vector3.Distance(camera.Position, camera.Target).Should().BeGreaterThan(0f);
    }
}
