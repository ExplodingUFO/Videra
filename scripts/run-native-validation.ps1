#!/usr/bin/env pwsh
param(
    [ValidateSet("Auto", "Linux", "macOS", "Windows")]
    [string]$Platform = "Auto",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

if ($Platform -eq "Auto")
{
    if ([OperatingSystem]::IsLinux())
    {
        $Platform = "Linux"
    }
    elseif ([OperatingSystem]::IsMacOS())
    {
        $Platform = "macOS"
    }
    elseif ([OperatingSystem]::IsWindows())
    {
        $Platform = "Windows"
    }
    else
    {
        throw "Auto-detection only supports Linux, macOS, or Windows hosts."
    }
}

switch ($Platform)
{
    "Linux"
    {
        if (-not [OperatingSystem]::IsLinux())
        {
            throw "Linux native validation must run on a Linux host."
        }

        if ([string]::IsNullOrWhiteSpace($env:DISPLAY))
        {
            throw "DISPLAY is not set. Start an X11 session or run under xvfb-run."
        }

        pwsh -File (Join-Path $root "verify.ps1") -Configuration $Configuration -IncludeNativeLinux
        break
    }

    "macOS"
    {
        if (-not [OperatingSystem]::IsMacOS())
        {
            throw "macOS native validation must run on a macOS host."
        }

        pwsh -File (Join-Path $root "verify.ps1") -Configuration $Configuration -IncludeNativeMacOS
        break
    }

    "Windows"
    {
        if (-not [OperatingSystem]::IsWindows())
        {
            throw "Windows native validation must run on a Windows host."
        }

        pwsh -File (Join-Path $root "verify.ps1") -Configuration $Configuration -IncludeNativeWindows
        break
    }
}
