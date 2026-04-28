#!/usr/bin/env pwsh
param(
    [string]$IssuesPath = ".beads/issues.jsonl",
    [string]$OutputPath = "docs/ROADMAP.generated.md"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

function Resolve-RepositoryPath([string]$Path)
{
    if ([System.IO.Path]::IsPathRooted($Path))
    {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $root $Path))
}

function Get-Scalar([object]$Value)
{
    if ($null -eq $Value)
    {
        return ""
    }

    return [string]$Value
}

function Get-Labels([object]$Issue)
{
    if ($null -eq $Issue.labels)
    {
        return ""
    }

    return (@($Issue.labels) | Sort-Object { [string]$_ }) -join ", "
}

function Format-RoadmapIssue([object]$Issue)
{
    $labels = Get-Labels $Issue
    $suffix = if ([string]::IsNullOrWhiteSpace($labels)) { "" } else { "; labels: $labels" }
    return "- **$($Issue.id)** - $($Issue.title) ($($Issue.issue_type), P$($Issue.priority), $($Issue.status)$suffix)"
}

function Add-IssueSection([string[]]$Lines, [string]$Title, [object[]]$Issues)
{
    $Lines += "## $Title"
    $Lines += ""

    if ($Issues.Count -eq 0)
    {
        $Lines += "_No matching beads in the exported snapshot._"
    }
    else
    {
        foreach ($issue in $Issues)
        {
            $Lines += Format-RoadmapIssue $issue
        }
    }

    $Lines += ""
    return $Lines
}

function Test-HasOpenBlockingDependency([object]$Issue, [hashtable]$IssueById)
{
    if ($null -eq $Issue.dependencies)
    {
        return $false
    }

    foreach ($dependency in @($Issue.dependencies))
    {
        if ((Get-Scalar $dependency.type) -ne "blocks")
        {
            continue
        }

        $dependencyId = Get-Scalar $dependency.depends_on_id
        if ([string]::IsNullOrWhiteSpace($dependencyId))
        {
            continue
        }

        if (-not $IssueById.ContainsKey($dependencyId))
        {
            return $true
        }

        if ((Get-Scalar $IssueById[$dependencyId].status) -ne "closed")
        {
            return $true
        }
    }

    return $false
}

$issuesPathFull = Resolve-RepositoryPath $IssuesPath
$outputPathFull = Resolve-RepositoryPath $OutputPath

if (-not (Test-Path -LiteralPath $issuesPathFull -PathType Leaf))
{
    throw "Beads export '$issuesPathFull' was not found."
}

$issues = @()
foreach ($line in Get-Content -LiteralPath $issuesPathFull)
{
    if ([string]::IsNullOrWhiteSpace($line))
    {
        continue
    }

    $item = $line | ConvertFrom-Json
    if ((Get-Scalar $item._type) -eq "issue")
    {
        $issues += $item
    }
}

$issueById = @{}
foreach ($issue in $issues)
{
    $issueById[(Get-Scalar $issue.id)] = $issue
}

$openIssues = @($issues | Where-Object { (Get-Scalar $_.status) -ne "closed" })
$active = @($openIssues | Where-Object { (Get-Scalar $_.status) -eq "in_progress" })
$ready = @($openIssues | Where-Object { (Get-Scalar $_.status) -eq "open" -and [int]$_.priority -le 2 -and -not (Test-HasOpenBlockingDependency $_ $issueById) })
$blocked = @($openIssues | Where-Object { (Get-Scalar $_.status) -eq "open" -and [int]$_.priority -le 2 -and (Test-HasOpenBlockingDependency $_ $issueById) })
$backlog = @($openIssues | Where-Object { (Get-Scalar $_.status) -eq "open" -and [int]$_.priority -gt 2 })
$recentlyClosed = @(
    $issues |
        Where-Object { (Get-Scalar $_.status) -eq "closed" } |
        Sort-Object @{ Expression = { Get-Scalar $_.closed_at }; Descending = $true }, @{ Expression = { Get-Scalar $_.id }; Descending = $false } |
        Select-Object -First 10
)

$roadmapOrder = @(
    @{ Expression = { [int]$_.priority }; Descending = $false },
    @{ Expression = { Get-Scalar $_.issue_type }; Descending = $false },
    @{ Expression = { Get-Scalar $_.title }; Descending = $false },
    @{ Expression = { Get-Scalar $_.id }; Descending = $false }
)

$active = @($active | Sort-Object $roadmapOrder)
$ready = @($ready | Sort-Object $roadmapOrder)
$blocked = @($blocked | Sort-Object $roadmapOrder)
$backlog = @($backlog | Sort-Object $roadmapOrder)

$lines = @(
    "# Videra Public Roadmap"
    ""
    'Generated from `.beads/issues.jsonl`.'
    ""
    "Beads remains the single authoritative task tracker. This file is a read-only public summary for readers who do not have the Docker-backed Dolt Beads service running."
    ""
    "Regenerate with:"
    ""
    '```powershell'
    "pwsh -File ./scripts/Export-BeadsRoadmap.ps1"
    '```'
    ""
)

$lines = Add-IssueSection $lines "Active" $active
$lines = Add-IssueSection $lines "Ready" $ready
$lines = Add-IssueSection $lines "Blocked" $blocked
$lines = Add-IssueSection $lines "Backlog" $backlog
$lines = Add-IssueSection $lines "Recently Closed" $recentlyClosed

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputPathFull) | Out-Null
$lines | Set-Content -LiteralPath $outputPathFull

Write-Host "Beads public roadmap written to '$outputPathFull'."
