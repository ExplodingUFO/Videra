namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Configures depth buffer behavior across all rendering backends.
/// All backends should use consistent depth configuration to ensure
/// identical rendering results on Windows (D3D11), Linux (Vulkan), and macOS (Metal).
/// </summary>
public readonly record struct DepthBufferConfiguration
{
    /// <summary>
    /// Logical depth buffer format selected by the backend.
    /// </summary>
    public DepthBufferFormat DepthFormat { get; init; }

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
    /// Default shared depth configuration used across backends.
    /// </summary>
    public static DepthBufferConfiguration Default => new(
        DepthBufferFormat.Depth32Float,
        1.0f,
        0,
        DepthComparisonFunction.LessEqual);

    public DepthBufferConfiguration(
        DepthBufferFormat depthFormat,
        float clearDepthValue,
        int clearStencilValue,
        DepthComparisonFunction depthComparison)
    {
        DepthFormat = depthFormat;
        ClearDepthValue = clearDepthValue;
        ClearStencilValue = clearStencilValue;
        DepthComparison = depthComparison;
    }
}

/// <summary>
/// Logical depth buffer formats used by Videra.
/// Individual backends map these to their nearest native format.
/// </summary>
public enum DepthBufferFormat
{
    Depth24UnormStencil8 = 0,
    Depth32Float = 1
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
