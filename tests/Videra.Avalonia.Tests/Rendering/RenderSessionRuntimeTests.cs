using FluentAssertions;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Xunit;

namespace Videra.Avalonia.Tests.Rendering;

public sealed class RenderSessionRuntimeTests
{
    [Fact]
    public void Invalidate_without_interactive_lease_renders_once_and_invokes_before_render()
    {
        using var engine = new VideraEngine();
        using var driver = new TrackingRenderLoopDriver();
        var beforeRenderCalls = 0;
        var requestRenderCalls = 0;
        using var session = new RenderSession(
            engine,
            beforeRender: () => beforeRenderCalls++,
            requestRender: () => requestRenderCalls++,
            renderLoopFactory: () => driver,
            bitmapFactory: static (_, _) => null);

        session.Attach(GraphicsBackendPreference.Software);
        session.Resize(128, 96, 1f);

        session.Invalidate(RenderInvalidationKinds.Scene);

        beforeRenderCalls.Should().Be(1);
        requestRenderCalls.Should().Be(1);
        driver.StartCalls.Should().Be(0);
    }

    [Fact]
    public void Interactive_lease_defers_dirty_frame_until_render_tick()
    {
        using var engine = new VideraEngine();
        using var driver = new TrackingRenderLoopDriver();
        var beforeRenderCalls = 0;
        var requestRenderCalls = 0;
        using var session = new RenderSession(
            engine,
            beforeRender: () => beforeRenderCalls++,
            requestRender: () => requestRenderCalls++,
            renderLoopFactory: () => driver,
            bitmapFactory: static (_, _) => null);

        session.Attach(GraphicsBackendPreference.Software);
        session.Resize(128, 96, 1f);

        using var lease = session.AcquireInteractiveLease();
        session.Invalidate(RenderInvalidationKinds.Interaction);

        beforeRenderCalls.Should().Be(0);
        requestRenderCalls.Should().Be(0);
        driver.StartCalls.Should().Be(1);

        driver.RaiseTick();

        beforeRenderCalls.Should().Be(1);
        requestRenderCalls.Should().Be(1);
    }

    private sealed class TrackingRenderLoopDriver : RenderSession.IRenderLoopDriver
    {
        private EventHandler? _tick;

        public int StartCalls { get; private set; }

        public int StopCalls { get; private set; }

        public void Start(TimeSpan interval, EventHandler tick)
        {
            _ = interval;
            StartCalls++;
            _tick = tick;
        }

        public void Stop()
        {
            StopCalls++;
            _tick = null;
        }

        public void RaiseTick()
        {
            _tick?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _tick = null;
        }
    }
}
