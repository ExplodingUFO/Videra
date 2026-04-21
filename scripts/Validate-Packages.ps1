#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$PackageRoot,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

$expectedPackages = @(
    "Videra.Core",
    "Videra.Import.Gltf",
    "Videra.Import.Obj",
    "Videra.Avalonia",
    "Videra.Platform.Windows",
    "Videra.Platform.Linux",
    "Videra.Platform.macOS"
)

$packageSizeBudgetContractPath = Join-Path $root "eng/package-size-budgets.json"
$expectedRepositoryUrl = "https://github.com/ExplodingUFO/Videra" # RepositoryUrl
$expectedLicenseExpression = "MIT" # PackageLicenseExpression
$expectedPackageIcon = "icon.png" # PackageIcon
$expectedPackageRoot = (Resolve-Path $PackageRoot).Path
$packageFiles = Get-ChildItem -Path $expectedPackageRoot -Recurse -Filter *.nupkg | Sort-Object Name
$symbolPackageFiles = Get-ChildItem -Path $expectedPackageRoot -Recurse -Filter *.snupkg | Sort-Object Name

function Read-JsonHashtable([string]$Path)
{
    if (-not (Test-Path -LiteralPath $Path))
    {
        throw "Required JSON file not found at '$Path'."
    }

    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable
}

function Format-ByteSize([long]$Bytes)
{
    if ($Bytes -ge 1MB)
    {
        return "{0:N2} MiB" -f ($Bytes / 1MB)
    }

    if ($Bytes -ge 1KB)
    {
        return "{0:N2} KiB" -f ($Bytes / 1KB)
    }

    return "$Bytes B"
}

function Get-PackageSizeBudgetMap([string]$Path, [string[]]$ExpectedPackageIds)
{
    $contract = Read-JsonHashtable $Path
    if ($contract.schemaVersion -ne 1)
    {
        throw "Unsupported package-size budget contract schema version '$($contract.schemaVersion)'."
    }

    $budgetMap = @{}
    foreach ($packageBudget in $contract.packages)
    {
        $packageId = [string]$packageBudget.id
        if ([string]::IsNullOrWhiteSpace($packageId))
        {
            throw "Package-size budget contract contains an entry without an id."
        }

        if ($budgetMap.ContainsKey($packageId))
        {
            throw "Package-size budget contract contains a duplicate entry for '$packageId'."
        }

        $nupkgMaxBytes = [long]$packageBudget.nupkgMaxBytes
        $snupkgMaxBytes = [long]$packageBudget.snupkgMaxBytes
        if ($nupkgMaxBytes -le 0 -or $snupkgMaxBytes -le 0)
        {
            throw "Package-size budget contract must define positive `.nupkg` and `.snupkg` byte budgets for '$packageId'."
        }

        $budgetMap[$packageId] = [ordered]@{
            nupkgMaxBytes = $nupkgMaxBytes
            snupkgMaxBytes = $snupkgMaxBytes
        }
    }

    $missingPackageIds = @($ExpectedPackageIds | Where-Object { -not $budgetMap.ContainsKey($_) })
    if ($missingPackageIds.Count -gt 0)
    {
        throw "Package-size budget contract is missing canonical public package ids: $($missingPackageIds -join ', ')."
    }

    $unexpectedPackageIds = @($budgetMap.Keys | Where-Object { $ExpectedPackageIds -notcontains $_ })
    if ($unexpectedPackageIds.Count -gt 0)
    {
        throw "Package-size budget contract contains unexpected package ids: $($unexpectedPackageIds -join ', ')."
    }

    return $budgetMap
}

$packageSizeBudgetById = Get-PackageSizeBudgetMap -Path $packageSizeBudgetContractPath -ExpectedPackageIds $expectedPackages

if ($packageFiles.Count -ne $expectedPackages.Count)
{
    throw "Expected exactly $($expectedPackages.Count) NuGet packages under '$expectedPackageRoot', found $($packageFiles.Count)."
}

$validationRoot = Join-Path $expectedPackageRoot ".validation"
Remove-Item -LiteralPath $validationRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $validationRoot | Out-Null

$metadataById = @{}
$symbolPackagesByBaseName = @{}

foreach ($symbolPackage in $symbolPackageFiles)
{
    $symbolPackagesByBaseName[$symbolPackage.BaseName] = $symbolPackage.FullName
}

