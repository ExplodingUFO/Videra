using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.RenderPipeline.Extensibility;

namespace Videra.Core.Graphics;

internal sealed class PassRegistry
{
    private readonly Dictionary<RenderPassSlot, IRenderPassContributor> _passContributorOverrides = new();
    private readonly Dictionary<RenderPassSlot, List<IRenderPassContributor>> _passContributorRegistrations = new();
    private readonly Dictionary<RenderFrameHookPoint, List<Action<RenderFrameHookContext>>> _frameHooks = new();

    public void RegisterPassContributor(RenderPassSlot slot, IRenderPassContributor contributor)
    {
        if (!_passContributorRegistrations.TryGetValue(slot, out var contributors))
        {
            contributors = [];
            _passContributorRegistrations[slot] = contributors;
        }

        contributors.Add(contributor);
    }

    public void ReplacePassContributor(RenderPassSlot slot, IRenderPassContributor contributor)
    {
        _passContributorOverrides[slot] = contributor;
    }

    public void RegisterFrameHook(RenderFrameHookPoint hookPoint, Action<RenderFrameHookContext> hook)
    {
        if (!_frameHooks.TryGetValue(hookPoint, out var hooks))
        {
            hooks = [];
            _frameHooks[hookPoint] = hooks;
        }

        hooks.Add(hook);
    }

    public bool TryGetReplacement(RenderPassSlot slot, out IRenderPassContributor? contributor)
    {
        return _passContributorOverrides.TryGetValue(slot, out contributor);
    }

    public IReadOnlyList<IRenderPassContributor> GetRegistrations(RenderPassSlot slot)
    {
        return _passContributorRegistrations.TryGetValue(slot, out var contributors)
            ? contributors
            : Array.Empty<IRenderPassContributor>();
    }

    public IReadOnlyList<Action<RenderFrameHookContext>> GetHooks(RenderFrameHookPoint hookPoint)
    {
        return _frameHooks.TryGetValue(hookPoint, out var hooks)
            ? hooks
            : Array.Empty<Action<RenderFrameHookContext>>();
    }
}
