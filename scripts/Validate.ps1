[CmdletBinding()]
param(
    [switch] $SkipRuntime,
    [switch] $SkipDocs
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

function Invoke-Checked {
    param(
        [Parameter(Mandatory)] [scriptblock] $Command,
        [Parameter(Mandatory)] [string] $FailureMessage
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$FailureMessage Exit code: $LASTEXITCODE."
    }
}

Push-Location $repositoryRoot
try {
    Invoke-Checked {
        dotnet run --project tools/S1Lua.Generator/S1Lua.Generator.csproj -c Release -- --repo-root $repositoryRoot --check
    } "Generated surface validation failed. Run scripts/Generate.ps1 and review the changes."

    Invoke-Checked {
        dotnet test tests/S1Lua.Tests/S1Lua.Tests.csproj -c Release
    } "S1Lua tests failed."

    if (-not $SkipDocs) {
        Invoke-Checked {
            dotnet tool restore
        } "DocFX tool restore failed."

        Invoke-Checked {
            dotnet docfx docs/docfx.json --warningsAsErrors
        } "S1Lua documentation build failed."
    }

    if (-not $SkipRuntime) {
        Invoke-Checked {
            dotnet build src/S1Lua.Runtime/S1Lua.Runtime.csproj -c MonoMelon
        } "The Mono runtime build failed."

        Invoke-Checked {
            dotnet build src/S1Lua.Runtime/S1Lua.Runtime.csproj -c Il2CppMelon
        } "The IL2CPP runtime build failed."
    }
}
finally {
    Pop-Location
}
