---
title: Editor setup and early error checking
description: Open the packaged S1Lua workspace with LuaLS autocomplete and diagnostics already configured.
---

# Editor setup and early error checking

S1Lua includes a complete editor workspace under `Mods/S1Lua`. Open that folder, not only one mod folder. The included `.luarc.json` gives Lua Language Server the correct Lua version and S1Lua autocomplete information.

## Visual Studio Code

1. Install [Visual Studio Code](https://code.visualstudio.com/).
2. Choose **File > Open Folder** and open `Schedule I/Mods/S1Lua`.
3. Accept the recommended **Lua** extension by LuaLS. If no recommendation appears, search Extensions for `sumneko.lua`.
4. Open `_StarterMod/mod.lua` and hover over `s1.mod`, `mod:item`, or another S1Lua function.

You should see its parameter types and description. Type `mod:on("` to see supported events. Events such as `sleep_ended` also show the value passed to their callback.

The `_StarterMod` folder is deliberately ignored by the game. Copy it to a new folder without a leading underscore before making your mod:

```text
Mods/S1Lua/
├── .luarc.json
├── Editor/
│   └── s1lua.lua
├── _StarterMod/       ignored template
│   └── mod.lua
└── MyFirstMod/        loaded by S1Lua
    └── mod.lua
```

## Other editors

Use any editor that supports Lua Language Server and open `Mods/S1Lua` as the workspace root. LuaLS reads the same `.luarc.json`; no VS Code-specific settings are required for the S1Lua definitions.

## What is checked before the game starts

LuaLS reports problems as you type, including:

- Lua syntax errors;
- misspelled S1Lua functions and option fields;
- many wrong argument and field types;
- unsupported `mod:on` event names;
- incorrect event callback parameters;
- undefined variables and unreachable code.

In Visual Studio Code, hover over an underline or open **View > Problems**. Fix editor errors before restarting the game.

## What still needs a game run

The editor cannot know whether a base-game item or NPC ID exists, whether an image is valid, or whether a game system is ready. It also cannot know whether the current player is the lobby host. The first game run checks those details.

LuaLS checks your Lua code, S1Lua fields, and event names while you type. The game then checks item IDs, NPC IDs, images, game readiness, and multiplayer permissions.
