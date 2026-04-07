#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$PackageRoot,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion
)

$ErrorActionPreference = "Stop"

$expectedPackages = @(
    "Videra.Core",
    "Videra.Avalonia",
    "Videra.Platform.Windows",
    "Videra.Platform.Linux",
    "Videra.Platform.macOS"
)

$expectedRepositoryUrl = "https://github.com/ExplodingUFO/Videra" # RepositoryUrl
$expectedLicenseExpression = "MIT" # PackageLicenseExpression
$expectedPackageRoot = (Resolve-Path $PackageRoot).Path
$packageFiles = Get-ChildItem -Path $expectedPackageRoot -Recurse -Filter *.nupkg | Sort-Object Name

if ($packageFiles.Count -lt $expectedPackages.Count)
{
    throw "Expected at least $($expectedPackages.Count) NuGet packages under '$expectedPackageRoot', found $($packageFiles.Count)."
}

$validationRoot = Join-Path $expectedPackageRoot ".validation"
Remove-Item -LiteralPath $validationRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $validationRoot | Out-Null

$metadataById = @{}

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

    if ([string]::IsNullOrWhiteSpace([string]$metadata.description))
    {
        throw "Package '$packageId' is missing description metadata."
    }

    if ([string]::IsNullOrWhiteSpace([string]$metadata.tags))
    {
        throw "Package '$packageId' is missing tags metadata."
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
}

$avaloniaMetadata = $metadataById["Videra.Avalonia"]
if ($avaloniaMetadata.Dependencies -notcontains "Videra.Core")
{
    throw "Videra.Avalonia must depend on Videra.Core."
}

foreach ($platformDependency in @("Videra.Platform.Windows", "Videra.Platform.Linux", "Videra.Platform.macOS"))
{
    if ($avaloniaMetadata.Dependencies -contains $platformDependency)
    {
        throw "Videra.Avalonia must not hard-depend on platform package '$platformDependency'."
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

Write-Host "Validated packages in '$expectedPackageRoot' for version '$ExpectedVersion'."
