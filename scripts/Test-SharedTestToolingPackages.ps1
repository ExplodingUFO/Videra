param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"

$sharedPackages = @(
    "Microsoft.NET.Test.Sdk",
    "xunit",
    "xunit.runner.visualstudio",
    "FluentAssertions",
    "coverlet.collector"
)

$testProjectsRoot = Join-Path $RepositoryRoot "tests"
$projectFiles = Get-ChildItem -Path $testProjectsRoot -Filter "*.csproj" -Recurse | Sort-Object FullName

if ($projectFiles.Count -eq 0)
{
    throw "No test projects found under '$testProjectsRoot'."
}

$failures = New-Object System.Collections.Generic.List[string]

foreach ($packageId in $sharedPackages)
{
    $versions = @{}

    foreach ($projectFile in $projectFiles)
    {
        [xml]$project = Get-Content -Raw -Path $projectFile.FullName
        $references = @($project.Project.ItemGroup.PackageReference) | Where-Object { $_.Include -eq $packageId }

        foreach ($reference in $references)
        {
            $version = [string]$reference.Version
            if ([string]::IsNullOrWhiteSpace($version))
            {
                $failures.Add("$($projectFile.FullName): PackageReference '$packageId' is missing Version.")
                continue
            }

            if (-not $versions.ContainsKey($version))
            {
                $versions[$version] = New-Object System.Collections.Generic.List[string]
            }

            $versions[$version].Add($projectFile.FullName)
        }
    }

    if ($versions.Count -gt 1)
    {
        $details = ($versions.GetEnumerator() | Sort-Object Name | ForEach-Object {
            "$($_.Name): $([string]::Join(", ", $_.Value))"
        }) -join "; "
        $failures.Add("Shared test package '$packageId' has split versions: $details")
    }
}

if ($failures.Count -gt 0)
{
    foreach ($failure in $failures)
    {
        Write-Error $failure
    }

    exit 1
}

Write-Host "Shared test tooling package versions are aligned across $($projectFiles.Count) test projects."
