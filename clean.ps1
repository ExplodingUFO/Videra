Write-Host "Cleaning temporary files..."
Get-ChildItem -Recurse -Filter "tmpclaude-*" | Remove-Item -Recurse -Force
Write-Host "Done."
