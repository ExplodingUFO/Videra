#!/usr/bin/env pwsh
param(
    [string]$OutputRoot = "artifacts/doctor",

    [switch]$RunPackageValidation,

    [string]$PackageRoot = "",

    [string]$ExpectedVersion = "",

    [switch]$RunBenchmarkThresholds,

    [ValidateSet("All", "Viewer", "SurfaceCharts")]
    [string]$BenchmarkSuite = "All",

    [string]$BenchmarkOutputRoot = "artifacts/benchmarks",

    [switch]$RunConsumerSmoke,

    [ValidateSet("ViewerOnly", "ViewerObj", "ViewerGltf", "SurfaceCharts")]
    [string]$ConsumerSmokeScenario = "ViewerObj",

    [switch]$RunNativeValidation,

    [ValidateSet("Auto", "Linux", "macOS", "Windows")]
    [string]$NativePlatform = "Auto"
)

$ErrorActionPreference = "Stop"

# Videra Doctor is repo-only and non-mutating. It reports repository support
# state and writes local artifacts; it is not a public package or installed tool.
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$defaultOutputRoot = "artifacts/doctor"

if ([System.IO.Path]::IsPathFullyQualified($OutputRoot))
{
    $resolvedOutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
}
else
{
    $resolvedOutputRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot))
}

New-Item -ItemType Directory -Force -Path $resolvedOutputRoot | Out-Null

$checks = [System.Collections.Generic.List[object]]::new()
$validations = [System.Collections.Generic.List[object]]::new()

function Add-Check([string]$Id, [string]$Status, [string]$Message, [string]$Path = "")
{
    $checks.Add([ordered]@{
        id = $Id
        status = $Status
        message = $Message
        path = $Path
    }) | Out-Null
}

function Add-Validation(
    [string]$Id,
    [string]$Status,
    [string]$Message,
    [string]$Script,
    [string[]]$Prerequisites = @(),
    [string[]]$Artifacts = @(),
    [bool]$Invoked = $false,
    $ExitCode = $null,
    [string]$LogPath = "")
{
    $validations.Add([ordered]@{
        id = $Id
        status = $Status
        message = $Message
        script = $Script
        prerequisites = @($Prerequisites)
        artifacts = @($Artifacts)
        invoked = $Invoked
        exitCode = $ExitCode
        logPath = $LogPath
    }) | Out-Null
}

function Invoke-ValidationScript(
    [string]$Id,
    [string]$Script,
    [string[]]$Arguments,
    [string[]]$Prerequisites = @(),
    [string[]]$Artifacts = @())
{
    $scriptPath = Join-Path $repositoryRoot $Script
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf))
    {
        Add-Validation -Id $Id -Status "unavailable" -Message "Validation script is missing." -Script $Script -Prerequisites $Prerequisites -Artifacts $Artifacts
        return
    }

    $safeId = $Id -replace "[^A-Za-z0-9._-]", "-"
    $logPath = Join-Path $resolvedOutputRoot "$safeId.log"
    $output = @()
    $exitCode = 0

    try
    {
        $output = @(& pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath @Arguments 2>&1)
        $exitCode = $LASTEXITCODE
    }
    catch
    {
        $output = @($_.Exception.Message)
        $exitCode = 1
    }

    $output | Set-Content -LiteralPath $logPath -Encoding utf8

    if ($exitCode -eq 0)
    {
        Add-Validation -Id $Id -Status "pass" -Message "Validation script completed successfully." -Script $Script -Prerequisites $Prerequisites -Artifacts $Artifacts -Invoked $true -ExitCode $exitCode -LogPath $logPath
        return
    }

    Add-Validation -Id $Id -Status "fail" -Message "Validation script failed. See logPath." -Script $Script -Prerequisites $Prerequisites -Artifacts $Artifacts -Invoked $true -ExitCode $exitCode -LogPath $logPath
}

function Test-RelativeFile([string]$Id, [string]$RelativePath)
{
    $path = Join-Path $repositoryRoot $RelativePath
    if (Test-Path -LiteralPath $path -PathType Leaf)
    {
        Add-Check -Id $Id -Status "pass" -Message "Found $RelativePath." -Path $RelativePath
        return $true
    }

    Add-Check -Id $Id -Status "unavailable" -Message "Missing $RelativePath." -Path $RelativePath
    return $false
}

