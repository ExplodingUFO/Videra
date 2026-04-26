#!/usr/bin/env pwsh
param(
    [ValidateSet("Auto", "Linux", "macOS", "Windows")]
    [string]$Platform = "Auto",

    [ValidateSet("Auto", "X11", "XWayland")]
    [string]$LinuxDisplayServer = "Auto",

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

        if ($LinuxDisplayServer -eq "Auto")
        {
            $LinuxDisplayServer = if ([string]::IsNullOrWhiteSpace($env:WAYLAND_DISPLAY)) { "X11" } else { "XWayland" }
        }

        if ($LinuxDisplayServer -eq "X11")
        {
            if ([string]::IsNullOrWhiteSpace($env:DISPLAY))
            {
                throw "DISPLAY is not set. Start an X11 session or run under xvfb-run."
            }

            pwsh -File (Join-Path $root "scripts/verify.ps1") -Configuration $Configuration -IncludeNativeLinux
            if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
            break
        }

        if ([string]::IsNullOrWhiteSpace($env:DISPLAY))
        {
            throw "DISPLAY is not set. Start an XWayland session or run under xwfb-run."
        }

        if ([string]::IsNullOrWhiteSpace($env:WAYLAND_DISPLAY))
        {
            throw "WAYLAND_DISPLAY is not set. Start a Wayland session with XWayland available."
        }

        if ([string]::IsNullOrWhiteSpace($env:XDG_SESSION_TYPE))
        {
            $env:XDG_SESSION_TYPE = "wayland"
        }

        pwsh -File (Join-Path $root "scripts/verify.ps1") -Configuration $Configuration -IncludeNativeLinuxXWayland
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        break
    }

    "macOS"
    {
        if (-not [OperatingSystem]::IsMacOS())
        {
            throw "macOS native validation must run on a macOS host."
        }

        pwsh -File (Join-Path $root "scripts/verify.ps1") -Configuration $Configuration -IncludeNativeMacOS
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        break
    }

    "Windows"
    {
        if (-not [OperatingSystem]::IsWindows())
        {
            throw "Windows native validation must run on a Windows host."
        }

        pwsh -File (Join-Path $root "scripts/verify.ps1") -Configuration $Configuration -IncludeNativeWindows
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        break
    }
}