foreach ($package in $packageFiles)
{
    $extractPath = Join-Path $validationRoot $package.BaseName
    Expand-Archive -LiteralPath $package.FullName -DestinationPath $extractPath -Force

    $nuspec = Get-ChildItem -Path $extractPath -Filter *.nuspec | Select-Object -First 1
    if ($null -eq $nuspec)
    {
        throw "Package '$($package.Name)' is missing a nuspec."
    }

    [xml]$nuspecXml = Get-Content -Raw $nuspec.FullName
    $metadata = $nuspecXml.package.metadata
    $packageId = [string]$metadata.id
    $packageVersion = [string]$metadata.version

    if ([string]::IsNullOrWhiteSpace($packageId))
    {
        throw "Package '$($package.Name)' is missing metadata.id."
    }

    if ($packageVersion -ne $ExpectedVersion)
    {
        throw "Package '$packageId' has version '$packageVersion' but expected '$ExpectedVersion'."
    }

    if (-not (Test-Path (Join-Path $extractPath "README.md")))
    {
        throw "Package '$packageId' is missing README.md."
    }

    if (-not (Test-Path (Join-Path $extractPath $expectedPackageIcon)))
    {
        throw "Package '$packageId' is missing package icon '$expectedPackageIcon'."
    }

    if ([string]::IsNullOrWhiteSpace([string]$metadata.description))
    {
        throw "Package '$packageId' is missing description metadata."
    }

    if ([string]::IsNullOrWhiteSpace([string]$metadata.tags))
    {
        throw "Package '$packageId' is missing tags metadata."
    }

    if ([string]::IsNullOrWhiteSpace([string]$metadata.icon))
    {
        throw "Package '$packageId' is missing PackageIcon metadata."
    }

    if ([string]$metadata.icon -ne $expectedPackageIcon)
    {
        throw "Package '$packageId' must use package icon '$expectedPackageIcon'."
    }

    $repositoryNode = $metadata.repository
    if ($null -eq $repositoryNode -or [string]::IsNullOrWhiteSpace([string]$repositoryNode.url))
    {
        throw "Package '$packageId' is missing repository metadata."
    }

    if ([string]$repositoryNode.url -ne $expectedRepositoryUrl)
    {
        throw "Package '$packageId' has repository url '$($repositoryNode.url)' but expected '$expectedRepositoryUrl'."
    }

    $licenseNode = $metadata.license
    if ($null -eq $licenseNode)
    {
        throw "Package '$packageId' is missing license metadata."
    }

    if ([string]$licenseNode.type -ne "expression" -or [string]$licenseNode.InnerText -ne $expectedLicenseExpression)
    {
        throw "Package '$packageId' must use license expression '$expectedLicenseExpression'."
    }

    $dependencies = @()
    if ($metadata.dependencies)
    {
        if ($metadata.dependencies.group)
        {
            foreach ($group in @($metadata.dependencies.group))
            {
                foreach ($dependency in @($group.dependency))
                {
                    if ($dependency.id)
                    {
                        $dependencies += [string]$dependency.id
                    }
                }
            }
        }
        elseif ($metadata.dependencies.dependency)
        {
            foreach ($dependency in @($metadata.dependencies.dependency))
            {
                if ($dependency.id)
                {
                    $dependencies += [string]$dependency.id
                }
            }
        }
    }

    $metadataById[$packageId] = [pscustomobject]@{
        Path = $package.FullName
        BaseName = $package.BaseName
        PackageSizeBytes = [long]$package.Length
        Description = [string]$metadata.description
        Tags = [string]$metadata.tags
        Dependencies = $dependencies
    }
}

foreach ($expectedPackage in $expectedPackages)
{
    if (-not $metadataById.ContainsKey($expectedPackage))
    {
        throw "Expected package '$expectedPackage' was not found in '$expectedPackageRoot'."
    }

    $packageMetadata = $metadataById[$expectedPackage]
    if (-not $symbolPackagesByBaseName.ContainsKey($packageMetadata.BaseName))
    {
        throw "Expected symbol package for '$expectedPackage' was not found in '$expectedPackageRoot'."
    }
}

foreach ($packageId in $metadataById.Keys)
{
    if ($expectedPackages -notcontains $packageId)
    {
        throw "Unexpected package '$packageId' was found in '$expectedPackageRoot'."
    }
}

$avaloniaMetadata = $metadataById["Videra.Avalonia"]
if ($avaloniaMetadata.Dependencies -notcontains "Videra.Core")
{
    throw "Videra.Avalonia must depend on Videra.Core."
}

foreach ($importDependency in @("Videra.Import.Gltf", "Videra.Import.Obj"))
{
    if ($avaloniaMetadata.Dependencies -notcontains $importDependency)
    {
        throw "Videra.Avalonia must depend on $importDependency."
    }
}

foreach ($platformDependency in @("Videra.Platform.Windows", "Videra.Platform.Linux", "Videra.Platform.macOS"))
{
    if ($avaloniaMetadata.Dependencies -contains $platformDependency)
    {
        throw "Videra.Avalonia must not hard-depend on platform package '$platformDependency'."
    }
}

foreach ($importPackage in @("Videra.Import.Gltf", "Videra.Import.Obj"))
{
    if ($metadataById[$importPackage].Dependencies -notcontains "Videra.Core")
    {
        throw "$importPackage must depend on Videra.Core."
    }
}