function Test-RelativeDirectory([string]$Id, [string]$RelativePath)
{
    $path = Join-Path $repositoryRoot $RelativePath
    if (Test-Path -LiteralPath $path -PathType Container)
    {
        Add-Check -Id $Id -Status "pass" -Message "Found $RelativePath." -Path $RelativePath
        return $true
    }

    Add-Check -Id $Id -Status "unavailable" -Message "Missing $RelativePath." -Path $RelativePath
    return $false
}

$dotnetVersion = $null
$dotnetInfo = $null
$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnetCommand)
{
    Add-Check -Id "dotnet-sdk" -Status "unavailable" -Message "dotnet was not found on PATH."
}
else
{
    $dotnetVersion = (& dotnet --version).Trim()
    $dotnetInfo = (& dotnet --info) -join [Environment]::NewLine
    Add-Check -Id "dotnet-sdk" -Status "pass" -Message "dotnet SDK $dotnetVersion is available."
}

$gitStatusLines = @()
$gitBranch = $null
$gitCommand = Get-Command git -ErrorAction SilentlyContinue
if ($null -eq $gitCommand)
{
    Add-Check -Id "git-repository" -Status "unavailable" -Message "git was not found on PATH."
}
else
{
    $gitStatusLines = @(& git -C $repositoryRoot status --short --branch 2>$null)
    if ($LASTEXITCODE -eq 0)
    {
        $gitBranch = if ($gitStatusLines.Count -gt 0) { $gitStatusLines[0] } else { "" }
        $status = if ($gitStatusLines.Count -eq 1) { "pass" } else { "warn" }
        $message = if ($status -eq "pass") { "Repository worktree is clean." } else { "Repository worktree has local changes." }
        Add-Check -Id "git-repository" -Status $status -Message $message
    }
    else
    {
        Add-Check -Id "git-repository" -Status "unavailable" -Message "git status could not read repository state."
    }
}

$contractFiles = @(
    @{ Id = "public-api-contract"; Path = "eng/public-api-contract.json" },
    @{ Id = "package-size-budgets"; Path = "eng/package-size-budgets.json" },
    @{ Id = "benchmark-contract"; Path = "benchmarks/benchmark-contract.json" },
    @{ Id = "benchmark-thresholds"; Path = "benchmarks/benchmark-thresholds.json" }
)

foreach ($contractFile in $contractFiles)
{
    Test-RelativeFile -Id $contractFile.Id -RelativePath $contractFile.Path | Out-Null
}

$validationScripts = @(
    "Validate-Packages.ps1",
    "Run-Benchmarks.ps1",
    "Test-BenchmarkThresholds.ps1",
    "Invoke-ConsumerSmoke.ps1",
    "run-native-validation.ps1",
    "Invoke-ReleaseDryRun.ps1"
)

foreach ($script in $validationScripts)
{
    Test-RelativeFile -Id "script:$script" -RelativePath "scripts/$script" | Out-Null
}

$platformProjects = @(
    "src/Videra.Platform.Windows/Videra.Platform.Windows.csproj",
    "src/Videra.Platform.Linux/Videra.Platform.Linux.csproj",
    "src/Videra.Platform.macOS/Videra.Platform.macOS.csproj"
)

foreach ($platformProject in $platformProjects)
{
    $platformName = [System.IO.Path]::GetFileNameWithoutExtension($platformProject)
    Test-RelativeFile -Id "platform:$platformName" -RelativePath $platformProject | Out-Null
}

$supportArtifactPaths = @(
    $defaultOutputRoot,
    "artifacts/benchmarks",
    "artifacts/release-dry-run",
    "artifacts/consumer-smoke",
    "artifacts/native-validation"
)

foreach ($supportArtifactPath in $supportArtifactPaths)
{
    Test-RelativeDirectory -Id "artifact:$supportArtifactPath" -RelativePath $supportArtifactPath | Out-Null
}

