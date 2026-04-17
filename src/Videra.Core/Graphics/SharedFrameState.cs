using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.RenderPipeline;

namespace Videra.Core.Graphics;

internal readonly record struct SharedFrameState(
    ICommandExecutor CommandExecutor,
    IResourceFactory ResourceFactory,
    IPipeline MeshPipeline,
    IBuffer? CameraBuffer,
    IBuffer? StyleUniformBuffer,
    uint Width,
    uint Height,
    float RenderScale,
    GraphicsBackendPreference? ActiveBackendPreference,
    RenderPipelineSnapshot? LastPipelineSnapshot);
