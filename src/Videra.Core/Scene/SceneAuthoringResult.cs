namespace Videra.Core.Scene;

public sealed class SceneAuthoringResult
{
    public SceneAuthoringResult(SceneDocument? document, IEnumerable<SceneAuthoringDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        Document = document;
        Diagnostics = diagnostics.ToArray();
    }

    public SceneDocument? Document { get; }

    public IReadOnlyList<SceneAuthoringDiagnostic> Diagnostics { get; }

    public bool Succeeded => Document is not null && Diagnostics.All(static diagnostic => diagnostic.Severity != SceneAuthoringDiagnosticSeverity.Error);
}
