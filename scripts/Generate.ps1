[CmdletBinding()]
param(
    [switch] $Check
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repositoryRoot
try {
    $arguments = @("--repo-root", $repositoryRoot)
    if ($Check) {
        $arguments += "--check"
    }

    dotnet run --project tools/S1Lua.Generator/S1Lua.Generator.csproj -c Release -- @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "S1Lua surface generation failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
