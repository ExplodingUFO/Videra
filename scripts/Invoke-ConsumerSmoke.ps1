#!/usr/bin/env pwsh
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateSet("ViewerOnly", "ViewerObj", "ViewerGltf", "SurfaceCharts")]
    [string]$Scenario = "ViewerObj",

    [string]$Project = "",

    [string]$OutputRoot = "artifacts/consumer-smoke",

    [int]$LightingProofHoldSeconds = 0,

    [switch]$BuildOnly,

    [switch]$TreatWarningsAsErrors
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$defaultViewerProject = "smoke/Videra.ConsumerSmoke/Videra.ConsumerSmoke.csproj"
$defaultSurfaceChartsProject = "smoke/Videra.SurfaceCharts.ConsumerSmoke/Videra.SurfaceCharts.ConsumerSmoke.csproj"
$resolvedProject =
    if ([string]::IsNullOrWhiteSpace($Project))
    {
        switch ($Scenario)
        {
            "ViewerOnly" { $defaultViewerProject }
            "ViewerObj" { $defaultViewerProject }
            "ViewerGltf" { $defaultViewerProject }
            "SurfaceCharts" { $defaultSurfaceChartsProject }
        }
    }
    else
    {
        $Project
    }
$projectPath = Join-Path $root $resolvedProject
$script:resolvedProject = $resolvedProject
$outputPath = Join-Path $root $OutputRoot
$jsonPath = Join-Path $outputPath "consumer-smoke-result.json"
$snapshotPath = Join-Path $outputPath "diagnostics-snapshot.txt"
$inspectionSnapshotPath = Join-Path $outputPath "inspection-snapshot.png"
$inspectionBundlePath = Join-Path $outputPath "inspection-bundle"
$surfaceChartsSupportSummaryPath = Join-Path $outputPath "surfacecharts-support-summary.txt"
$tracePath = Join-Path $outputPath "consumer-smoke-trace.log"
$stdoutPath = Join-Path $outputPath "consumer-smoke-stdout.log"
$stderrPath = Join-Path $outputPath "consumer-smoke-stderr.log"
$environmentPath = Join-Path $outputPath "consumer-smoke-environment.txt"
$packageOutputPath = Join-Path $outputPath "packages"
$packagesCachePath = Join-Path $outputPath "global-packages"
$nugetConfigPath = Join-Path $outputPath "NuGet.Config"
$modelFormat = switch ($Scenario)
{
    "ViewerOnly" { "None" }
    "ViewerObj" { "Obj" }
    "ViewerGltf" { "Gltf" }
    "SurfaceCharts" { "None" }
}
$publicPackageProjects = @(
    "src/Videra.Core/Videra.Core.csproj",
    "src/Videra.Import.Gltf/Videra.Import.Gltf.csproj",
    "src/Videra.Import.Obj/Videra.Import.Obj.csproj",
    "src/Videra.Avalonia/Videra.Avalonia.csproj",
    "src/Videra.Platform.Windows/Videra.Platform.Windows.csproj",
    "src/Videra.Platform.Linux/Videra.Platform.Linux.csproj",
    "src/Videra.Platform.macOS/Videra.Platform.macOS.csproj",
    "src/Videra.SurfaceCharts.Core/Videra.SurfaceCharts.Core.csproj",
    "src/Videra.SurfaceCharts.Rendering/Videra.SurfaceCharts.Rendering.csproj",
    "src/Videra.SurfaceCharts.Processing/Videra.SurfaceCharts.Processing.csproj",
    "src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj"
)
$sessionEnvironment = [ordered]@{
    DISPLAY = $env:DISPLAY
    WAYLAND_DISPLAY = $env:WAYLAND_DISPLAY
    XDG_RUNTIME_DIR = $env:XDG_RUNTIME_DIR
    XDG_SESSION_TYPE = $env:XDG_SESSION_TYPE
}

function Get-CurrentPlatformPackageId
{
    if ($IsWindows)
    {
        return "Videra.Platform.Windows"
    }

    if ($IsLinux)
    {
        return "Videra.Platform.Linux"
    }

    if ($IsMacOS)
    {
        return "Videra.Platform.macOS"
    }

    return "Videra.Platform.Unknown"
}

