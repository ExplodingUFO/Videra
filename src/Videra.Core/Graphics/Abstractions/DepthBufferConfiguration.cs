namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Configures depth buffer behavior across all rendering backends.
/// All backends should use consistent depth configuration to ensure
/// identical rendering results on Windows (D3D11), Linux (Vulkan), and macOS (Metal).
/// </summary>
public readonly record struct DepthBufferConfiguration
{
    /// <summary>
    /// Depth clear value. All backends clear to this value at the start of each frame.
    /// Standard value: 1.0f (maximum depth, representing the far plane).
    /// </summary>
    public float ClearDepthValue { get; init; }

    /// <summary>
    /// Stencil clear value. Standard: 0.
    /// </summary>
    public int ClearStencilValue { get; init; }

    /// <summary>
    /// Depth comparison function used for depth testing.
    /// Standard: LessEqual (fragment passes if its depth is less than or equal to the buffer).
    /// </summary>
    public DepthComparisonFunction DepthComparison { get; init; }

    /// <summary>
    /// Default depth buffer configuration used across all backends.
    /// </summary>
    public static DepthBufferConfiguration Default => new()
    {
        ClearDepthValue = 1.0f,
        ClearStencilValue = 0,
        DepthComparison = DepthComparisonFunction.LessEqual
    };

    public DepthBufferConfiguration(float clearDepthValue, int clearStencilValue, DepthComparisonFunction depthComparison)
    {
        ClearDepthValue = clearDepthValue;
        ClearStencilValue = clearStencilValue;
        DepthComparison = depthComparison;
    }
}

/// <summary>
/// Depth comparison functions, mapping to native equivalents on each platform:
/// D3D11: COMPARISON_FUNC, Vulkan: VkCompareOp, Metal: MTLCompareFunction.
/// </summary>
public enum DepthComparisonFunction
{
    Never = 0,
    Less = 1,
    Equal = 2,
    LessEqual = 3,
    Greater = 4,
    NotEqual = 5,
    GreaterEqual = 6,
    Always = 7
}
