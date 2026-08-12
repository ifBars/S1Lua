[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $S1ApiReleaseDirectory,
    [Parameter(Mandatory)] [string] $MonoAssembliesDirectory,
    [Parameter(Mandatory)] [string] $Il2CppAssembliesDirectory
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

function Resolve-RequiredFile {
    param([Parameter(Mandatory)] [string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required CI input does not exist: $Path"
    }
    return (Resolve-Path -LiteralPath $Path).Path
}

function ConvertTo-XmlText {
    param([Parameter(Mandatory)] [string] $Value)

    return [System.Security.SecurityElement]::Escape($Value)
}

$monoS1Api = Resolve-RequiredFile (Join-Path $S1ApiReleaseDirectory "Mods/S1API.Mono.MelonLoader.dll")
$il2CppS1Api = Resolve-RequiredFile (Join-Path $S1ApiReleaseDirectory "Mods/S1API.Il2Cpp.MelonLoader.dll")
$monoUnity = Resolve-RequiredFile (Join-Path $MonoAssembliesDirectory "UnityEngine.CoreModule.dll")
$il2CppUnity = Resolve-RequiredFile (Join-Path $Il2CppAssembliesDirectory "UnityEngine.CoreModule.dll")

$monoRoot = Split-Path -Parent $monoUnity
$il2CppRoot = Split-Path -Parent $il2CppUnity
$missingProject = Join-Path $repositoryRoot ".ci/no-s1api-project/S1API.csproj"

$contents = @"
<Project>
  <PropertyGroup>
    <S1ApiProjectPath>$(ConvertTo-XmlText $missingProject)</S1ApiProjectPath>
    <S1ApiMonoAssemblyPath>$(ConvertTo-XmlText $monoS1Api)</S1ApiMonoAssemblyPath>
    <S1ApiIl2CppAssemblyPath>$(ConvertTo-XmlText $il2CppS1Api)</S1ApiIl2CppAssemblyPath>
    <MonoAssembliesPath>$(ConvertTo-XmlText $monoRoot)</MonoAssembliesPath>
    <Il2CppAssembliesPath>$(ConvertTo-XmlText $il2CppRoot)</Il2CppAssembliesPath>
    <S1LuaAutomateLocalDeployment>false</S1LuaAutomateLocalDeployment>
  </PropertyGroup>
</Project>
"@

$outputPath = Join-Path $repositoryRoot "local.build.props"
[System.IO.File]::WriteAllText($outputPath, $contents, [System.Text.UTF8Encoding]::new($false))
Write-Host "Prepared CI build properties at $outputPath"
