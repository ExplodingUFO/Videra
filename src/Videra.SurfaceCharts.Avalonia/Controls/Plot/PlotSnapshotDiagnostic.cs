namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes an explicit diagnostic produced during snapshot request validation or export.
/// </summary>
public sealed class PlotSnapshotDiagnostic
{
    internal PlotSnapshotDiagnostic(string diagnosticCode, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnosticCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        DiagnosticCode = diagnosticCode;
        Message = message;
    }

    /// <summary>
    /// Gets a deterministic diagnostic code.
    /// </summary>
    public string DiagnosticCode { get; }

    /// <summary>
    /// Gets a host-facing diagnostic message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a new <see cref="PlotSnapshotDiagnostic"/> with the specified code and message.
    /// </summary>
    /// <param name="diagnosticCode">The deterministic diagnostic code.</param>
    /// <param name="message">The host-facing diagnostic message.</param>
    /// <returns>A new diagnostic instance.</returns>
    internal static PlotSnapshotDiagnostic Create(string diagnosticCode, string message)
    {
        return new PlotSnapshotDiagnostic(diagnosticCode, message);
    }
}
