using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Videra.SurfaceCharts.Processing.Tests")]

namespace Videra.SurfaceCharts.Processing;

internal interface ISurfaceCacheFileSystem
{
    Stream CreateFile(string path);

    Stream OpenRead(string path);

    bool FileExists(string path);

    void CreateDirectory(string path);

    void ReplaceFile(string sourcePath, string destinationPath, string? backupPath);

    void MoveFile(string sourcePath, string destinationPath);

    void DeleteFile(string path);
}

internal static class SurfaceCacheFileSystem
{
    private static readonly ISurfaceCacheFileSystem Physical = new PhysicalSurfaceCacheFileSystem();
    private static readonly AsyncLocal<ISurfaceCacheFileSystem?> CurrentOverride = new();

    internal static ISurfaceCacheFileSystem Current
    {
        get => CurrentOverride.Value ?? Physical;
        set => CurrentOverride.Value = value;
    }

    internal static void ResetForTests()
    {
        CurrentOverride.Value = null;
    }

    private sealed class PhysicalSurfaceCacheFileSystem : ISurfaceCacheFileSystem
    {
        public Stream CreateFile(string path)
        {
            return File.Create(path);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void ReplaceFile(string sourcePath, string destinationPath, string? backupPath)
        {
            File.Replace(sourcePath, destinationPath, destinationBackupFileName: backupPath, ignoreMetadataErrors: true);
        }

        public void MoveFile(string sourcePath, string destinationPath)
        {
            File.Move(sourcePath, destinationPath);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}
