param(
    [string]$ExpectedHost = "127.0.0.1",
    [int]$ExpectedPort = 3306,
    [string]$ExpectedDatabase = "Videra",
    [string]$ExpectedProjectId = "cf27bb80-40f6-4ba7-95f7-bc455a484d7b",
    [string]$DoltContainer = "dolt-sql-server"
)

$ErrorActionPreference = "Stop"

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $output = & $FilePath @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath $($Arguments -join ' ') failed with exit code $LASTEXITCODE`n$output"
    }

    return ($output -join [Environment]::NewLine)
}

Write-Host "Checking Beads context..."
$contextJson = Invoke-Checked -FilePath "bd" -Arguments @("context", "--json")
$context = $contextJson | ConvertFrom-Json

if ($context.backend -ne "dolt") { throw "Expected backend 'dolt', got '$($context.backend)'." }
if ($context.dolt_mode -ne "server") { throw "Expected dolt_mode 'server', got '$($context.dolt_mode)'." }
if ($context.server_host -ne $ExpectedHost) { throw "Expected host '$ExpectedHost', got '$($context.server_host)'." }
if ([int]$context.server_port -ne $ExpectedPort) { throw "Expected port '$ExpectedPort', got '$($context.server_port)'." }
if ($context.database -ne $ExpectedDatabase) { throw "Expected database '$ExpectedDatabase', got '$($context.database)'." }
if ($context.project_id -ne $ExpectedProjectId) { throw "Expected project id '$ExpectedProjectId', got '$($context.project_id)'." }

Write-Host "Checking bd doctor..."
Invoke-Checked -FilePath "bd" -Arguments @("doctor") | Out-Null

Write-Host "Checking worktree redirects..."
$worktrees = Invoke-Checked -FilePath "bd" -Arguments @("worktree", "list")
if ($worktrees -notmatch "\bshared\b") {
    throw "Expected main checkout to report shared Beads state."
}

if ((Test-Path ".worktrees") -and $worktrees -notmatch "redirect\s+.?\s*Videra") {
    throw "Expected existing worktrees to report redirect -> Videra."
}

Write-Host "Checking Docker-backed Dolt metadata..."
$metadata = Invoke-Checked -FilePath "docker" -Arguments @(
    "exec",
    $DoltContainer,
    "dolt",
    "sql",
    "-q",
    "select * from $ExpectedDatabase.metadata;"
)

if ($metadata -notmatch [regex]::Escape($ExpectedProjectId)) {
    throw "Dolt metadata did not contain expected project id '$ExpectedProjectId'."
}

Write-Host "Beads coordination validation passed."
