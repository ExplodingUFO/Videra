namespace Videra.Core.Scene;

public interface IVideraModelImporter
{
    bool CanImport(string path);

    ValueTask<ModelImportResult> ImportAsync(
        ModelImportRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record ModelImportRequest(
    string Path,
    string? DisplayName = null);

public enum ModelImportDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record ModelImportDiagnostic(
    ModelImportDiagnosticSeverity Severity,
    string Message,
    string? Code = null);

public sealed record ModelImportResult
{
    public ModelImportResult(
        ImportedSceneAsset? asset,
        IReadOnlyList<ModelImportDiagnostic>? diagnostics,
        TimeSpan importDuration)
    {
        Asset = asset;
        Diagnostics = diagnostics ?? Array.Empty<ModelImportDiagnostic>();
        ImportDuration = importDuration;
    }

    public ImportedSceneAsset? Asset { get; }

    public IReadOnlyList<ModelImportDiagnostic> Diagnostics { get; }

    public TimeSpan ImportDuration { get; }

    public bool Succeeded => Asset is not null;

    public static ModelImportResult Success(
        ImportedSceneAsset asset,
        IReadOnlyList<ModelImportDiagnostic>? diagnostics = null,
        TimeSpan importDuration = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        return new ModelImportResult(asset, diagnostics, importDuration);
    }

    public static ModelImportResult Failed(
        Exception exception,
        TimeSpan importDuration = default,
        string? code = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new ModelImportResult(
            null,
            [new ModelImportDiagnostic(ModelImportDiagnosticSeverity.Error, exception.Message, code)],
            importDuration);
    }
}
