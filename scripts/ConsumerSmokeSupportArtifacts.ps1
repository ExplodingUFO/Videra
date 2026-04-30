function Get-ConsumerSmokeSupportArtifactPaths
{
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("ViewerOnly", "ViewerObj", "ViewerGltf", "SurfaceCharts")]
        [string]$Scenario,

        [Parameter(Mandatory = $true)]
        [string]$DiagnosticsSnapshotPath,

        [Parameter(Mandatory = $true)]
        [string]$InspectionSnapshotPath,

        [Parameter(Mandatory = $true)]
        [string]$InspectionBundlePath,

        [Parameter(Mandatory = $true)]
        [string]$SurfaceChartsSupportSummaryPath,

        [Parameter(Mandatory = $true)]
        [string]$SurfaceChartsSnapshotPath,

        [Parameter(Mandatory = $true)]
        [string]$TracePath,

        [Parameter(Mandatory = $true)]
        [string]$StdoutPath,

        [Parameter(Mandatory = $true)]
        [string]$StderrPath,

        [Parameter(Mandatory = $true)]
        [string]$EnvironmentPath
    )

    $scenarioSupportArtifactPaths =
        if ($Scenario -eq "SurfaceCharts")
        {
            @($SurfaceChartsSupportSummaryPath, $SurfaceChartsSnapshotPath)
        }
        else
        {
            @($InspectionSnapshotPath, $InspectionBundlePath)
        }

    return @(
        $DiagnosticsSnapshotPath
        $scenarioSupportArtifactPaths
        $TracePath
        $StdoutPath
        $StderrPath
        $EnvironmentPath
    ) | Where-Object {
        -not [string]::IsNullOrWhiteSpace($_) -and (Test-Path -LiteralPath $_)
    }
}
