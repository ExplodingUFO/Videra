namespace Videra.Core.Scene;

public enum SceneAuthoringDiagnosticSeverity
{
    Warning,
    Error
}

public sealed record SceneAuthoringDiagnostic(
    SceneAuthoringDiagnosticSeverity Severity,
    string Code,
    string Message,
    string? Target = null);
