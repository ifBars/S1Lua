---
title: Install S1Lua
description: Match your S1API runtime, extract one archive, and verify S1Lua in the MelonLoader console.
---

# Install S1Lua

The goal is simple: extract the matching release archive into the game folder and see S1Lua report that it loaded. You do not need Visual Studio, Unity, or a compiler.

## 1. Match your S1API runtime

S1Lua runs on top of MelonLoader and S1API. Its runtime label must match the S1API build you already installed.

| Your S1API download says | Download from S1Lua |
| --- | --- |
| `Mono` | the archive ending in `Mono.zip` |
| `Il2Cpp` | the archive ending in `Il2Cpp.zip` |

> [!IMPORTANT]
> Do not guess or mix runtimes. If you are unsure, check the filename of the S1API archive or DLL you installed before downloading S1Lua.

You need:

- Schedule I with MelonLoader installed;
- the current S1API release;
- the matching archive from the [S1Lua releases page](https://github.com/ifBars/S1Lua/releases).

## 2. Extract the archive

1. Open the Schedule I game folder.
2. Extract the S1Lua archive directly into that folder.
3. Allow your archive tool to merge the included `Mods` and `UserLibs` folders.
4. Start the game and wait for the MelonLoader console.

The result should look like this:

```text
Schedule I/
├── Mods/
│   ├── S1Lua.dll
│   └── S1Lua/
│       ├── .luarc.json
│       ├── Editor/
│       │   └── s1lua.lua
│       └── _StarterMod/
│           └── mod.lua
├── UserLibs/
│   ├── S1Lua.Core.dll
│   └── MoonSharp.Interpreter.dll
```

The game ignores `Mods/S1Lua/Editor/s1lua.lua`. Your editor uses it to provide S1Lua autocomplete and early error checking.

## 3. Confirm S1Lua loaded

Look for both of these messages or their matching versioned prefixes:

```text
Loaded 0 S1Lua mod(s)
No Lua mods found
```

Those messages mean installation succeeded. Folders beginning with `_` are templates and are ignored. S1Lua is waiting for a file named `mod.lua` under a folder such as `Mods/S1Lua/MyFirstMod`.

If S1Lua never appears in the console, check that:

- `S1Lua.dll` is directly inside `Mods`, not inside an extra archive folder;
- `S1Lua.Core.dll` and `MoonSharp.Interpreter.dll` are directly inside `UserLibs`;
- S1Lua and S1API have the same runtime label;
- MelonLoader and S1API load without errors before S1Lua.

Continue with [Your first mod](getting-started.md). The fastest path is already installed: open `Mods/S1Lua` in your editor and copy `_StarterMod` to `MyFirstMod`.
