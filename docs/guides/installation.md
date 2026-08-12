# Installation

S1Lua runs on top of MelonLoader and S1API. Install the build that matches your game and S1API runtime; Mono and IL2CPP files cannot be mixed.

## Before you install

You need:

- Schedule I with MelonLoader installed;
- the current S1API release for the same runtime;
- the matching S1Lua release archive: `Mono` or `Il2Cpp`.

If you are unsure which runtime you use, check the S1API archive you installed. Choose the S1Lua archive with the same runtime label.

## Install S1Lua

1. Download the matching archive from the [S1Lua releases page](https://github.com/ifBars/S1Lua/releases).
2. Open the Schedule I game folder.
3. Extract the archive directly into that folder. Allow it to merge the included `Mods` and `UserLibs` folders.
4. Start the game and wait for the MelonLoader console.

The installed files are:

```text
Schedule I/
├── Mods/
│   ├── S1Lua.dll
│   └── S1Lua/
│       └── MyFirstMod/
│           └── mod.lua.example
├── UserLibs/
│   ├── S1Lua.Core.dll
│   └── MoonSharp.Interpreter.dll
└── Editor/
    └── s1lua.lua
```

The `Editor` file is optional autocomplete metadata. It is not loaded by the game.

## Verify the installation

A successful startup prints an S1Lua message in the MelonLoader console. Seeing `No Lua mods found` is also a successful result: S1Lua loaded and is waiting for a script under `Mods/S1Lua`.

If S1Lua does not appear in the console, verify that:

- `S1Lua.dll` is directly inside `Mods`, not inside an extra extracted folder;
- both S1Lua support DLLs are directly inside `UserLibs`;
- S1Lua and S1API use the same runtime;
- MelonLoader and S1API load without errors first.

Next, follow the [Quickstart](getting-started.md) to create your first mod.
