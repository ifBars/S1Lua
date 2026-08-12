<!-- Generated from surface/s1lua.surface.json. Do not edit by hand. -->
# S1Lua reference

Surface version `0.2.1` for S1API `3.1.15`.

A deliberately small Lua surface for first-time Schedule I modders.

## Functions

### `s1.mod(options) -> mod`

Creates the single mod declared by this script.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `options` | `ModOptions` | yes | Mod identity and display information. |

```lua
local mod = s1.mod { id = "alex.golden-cuke", name = "Golden Cuke" }
```

### `s1.log(message)`

Writes an informational line to the MelonLoader log.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `message` | `string` | yes | Message to write. |

### `s1.warn(message)`

Writes a warning to the MelonLoader log.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `message` | `string` | yes | Warning to write. |

### `s1.time() -> TimeInfo|nil`

Returns the current game-time snapshot, or nil outside a loaded game.

### `s1.weather() -> WeatherInfo|nil`

Returns current weather weights, or nil before weather is available.

### `mod:item(options)`

Declares a beginner-friendly S1API item without exposing builders or Unity objects.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `options` | `ItemOptions` | yes | Item fields and optional shop placement. |

```lua
mod:item { id = "golden_cuke", clone = "cuke", name = "Golden Cuke", price = 250, shops = "compatible" }
```

### `mod:on(event, callback)`

Runs a function when a supported game event occurs.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `event` | `string` | yes | Event name from the list below. |
| `callback` | `fun(value?: number)` | yes | Function to run. sleep_ended receives minutes skipped. |

```lua
mod:on("game_loaded", function() s1.log("Ready!") end)
```

### `mod:get(key, default) -> value`

Reads a saved string, number, or boolean for this mod.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `key` | `string` | yes | Storage key. |
| `default` | `string|number|boolean|nil` | no | Value returned when the key has not been saved. |

### `mod:set(key, value)`

Stores a string, number, boolean, or nil in this save.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `key` | `string` | yes | Storage key. |
| `value` | `string|number|boolean|nil` | yes | Value to save; nil removes it. |

### `mod:save() -> boolean`

Asks the game to save now and returns whether the request was accepted.

### `mod:npc(id) -> Npc`

Creates a reload-safe proxy for an existing NPC ID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `string` | yes | Stable NPC ID such as benji_coleman. |

### `mod:marker(options) -> string`

Declares a phone-map marker created after the game loads.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `options` | `MarkerOptions` | yes | Marker identity, target, and presentation. |

### `mod:call(options) -> boolean`

Queues a simple phone call and returns false when its NPC caller is unavailable.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `options` | `PhoneCallOptions` | yes | Caller and staged text. |

### `mod:quest(name) -> Quest`

Creates a read-only proxy for a known base-game quest title or compact ID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | `string` | yes | Quest title such as Getting Started, or ID such as gettingstarted. |

### `npc:info() -> NpcInfo|nil`

Returns a primitive NPC snapshot, or nil while that NPC is unavailable.

### `npc:say(text, seconds) -> boolean`

Shows temporary world-space text above the NPC.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `text` | `string` | yes | Text to show. |
| `seconds` | `number` | no | Display duration from 0.25 to 60 seconds. |

### `npc:text(message) -> boolean`

Sends a networked phone message from the NPC.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `message` | `string` | yes | Message to send. |

### `npc:add_relationship(amount) -> boolean`

Adds a networked relationship delta from -5 to 5.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `amount` | `number` | yes | Relationship delta. |

### `npc:unlock() -> boolean`

Unlocks the NPC using S1API's direct-approach default.

### `npc:on(event, callback)`

Listens for relationship_changed, unlocked, or died and rebinds after loads.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `event` | `"relationship_changed"|"unlocked"|"died"` | yes | NPC event name. |
| `callback` | `function` | yes | Callback; relationship_changed receives a number and unlocked receives type and notify. |

### `quest:info() -> QuestInfo|nil`

