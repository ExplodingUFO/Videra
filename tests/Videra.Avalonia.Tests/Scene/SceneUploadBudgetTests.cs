using FluentAssertions;
using Videra.Avalonia.Runtime.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneUploadBudgetTests
{
    [Fact]
    public void Resolve_increases_idle_budget_when_queue_pressure_is_high()
    {
        var light = SceneUploadBudget.Resolve(
            isInteractive: false,
            pendingObjectCount: 1,
            pendingUploadBytes: 1 * 1024 * 1024);
        var heavy = SceneUploadBudget.Resolve(
            isInteractive: false,
            pendingObjectCount: 12,
            pendingUploadBytes: 96L * 1024 * 1024);

        heavy.MaxObjectsPerFrame.Should().BeGreaterThan(light.MaxObjectsPerFrame);
        heavy.MaxBytesPerFrame.Should().BeGreaterThan(light.MaxBytesPerFrame);
    }

    [Fact]
    public void Resolve_keeps_interactive_budget_tighter_than_idle_budget_for_same_backlog()
    {
        var interactive = SceneUploadBudget.Resolve(
            isInteractive: true,
            pendingObjectCount: 8,
            pendingUploadBytes: 64L * 1024 * 1024);
        var idle = SceneUploadBudget.Resolve(
            isInteractive: false,
            pendingObjectCount: 8,
            pendingUploadBytes: 64L * 1024 * 1024);

        interactive.MaxObjectsPerFrame.Should().BeLessThan(idle.MaxObjectsPerFrame);
        interactive.MaxBytesPerFrame.Should().BeLessThan(idle.MaxBytesPerFrame);
    }
}
