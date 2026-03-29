using FluentAssertions;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;
using Videra.Core.Styles.Services;
using Xunit;

namespace Videra.Core.Tests.Styles;

public class RenderStyleServiceTests
{
    [Fact]
    public void Constructor_DefaultPreset_IsRealistic()
    {
        // Arrange & Act
        var service = new RenderStyleService();

        // Assert
        service.CurrentPreset.Should().Be(RenderStylePreset.Realistic);
    }

    [Fact]
    public void Constructor_DefaultParameters_AreRealistic()
    {
        // Arrange & Act
        var service = new RenderStyleService();
        var expectedParams = RenderStylePresets.CreateRealistic();

        // Assert
        service.CurrentParameters.Should().Be(expectedParams);
    }

    [Fact]
    public void ApplyPreset_TechPreset_ChangesCurrentPreset()
    {
        // Arrange
        var service = new RenderStyleService();

        // Act
        service.ApplyPreset(RenderStylePreset.Tech);

        // Assert
        service.CurrentPreset.Should().Be(RenderStylePreset.Tech);
    }

    [Fact]
    public void ApplyPreset_TechPreset_ChangesParameters()
    {
        // Arrange
        var service = new RenderStyleService();
        var paramsBefore = service.CurrentParameters;

        // Act
        service.ApplyPreset(RenderStylePreset.Tech);

        // Assert
        service.CurrentParameters.Should().NotBe(paramsBefore);
        var techParams = RenderStylePresets.CreateTech();
        service.CurrentParameters.Should().Be(techParams);
    }

    [Fact]
    public void ApplyPreset_CartoonPreset_SetsOutlineEnabled()
    {
        // Arrange
        var service = new RenderStyleService();

        // Act
        service.ApplyPreset(RenderStylePreset.Cartoon);

        // Assert
        service.CurrentParameters.Outline.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ApplyPreset_RaisesStyleChangedEvent()
    {
        // Arrange
        var service = new RenderStyleService();
        StyleChangedEventArgs? eventArgs = null;
        service.StyleChanged += (_, args) => eventArgs = args;

        // Act
        service.ApplyPreset(RenderStylePreset.Clay);

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Preset.Should().Be(RenderStylePreset.Clay);
        eventArgs.Parameters.Should().NotBeNull();
    }

    [Fact]
    public void UpdateParameters_ChangesToCustomPreset()
    {
        // Arrange
        var service = new RenderStyleService();
        var customParams = RenderStylePresets.CreateCartoon();

        // Act
        service.UpdateParameters(customParams);

        // Assert
        service.CurrentPreset.Should().Be(RenderStylePreset.Custom);
    }

    [Fact]
    public void UpdateParameters_SetsParameters_ToProvidedValues()
    {
        // Arrange
        var service = new RenderStyleService();
        var customParams = RenderStylePresets.CreateXRay();

        // Act
        service.UpdateParameters(customParams);

        // Assert
        service.CurrentParameters.Should().Be(customParams);
    }

    [Fact]
    public void UpdateParameters_RaisesStyleChangedEvent()
    {
        // Arrange
        var service = new RenderStyleService();
        StyleChangedEventArgs? eventArgs = null;
        service.StyleChanged += (_, args) => eventArgs = args;
        var customParams = RenderStylePresets.CreateXRay();

        // Act
        service.UpdateParameters(customParams);

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Preset.Should().Be(RenderStylePreset.Custom);
    }

    [Fact]
    public void UpdateParameter_ChangesSingleParameter_ToCustomPreset()
    {
        // Arrange
        var service = new RenderStyleService();

        // Act
        service.UpdateParameter(p => p.Material.Opacity, 0.5f);

        // Assert
        service.CurrentPreset.Should().Be(RenderStylePreset.Custom);
        service.CurrentParameters.Material.Opacity.Should().Be(0.5f);
    }

    [Fact]
    public void ExportToJson_ReturnsNonEmptyString()
    {
        // Arrange
        var service = new RenderStyleService();

        // Act
        var json = service.ExportToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ImportFromJson_RestoresParameters()
    {
        // Arrange
        var service = new RenderStyleService();
        service.ApplyPreset(RenderStylePreset.Tech);
        var json = service.ExportToJson();

        // Act
        var service2 = new RenderStyleService();
        service2.ImportFromJson(json);

        // Assert
        service2.CurrentPreset.Should().Be(RenderStylePreset.Tech);
    }

    [Fact]
    public void PropertyChanged_Fires_OnPresetChange()
    {
        // Arrange
        var service = new RenderStyleService();
        var changedProperties = new List<string>();
        ((System.ComponentModel.INotifyPropertyChanged)service).PropertyChanged += (_, args) =>
            changedProperties.Add(args.PropertyName!);

        // Act
        service.ApplyPreset(RenderStylePreset.Wireframe);

        // Assert
        changedProperties.Should().Contain("CurrentPreset");
        changedProperties.Should().Contain("CurrentParameters");
    }

    [Fact]
    public void UpdateParameters_Null_ThrowsArgumentNullException()
    {
        var service = new RenderStyleService();
        var act = () => service.UpdateParameters(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ImportFromJson_Null_ThrowsArgumentException()
    {
        var service = new RenderStyleService();
        var act = () => service.ImportFromJson(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ImportFromJson_Empty_ThrowsArgumentException()
    {
        var service = new RenderStyleService();
        var act = () => service.ImportFromJson(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task SaveToFileAsync_NullPath_ThrowsArgumentException()
    {
        var service = new RenderStyleService();
        var act = () => service.SaveToFileAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task LoadFromFileAsync_NullPath_ThrowsArgumentException()
    {
        var service = new RenderStyleService();
        var act = () => service.LoadFromFileAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void UpdateParameter_NullSelector_ThrowsArgumentNullException()
    {
        var service = new RenderStyleService();
        var act = () => service.UpdateParameter<float>(null!, 0.5f);
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }
}
