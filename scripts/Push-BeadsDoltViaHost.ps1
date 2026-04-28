#!/usr/bin/env pwsh
param(
    [string]$ContainerName = "dolt-sql-server",
    [string]$DoltRepositoryPath = "/var/lib/dolt/Videra",
    [string]$RemoteUrl = "https://github.com/ExplodingUFO/Videra.git",
    [string]$Branch = "main"
)

$ErrorActionPreference = "Stop"

$syncName = "videra-dolt-remote-sync.git"
$hostBare = Join-Path ([System.IO.Path]::GetTempPath()) $syncName
$hostWork = Join-Path ([System.IO.Path]::GetTempPath()) "videra-dolt-remote-sync-work"
$containerBare = "/tmp/$syncName"
$beforeRef = $null

function Remove-PathIfExists([string]$Path)
{
    if (Test-Path -LiteralPath $Path)
    {
        Remove-Item -LiteralPath $Path -Recurse -Force
    }
}

try
{
    Remove-PathIfExists $hostBare
    Remove-PathIfExists $hostWork

    git init --bare $hostBare | Out-Host
    git --git-dir=$hostBare fetch $RemoteUrl refs/dolt/data:refs/dolt/data | Out-Host
    $beforeRef = (git --git-dir=$hostBare rev-parse refs/dolt/data).Trim()

    git clone $hostBare $hostWork | Out-Host
    git -C $hostWork checkout -b $Branch | Out-Host
    git -C $hostWork config user.name beads
    git -C $hostWork config user.email beads@example.local
    Set-Content -LiteralPath (Join-Path $hostWork "README.md") -Value "init"
    git -C $hostWork add README.md | Out-Host
    git -C $hostWork commit -m init | Out-Host
    git -C $hostWork push origin $Branch | Out-Host

    docker exec $ContainerName rm -rf $containerBare | Out-Host
    docker cp $hostBare "${ContainerName}:$containerBare" | Out-Host
    docker exec $ContainerName sh -lc "cd '$DoltRepositoryPath' && (dolt remote remove local-sync >/dev/null 2>&1 || true) && dolt remote add local-sync git+file://$containerBare && dolt push local-sync $Branch && dolt remote remove local-sync" | Out-Host

    Remove-PathIfExists $hostBare
    docker cp "${ContainerName}:$containerBare" $hostBare | Out-Host

    git --git-dir=$hostBare merge-base --is-ancestor $beforeRef refs/dolt/data | Out-Host
    git --git-dir=$hostBare push $RemoteUrl refs/dolt/data:refs/dolt/data | Out-Host

    $afterRef = (git ls-remote $RemoteUrl refs/dolt/data).Split("`t")[0]
    Write-Host "Dolt refs/dolt/data pushed: $beforeRef -> $afterRef"
}
finally
{
    docker exec $ContainerName rm -rf $containerBare 2>$null | Out-Null
    Remove-PathIfExists $hostBare
    Remove-PathIfExists $hostWork
}
