<!-- Generated from surface/s1lua.surface.json. Do not edit by hand. -->
# S1Lua reference

Surface version `0.3.1` for S1API `3.2.0`.

A deliberately small Lua surface for first-time Schedule I modders.

This page lists every function and option currently available in S1Lua. If something is not listed here, it is not supported yet.

## Functions

### Global API

#### `s1.mod(options) -> mod`

Starts your mod and sets its identity and display information.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `options` | `ModOptions` | yes | Mod identity and display information. |

```lua
local mod = s1.mod { id = "alex.golden-cuke", name = "Golden Cuke" }
```

#### `s1.log(message)`

Writes an informational line to the MelonLoader log.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `message` | `string` | yes | Message to write. |

#### `s1.warn(message)`

Writes a warning to the MelonLoader log.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `message` | `string` | yes | Warning to write. |

#### `s1.time() -> TimeInfo|nil`

Returns the current in-game day and time, or nil when no save is loaded.

#### `s1.weather() -> WeatherInfo|nil`

Returns the current weather conditions, or nil until they are available.

#### `s1.money() -> MoneyInfo`

Returns current cash, online balance, and net worth. Values are zero until balance information is available.

#### `s1.progress() -> ProgressInfo|nil`

Returns the player's current rank and XP, or nil until this information is available.

#### `s1.player() -> PlayerInfo|nil`

Returns the local player's current status, or nil until the player has spawned.

### Mod API

#### `mod:item(options)`

Creates an item and optionally adds it to shops.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `options` | `ItemOptions` | yes | Item fields and optional shop placement. |

```lua
mod:item { id = "golden_cuke", clone = "cuke", name = "Golden Cuke", price = 250, shops = "compatible" }
```

```lua
mod:item { id = "painted_cap", clone = "cap", name = "Painted Cap", clothing = { texture = "painted-cap.png", colorable = false } }
```

#### `mod:on(event, callback)`

Runs a function when a supported game event occurs.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `event` | `S1EventName` | yes | Supported event name; your editor suggests every choice. |
| `callback` | `fun(value?: number)` | yes | Function to run. sleep_ended provides minutes skipped; trash_recycled provides the number of objects processed. |

```lua
mod:on("game_loaded", function() s1.log("Ready!") end)
```

#### `mod:require(path) -> value`

Loads a helper Lua file from this mod folder.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `path` | `string` | yes | Relative .lua path inside this mod folder; the extension may be omitted. |

```lua
local messages = mod:require("messages")
```

#### `mod:after(seconds, callback) -> integer`

Runs a function once after the chosen number of gameplay seconds.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `seconds` | `number` | yes | Delay from 0.05 to 86400 seconds; paused game time does not advance it. |
| `callback` | `fun()` | yes | Function to run once. |

```lua
mod:after(5, function() s1.log("Five seconds passed") end)
```

#### `mod:every(seconds, callback) -> integer`

Runs a function repeatedly at the chosen interval.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `seconds` | `number` | yes | Time between runs, from 0.05 to 86400 seconds. Pauses do not count, and missed intervals are not replayed. |
| `callback` | `fun()` | yes | Function to run at each interval. |

```lua
local timer_id = mod:every(60, function() s1.log("One minute passed") end)
```

#### `mod:cancel(timer_id) -> boolean`

Stops a delayed or repeating function created by this mod.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `timer_id` | `integer` | yes | Timer ID returned by mod:after or mod:every. |

#### `mod:get(key, default) -> value`

Reads a value previously saved by this mod.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `key` | `string` | yes | Storage key. |
| `default` | `string|number|boolean|nil` | no | Value returned when the key has not been saved. |

#### `mod:set(key, value)`

Saves a string, number, boolean, or nil in the current save file.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `key` | `string` | yes | Storage key. |
| `value` | `string|number|boolean|nil` | yes | Value to save; nil removes it. |

#### `mod:save() -> boolean`

Asks the game to save now and returns whether the request was accepted.

#### `mod:change_cash(amount, visualize?, sound?)`

Adds or removes carried cash after the game is loaded.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `amount` | `number` | yes | Cash delta from -1000000000 to 1000000000. Positive adds cash; negative removes it. |
| `visualize` | `boolean` | no | Whether the game shows the cash change on the HUD. |
| `sound` | `boolean` | no | Whether the game plays its cash-change sound. |

```lua
mod:on("game_loaded", function() mod:change_cash(250, true, true) end)
```

#### `mod:add_xp(amount) -> boolean`

Adds XP and returns true when successful. In multiplayer, only the lobby host can add XP.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `amount` | `integer` | yes | Positive whole-number XP award from 1 to 1000000. |

```lua
mod:on("game_loaded", function() mod:add_xp(100) end)
```

#### `mod:npc(id) -> Npc`

Finds an NPC by ID and provides functions for working with them.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `string` | yes | Stable NPC ID such as benji_coleman. |

#### `mod:marker(options) -> string`

Creates a phone map marker after the save finishes loading.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `options` | `MarkerOptions` | yes | Marker identity, target, and presentation. |

#### `mod:call(options) -> boolean`