$scenarioPackageIds = switch ($Scenario)
{
    "ViewerOnly" { @("Videra.Avalonia", (Get-CurrentPlatformPackageId)) }
    "ViewerObj" { @("Videra.Avalonia", "Videra.Import.Obj", (Get-CurrentPlatformPackageId)) }
    "ViewerGltf" { @("Videra.Avalonia", "Videra.Import.Gltf", (Get-CurrentPlatformPackageId)) }
    "SurfaceCharts" { @("Videra.SurfaceCharts.Avalonia", "Videra.SurfaceCharts.Processing") }
}
$isViewerScenario = $Scenario -ne "SurfaceCharts"

if (-not (Test-Path -LiteralPath $projectPath))
{
    throw "Consumer smoke project '$projectPath' was not found."
}

if (Test-Path -LiteralPath $outputPath)
{
    Remove-Item -LiteralPath $outputPath -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null
New-Item -ItemType Directory -Force -Path $packagesCachePath | Out-Null

function Format-ConsumerSmokeEnvironmentValue([string]$value)
{
    if ([string]::IsNullOrWhiteSpace($value))
    {
        return "<unset>"
    }

    return $value
}

function Write-ConsumerSmokeEnvironmentSnapshot
{
    $lines = foreach ($entry in $script:sessionEnvironment.GetEnumerator())
    {
        "{0}={1}" -f $entry.Key, (Format-ConsumerSmokeEnvironmentValue $entry.Value)
    }

    $lines | Set-Content -Path $script:environmentPath

    Write-Host "=== Consumer Smoke Session Environment ===" -ForegroundColor Cyan
    Get-Content -LiteralPath $script:environmentPath
}

function Show-ConsumerSmokeLogs
{
    if (Test-Path -LiteralPath $script:tracePath)
    {
        Write-Host "=== Consumer Smoke Trace ===" -ForegroundColor Yellow
        Get-Content -LiteralPath $script:tracePath
    }

    if (Test-Path -LiteralPath $script:stdoutPath)
    {
        Write-Host "=== Consumer Smoke Stdout ===" -ForegroundColor Yellow
        Get-Content -LiteralPath $script:stdoutPath
    }

    if (Test-Path -LiteralPath $script:stderrPath)
    {
        Write-Host "=== Consumer Smoke Stderr ===" -ForegroundColor Yellow
        Get-Content -LiteralPath $script:stderrPath
    }

    Write-Host "=== Consumer Smoke Output Directory ===" -ForegroundColor Yellow
    Get-ChildItem -LiteralPath $script:outputPath -Recurse | Select-Object FullName, Length
}

function Write-FallbackConsumerSmokeArtifacts([string]$failure, [int]$processExitCode)
{
    if (-not (Test-Path -LiteralPath $script:jsonPath))
    {
        $fallbackReport = [ordered]@{
            Scenario = $Scenario
            Succeeded = $false
            FrameAllReturned = $false
            FirstChartRendered = $false
            Failure = $failure
            RequestedBackend = "Unknown"
            ResolvedBackend = "Unknown"
            ActiveBackend = "Unknown"
            IsReady = $false
            IsUsingSoftwareFallback = $false
            IsFallback = $false
            FallbackReason = $null
            NativeHostBound = $false
            UsesNativeSurface = $false
            ResidentTileCount = 0
            InteractionQuality = "Unavailable"
            ResolvedDisplayServer = $null
            DisplayServerFallbackUsed = $false
            DisplayServerFallbackReason = $null
            DisplayServerCompatibility = "Unavailable because the consumer smoke app exited before managed completion."
            LastInitializationError = $failure
            DiagnosticsSnapshotPath = $script:snapshotPath
            InspectionSnapshotPath = if (Test-Path -LiteralPath $script:inspectionSnapshotPath) { $script:inspectionSnapshotPath } else { $null }
            InspectionBundlePath = if (Test-Path -LiteralPath $script:inspectionBundlePath) { $script:inspectionBundlePath } else { $null }
            SupportSummaryPath = if (Test-Path -LiteralPath $script:surfaceChartsSupportSummaryPath) { $script:surfaceChartsSupportSummaryPath } else { $null }
            ProcessExitCode = $processExitCode
            Display = Format-ConsumerSmokeEnvironmentValue $script:sessionEnvironment.DISPLAY
            WaylandDisplay = Format-ConsumerSmokeEnvironmentValue $script:sessionEnvironment.WAYLAND_DISPLAY
            XdgRuntimeDir = Format-ConsumerSmokeEnvironmentValue $script:sessionEnvironment.XDG_RUNTIME_DIR
            XdgSessionType = Format-ConsumerSmokeEnvironmentValue $script:sessionEnvironment.XDG_SESSION_TYPE
            TracePath = if (Test-Path -LiteralPath $script:tracePath) { $script:tracePath } else { $null }
            StdoutPath = if (Test-Path -LiteralPath $script:stdoutPath) { $script:stdoutPath } else { $null }
            StderrPath = if (Test-Path -LiteralPath $script:stderrPath) { $script:stderrPath } else { $null }
            EnvironmentPath = $script:environmentPath
            PackageVersion = $script:packageVersion
            PackageIds = @($script:scenarioPackageIds)
            ModelFormat = $script:modelFormat
        }

        $fallbackReport | ConvertTo-Json -Depth 4 | Set-Content -Path $script:jsonPath
    }

    if (-not (Test-Path -LiteralPath $script:snapshotPath))
    {
        @(
            "Consumer smoke fallback diagnostics"
            "Scenario: $Scenario"
            "ModelFormat: $script:modelFormat"
            "Failure: $failure"
            "ProcessExitCode: $processExitCode"
            "DISPLAY: $(Format-ConsumerSmokeEnvironmentValue $script:sessionEnvironment.DISPLAY)"
            "WAYLAND_DISPLAY: $(Format-ConsumerSmokeEnvironmentValue $script:sessionEnvironment.WAYLAND_DISPLAY)"
            "XDG_RUNTIME_DIR: $(Format-ConsumerSmokeEnvironmentValue $script:sessionEnvironment.XDG_RUNTIME_DIR)"
            "XDG_SESSION_TYPE: $(Format-ConsumerSmokeEnvironmentValue $script:sessionEnvironment.XDG_SESSION_TYPE)"
            "TracePath: $script:tracePath"
            "StdoutPath: $script:stdoutPath"
            "StderrPath: $script:stderrPath"
            "EnvironmentPath: $script:environmentPath"
        ) | Set-Content -Path $script:snapshotPath
    }
}

function Set-ConsumerSmokeReportMetadata
{
    if (-not (Test-Path -LiteralPath $script:jsonPath))
    {
        return
    }

    $report = Get-Content -Raw $script:jsonPath | ConvertFrom-Json
    $supportArtifactPaths = @(
        $script:snapshotPath,
        $script:inspectionSnapshotPath,
        $script:inspectionBundlePath,
        $script:surfaceChartsSupportSummaryPath,
        $script:tracePath,
        $script:stdoutPath,
        $script:stderrPath,
        $script:environmentPath
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    $report | Add-Member -NotePropertyName Scenario -NotePropertyValue $Scenario -Force
    $report | Add-Member -NotePropertyName PackageVersion -NotePropertyValue $script:packageVersion -Force
    $report | Add-Member -NotePropertyName PackageIds -NotePropertyValue @($script:scenarioPackageIds) -Force
    $report | Add-Member -NotePropertyName ModelFormat -NotePropertyValue $script:modelFormat -Force
    $report | Add-Member -NotePropertyName ProjectPath -NotePropertyValue $script:resolvedProject -Force
    $report | Add-Member -NotePropertyName SupportArtifactPaths -NotePropertyValue @($supportArtifactPaths) -Force

    $report | ConvertTo-Json -Depth 8 | Set-Content -Path $script:jsonPath
}

function Test-SurfaceChartsSupportSummaryContract([string]$summaryPath)
{
    $requiredPrefixes = @(
        "GeneratedUtc:",
        "EvidenceKind:",
        "EvidenceOnly:",
        "ChartControl:",
        "EnvironmentRuntime:",
        "AssemblyIdentity:",
        "BackendDisplayEnvironment:",
        "RenderingStatus"
    )

    $lines = @(Get-Content -LiteralPath $summaryPath)
    $missingPrefixes = foreach ($prefix in $requiredPrefixes)
    {
        $hasPrefix = $false
        foreach ($line in $lines)
        {
            if ($line.StartsWith($prefix, [System.StringComparison]::Ordinal))
            {
                $hasPrefix = $true
                break
            }
        }

        if (-not $hasPrefix)
        {
            $prefix
        }
    }

    if ($missingPrefixes.Count -gt 0)
    {
        throw "SurfaceCharts support summary '$summaryPath' is missing required field(s): $($missingPrefixes -join ', ')."
    }
}

Write-ConsumerSmokeEnvironmentSnapshot

Write-Host "=== Pack Public Consumer Packages ($Scenario) ===" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $packageOutputPath | Out-Null

$versionProject = Join-Path $root "src/Videra.Avalonia/Videra.Avalonia.csproj"
$resolvedVersion = (dotnet msbuild $versionProject -nologo -getProperty:Version | Select-Object -Last 1).Trim()
if ([string]::IsNullOrWhiteSpace($resolvedVersion))
{
    throw "Unable to resolve Videra consumer package version from '$versionProject'."
}

$packageVersion =
    if ($resolvedVersion.Contains('-'))
    {
        "$resolvedVersion.consumer-smoke"
    }
    else
    {
        "$resolvedVersion-consumer-smoke"
    }

$script:packageVersion = $packageVersion
$script:scenarioPackageIds = @($scenarioPackageIds)
$script:modelFormat = $modelFormat

foreach ($relativeProject in $publicPackageProjects)
{
    $fullProjectPath = Join-Path $root $relativeProject
    $packArgs = @(
        $fullProjectPath,
        "--configuration", $Configuration,
        "--output", $packageOutputPath,
        "-p:PackageVersion=$packageVersion"
    )

    if ($TreatWarningsAsErrors)
    {
        $packArgs += "-p:TreatWarningsAsErrors=true"
    }

    dotnet pack @packArgs
    if ($LASTEXITCODE -ne 0)
    {
        throw "Packing '$relativeProject' failed."
    }
}

$nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="consumer-local" value="$packageOutputPath" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
$nugetConfig | Set-Content -Path $nugetConfigPath

Write-Host "=== Consumer Smoke Restore ===" -ForegroundColor Cyan
$restoreArgs = @(
    $projectPath,
    "--configfile", $nugetConfigPath,
    "--packages", $packagesCachePath,
    "-p:VideraConsumerPackageVersion=$packageVersion",
    "-p:VideraConsumerSmokeModelFormat=$modelFormat"
)
dotnet restore @restoreArgs
if ($LASTEXITCODE -ne 0)
{
    throw "Consumer smoke restore failed."
}

Write-Host "=== Consumer Smoke Build ===" -ForegroundColor Cyan
$buildArgs = @(
    $projectPath,
    "--configuration", $Configuration,
    "--no-restore",
    "--packages", $packagesCachePath,
    "-p:VideraConsumerPackageVersion=$packageVersion",
    "-p:VideraConsumerSmokeModelFormat=$modelFormat"
)

if ($TreatWarningsAsErrors)
{
    $buildArgs += "-p:TreatWarningsAsErrors=true"
}

dotnet build @buildArgs
if ($LASTEXITCODE -ne 0)
{
    throw "Consumer smoke build failed."
}

if ($BuildOnly)
{
    Write-Host "Consumer smoke packaged build passed." -ForegroundColor Green
    Write-Host "Resolved package version: $packageVersion"
    return
}

Write-Host "=== Consumer Smoke Run ===" -ForegroundColor Cyan
$previousOutput = $env:VIDERA_CONSUMER_SMOKE_OUTPUT
$hadPreviousOutput = Test-Path Env:VIDERA_CONSUMER_SMOKE_OUTPUT
$previousTrace = $env:VIDERA_CONSUMER_SMOKE_TRACE
$hadPreviousTrace = Test-Path Env:VIDERA_CONSUMER_SMOKE_TRACE
$previousLightingProofHoldSeconds = $env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS
$hadPreviousLightingProofHoldSeconds = Test-Path Env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS
$previousScenario = $env:VIDERA_CONSUMER_SMOKE_SCENARIO
$hadPreviousScenario = Test-Path Env:VIDERA_CONSUMER_SMOKE_SCENARIO
$previousPackageVersion = $env:VIDERA_CONSUMER_SMOKE_PACKAGE_VERSION
$hadPreviousPackageVersion = Test-Path Env:VIDERA_CONSUMER_SMOKE_PACKAGE_VERSION
$previousPackageIds = $env:VIDERA_CONSUMER_SMOKE_PACKAGE_IDS
$hadPreviousPackageIds = Test-Path Env:VIDERA_CONSUMER_SMOKE_PACKAGE_IDS

try
{
    $env:VIDERA_CONSUMER_SMOKE_OUTPUT = $jsonPath
    $env:VIDERA_CONSUMER_SMOKE_TRACE = $tracePath
    $env:VIDERA_CONSUMER_SMOKE_SCENARIO = $Scenario
    $env:VIDERA_CONSUMER_SMOKE_PACKAGE_VERSION = $packageVersion
    $env:VIDERA_CONSUMER_SMOKE_PACKAGE_IDS = ($scenarioPackageIds -join ",")
    if ($LightingProofHoldSeconds -gt 0)
    {
        $env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS = $LightingProofHoldSeconds
    }
    else
    {
        Remove-Item Env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS -ErrorAction SilentlyContinue
    }
    $consumerSmokeProcess = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList @(
            "run",
            "--project",
            $projectPath,
            "--configuration",
            $Configuration,
            "--no-build",
            "--packages",
            $packagesCachePath,
            "-p:VideraConsumerPackageVersion=$packageVersion",
            "-p:VideraConsumerSmokeModelFormat=$modelFormat") `
        -Wait `
        -PassThru `
        -NoNewWindow `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath

    if ($consumerSmokeProcess.ExitCode -ne 0)
    {
        $failureMessage = "Consumer smoke app exited with code $($consumerSmokeProcess.ExitCode)."
        Write-FallbackConsumerSmokeArtifacts -failure $failureMessage -processExitCode $consumerSmokeProcess.ExitCode
        Show-ConsumerSmokeLogs
        throw "Consumer smoke app exited with code $($consumerSmokeProcess.ExitCode)."
    }
}
finally
{
    if ($hadPreviousOutput)
    {
        $env:VIDERA_CONSUMER_SMOKE_OUTPUT = $previousOutput
    }
    else
    {
        Remove-Item Env:VIDERA_CONSUMER_SMOKE_OUTPUT -ErrorAction SilentlyContinue
    }

    if ($hadPreviousTrace)
    {
        $env:VIDERA_CONSUMER_SMOKE_TRACE = $previousTrace
    }
    else
    {
        Remove-Item Env:VIDERA_CONSUMER_SMOKE_TRACE -ErrorAction SilentlyContinue
    }

    if ($hadPreviousLightingProofHoldSeconds)
    {
        $env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS = $previousLightingProofHoldSeconds
    }
    else
    {
        Remove-Item Env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS -ErrorAction SilentlyContinue
    }

    if ($hadPreviousScenario)
    {
        $env:VIDERA_CONSUMER_SMOKE_SCENARIO = $previousScenario
    }
    else
    {
        Remove-Item Env:VIDERA_CONSUMER_SMOKE_SCENARIO -ErrorAction SilentlyContinue
    }

    if ($hadPreviousPackageVersion)
    {
        $env:VIDERA_CONSUMER_SMOKE_PACKAGE_VERSION = $previousPackageVersion
    }
    else
    {
        Remove-Item Env:VIDERA_CONSUMER_SMOKE_PACKAGE_VERSION -ErrorAction SilentlyContinue
    }

    if ($hadPreviousPackageIds)
    {
        $env:VIDERA_CONSUMER_SMOKE_PACKAGE_IDS = $previousPackageIds
    }
    else
    {
        Remove-Item Env:VIDERA_CONSUMER_SMOKE_PACKAGE_IDS -ErrorAction SilentlyContinue
    }
}

$missingArtifactFailure = $null
if (-not (Test-Path -LiteralPath $jsonPath))
{
    $missingArtifactFailure = "Consumer smoke did not produce '$jsonPath'."
}
elseif (-not (Test-Path -LiteralPath $snapshotPath))
{
    $missingArtifactFailure = "Consumer smoke did not produce '$snapshotPath'."
}
elseif ($isViewerScenario)
{
    if (-not (Test-Path -LiteralPath $inspectionSnapshotPath))
    {
        $missingArtifactFailure = "Consumer smoke did not produce '$inspectionSnapshotPath'."
    }
    elseif ((Get-Item -LiteralPath $inspectionSnapshotPath).Length -le 0)
    {
        $missingArtifactFailure = "Consumer smoke produced '$inspectionSnapshotPath' but it was empty."
    }
    elseif (-not (Test-Path -LiteralPath $inspectionBundlePath))
    {
        $missingArtifactFailure = "Consumer smoke did not produce '$inspectionBundlePath'."
    }
    elseif (-not (Test-Path -LiteralPath (Join-Path $inspectionBundlePath "inspection-state.json")))
    {
        $missingArtifactFailure = "Consumer smoke bundle did not include 'inspection-state.json'."
    }
    elseif (-not (Test-Path -LiteralPath (Join-Path $inspectionBundlePath "asset-manifest.json")))
    {
        $missingArtifactFailure = "Consumer smoke bundle did not include 'asset-manifest.json'."
    }
}
elseif ($Scenario -eq "SurfaceCharts")
{
    if (-not (Test-Path -LiteralPath $surfaceChartsSupportSummaryPath))
    {
        $missingArtifactFailure = "Consumer smoke did not produce '$surfaceChartsSupportSummaryPath'."
    }
    elseif ((Get-Item -LiteralPath $surfaceChartsSupportSummaryPath).Length -le 0)
    {
        $missingArtifactFailure = "Consumer smoke produced '$surfaceChartsSupportSummaryPath' but it was empty."
    }
    else
    {
        try
        {
            Test-SurfaceChartsSupportSummaryContract -summaryPath $surfaceChartsSupportSummaryPath
        }
        catch
        {
            $missingArtifactFailure = $_.Exception.Message
        }
    }
}

if ($null -ne $missingArtifactFailure)
{
    Write-FallbackConsumerSmokeArtifacts -failure $missingArtifactFailure -processExitCode 0
    Show-ConsumerSmokeLogs
    throw $missingArtifactFailure
}

Set-ConsumerSmokeReportMetadata
$report = Get-Content -Raw $jsonPath | ConvertFrom-Json
if (-not $report.Succeeded)
{
    throw "Consumer smoke reported failure: $($report.Failure)"
}

if (-not $report.IsReady)
{
    throw "Consumer smoke completed without a ready backend diagnostics snapshot."
}

if ($isViewerScenario -and -not $report.FrameAllReturned)
{
    throw "Consumer smoke completed without a successful FrameAll result."
}

if ($Scenario -eq "SurfaceCharts" -and -not $report.FirstChartRendered)
{
    throw "Consumer smoke completed without a successful packaged first-chart render."
}

Write-Host "Consumer smoke passed ($Scenario)." -ForegroundColor Green
Write-Host "Resolved package version: $packageVersion"
if ($isViewerScenario)
{
    Write-Host "ResolvedBackend: $($report.ResolvedBackend)"
    Write-Host "ResolvedDisplayServer: $($report.ResolvedDisplayServer)"
    Write-Host "DisplayServerCompatibility: $($report.DisplayServerCompatibility)"
    Write-Host "IsUsingSoftwareFallback: $($report.IsUsingSoftwareFallback)"
}
else
{
    Write-Host "ActiveBackend: $($report.ActiveBackend)"
    Write-Host "IsFallback: $($report.IsFallback)"
    Write-Host "UsesNativeSurface: $($report.UsesNativeSurface)"
    Write-Host "ResidentTileCount: $($report.ResidentTileCount)"
    Write-Host "InteractionQuality: $($report.InteractionQuality)"
}
Write-Host "DiagnosticsSnapshot: $snapshotPath"
if ($isViewerScenario)
{
    Write-Host "InspectionSnapshot: $inspectionSnapshotPath"
    Write-Host "InspectionBundle: $inspectionBundlePath"
}
else
{
    Write-Host "Support summary: $surfaceChartsSupportSummaryPath"
}
