# S1Lua

S1Lua is a small Lua scripting layer for [S1API](https://github.com/ifBars/S1API). It is for someone who has never made a mod or written a program and wants to change something in Schedule I in an afternoon.

```lua
local mod = s1.mod {
    id = "yourname.golden-cuke",
    name = "Golden Cuke"
}

mod:item {
    id = "golden_cuke",
    clone = "cuke",
    name = "Golden Cuke",
    description = "A suspiciously expensive energy drink.",
    price = 250,
    shops = "compatible"
}
```

There are no .NET types, Unity objects, builders, registration stages, or runtime-specific APIs in that script. S1Lua translates the declaration into the correct S1API calls.

## Who this is for

Use S1Lua when you want to learn by changing a short text file. If you already know C# and Unity, use S1API directly; it is more capable and is the foundation underneath S1Lua.

The first surface deliberately covers a useful, teachable slice:

- clone or create items;
- change item names, descriptions, prices, stack limits, legality, and icons;
- add items to compatible or named shops;
- react to a few clear game/save events;
- save small strings, numbers, and booleans per save.

S1Lua does not expose arbitrary CLR or Unity objects, load .NET code, or promise hot reloading. A small surface is easier for beginners to understand and easier to keep working as S1API changes.

## Install a release

1. Install MelonLoader and the matching S1API release for your game runtime.
2. Download the S1Lua archive marked `Mono` or `Il2Cpp` to match S1API.
3. Extract the archive into the Schedule I game folder.
4. Start the game once. S1Lua creates `Mods/S1Lua` if it does not exist.

S1Lua's release archive puts the mod in `Mods` and its private runtime libraries in `UserLibs`. Do not mix the Mono and IL2CPP builds.

## Make your first mod

1. Create `Mods/S1Lua/MyFirstMod` in the game folder.
2. Copy [examples/MyFirstMod/mod.lua](examples/MyFirstMod/mod.lua) into that folder.
3. Open `mod.lua` in any text editor and change the ID, name, and item fields.
4. Start the game and look for `Loaded 1 S1Lua mod(s)` in the MelonLoader console.

The full walkthrough is in [Getting started](docs/guides/getting-started.md). The generated [API reference](docs/api/reference.md) lists every supported function and field.

The same guides and generated Lua reference are published as a standalone DocFX site at [ifbars.github.io/S1Lua](https://ifbars.github.io/S1Lua/). The site documents the Lua surface; it intentionally does not publish S1Lua's internal C# runtime as the modding API.

## Repository layout

| Path | Purpose |
| --- | --- |
| `docs/` | Complete DocFX project: configuration, guides, generated API reference, and site templates. |
| `contributing/` | Maintainer-only release, automation, and surface-development documentation. |
| `examples/` | Runnable Lua examples, including the starter mod packaged with releases. |
| `surface/` | Curated Lua API definition and S1API compatibility anchors. |
| `generated/` | Generated Lua Language Server metadata and compatibility snapshot. |
| `src/` | C# host and runtime implementation. |
| `tools/` | Source generator implementation. |
| `scripts/` | Build, validation, packaging, and release helpers. |
| `tests/` | Host-independent runtime and generator tests. |

## Why maintenance stays small

The public Lua API is declared once in [surface/s1lua.surface.json](surface/s1lua.surface.json). Each Lua feature names the exact S1API DocFX UIDs it relies on. The generator validates those UIDs against a sibling S1API checkout, then regenerates:

- C# registration code;
- Lua Language Server types and autocomplete;
- the beginner API reference;
- a deterministic compatibility snapshot.

When S1API changes, `scripts/Generate.ps1` fails at the broken API anchor instead of letting documentation, editor support, and runtime registration drift apart.

## Develop S1Lua

Requirements are .NET 9 or newer, PowerShell 7 or Windows PowerShell 5.1, and a sibling `S1API` checkout. Runtime builds also use the same local game assembly paths as S1API.

```powershell
./scripts/Generate.ps1
./scripts/Validate.ps1
./scripts/Package.ps1 -Runtime MonoMelon
./scripts/Package.ps1 -Runtime Il2CppMelon
```

Copy [local.build.props.example](local.build.props.example) to `local.build.props` only if the sibling S1API configuration is not enough. Local game files, generated interop assemblies, deployment paths, and release archives are ignored by git.

See [Maintaining the surface](contributing/maintaining.md) for the update workflow and [Security boundaries](docs/guides/security.md) for the sandbox model.

Restore the pinned DocFX tool and preview the documentation locally with:

```powershell
dotnet tool restore
dotnet docfx docs/docfx.json --serve
```

The [CI and release automation](contributing/automation.md) watches stable S1API releases, opens a generated compatibility PR, runs full dual-runtime CI, and publishes S1Lua after that PR is reviewed and merged.

## Status

The current S1Lua and S1API compatibility versions are recorded in `version.txt` and the generated reference. Mono and IL2CPP are separate build artifacts over the same generated beginner API.

S1Lua is licensed under MIT. MoonSharp is redistributed under its BSD-style license; see [third-party notices](THIRD_PARTY_NOTICES.md).