Starts a simple phone call. Returns false when the chosen NPC is unavailable.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `options` | `PhoneCallOptions` | yes | Caller and staged text. |

#### `mod:quest(name) -> Quest`

Finds a base-game quest by title or ID so you can read its details and events.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | `string` | yes | Quest title such as Getting Started, or ID such as gettingstarted. |

### NPC functions

#### `npc:info() -> NpcInfo|nil`

Returns the NPC's current details, or nil when that NPC is unavailable.

#### `npc:say(text, seconds) -> boolean`

Shows temporary world-space text above the NPC.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `text` | `string` | yes | Text to show. |
| `seconds` | `number` | no | Display duration from 0.25 to 60 seconds. |

#### `npc:text(message) -> boolean`

Sends the player a phone message from the NPC.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `message` | `string` | yes | Message to send. |

#### `npc:add_relationship(amount) -> boolean`

Changes the NPC's relationship by an amount from -5 to 5.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `amount` | `number` | yes | Relationship delta. |

#### `npc:unlock() -> boolean`

Unlocks the NPC as though the player met them directly.

#### `npc:on(event, callback)`

Runs a function when the NPC's relationship changes, they are unlocked, or they die.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `event` | `"relationship_changed"|"unlocked"|"died"` | yes | NPC event name. |
| `callback` | `function` | yes | Function to run. relationship_changed provides the new relationship; unlocked provides the unlock type and notification setting. |

### Quest functions

#### `quest:info() -> QuestInfo|nil`

Returns the quest's current details, or nil when the quest is unavailable.

#### `quest:on(event, callback)`

Runs a function when the quest is completed or failed.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `event` | `"completed"|"failed"` | yes | Quest event name. |
| `callback` | `fun()` | yes | Function to run. |

## Events

| Event | Callback values | When it runs |
| --- | --- | --- |
| `game_loading` | none | The game is about to load save data. |
| `game_loaded` | none | The save has loaded and gameplay is ready. |
| `scene_changing` | none | The game is leaving the current scene. |
| `before_save` | none | The game is about to save. |
| `after_save` | none | The game finished saving. |
| `hour_passed` | none | A new in-game hour started. |
| `day_passed` | none | A new in-game day started. |
| `week_passed` | none | A new in-game week started. |
| `sleep_started` | none | The player started sleeping. |
| `sleep_ended` | `minutes_skipped: integer` | Sleep ended. The function receives the number of minutes skipped. |
| `weather_changed` | none | The current weather changed. |
| `balance_changed` | none | Cash or online balance changed. |
| `xp_changed` | none | XP changed; call s1.progress() to read the new values. |
| `rank_up` | none | The player advanced to a new tier or rank. |
| `player_ready` | none | The local player has spawned and s1.player() is ready. |
| `player_died` | none | The local player died. |
| `player_revived` | none | The local player revived. |
| `trash_recycled` | `item_count: integer` | The local player finished recycling. The function receives the number of objects processed. A filled trash bag counts as one object. |

## Option tables

### `ModOptions`

Names and identifies one Lua mod.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | - | Stable lowercase ID such as yourname.my-first-mod. |
| `name` | `string` | yes | - | Name shown in logs and diagnostics. |
| `version` | `string` | no | 1.0.0 | Your mod version. |
| `author` | `string` | no | - | Your name or handle. |
| `description` | `string` | no | - | A short description of the mod. |

### `ClothingOptions`

Controls how a clothing item is worn and displayed.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `slot` | `string` | no | - | feet, bottom, waist, top, outerwear, hands, neck, eyes, head, or wrist. A clone keeps its original slot when omitted. |
| `application` | `string` | no | - | body_layer, face_layer, or accessory. A clone keeps its original application when omitted. |
| `asset` | `string` | no | - | Base-game clothing asset path to use or retexture. Required when the item does not clone clothing. |
| `texture` | `string` | no | - | Optional PNG inside this mod folder that replaces the clothing texture. |
| `colorable` | `boolean` | no | - | Whether the player can choose a clothing color. |
| `default_color` | `string` | no | - | Default color such as white, black, red, sky_blue, navy, purple, or hot_pink. |
| `blocked_slots` | `string[]` | no | - | Other clothing slots blocked while this item is equipped. |

### `ItemOptions`

Describes an item and where players can buy it.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | - | Local item ID. S1Lua prefixes it with the mod ID. |
| `clone` | `string` | no | - | Base-game item ID to clone. Recommended for first mods. |
| `name` | `string` | no | - | Display name. Required for items that do not clone another item. |
| `description` | `string` | no | - | Inventory description. A clone keeps its original description when omitted. |
| `category` | `string` | no | - | product, packaging, growing, tools, furniture, lighting, cash, consumable, equipment, ingredient, decoration, or clothing. Use the clothing table for equipable clothing settings. |
| `stack` | `integer` | no | - | Maximum quantity in one slot, from 1 to 999. |
| `price` | `number` | no | - | Purchase price, zero or greater. |
| `resell` | `number` | no | - | Resell fraction from 0 to 1. |
| `legal` | `boolean` | no | - | Whether the item is legal. |
| `icon` | `string` | no | - | PNG path inside this mod folder. |
| `clothing` | `ClothingOptions` | no | - | Equipable-clothing settings. Cloning an existing clothing item is the most reliable starting point. |
| `shops` | `string|string[]` | no | - | Use compatible for every suitable shop, or provide a list of shop names. |

