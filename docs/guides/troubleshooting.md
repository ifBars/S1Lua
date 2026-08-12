# Troubleshooting

Start with the first S1Lua error in the MelonLoader console. Later messages are often consequences of that first failure.

## Installation problems

If no S1Lua message appears, return to [Installation](installation.md) and check the archive layout and runtime match. S1Lua cannot start if its files are nested one folder too deep or if Mono and IL2CPP builds are mixed.

## Common script messages

| Message | What it means | What to change |
| --- | --- | --- |
| `call s1.mod first` | A `mod:` function ran before the script declared its mod. | Put `local mod = s1.mod { ... }` before other `mod:` calls. |
| `unknown event` | The event name is not supported or is misspelled. | Choose an event from the [Lua API reference](../api/reference.md#events). |
| `clone source was not found` | The base-game item ID is invalid. | Correct the `clone` value. |
| `ran for more than 1 second` | Startup code or a callback exceeded its execution budget, often because a loop never ends. | Remove the endless loop or shorten the work performed by the callback. |
| `icon path must stay inside` | The icon points outside the mod folder or is not a PNG. | Place the PNG beside the mod and use a relative path. |

## Isolate a broken script

Each Lua mod has its own folder and script environment. Temporarily move the failing mod folder outside `Mods/S1Lua`, restart the game, and confirm the remaining scripts load. A failure in one script does not prevent sibling scripts from loading.

S1Lua does not hot reload scripts. Restart the game after changing `mod.lua` so registration and saved state begin from a predictable lifecycle.

## Getting more detail

Include the first S1Lua error, the S1Lua and S1API versions, and whether the game uses Mono or IL2CPP when reporting a problem on [GitHub](https://github.com/ifBars/S1Lua/issues).
