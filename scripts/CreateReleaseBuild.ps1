[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GodotExe,

    [string[]]$Presets = @("Windows Desktop", "Linux"),

    [string]$Version = "",

    [ValidateSet("demo", "export")]
    [string]$Edition = "",

    [switch]$SkipTests,

    [switch]$SkipZip
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptRoot
Set-Location $RepoRoot

function Get-ProjectSettingValue {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Pattern
    )

    $line = Select-String -Path (Join-Path $RepoRoot "project.godot") -Pattern $Pattern | Select-Object -First 1
    if ($null -eq $line) {
        throw "Could not find project setting matching pattern '$Pattern'."
    }

    $parts = $line.Line.Split("=", 2)
    if ($parts.Length -ne 2) {
        throw "Could not parse project setting line '$($line.Line)'."
    }

    return $parts[1].Trim().Trim('"')
}

function Set-ProjectSettingValue {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $projectPath = Join-Path $RepoRoot "project.godot"
    $content = Get-Content -Raw $projectPath
    $escapedPath = [Regex]::Escape($Path)
    $pattern = "(?m)^$escapedPath=.*$"
    $replacement = "$Path=`"$Value`""
    $updated = [Regex]::Replace($content, $pattern, $replacement, 1)
    if ($updated -eq $content) {
        throw "Could not update project setting '$Path'."
    }

    Set-Content -Path $projectPath -Value $updated -Encoding UTF8
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = Get-ProjectSettingValue -Pattern '^config/version='
}

$UserFacingVersion = Get-ProjectSettingValue -Pattern '^config/user_facing_version='

$ProjectFilePath = Join-Path $RepoRoot "project.godot"
$OriginalProjectFile = Get-Content -Raw $ProjectFilePath
$ConfiguredReleaseChannel = Get-ProjectSettingValue -Pattern '^config/release_channel='
$RequestedEdition = $ConfiguredReleaseChannel
if (-not [string]::IsNullOrWhiteSpace($Edition)) {
    $RequestedEdition = $Edition
}

if ($RequestedEdition -ne "export") {
    $RequestedEdition = "demo"
}

$DisplayVersion = "$UserFacingVersion"
$EditionLabel = "demo"
if ($RequestedEdition -eq "export") {
    $EditionLabel = "export"
}

$ArtifactLabel = $DisplayVersion
$OutputFolderName = $DisplayVersion
if ($EditionLabel -eq "export") {
    $ArtifactLabel = "$DisplayVersion-export"
    $OutputFolderName = "$DisplayVersion-export"
}

$OutputRoot = Join-Path $RepoRoot "release\$Version\$OutputFolderName"

$PresetMap = @{
    "Windows Desktop" = @{
        Folder = "windows"
        Entry = "StarGen.exe"
        Zip = "StarGen-$ArtifactLabel-windows.zip"
        ButlerChannel = "windows"
    }
    "Linux" = @{
        Folder = "linux"
        Entry = "stargen.x86_64"
        Zip = "StarGen-$ArtifactLabel-linux.zip"
        ButlerChannel = "linux"
    }
    "Web" = @{
        Folder = "web"
        Entry = "index.html"
        Zip = "StarGen-$ArtifactLabel-web.zip"
        ButlerChannel = "web"
    }
}

Write-Host ""
Write-Host "StarGen release build"
Write-Host "Internal version: $Version"
Write-Host "User-facing version: $DisplayVersion"
Write-Host "Edition channel: $EditionLabel"
Write-Host "Artifact label: $ArtifactLabel"
Write-Host "Output root: $OutputRoot"
Write-Host ""

try {
    if ($ConfiguredReleaseChannel -ne $RequestedEdition) {
        Write-Host "Temporarily setting release channel to '$RequestedEdition'..."
        Set-ProjectSettingValue -Path "config/release_channel" -Value $RequestedEdition
    }

    Write-Host "Building .NET solution..."
    & dotnet build StarGen.sln
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE."
    }

    if (-not $SkipTests) {
        Write-Host ""
        Write-Host "Running headless harness..."
        & $GodotExe --headless --path . --script res://Tests/RunTestsHeadless.gd
        if ($LASTEXITCODE -ne 0) {
            throw "Headless harness failed with exit code $LASTEXITCODE."
        }
    }

    New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null

    $BuiltArtifacts = @()

    foreach ($preset in $Presets) {
        if (-not $PresetMap.ContainsKey($preset)) {
            throw "Unsupported preset '$preset'. Supported values: $($PresetMap.Keys -join ', ')."
        }

        $presetInfo = $PresetMap[$preset]
        $platformFolder = Join-Path $OutputRoot $presetInfo.Folder
        New-Item -ItemType Directory -Force -Path $platformFolder | Out-Null

        $entryPath = Join-Path $platformFolder $presetInfo.Entry

        Write-Host ""
        Write-Host "Exporting preset '$preset' to '$entryPath'..."
        & $GodotExe --headless --path . --export-release $preset $entryPath
        if ($LASTEXITCODE -ne 0) {
            throw "Export for preset '$preset' failed with exit code $LASTEXITCODE."
        }

        if (-not (Test-Path $entryPath)) {
            throw "Expected export entry '$entryPath' was not created."
        }

        $artifact = [PSCustomObject]@{
            Preset = $preset
            Folder = $platformFolder
            Entry = $entryPath
            Zip = Join-Path $OutputRoot $presetInfo.Zip
            ButlerChannel = $presetInfo.ButlerChannel
        }

        if (-not $SkipZip) {
            if (Test-Path $artifact.Zip) {
                Remove-Item -Force $artifact.Zip
            }

            Write-Host "Creating archive '$($artifact.Zip)'..."
            Compress-Archive -Path (Join-Path $platformFolder '*') -DestinationPath $artifact.Zip -Force
        }

        $BuiltArtifacts += $artifact
    }

    Write-Host ""
    Write-Host "Build complete."
    Write-Host ""
    Write-Host "Artifacts:"
    foreach ($artifact in $BuiltArtifacts) {
        Write-Host " - $($artifact.Preset): $($artifact.Folder)"
        if (-not $SkipZip) {
            Write-Host "   Zip: $($artifact.Zip)"
        }
    }

    Write-Host ""
    Write-Host "Suggested itch uploads:"
    foreach ($artifact in $BuiltArtifacts) {
        Write-Host " butler push `"$($artifact.Folder)`" your-itch-user/stargen:$($artifact.ButlerChannel)"
    }
}
finally {
    $currentProjectFile = Get-Content -Raw $ProjectFilePath
    if ($currentProjectFile -ne $OriginalProjectFile) {
        Set-Content -Path $ProjectFilePath -Value $OriginalProjectFile -Encoding UTF8
    }
}
