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

function Get-CheckStatus([string]$Id)
{
    foreach ($check in $checks)
    {
        if ($check.id -eq $Id)
        {
            return $check.status
        }
    }

    return "unavailable"
}

function Test-EvidencePath([string]$RelativePath, [string]$Kind)
{
    $path = Join-Path $repositoryRoot $RelativePath
    if ($Kind -eq "directory")
    {
        if (Test-Path -LiteralPath $path -PathType Container)
        {
            return "present"
        }

        return "missing"
    }

    if (Test-Path -LiteralPath $path -PathType Leaf)
    {
        return "present"
    }

    return "missing"
}

function New-EvidenceArtifact(
    [string]$Id,
    [string]$Category,
    [string]$Path,
    [string]$ProducedBy,
    [string]$Kind = "file")
{
    [ordered]@{
        id = $Id
        category = $Category
        path = $Path
        kind = $Kind
        producedBy = $ProducedBy
        status = Test-EvidencePath -RelativePath $Path -Kind $Kind
    }
}

function Get-PerformanceLabVisualEvidence
{
    $manifestPath = "artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json"
    $resolvedManifestPath = Join-Path $repositoryRoot $manifestPath

    if (-not (Test-Path -LiteralPath $resolvedManifestPath -PathType Leaf))
    {
        return [ordered]@{
            status = "missing"
            message = "Performance Lab visual evidence manifest is missing. Run scripts/Invoke-PerformanceLabVisualEvidence.ps1 when visual evidence is needed."
            manifestPath = $manifestPath
            summaryPath = ""
            generatedAtUtc = $null
            schemaVersion = $null
            evidenceKind = "PerformanceLabVisualEvidence"
            screenshotPaths = @()
            diagnosticsPaths = @()
            entries = @()
        }
    }

    try
    {
        $manifest = Get-Content -LiteralPath $resolvedManifestPath -Raw | ConvertFrom-Json
    }
    catch
    {
        return [ordered]@{
            status = "unavailable"
            message = "Performance Lab visual evidence manifest could not be parsed."
            manifestPath = $manifestPath
            summaryPath = ""
            generatedAtUtc = $null
            schemaVersion = $null
            evidenceKind = "PerformanceLabVisualEvidence"
            screenshotPaths = @()
            diagnosticsPaths = @()
            entries = @()
        }
    }

    $entries = @($manifest.entries | ForEach-Object {
        [ordered]@{
            id = [string]$_.id
            scenarioType = [string]$_.scenarioType
            displayName = [string]$_.displayName
            status = [string]$_.status
            pngPath = if ($null -eq $_.pngPath) { "" } else { [string]$_.pngPath }
            diagnosticsPath = [string]$_.diagnosticsPath
        }
    })

    $screenshotPaths = @($entries | Where-Object { -not [string]::IsNullOrWhiteSpace($_.pngPath) } | ForEach-Object { $_.pngPath })
    $diagnosticsPaths = @($entries | Where-Object { -not [string]::IsNullOrWhiteSpace($_.diagnosticsPath) } | ForEach-Object { $_.diagnosticsPath })
    $manifestStatus = [string]$manifest.status
    $status = if ($manifestStatus -eq "unavailable") { "unavailable" } else { "present" }
    $message = if ($status -eq "unavailable") { "Performance Lab visual evidence manifest records unavailable capture state." } else { "Performance Lab visual evidence bundle is present." }

    return [ordered]@{
        status = $status
        captureStatus = $manifestStatus
        message = $message
        manifestPath = $manifestPath
        summaryPath = [string]$manifest.summaryPath
        generatedAtUtc = [string]$manifest.generatedUtc
        schemaVersion = $manifest.schemaVersion
        evidenceKind = [string]$manifest.evidenceKind
        screenshotPaths = $screenshotPaths
        diagnosticsPaths = $diagnosticsPaths
        entries = $entries
    }
}

