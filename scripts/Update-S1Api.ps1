[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')]
    [string] $Version
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repositoryRoot
try {
    dotnet run --project tools/S1Lua.Generator/S1Lua.Generator.csproj -c Release -- `
        --repo-root $repositoryRoot `
        --update-s1api $Version `
        --bump-surface-patch
    if ($LASTEXITCODE -ne 0) {
        throw "S1API compatibility update failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
