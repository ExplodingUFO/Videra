namespace Videra.Avalonia.Controls;

public partial class VideraView
{
    /// <summary>
    /// Captures the current camera, selection, clipping, and measurement truth for later restoration.
    /// </summary>
    public VideraInspectionState CaptureInspectionState() => _runtime.CaptureInspectionState();

    /// <summary>
    /// Restores a previously captured inspection state onto the current view.
    /// </summary>
    /// <param name="state">The inspection state to apply.</param>
    public void ApplyInspectionState(VideraInspectionState state) => _runtime.ApplyInspectionState(state);

    /// <summary>
    /// Exports the current inspection view to an image artifact using the software export path.
    /// </summary>
    /// <param name="path">The destination file path for the exported PNG.</param>
    /// <param name="cancellationToken">Cancellation token for export work.</param>
    public Task<VideraSnapshotExportResult> ExportSnapshotAsync(string path, CancellationToken cancellationToken = default) =>
        _runtime.ExportSnapshotAsync(path, cancellationToken);

    internal VideraInspectionBundleAssetManifest CaptureInspectionBundleAssetManifestForRuntime() =>
        _runtime.CaptureInspectionBundleAssetManifest();
}