foreach ($platformPackage in @("Videra.Platform.Windows", "Videra.Platform.Linux", "Videra.Platform.macOS"))
{
    $platformMetadata = $metadataById[$platformPackage]
    if ($platformMetadata.Dependencies -notcontains "Videra.Core")
    {
        throw "$platformPackage must depend on Videra.Core."
    }

    foreach ($forbiddenDependency in @("Videra.Avalonia", "Videra.Import.Gltf", "Videra.Import.Obj"))
    {
        if ($platformMetadata.Dependencies -contains $forbiddenDependency)
        {
            throw "$platformPackage must not depend on $forbiddenDependency."
        }
    }

    foreach ($otherPlatformPackage in @("Videra.Platform.Windows", "Videra.Platform.Linux", "Videra.Platform.macOS"))
    {
        if ($otherPlatformPackage -eq $platformPackage)
        {
            continue
        }

        if ($platformMetadata.Dependencies -contains $otherPlatformPackage)
        {
            throw "$platformPackage must not depend on peer platform package '$otherPlatformPackage'."
        }
    }
}

if ($metadataById["Videra.Platform.Windows"].Description -notmatch "Windows")
{
    throw "Videra.Platform.Windows description must mention Windows."
}

if ($metadataById["Videra.Platform.Linux"].Description -notmatch "Linux")
{
    throw "Videra.Platform.Linux description must mention Linux."
}

if ($metadataById["Videra.Platform.macOS"].Description -notmatch "macOS")
{
    throw "Videra.Platform.macOS description must mention macOS."
}

$sizeEvaluationPath = Join-Path $validationRoot "package-size-evaluation.json"
$sizeSummaryPath = Join-Path $validationRoot "package-size-summary.txt"
$sizeEvaluations = @()
$sizeFailures = @()

foreach ($expectedPackage in $expectedPackages)
{
    $packageMetadata = $metadataById[$expectedPackage]
    $budget = $packageSizeBudgetById[$expectedPackage]
    $symbolPackagePath = $symbolPackagesByBaseName[$packageMetadata.BaseName]
    $symbolPackageSizeBytes = [long](Get-Item -LiteralPath $symbolPackagePath).Length

    $packagePassed = $packageMetadata.PackageSizeBytes -le $budget.nupkgMaxBytes
    $symbolPassed = $symbolPackageSizeBytes -le $budget.snupkgMaxBytes

    $sizeEvaluations += [ordered]@{
        id = $expectedPackage
        nupkg = [ordered]@{
            actualBytes = $packageMetadata.PackageSizeBytes
            maxBytes = $budget.nupkgMaxBytes
            deltaBytes = $packageMetadata.PackageSizeBytes - $budget.nupkgMaxBytes
            passed = $packagePassed
        }
        snupkg = [ordered]@{
            actualBytes = $symbolPackageSizeBytes
            maxBytes = $budget.snupkgMaxBytes
            deltaBytes = $symbolPackageSizeBytes - $budget.snupkgMaxBytes
            passed = $symbolPassed
        }
    }

    if (-not $packagePassed)
    {
        $sizeFailures += "[FAIL] $expectedPackage .nupkg actual $(Format-ByteSize $packageMetadata.PackageSizeBytes) exceeded max $(Format-ByteSize $budget.nupkgMaxBytes)."
    }

    if (-not $symbolPassed)
    {
        $sizeFailures += "[FAIL] $expectedPackage .snupkg actual $(Format-ByteSize $symbolPackageSizeBytes) exceeded max $(Format-ByteSize $budget.snupkgMaxBytes)."
    }
}

$sizeSummaryLines = @(
    "Package root: $expectedPackageRoot"
    "Package-size budget contract: eng/package-size-budgets.json"
    ""
)

foreach ($evaluation in $sizeEvaluations)
{
    foreach ($packageKind in @("nupkg", "snupkg"))
    {
        $kindEvaluation = $evaluation[$packageKind]
        $verdict = if ($kindEvaluation.passed) { "PASS" } else { "FAIL" }
        $sizeSummaryLines += "[{0}] {1} .{2} actual {3} vs max {4} (delta {5} bytes)" -f `
            $verdict, `
            $evaluation.id, `
            $packageKind, `
            (Format-ByteSize $kindEvaluation.actualBytes), `
            (Format-ByteSize $kindEvaluation.maxBytes), `
            $kindEvaluation.deltaBytes
    }
}

$sizeEvaluationDocument = [ordered]@{
    schemaVersion = 1
    evaluatedAtUtc = [DateTime]::UtcNow.ToString("O")
    packageRoot = $expectedPackageRoot
    budgetContractPath = "eng/package-size-budgets.json"
    packages = $sizeEvaluations
}

$sizeEvaluationDocument | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $sizeEvaluationPath
$sizeSummaryLines | Set-Content -LiteralPath $sizeSummaryPath

if ($sizeFailures.Count -gt 0)
{
    throw "Package size budgets failed:`n$($sizeFailures -join "`n")"
}

Write-Host "Validated packages in '$expectedPackageRoot' for version '$ExpectedVersion'." -ForegroundColor Green
Write-Host "Package-size evaluation written to '$sizeEvaluationPath'." -ForegroundColor Green
