---
title: Copyable S1Lua recipes
description: Start from small working S1Lua mods for bonuses, player status, progression, and items.
---

# Copyable recipes

Each example below is a complete `mod.lua`, not an isolated API fragment. Create a new folder under `Mods/S1Lua`, paste one example into that folder as `mod.lua`, and change the mod ID before sharing your version.

> [!IMPORTANT]
> Every installed mod needs a unique lowercase `id`. Treat that ID as permanent once a save depends on it.

## Pay a welcome bonus once per save

This mod gives the player $250 after a save loads, remembers that it paid the bonus, and requests a save. Restarting the game does not pay it twice.

[!code-lua[](../../examples/WelcomeBonus/mod.lua)]

Change `250` to adjust the bonus. Keep the `bonus_paid` check unless you intentionally want to pay on every load.

## Report player, money, and rank status

This read-only example shows how to handle player information that may not be ready yet and how to react to events. Its output appears in the MelonLoader console.

[!code-lua[](../../examples/StatusWatcher/mod.lua)]

These information functions may return `nil` until the related part of the game is ready:

- `s1.player()` returns `nil` until the local player exists;
- `s1.progress()` returns `nil` until progression exists;
- `s1.money()` returns numeric balances and uses zeros before the money manager exists.

## Award XP for recycling trash

This example adds 5 XP for each trash object processed by the recycler. The normal cash reward remains unchanged.

[!code-lua[](../../examples/RecycleForXp/mod.lua)]

Change `XP_PER_ITEM` to tune the reward. A filled trash bag counts as one recycled object. XP awards work in single-player and for the lobby host.

## Create a shop item

The starter example copies an existing item, changes how it appears, and adds it to suitable shops.

[!code-lua[](../../examples/MyFirstMod/mod.lua)]

Change one field at a time and restart the game. Keep `id` stable. You can freely change `name`, `description`, `price`, or `stack` while testing.

## Split a mod into modules and delay work

The `ModularTimer` example loads a helper file from its own folder, then runs a function after the game loads.

[!code-lua[](../../examples/ModularTimer/mod.lua)]

Keep `messages.lua` beside `mod.lua`. For larger mods, use relative paths such as `mod:require("features/reminders")`; paths cannot leave the mod folder.

## Combine recipes

Use one `s1.mod { ... }` declaration per `mod.lua`. To combine examples, keep the declaration from your own mod and copy only the `mod:on(...)` or `mod:item { ... }` blocks you need.

Next, use the [Lua API reference](../api/reference.md) to check accepted fields and event names. If the script does not load, the [troubleshooting guide](troubleshooting.md) maps common console messages to fixes.