### `Position`

A fixed position in the game world.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `x` | `number` | yes | - | World X coordinate. |
| `y` | `number` | yes | - | World Y coordinate. |
| `z` | `number` | yes | - | World Z coordinate. |

### `MarkerOptions`

Describes a phone map marker at a fixed position or following an NPC.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | - | Local marker ID. S1Lua prefixes it with the mod ID. |
| `label` | `string` | no | - | Text shown beside the marker. |
| `position` | `Position` | no | - | Fixed world position. Use this or npc, not both. |
| `npc` | `string` | no | - | NPC ID for a marker that follows that NPC. Use this or position, not both. |
| `icon` | `string` | no | - | PNG path inside this mod folder. |
| `text` | `"always"|"hover"|"off"` | no | always | When the marker label is shown. |
| `visible` | `boolean` | no | true | Whether the marker starts visible. |

### `PhoneCallOptions`

Describes a phone call with one or more lines of dialogue.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `caller` | `string` | no | Unknown Caller | Caller name when npc is not supplied. |
| `npc` | `string` | no | - | NPC ID whose name and portrait should be used. |
| `icon` | `string` | no | - | PNG caller portrait used when npc is not supplied. |
| `stages` | `string[]` | yes | - | One to twenty lines shown in order. |

### `NpcInfo`

Current details about an NPC.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | - | Stable in-game NPC ID. |
| `name` | `string` | yes | - | NPC full name. |
| `region` | `string` | yes | - | NPC's assigned region. |
| `relationship` | `number` | yes | - | Relationship from 0 for a stranger to 1 for the maximum. |
| `is_unlocked` | `boolean` | yes | - | Whether the NPC is unlocked. |
| `is_dead` | `boolean` | yes | - | Whether the NPC is dead. |

### `TimeInfo`

The current in-game day and time.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `day` | `string` | yes | - | Current weekday in lowercase. |
| `time` | `integer` | yes | - | Current 24-hour time such as 1330. |
| `formatted` | `string` | yes | - | Current time formatted for display. |
| `elapsed_days` | `integer` | yes | - | Number of elapsed game days. |
| `is_night` | `boolean` | yes | - | Whether it is currently nighttime. |
| `is_sleeping` | `boolean` | yes | - | Whether sleep is in progress. |

### `WeatherInfo`

The current weather conditions.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `primary` | `string` | yes | - | Weather component with the highest weight. |
| `sunny` | `number` | yes | - | Sunny weight from 0 to 1. |
| `cloudy` | `number` | yes | - | Cloudy weight from 0 to 1. |
| `rainy` | `number` | yes | - | Rainy weight from 0 to 1. |
| `stormy` | `number` | yes | - | Stormy weight from 0 to 1. |
| `snowy` | `number` | yes | - | Snowy weight from 0 to 1. |
| `foggy` | `number` | yes | - | Foggy weight from 0 to 1. |
| `windy` | `number` | yes | - | Windy weight from 0 to 1. |
| `hail` | `number` | yes | - | Hail weight from 0 to 1. |
| `sleet` | `number` | yes | - | Sleet weight from 0 to 1. |

### `MoneyInfo`

The player's current balances.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `cash` | `number` | yes | - | Cash currently carried by the player. |
| `online` | `number` | yes | - | Current online account balance. |
| `net_worth` | `number` | yes | - | Current total net worth reported by the game. |

### `ProgressInfo`

The player's current rank and XP.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `rank` | `string` | yes | - | Stable lowercase rank ID such as street_rat or shot_caller. |
| `tier` | `integer` | yes | - | Current tier within the rank. |
| `xp` | `integer` | yes | - | XP earned within the current tier. |
| `total_xp` | `integer` | yes | - | Total XP accumulated across all ranks. |
| `xp_to_next_tier` | `number` | yes | - | XP threshold for completing the current tier. |

### `PlayerInfo`

The local player's current status and position.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `name` | `string` | yes | - | Current player name. |
| `health` | `number` | yes | - | Current health. |
| `max_health` | `number` | yes | - | Maximum supported health. |
| `is_dead` | `boolean` | yes | - | Whether the player is dead. |
| `is_in_vehicle` | `boolean` | yes | - | Whether the player is currently in a vehicle. |
| `is_sleeping` | `boolean` | yes | - | Whether the player is currently sleeping. |
| `is_arrested` | `boolean` | yes | - | Whether the player is under arrest. |
| `region` | `string` | yes | - | Stable lowercase ID for the player's current region. |
| `position` | `Position` | yes | - | Current world position with x, y, and z coordinates. |

### `QuestInfo`

Basic details about a base-game quest.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | - | Normalized quest identifier. |
| `title` | `string` | yes | - | Quest title shown in game. |
