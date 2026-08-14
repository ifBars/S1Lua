[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string[]] $Archive
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Read-ZipEntryText {
    param(
        [Parameter(Mandatory)] [System.IO.Compression.ZipArchive] $Zip,
        [Parameter(Mandatory)] [string] $Path
    )

    $entry = $Zip.GetEntry($Path)
    if ($null -eq $entry) {
        throw "Package entry does not exist: $Path"
    }

    $reader = [System.IO.StreamReader]::new($entry.Open())
    try {
        return $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }
}

$expected = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
@(
    "INSTALL.txt",
    "Licenses/MoonSharp.LICENSE.txt",
    "Licenses/S1Lua.LICENSE.txt",
    "Licenses/THIRD_PARTY_NOTICES.md",
    "Mods/S1Lua.dll",
    "Mods/S1Lua/.luarc.json",
    "Mods/S1Lua/.vscode/extensions.json",
    "Mods/S1Lua/Editor/s1lua.lua",
    "Mods/S1Lua/_StarterMod/mod.lua",
    "UserLibs/MoonSharp.Interpreter.dll",
    "UserLibs/S1Lua.Core.dll"
) | ForEach-Object { [void] $expected.Add($_) }

foreach ($archivePath in $Archive) {
    if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf)) {
        throw "Package archive does not exist: $archivePath"
    }

    $zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $archivePath).Path)
    try {
        $actual = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
        foreach ($entry in $zip.Entries) {
            if ([string]::IsNullOrEmpty($entry.Name)) {
                continue
            }
            if ($entry.Length -eq 0) {
                throw "Package entry is empty: $($entry.FullName) in $archivePath"
            }
            [void] $actual.Add($entry.FullName.Replace('\', '/'))
        }

        $missing = $expected.Where({ -not $actual.Contains($_) })
        $unexpected = $actual.Where({ -not $expected.Contains($_) })
        if ($missing.Count -gt 0 -or $unexpected.Count -gt 0) {
            throw "Unexpected package layout in $archivePath. Missing: $($missing -join ', '). Unexpected: $($unexpected -join ', ')."
        }

        $forbidden = $actual.Where({
            $_ -match '(^|/)(S1API|Assembly-CSharp|UnityEngine|Il2Cpp).*\.dll$'
        })
        if ($forbidden.Count -gt 0) {
            throw "Package contains forbidden external assemblies: $($forbidden -join ', ')."
        }

        $luaConfig = Read-ZipEntryText -Zip $zip -Path "Mods/S1Lua/.luarc.json" | ConvertFrom-Json
        if ($luaConfig.'runtime.version' -ne "Lua 5.2") {
            throw "Packaged .luarc.json must target MoonSharp-compatible Lua 5.2."
        }
        if (@($luaConfig.'workspace.library') -notcontains "Editor") {
            throw "Packaged .luarc.json must load the colocated Editor definitions."
        }

        $vscodeConfig = Read-ZipEntryText -Zip $zip -Path "Mods/S1Lua/.vscode/extensions.json" | ConvertFrom-Json
        if (@($vscodeConfig.recommendations) -notcontains "sumneko.lua") {
            throw "Packaged VS Code recommendations must include Lua Language Server."
        }
    }
    finally {
        $zip.Dispose()
    }

    Write-Host "Validated package: $archivePath"
}
