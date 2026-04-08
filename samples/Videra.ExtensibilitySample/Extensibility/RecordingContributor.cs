using Videra.Core.Graphics.RenderPipeline.Extensibility;

namespace Videra.ExtensibilitySample.Extensibility;

internal sealed class RecordingContributor : IRenderPassContributor
{
    public event EventHandler<RenderPassObservationEventArgs>? ObservationRecorded;

    public RenderPassObservationEventArgs? LastObservation { get; private set; }

    public void Contribute(RenderPassContributionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var snapshot = context.LastPipelineSnapshot;
        var observation = new RenderPassObservationEventArgs(
            context.Slot,
            context.ActiveBackendPreference?.ToString() ?? "Unknown",
            snapshot?.Profile.ToString() ?? "Unavailable",
            snapshot?.StageNames.ToArray() ?? Array.Empty<string>(),
            context.SceneObjects.Count);

        LastObservation = observation;
        ObservationRecorded?.Invoke(this, observation);
    }
}

internal sealed class RenderPassObservationEventArgs : EventArgs
{
    public RenderPassObservationEventArgs(
        RenderPassSlot slot,
        string backend,
        string pipelineProfile,
        IReadOnlyList<string> stageNames,
        int sceneObjectCount)
    {
        Slot = slot;
        Backend = backend;
        PipelineProfile = pipelineProfile;
        StageNames = stageNames;
        SceneObjectCount = sceneObjectCount;
    }

    public RenderPassSlot Slot { get; }

    public string Backend { get; }

    public string PipelineProfile { get; }

    public IReadOnlyList<string> StageNames { get; }

    public int SceneObjectCount { get; }

    public override string ToString()
    {
        var stages = StageNames.Count == 0 ? "Unavailable" : string.Join(", ", StageNames);
        return $"Slot={Slot}; Backend={Backend}; Profile={PipelineProfile}; SceneObjects={SceneObjectCount}; Stages={stages}";
    }
}
