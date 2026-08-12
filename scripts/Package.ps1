[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet("MonoMelon", "Il2CppMelon")]
    [string] $Runtime,

    [string] $Version,

    [switch] $SkipBuild
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = (Get-Content -LiteralPath (Join-Path $repositoryRoot "version.txt") -Raw).Trim()
}
$framework = if ($Runtime -eq "MonoMelon") { "netstandard2.1" } else { "net6.0" }
$runtimeLabel = if ($Runtime -eq "MonoMelon") { "Mono" } else { "Il2Cpp" }
$outputDirectory = Join-Path $repositoryRoot "src/S1Lua.Runtime/bin/$Runtime/$framework"
$artifactsDirectory = Join-Path $repositoryRoot "artifacts"
$packageName = "S1Lua-$Version-$runtimeLabel"
$packageRoot = Join-Path $artifactsDirectory $packageName
$archivePath = Join-Path $artifactsDirectory "$packageName.zip"
$centralPackages = [xml] (Get-Content -LiteralPath (Join-Path $repositoryRoot "Directory.Packages.props") -Raw)
$moonSharpVersion = [string] $centralPackages.Project.ItemGroup.PackageVersion.Where({ $_.Include -eq "MoonSharp" }).Version
$nugetPackagesRoot = $env:NUGET_PACKAGES
if ([string]::IsNullOrWhiteSpace($nugetPackagesRoot)) {
    $nugetPackagesRoot = Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)) ".nuget/packages"
}
$moonSharpFramework = if ($Runtime -eq "MonoMelon") { "net40-client" } else { "netstandard1.6" }
$expectedMoonSharpPath = Join-Path $nugetPackagesRoot "moonsharp/$moonSharpVersion/lib/$moonSharpFramework/MoonSharp.Interpreter.dll"

Push-Location $repositoryRoot
try {
    if (-not $SkipBuild) {
        dotnet build src/S1Lua.Runtime/S1Lua.Runtime.csproj -c $Runtime
        if ($LASTEXITCODE -ne 0) {
            throw "$Runtime build failed with exit code $LASTEXITCODE."
        }
    }

    $requiredFiles = @(
        (Join-Path $outputDirectory "S1Lua.dll"),
        (Join-Path $outputDirectory "S1Lua.Core.dll"),
        (Join-Path $outputDirectory "MoonSharp.Interpreter.dll")
    )
    foreach ($requiredFile in $requiredFiles) {
        if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
            throw "Required package file was not produced: $requiredFile"
        }
    }

    if (-not (Test-Path -LiteralPath $expectedMoonSharpPath -PathType Leaf)) {
        throw "Expected MoonSharp $moonSharpFramework package asset was not restored: $expectedMoonSharpPath"
    }
    $actualMoonSharpHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $outputDirectory "MoonSharp.Interpreter.dll")).Hash
    $expectedMoonSharpHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $expectedMoonSharpPath).Hash
    if ($actualMoonSharpHash -ne $expectedMoonSharpHash) {
        throw "$Runtime output contains the wrong MoonSharp runtime asset; expected $moonSharpFramework."
    }

    if (Test-Path -LiteralPath $packageRoot) {
        Remove-Item -LiteralPath $packageRoot -Recurse -Force
    }
    if (Test-Path -LiteralPath $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
    }

    $modsDirectory = New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot "Mods")
    $userLibsDirectory = New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot "UserLibs")
    $starterDirectory = New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot "Mods/S1Lua/MyFirstMod")
    $editorDirectory = New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot "Editor")
    $licensesDirectory = New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot "Licenses")

    Copy-Item -LiteralPath (Join-Path $outputDirectory "S1Lua.dll") -Destination (Join-Path $modsDirectory "S1Lua.dll")
    Copy-Item -LiteralPath (Join-Path $outputDirectory "S1Lua.Core.dll") -Destination (Join-Path $userLibsDirectory "S1Lua.Core.dll")
    Copy-Item -LiteralPath (Join-Path $outputDirectory "MoonSharp.Interpreter.dll") -Destination (Join-Path $userLibsDirectory "MoonSharp.Interpreter.dll")
    Copy-Item -LiteralPath "templates/MyFirstMod/mod.lua" -Destination (Join-Path $starterDirectory "mod.lua.example")
    Copy-Item -LiteralPath "generated/s1lua.lua" -Destination (Join-Path $editorDirectory "s1lua.lua")
    Copy-Item -LiteralPath "packaging/INSTALL.txt" -Destination (Join-Path $packageRoot "INSTALL.txt")
    Copy-Item -LiteralPath "LICENSE" -Destination (Join-Path $licensesDirectory "S1Lua.LICENSE.txt")
    Copy-Item -LiteralPath "licenses/MoonSharp.LICENSE.txt" -Destination (Join-Path $licensesDirectory "MoonSharp.LICENSE.txt")
    Copy-Item -LiteralPath "THIRD_PARTY_NOTICES.md" -Destination (Join-Path $licensesDirectory "THIRD_PARTY_NOTICES.md")

    Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $archivePath -CompressionLevel Optimal
    Write-Host "Created $archivePath"
}
finally {
    Pop-Location
}