if ($RunPackageValidation)
{
    if ([string]::IsNullOrWhiteSpace($PackageRoot) -or [string]::IsNullOrWhiteSpace($ExpectedVersion))
    {
        Add-Validation -Id "package-validation" -Status "skip" -Message "Package validation requires -PackageRoot and -ExpectedVersion." -Script "scripts/Validate-Packages.ps1" -Prerequisites @("-PackageRoot", "-ExpectedVersion") -Artifacts @("artifacts/release-dry-run")
    }
    elseif (-not (Test-Path -LiteralPath $PackageRoot -PathType Container))
    {
        Add-Validation -Id "package-validation" -Status "unavailable" -Message "PackageRoot directory was not found." -Script "scripts/Validate-Packages.ps1" -Prerequisites @($PackageRoot, $ExpectedVersion) -Artifacts @("artifacts/release-dry-run")
    }
    else
    {
        Invoke-ValidationScript -Id "package-validation" -Script "scripts/Validate-Packages.ps1" -Arguments @("-PackageRoot", $PackageRoot, "-ExpectedVersion", $ExpectedVersion) -Prerequisites @($PackageRoot, $ExpectedVersion) -Artifacts @("artifacts/release-dry-run")
    }
}
else
{
    Add-Validation -Id "package-validation" -Status "skip" -Message "Pass -RunPackageValidation with -PackageRoot and -ExpectedVersion to invoke package validation." -Script "scripts/Validate-Packages.ps1" -Prerequisites @("-PackageRoot", "-ExpectedVersion") -Artifacts @("artifacts/release-dry-run")
}

$allBenchmarkSuites = @("Viewer", "SurfaceCharts")
$selectedBenchmarkSuites = if ($BenchmarkSuite -eq "All") { $allBenchmarkSuites } else { @($BenchmarkSuite) }
$benchmarkArtifactDirectories = @{
    Viewer = "viewer"
    SurfaceCharts = "surfacecharts"
}

foreach ($suite in $allBenchmarkSuites)
{
    $validationId = "benchmark-thresholds:$suite"
    $artifactPath = Join-Path $BenchmarkOutputRoot $benchmarkArtifactDirectories[$suite]
    $manifestPath = Join-Path $artifactPath "benchmark-manifest.json"

    if ($selectedBenchmarkSuites -notcontains $suite)
    {
        Add-Validation -Id $validationId -Status "skip" -Message "Benchmark suite was not selected." -Script "scripts/Test-BenchmarkThresholds.ps1" -Prerequisites @("-BenchmarkSuite $suite") -Artifacts @($artifactPath)
        continue
    }

    if (-not $RunBenchmarkThresholds)
    {
        Add-Validation -Id $validationId -Status "skip" -Message "Pass -RunBenchmarkThresholds to invoke threshold evaluation." -Script "scripts/Test-BenchmarkThresholds.ps1" -Prerequisites @("Run scripts/Run-Benchmarks.ps1 -Suite $suite first") -Artifacts @($artifactPath, $manifestPath)
        continue
    }

    $resolvedManifestPath = Join-Path $repositoryRoot $manifestPath
    if (-not (Test-Path -LiteralPath $resolvedManifestPath -PathType Leaf))
    {
        Add-Validation -Id $validationId -Status "unavailable" -Message "Benchmark manifest is missing. Run Run-Benchmarks.ps1 first." -Script "scripts/Test-BenchmarkThresholds.ps1" -Prerequisites @("scripts/Run-Benchmarks.ps1 -Suite $suite") -Artifacts @($artifactPath, $manifestPath)
        continue
    }

    Invoke-ValidationScript -Id $validationId -Script "scripts/Test-BenchmarkThresholds.ps1" -Arguments @("-Suite", $suite, "-OutputRoot", $BenchmarkOutputRoot) -Prerequisites @("scripts/Run-Benchmarks.ps1 -Suite $suite") -Artifacts @($artifactPath, $manifestPath)
}