Returns a primitive quest snapshot, or nil before the quest is available.

### `quest:on(event, callback)`

Listens for completed or failed and rebinds after loads.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `event` | `"completed"|"failed"` | yes | Quest event name. |
| `callback` | `fun()` | yes | Function to run. |

## Events

| Event | When it runs |
| --- | --- |
| `game_loading` | The game is about to load save data. |
| `game_loaded` | The save is loaded and game objects are ready. |
| `scene_changing` | The current scene is about to change. |
| `before_save` | The host is beginning a game save. |
| `after_save` | The host finished a game save. |
| `hour_passed` | A new in-game hour started. |
| `day_passed` | A new in-game day started. |
| `week_passed` | A new in-game week started. |
| `sleep_started` | The player started sleeping. |
| `sleep_ended` | Sleep ended; the callback receives minutes skipped. |
| `weather_changed` | Weather became available or its weights changed. |

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

### `ItemOptions`

Declares an item. S1Lua registers it at the safe S1API lifecycle stage.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | - | Local item ID. S1Lua prefixes it with the mod ID. |
| `clone` | `string` | no | - | Base-game item ID to clone. Recommended for first mods. |
| `name` | `string` | no | - | Display name. Required for items that do not clone another item. |
| `description` | `string` | no | - | Inventory description. A clone keeps its original description when omitted. |
| `category` | `string` | no | - | product, packaging, growing, tools, furniture, lighting, cash, consumable, equipment, ingredient, decoration, or clothing. |
| `stack` | `integer` | no | - | Maximum quantity in one slot, from 1 to 999. |
| `price` | `number` | no | - | Purchase price, zero or greater. |
| `resell` | `number` | no | - | Resell fraction from 0 to 1. |
| `legal` | `boolean` | no | - | Whether the item is legal. |
| `icon` | `string` | no | - | PNG path inside this mod folder. |
| `shops` | `string|string[]` | no | - | Use compatible or a list of in-game shop names. |

### `Position`

A fixed position in the game world.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `x` | `number` | yes | - | World X coordinate. |
| `y` | `number` | yes | - | World Y coordinate. |
| `z` | `number` | yes | - | World Z coordinate. |

### `MarkerOptions`

Declares a phone-map marker at a position or following an NPC.

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

Queues a simple staged phone call.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `caller` | `string` | no | Unknown Caller | Caller name when npc is not supplied. |
| `npc` | `string` | no | - | NPC ID whose name and portrait should be used. |
| `icon` | `string` | no | - | PNG caller portrait used when npc is not supplied. |
| `stages` | `string[]` | yes | - | One to twenty lines shown in order. |

### `NpcInfo`

A primitive snapshot of an NPC. It contains no Unity or CLR objects.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | - | Stable in-game NPC ID. |
| `name` | `string` | yes | - | NPC full name. |
| `region` | `string` | yes | - | NPC's assigned region. |
| `relationship` | `number` | yes | - | Normalized relationship from 0 to 1. |
| `is_unlocked` | `boolean` | yes | - | Whether the NPC is unlocked. |
| `is_dead` | `boolean` | yes | - | Whether the NPC is dead. |

### `TimeInfo`

A primitive snapshot of the current game time.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `day` | `string` | yes | - | Current weekday in lowercase. |
| `time` | `integer` | yes | - | Current 24-hour time such as 1330. |
| `formatted` | `string` | yes | - | Current time formatted for display. |
| `elapsed_days` | `integer` | yes | - | Number of elapsed game days. |
| `is_night` | `boolean` | yes | - | Whether it is currently nighttime. |
| `is_sleeping` | `boolean` | yes | - | Whether sleep is in progress. |

### `WeatherInfo`

A primitive snapshot of the current weather weights.

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

### `QuestInfo`

A primitive snapshot of a known base-game quest.

| Field | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | - | Normalized quest identifier. |
| `title` | `string` | yes | - | Quest title shown in game. |