function Get-SurfaceChartsSupportReport
{
    $summaryPath = "artifacts/consumer-smoke/surfacecharts-support-summary.txt"
    $resolvedSummaryPath = Join-Path $repositoryRoot $summaryPath

    if (-not (Test-Path -LiteralPath $resolvedSummaryPath -PathType Leaf))
    {
        return [ordered]@{
            status = "missing"
            message = "SurfaceCharts support summary is missing. Run SurfaceCharts consumer smoke or copy the Videra.SurfaceCharts.Demo support summary when chart support evidence is needed."
            supportSummaryPath = $summaryPath
            generatedAtUtc = $null
            evidenceKind = $null
            evidenceOnly = $null
            chartControl = ""
            environmentRuntime = ""
            assemblyIdentity = ""
            backendDisplayEnvironment = ""
            renderingStatusPresent = $false
            isStructuredComplete = $false
            missingFields = @("GeneratedUtc", "EvidenceKind", "EvidenceOnly", "ChartControl", "EnvironmentRuntime", "AssemblyIdentity", "BackendDisplayEnvironment", "RenderingStatus")
        }
    }

    try
    {
        $lines = @(Get-Content -LiteralPath $resolvedSummaryPath)
    }
    catch
    {
        return [ordered]@{
            status = "unavailable"
            message = "SurfaceCharts support summary could not be read."
            supportSummaryPath = $summaryPath
            generatedAtUtc = $null
            evidenceKind = $null
            evidenceOnly = $null
            chartControl = ""
            environmentRuntime = ""
            assemblyIdentity = ""
            backendDisplayEnvironment = ""
            renderingStatusPresent = $false
            isStructuredComplete = $false
            missingFields = @("GeneratedUtc", "EvidenceKind", "EvidenceOnly", "ChartControl", "EnvironmentRuntime", "AssemblyIdentity", "BackendDisplayEnvironment", "RenderingStatus")
        }
    }

    function Get-SurfaceChartsSupportValue([string]$Prefix)
    {
        foreach ($line in $lines)
        {
            if ($line.StartsWith($Prefix, [System.StringComparison]::Ordinal))
            {
                return $line.Substring($Prefix.Length).Trim()
            }
        }

        return ""
    }

    $evidenceOnlyText = Get-SurfaceChartsSupportValue -Prefix "EvidenceOnly:"
    $generatedAtUtc = Get-SurfaceChartsSupportValue -Prefix "GeneratedUtc:"
    $evidenceKind = Get-SurfaceChartsSupportValue -Prefix "EvidenceKind:"
    $chartControl = Get-SurfaceChartsSupportValue -Prefix "ChartControl:"
    $environmentRuntime = Get-SurfaceChartsSupportValue -Prefix "EnvironmentRuntime:"
    $assemblyIdentity = Get-SurfaceChartsSupportValue -Prefix "AssemblyIdentity:"
    $backendDisplayEnvironment = Get-SurfaceChartsSupportValue -Prefix "BackendDisplayEnvironment:"
    $renderingStatusPresent = ($lines | Where-Object { $_.StartsWith("RenderingStatus", [System.StringComparison]::Ordinal) }).Count -gt 0
    $missingFields = @()
    if ([string]::IsNullOrWhiteSpace($generatedAtUtc)) { $missingFields += "GeneratedUtc" }
    if ([string]::IsNullOrWhiteSpace($evidenceKind)) { $missingFields += "EvidenceKind" }
    if ([string]::IsNullOrWhiteSpace($evidenceOnlyText)) { $missingFields += "EvidenceOnly" }
    if ([string]::IsNullOrWhiteSpace($chartControl)) { $missingFields += "ChartControl" }
    if ([string]::IsNullOrWhiteSpace($environmentRuntime)) { $missingFields += "EnvironmentRuntime" }
    if ([string]::IsNullOrWhiteSpace($assemblyIdentity)) { $missingFields += "AssemblyIdentity" }
    if ([string]::IsNullOrWhiteSpace($backendDisplayEnvironment)) { $missingFields += "BackendDisplayEnvironment" }
    if (-not $renderingStatusPresent) { $missingFields += "RenderingStatus" }

    return [ordered]@{
        status = "present"
        message = if ($missingFields.Count -eq 0) { "SurfaceCharts support summary is present." } else { "SurfaceCharts support summary is present but missing structured field(s): $($missingFields -join ', ')." }
        supportSummaryPath = $summaryPath
        generatedAtUtc = $generatedAtUtc
        evidenceKind = $evidenceKind
        evidenceOnly = if ([string]::IsNullOrWhiteSpace($evidenceOnlyText)) { $null } else { $evidenceOnlyText.StartsWith("true", [System.StringComparison]::OrdinalIgnoreCase) }
        chartControl = $chartControl
        environmentRuntime = $environmentRuntime
        assemblyIdentity = $assemblyIdentity
        backendDisplayEnvironment = $backendDisplayEnvironment
        renderingStatusPresent = $renderingStatusPresent
        isStructuredComplete = $missingFields.Count -eq 0
        missingFields = @($missingFields)
    }
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
    "Invoke-ReleaseDryRun.ps1",
    "Invoke-PublicReleasePreflight.ps1"
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
    "artifacts/native-validation",
    "artifacts/performance-lab-visual-evidence"
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
$performanceLabVisualEvidence = Get-PerformanceLabVisualEvidence
$surfaceChartsSupportReport = Get-SurfaceChartsSupportReport

$evidencePacket = [ordered]@{
    repository = [ordered]@{
        root = $repositoryRoot
        branch = $gitBranch
        status = Get-CheckStatus -Id "git-repository"
    }
    machine = [ordered]@{
        osDescription = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        processArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
        frameworkDescription = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        powerShellVersion = $PSVersionTable.PSVersion.ToString()
        dotnetVersion = $dotnetVersion
        dotnetStatus = Get-CheckStatus -Id "dotnet-sdk"
    }
    packageContracts = @($contractFiles | ForEach-Object {
        [ordered]@{
            id = [string]$_.Id
            path = [string]$_.Path
            status = Get-CheckStatus -Id ([string]$_.Id)
        }
    })
    validationScripts = @($validationScripts | ForEach-Object {
        [ordered]@{
            id = [string]$_
            path = "scripts/$_"
            status = Get-CheckStatus -Id "script:$_"
        }
    })
    supportArtifacts = @($supportArtifactPaths | ForEach-Object {
        [ordered]@{
            id = [string]$_
            path = [string]$_
            kind = "directory"
            status = Get-CheckStatus -Id "artifact:$_"
        }
    })
    outputArtifacts = @(
        [ordered]@{
            id = "doctor-summary"
            path = $summaryPath
            kind = "file"
            producedBy = "scripts/Invoke-VideraDoctor.ps1"
        },
        [ordered]@{
            id = "doctor-report"
            path = $reportPath
            kind = "file"
            producedBy = "scripts/Invoke-VideraDoctor.ps1"
        }
    )
    performanceLabVisualEvidence = $performanceLabVisualEvidence
    surfaceChartsSupportReport = $surfaceChartsSupportReport
    artifactReferences = @(
        (New-EvidenceArtifact -Id "release-dry-run-summary-json" -Category "release-dry-run" -Path "artifacts/release-dry-run/release-dry-run-summary.json" -ProducedBy "scripts/Invoke-ReleaseDryRun.ps1"),
        (New-EvidenceArtifact -Id "release-dry-run-summary-text" -Category "release-dry-run" -Path "artifacts/release-dry-run/release-dry-run-summary.txt" -ProducedBy "scripts/Invoke-ReleaseDryRun.ps1"),
        (New-EvidenceArtifact -Id "release-candidate-evidence-index-json" -Category "release-dry-run" -Path "artifacts/release-dry-run/release-candidate-evidence-index.json" -ProducedBy "scripts/New-ReleaseCandidateEvidenceIndex.ps1"),
        (New-EvidenceArtifact -Id "release-candidate-evidence-index-text" -Category "release-dry-run" -Path "artifacts/release-dry-run/release-candidate-evidence-index.txt" -ProducedBy "scripts/New-ReleaseCandidateEvidenceIndex.ps1"),
        (New-EvidenceArtifact -Id "package-size-evaluation" -Category "package-validation" -Path "artifacts/release-dry-run/packages/.validation/package-size-evaluation.json" -ProducedBy "scripts/Validate-Packages.ps1"),
        (New-EvidenceArtifact -Id "package-size-summary" -Category "package-validation" -Path "artifacts/release-dry-run/packages/.validation/package-size-summary.txt" -ProducedBy "scripts/Validate-Packages.ps1"),
        (New-EvidenceArtifact -Id "viewer-benchmark-manifest" -Category "benchmark" -Path "artifacts/benchmarks/viewer/benchmark-manifest.json" -ProducedBy "scripts/Run-Benchmarks.ps1"),
        (New-EvidenceArtifact -Id "surfacecharts-benchmark-manifest" -Category "benchmark" -Path "artifacts/benchmarks/surfacecharts/benchmark-manifest.json" -ProducedBy "scripts/Run-Benchmarks.ps1"),
        (New-EvidenceArtifact -Id "consumer-smoke-result" -Category "consumer-smoke" -Path "artifacts/consumer-smoke/consumer-smoke-result.json" -ProducedBy "scripts/Invoke-ConsumerSmoke.ps1"),
        (New-EvidenceArtifact -Id "consumer-smoke-diagnostics" -Category "consumer-smoke" -Path "artifacts/consumer-smoke/diagnostics-snapshot.txt" -ProducedBy "scripts/Invoke-ConsumerSmoke.ps1"),
        (New-EvidenceArtifact -Id "consumer-smoke-surfacecharts-support" -Category "consumer-smoke" -Path "artifacts/consumer-smoke/surfacecharts-support-summary.txt" -ProducedBy "scripts/Invoke-ConsumerSmoke.ps1"),
        (New-EvidenceArtifact -Id "native-validation-root" -Category "native-validation" -Path "artifacts/native-validation" -ProducedBy "scripts/run-native-validation.ps1" -Kind "directory"),
        (New-EvidenceArtifact -Id "public-release-preflight-summary-json" -Category "public-release-preflight" -Path "artifacts/public-release-preflight/public-release-preflight-summary.json" -ProducedBy "scripts/Invoke-PublicReleasePreflight.ps1"),
        (New-EvidenceArtifact -Id "public-release-preflight-summary-text" -Category "public-release-preflight" -Path "artifacts/public-release-preflight/public-release-preflight-summary.txt" -ProducedBy "scripts/Invoke-PublicReleasePreflight.ps1"),
        (New-EvidenceArtifact -Id "demo-diagnostics-snapshot" -Category "demo-support" -Path "artifacts/consumer-smoke/diagnostics-snapshot.txt" -ProducedBy "Videra.Demo"),
        (New-EvidenceArtifact -Id "surfacecharts-support-summary" -Category "demo-support" -Path "artifacts/consumer-smoke/surfacecharts-support-summary.txt" -ProducedBy "Videra.SurfaceCharts.Demo"),
        (New-EvidenceArtifact -Id "performance-lab-visual-evidence-manifest" -Category "performance-lab-visual-evidence" -Path "artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json" -ProducedBy "scripts/Invoke-PerformanceLabVisualEvidence.ps1"),
        (New-EvidenceArtifact -Id "performance-lab-visual-evidence-summary" -Category "performance-lab-visual-evidence" -Path "artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-summary.txt" -ProducedBy "scripts/Invoke-PerformanceLabVisualEvidence.ps1")
    )
}

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
    evidencePacket = $evidencePacket
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
$summaryLines.Add("Evidence packet:") | Out-Null
foreach ($artifact in $evidencePacket.artifactReferences)
{
    $line = "- {0}/{1}: {2} - {3}" -f $artifact.category, $artifact.id, $artifact.status, $artifact.path
    $summaryLines.Add($line) | Out-Null
}
$summaryLines.Add("") | Out-Null
$summaryLines.Add("Performance Lab visual evidence:") | Out-Null
$summaryLines.Add(("- status: {0} - {1}" -f $performanceLabVisualEvidence.status, $performanceLabVisualEvidence.message)) | Out-Null
$summaryLines.Add(("- manifest: {0}" -f $performanceLabVisualEvidence.manifestPath)) | Out-Null
if (-not [string]::IsNullOrWhiteSpace($performanceLabVisualEvidence.summaryPath))
{
    $summaryLines.Add(("- summary: {0}" -f $performanceLabVisualEvidence.summaryPath)) | Out-Null
}
foreach ($screenshotPath in $performanceLabVisualEvidence.screenshotPaths)
{
    $summaryLines.Add(("- screenshot: {0}" -f $screenshotPath)) | Out-Null
}
foreach ($diagnosticsPath in $performanceLabVisualEvidence.diagnosticsPaths)
{
    $summaryLines.Add(("- diagnostics: {0}" -f $diagnosticsPath)) | Out-Null
}
$summaryLines.Add("") | Out-Null
$summaryLines.Add("SurfaceCharts support report:") | Out-Null
$summaryLines.Add(("- status: {0} - {1}" -f $surfaceChartsSupportReport.status, $surfaceChartsSupportReport.message)) | Out-Null
$summaryLines.Add(("- summary: {0}" -f $surfaceChartsSupportReport.supportSummaryPath)) | Out-Null
$summaryLines.Add(("- structured complete: {0}" -f $surfaceChartsSupportReport.isStructuredComplete)) | Out-Null
if ($surfaceChartsSupportReport.missingFields.Count -gt 0)
{
    $summaryLines.Add(("- missing fields: {0}" -f ($surfaceChartsSupportReport.missingFields -join ", "))) | Out-Null
}
if (-not [string]::IsNullOrWhiteSpace($surfaceChartsSupportReport.evidenceKind))
{
    $summaryLines.Add(("- evidence kind: {0}" -f $surfaceChartsSupportReport.evidenceKind)) | Out-Null
}
if (-not [string]::IsNullOrWhiteSpace($surfaceChartsSupportReport.chartControl))
{
    $summaryLines.Add(("- chart control: {0}" -f $surfaceChartsSupportReport.chartControl)) | Out-Null
}
if (-not [string]::IsNullOrWhiteSpace($surfaceChartsSupportReport.assemblyIdentity))
{
    $summaryLines.Add(("- assembly identity: {0}" -f $surfaceChartsSupportReport.assemblyIdentity)) | Out-Null
}
if (-not [string]::IsNullOrWhiteSpace($surfaceChartsSupportReport.backendDisplayEnvironment))
{
    $summaryLines.Add(("- backend/display environment: {0}" -f $surfaceChartsSupportReport.backendDisplayEnvironment)) | Out-Null
}
$summaryLines.Add("") | Out-Null
$summaryLines.Add("Reports:") | Out-Null
$summaryLines.Add("- $summaryPath") | Out-Null
$summaryLines.Add("- $reportPath") | Out-Null

Set-Content -LiteralPath $summaryPath -Value $summaryLines -Encoding utf8
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding utf8

Write-Host "Videra Doctor wrote $summaryPath"
Write-Host "Videra Doctor wrote $reportPath"