if ($RunConsumerSmoke)
{
    Invoke-ValidationScript -Id "consumer-smoke:$ConsumerSmokeScenario" -Script "scripts/Invoke-ConsumerSmoke.ps1" -Arguments @("-Scenario", $ConsumerSmokeScenario, "-OutputRoot", "artifacts/consumer-smoke") -Prerequisites @("dotnet SDK", "local package build path managed by Invoke-ConsumerSmoke.ps1") -Artifacts @("artifacts/consumer-smoke", "artifacts/consumer-smoke/consumer-smoke-result.json")
}
else
{
    Add-Validation -Id "consumer-smoke:ViewerObj" -Status "skip" -Message "Pass -RunConsumerSmoke to invoke consumer smoke validation." -Script "scripts/Invoke-ConsumerSmoke.ps1" -Prerequisites @("dotnet SDK") -Artifacts @("artifacts/consumer-smoke", "artifacts/consumer-smoke/consumer-smoke-result.json")
}

if ($RunNativeValidation)
{
    Invoke-ValidationScript -Id "native-validation" -Script "scripts/run-native-validation.ps1" -Arguments @("-Platform", $NativePlatform) -Prerequisites @("matching native host for $NativePlatform") -Artifacts @("artifacts/native-validation")
}
else
{
    Add-Validation -Id "native-validation" -Status "skip" -Message "Pass -RunNativeValidation to invoke native validation on a matching host." -Script "scripts/run-native-validation.ps1" -Prerequisites @("matching native host") -Artifacts @("artifacts/native-validation")
}

Add-Validation -Id "demo-diagnostics" -Status "skip" -Message "Reference-only: attach copied diagnostics from Videra.Demo or Videra.SurfaceCharts.Demo when reporting UI/runtime issues." -Script "" -Prerequisites @("Run the relevant demo support surface") -Artifacts @($defaultOutputRoot, "artifacts/consumer-smoke/diagnostics-snapshot.txt", "artifacts/consumer-smoke/surfacecharts-support-summary.txt")

$summaryPath = Join-Path $resolvedOutputRoot "doctor-summary.txt"
$reportPath = Join-Path $resolvedOutputRoot "doctor-report.json"

$report = [ordered]@{
    schemaVersion = 1
    name = "Videra Doctor"
    repoOnly = $true
    mutatesConfiguration = $false
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    repositoryRoot = $repositoryRoot
    outputRoot = $resolvedOutputRoot
    environment = [ordered]@{
        osDescription = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        processArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
        frameworkDescription = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        powerShellVersion = $PSVersionTable.PSVersion.ToString()
        dotnetVersion = $dotnetVersion
        dotnetInfo = $dotnetInfo
    }
    git = [ordered]@{
        branch = $gitBranch
        statusLines = $gitStatusLines
    }
    contractFiles = $contractFiles
    validationScripts = $validationScripts
    platformProjects = $platformProjects
    supportArtifactPaths = $supportArtifactPaths
    checks = @($checks)
    validations = @($validations)
}

$summaryLines = [System.Collections.Generic.List[string]]::new()
$summaryLines.Add("Videra Doctor") | Out-Null
$summaryLines.Add("Mode: repo-only, non-mutating") | Out-Null
$summaryLines.Add("Repository: $repositoryRoot") | Out-Null
$summaryLines.Add("Output: $resolvedOutputRoot") | Out-Null
$summaryLines.Add("") | Out-Null
$summaryLines.Add("Checks:") | Out-Null
foreach ($check in $checks)
{
    $line = "- {0}: {1} - {2}" -f $check.id, $check.status, $check.message
    $summaryLines.Add($line) | Out-Null
}
$summaryLines.Add("") | Out-Null
$summaryLines.Add("Validations:") | Out-Null
foreach ($validation in $validations)
{
    $line = "- {0}: {1} - {2}" -f $validation.id, $validation.status, $validation.message
    $summaryLines.Add($line) | Out-Null
}
$summaryLines.Add("") | Out-Null
$summaryLines.Add("Reports:") | Out-Null
$summaryLines.Add("- $summaryPath") | Out-Null
$summaryLines.Add("- $reportPath") | Out-Null

Set-Content -LiteralPath $summaryPath -Value $summaryLines -Encoding utf8
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding utf8

Write-Host "Videra Doctor wrote $summaryPath"
Write-Host "Videra Doctor wrote $reportPath"
