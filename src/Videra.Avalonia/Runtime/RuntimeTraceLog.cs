namespace Videra.Avalonia.Runtime;

internal static class RuntimeTraceLog
{
    private const string TracePathVariable = "VIDERA_CONSUMER_SMOKE_TRACE";

    public static void Write(string message)
    {
        var path = Environment.GetEnvironmentVariable(TracePathVariable);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.AppendAllText(
                path,
                $"[{DateTimeOffset.UtcNow:O}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Best-effort diagnostics only.
        }
    }
}
