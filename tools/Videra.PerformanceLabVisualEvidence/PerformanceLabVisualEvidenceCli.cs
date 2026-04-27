using System.Globalization;

namespace Videra.PerformanceLabVisualEvidence;

public static class PerformanceLabVisualEvidenceCli
{
    public static Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(args);

        try
        {
            var options = Parse(args);
            var result = PerformanceLabVisualEvidenceCapture.Capture(options, cancellationToken);
            Console.WriteLine($"Performance Lab visual evidence: {result.Status}");
            Console.WriteLine($"Manifest: {result.ManifestPath}");
            return Task.FromResult(0);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        {
            Console.Error.WriteLine(ex.Message);
            return Task.FromResult(1);
        }
    }

    private static PerformanceLabVisualEvidenceOptions Parse(IReadOnlyList<string> args)
    {
        var outputRoot = Path.Combine("artifacts", "performance-lab-visual-evidence");
        var width = 1280;
        var height = 720;
        var simulateUnavailable = false;
        string? viewerScenarios = null;
        string? scatterScenarios = null;

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--output-root":
                    outputRoot = RequireValue(args, ref i, arg);
                    break;
                case "--width":
                    width = ParsePositiveInt(RequireValue(args, ref i, arg), arg);
                    break;
                case "--height":
                    height = ParsePositiveInt(RequireValue(args, ref i, arg), arg);
                    break;
                case "--viewer-scenarios":
                    viewerScenarios = RequireValue(args, ref i, arg);
                    break;
                case "--scatter-scenarios":
                    scatterScenarios = RequireValue(args, ref i, arg);
                    break;
                case "--simulate-unavailable":
                    simulateUnavailable = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{arg}'.");
            }
        }

        return new PerformanceLabVisualEvidenceOptions(
            outputRoot,
            width,
            height,
            ParseIds(viewerScenarios),
            ParseIds(scatterScenarios),
            simulateUnavailable);
    }

    private static string RequireValue(IReadOnlyList<string> args, ref int index, string name)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"{name} requires a value.");
        }

        index++;
        return args[index];
    }

    private static int ParsePositiveInt(string value, string name)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            throw new ArgumentException($"{name} must be a positive integer.");
        }

        return parsed;
    }

    private static IReadOnlyList<string>? ParseIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || StringComparer.OrdinalIgnoreCase.Equals(value, "all"))
        {
            return null;
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static id => id.Length > 0)
            .ToArray();
    }
}
