param(
    [string]$Configuration = "Release",
    [string]$OutputRoot = "artifacts/performance-lab-visual-evidence",
    [string]$ViewerScenarios = "all",
    [string]$ScatterScenarios = "all",
    [int]$Width = 1280,
    [int]$Height = 720,
    [switch]$SimulateUnavailable
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "tools/Videra.PerformanceLabVisualEvidence/Videra.PerformanceLabVisualEvidence.csproj"

$toolArgs = @(
    "--output-root", $OutputRoot,
    "--viewer-scenarios", $ViewerScenarios,
    "--scatter-scenarios", $ScatterScenarios,
    "--width", $Width,
    "--height", $Height
)

if ($SimulateUnavailable)
{
    $toolArgs += "--simulate-unavailable"
}

dotnet run --project $project --configuration $Configuration -- @toolArgs
