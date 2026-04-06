using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Tests.Common.Platform;

public sealed class SupportedOSFactAttributeTests
{
    [Fact]
    public void CustomFactAttributes_ShouldDeclareAttributeUsage()
    {
        HasDirectAttributeUsage(typeof(WindowsFactAttribute)).Should().BeTrue();
        HasDirectAttributeUsage(typeof(LinuxFactAttribute)).Should().BeTrue();
        HasDirectAttributeUsage(typeof(MacOSFactAttribute)).Should().BeTrue();
    }

    private static bool HasDirectAttributeUsage(Type attributeType)
        => attributeType.CustomAttributes.Any(a => a.AttributeType == typeof(AttributeUsageAttribute));
}
