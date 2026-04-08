Write-Host "Cleaning temporary files..."

$repoRoot = Split-Path -Parent $PSScriptRoot
Get-ChildItem -LiteralPath $repoRoot -Recurse -Force -Filter "tmpclaude-*" -ErrorAction SilentlyContinue |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Done."
