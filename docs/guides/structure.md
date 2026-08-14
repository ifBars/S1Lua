---
title: Structure, modules, and timing
description: Organize S1Lua mods without global hooks, per-frame polling, or cross-mod state.
---

# Structure, modules, and timing

S1Lua keeps the useful structure of larger Lua mod systems while making ownership explicit. Each folder is one isolated mod, `mod.lua` is its entry point, and the `s1.mod { ... }` declaration owns metadata and the permanent mod ID. A separate manifest is unnecessary.

```text
Mods/S1Lua/StatusReminder/
├── mod.lua
└── messages.lua
```

## Split code into local modules

Use `mod:require("messages")` to load `messages.lua`. Modules should return a table and keep their implementation local:

```lua
-- messages.lua
local messages = {}

function messages.ready(name)
    return "Ready, " .. name .. "!"
end

return messages
```

```lua
-- mod.lua
local mod = s1.mod { id = "alex.status-reminder", name = "Status Reminder" }
local messages = mod:require("messages")

mod:on("player_ready", function()
    local player = s1.player()
    if player then
        s1.log(messages.ready(player.name))
    end
end)
```

Modules are cached after their first load. Paths stay inside the current mod folder; modules cannot import another mod or access arbitrary files.

## Prefer events, then timers

Use `mod:on(...)` when you want to react to something in the game. `player_ready` means the local player has spawned. `game_loaded` means the save and its game systems are ready.

When no event fits, use a bounded timer instead of per-frame polling:

```lua
local reminder

mod:on("game_loaded", function()
    reminder = mod:every(60, function()
        local money = s1.money()
        s1.log("Cash: $" .. money.cash)
    end)
end)

mod:on("scene_changing", function()
    if reminder then
        mod:cancel(reminder)
        reminder = nil
    end
end)
```

`mod:after(...)` runs once. `mod:every(...)` repeats until cancelled. Both use game-running seconds, enforce the normal callback execution budget, and avoid catch-up bursts after a delayed frame.

## Persist only durable state

Keep temporary values in local variables. Use `mod:set(...)` for strings, numbers, and booleans that belong to the current save. Call `mod:save()` when a player action should save immediately. Do not save timer IDs or returned status tables. Create them again after the game loads.

## Intentional differences from ScheduleLua

S1Lua does not support global lifecycle functions, per-frame `Update()` code, direct Unity object access, shared globals, cross-mod imports, or arbitrary console commands. The Lua reference lists everything supported today.
