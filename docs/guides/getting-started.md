---
title: Your first S1Lua mod
description: Enable the included starter, change one item, and verify it in Schedule I.
---

# Your first S1Lua mod

In a few minutes you will load a Lua script, see its message in the MelonLoader console, and register a custom shop item. No C# project or Unity setup is required.

Before continuing, [install S1Lua](installation.md) and start the game once. Seeing `No Lua mods found` means the installation is ready.

## 1. Open the authoring workspace

Open this folder in your code editor:

```text
Mods/S1Lua
```

For Visual Studio Code, accept the recommended Lua extension. S1Lua already includes autocomplete and error checking for every supported function and option.

See [Editor setup and early error checking](editor-setup.md) for other editors and how to confirm LuaLS is active.

## 2. Copy the included starter

Copy `_StarterMod` and name the copy `MyFirstMod`. Do not edit the original template.

> [!TIP]
> A folder beginning with `_` is ignored by S1Lua. Your copied folder must not begin with an underscore.

If the template is missing, create `MyFirstMod/mod.lua` and copy the complete starter below.

[!code-lua[](../../examples/MyFirstMod/mod.lua)]

## 3. Make the mod yours

Open `mod.lua` in any text editor and change these values:

```lua
id = "yourname.my-first-mod",
name = "My First Mod",
author = "Your Name"
```

The mod `id` is permanent identity, not display text. Use lowercase letters, numbers, dots, underscores, or hyphens. Keep it stable after a save or another mod depends on it.

Before starting the game, look at your editor's Problems panel. Fix red syntax or type errors first.

## 4. Start the game

Restart Schedule I after saving the file. S1Lua intentionally does not hot reload scripts.

Look for these parts in the MelonLoader console:

```text
My First Mod is ready!
Loaded 1 S1Lua mod(s)
```

If the script has a mistake, S1Lua reports the mod name and a readable error. Start with the first S1Lua error, fix it, and restart the game.

## 5. Change one visible thing

The starter creates a `Golden Cuke` by cloning the base-game `cuke` item. Change one field inside `mod:item { ... }`:

```lua
name = "Golden Cuke Deluxe",
price = 500,
stack = 20,
```

Restart the game and check a shop that already sells this kind of item.

S1Lua prefixes the item ID with your mod ID. `golden_cuke` becomes `yourname.my-first-mod:golden_cuke`, preventing collisions with other authors.

## 6. Add an icon when you are ready

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

Icon paths must stay inside the mod folder and point to PNG files.

## Where to go next

- Copy a working bonus or status mod from [Copyable recipes](recipes.md).
- Find every supported field and event in the [Lua API reference](../api/reference.md).
- Use [Troubleshooting](troubleshooting.md) if the console reports an error.

Keep opening `Mods/S1Lua` as the workspace root so every mod gets S1Lua autocomplete and error checking.
