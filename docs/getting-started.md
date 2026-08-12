# Getting started

Your first S1Lua mod is one folder containing one file. You do not need Visual Studio, a compiler, or a Unity project.

## Before you begin

Install the S1Lua release that matches your S1API runtime: Mono with Mono, or IL2CPP with IL2CPP. Start the game once and confirm the MelonLoader console contains an S1Lua line.

If S1Lua reports that no Lua mods were found, the installation is working; it is waiting for your first script.

## 1. Create the folder

Inside the game folder, create:

```text
Mods/
└── S1Lua/
    └── MyFirstMod/
        └── mod.lua
```

The entry file must be named exactly `mod.lua`. S1Lua intentionally loads only one level of mod folders, so one broken script cannot silently replace another.

## 2. Paste a complete first mod

```lua
local mod = s1.mod {
    id = "yourname.golden-cuke",
    name = "Golden Cuke",
    version = "1.0.0",
    author = "Your Name"
}

mod:item {
    id = "golden_cuke",
    clone = "cuke",
    name = "Golden Cuke",
    description = "A suspiciously expensive energy drink.",
    price = 250,
    stack = 10,
    shops = "compatible"
}

mod:on("game_loaded", function()
    local times_loaded = mod:get("times_loaded", 0) + 1
    mod:set("times_loaded", times_loaded)
    s1.log("This save has loaded " .. times_loaded .. " time(s).")
end)
```

Save the file and start the game. S1Lua reports the script name and any mistake with a readable message in the MelonLoader console.

## 3. Change one thing at a time

Try changing `name`, `description`, `price`, or `stack`. Restart the game after editing the script. S1Lua does not hot reload scripts; the predictable restart cycle avoids half-applied game state.

The `id` values are permanent identity, not display text. Keep them lowercase and do not change them after other content or saves depend on them. S1Lua prefixes the item ID, so the example becomes `yourname.golden-cuke:golden_cuke` and will not collide with another author's item.

## Add an icon

Place a PNG beside `mod.lua`:

```text
MyFirstMod/
├── mod.lua
└── golden-cuke.png
```

Then add this field inside `mod:item`:

```lua
icon = "golden-cuke.png"
```

Icon paths must remain inside the mod folder and must point to PNG files. S1Lua passes the file through S1API's image utility; scripts never receive the Unity sprite.

## Choose shops

Use `shops = "compatible"` to let S1API choose shops that accept the item. To name shops explicitly, use a Lua list:

```lua
shops = { "Gas-Mart", "Hardware Store" }
```

If an in-game shop name is wrong, S1Lua logs the registration error without stopping other Lua mods.

## Read errors

Start at the first S1Lua error in the MelonLoader console. Common messages explain the correction directly:

- `call s1.mod first` means a `mod:` line appeared before the mod declaration;
- `unknown event` means the event is not in the generated reference;
- `clone source was not found` means the base-game item ID is wrong;
- `ran for more than 1 second` usually means a loop never ends;
- `icon path must stay inside` means the path points outside this mod folder.

One script failing does not prevent sibling scripts from loading, and one event callback failing does not disable callbacks from other mods.

## Editor autocomplete (optional)

The release archive includes `Editor/s1lua.lua`. Configure Lua Language Server to use that file as a library to get descriptions and completion for every supported S1Lua field. Repository contributors can open this repo directly; the [repository's `.luarc.json`](https://github.com/ifBars/S1Lua/blob/main/.luarc.json) already points at the generated stub.

Continue with the generated [S1Lua reference](reference.md) when you want to see every available option.
