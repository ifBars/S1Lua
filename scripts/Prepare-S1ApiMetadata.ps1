[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')]
    [string] $Version,

    [string] $S1ApiSourceDirectory,

    [string] $WorkingDirectory
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($S1ApiSourceDirectory)) {
    $S1ApiSourceDirectory = Join-Path (Split-Path -Parent $repositoryRoot) "S1API"
}
if ([string]::IsNullOrWhiteSpace($WorkingDirectory)) {
    $WorkingDirectory = Join-Path $repositoryRoot ".ci/s1api-metadata/$Version"
}

$sourceRoot = [System.IO.Path]::GetFullPath($S1ApiSourceDirectory)
$workRoot = [System.IO.Path]::GetFullPath($WorkingDirectory)
$docfxConfig = Join-Path $sourceRoot "S1API/docfx.json"
$metadataDirectory = Join-Path $sourceRoot "S1API/api"
$assemblyDirectory = Join-Path $sourceRoot "S1API/bin/MonoMelon/netstandard2.1"
$assemblyPath = Join-Path $assemblyDirectory "S1API.dll"
$archivePath = Join-Path $workRoot "S1API-Forked-$Version.zip"
$releaseDirectory = Join-Path $workRoot "release"
$releasedAssembly = Join-Path $releaseDirectory "Mods/S1API.Mono.MelonLoader.dll"

if (-not (Test-Path -LiteralPath $docfxConfig -PathType Leaf)) {
    throw "S1API DocFX configuration does not exist: $docfxConfig"
}

New-Item -ItemType Directory -Force -Path $workRoot | Out-Null
if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf)) {
    gh release download "v$Version" `
        --repo ifBars/S1API `
        --pattern "S1API-Forked-$Version.zip" `
        --dir $workRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Could not download the published S1API $Version release archive."
    }
}

Expand-Archive -LiteralPath $archivePath -DestinationPath $releaseDirectory -Force
if (-not (Test-Path -LiteralPath $releasedAssembly -PathType Leaf)) {
    throw "Published S1API Mono assembly does not exist in the release archive: $releasedAssembly"
}

New-Item -ItemType Directory -Force -Path $assemblyDirectory | Out-Null
Copy-Item -LiteralPath $releasedAssembly -Destination $assemblyPath -Force

dotnet docfx metadata $docfxConfig
if ($LASTEXITCODE -ne 0) {
    throw "S1API DocFX metadata generation failed with exit code $LASTEXITCODE."
}
if (-not (Test-Path -LiteralPath $metadataDirectory -PathType Container)) {
    throw "S1API DocFX metadata directory was not generated: $metadataDirectory"
}

Write-Host "Prepared S1API $Version UID metadata at $metadataDirectory."
