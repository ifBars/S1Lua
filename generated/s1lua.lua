---@meta
-- Generated from surface/s1lua.surface.json. Do not edit by hand.
-- Surface version 0.3.0; S1API 3.1.15.

---A supported S1Lua game event. Type a quote after mod:on( to see every choice.
---@alias S1EventName
---| '"game_loading"' # The game is about to load save data.
---| '"game_loaded"' # The save has loaded and gameplay is ready.
---| '"scene_changing"' # The game is leaving the current scene.
---| '"before_save"' # The game is about to save.
---| '"after_save"' # The game finished saving.
---| '"hour_passed"' # A new in-game hour started.
---| '"day_passed"' # A new in-game day started.
---| '"week_passed"' # A new in-game week started.
---| '"sleep_started"' # The player started sleeping.
---| '"sleep_ended"' # Sleep ended. The function receives the number of minutes skipped.
---| '"weather_changed"' # The current weather changed.
---| '"balance_changed"' # Cash or online balance changed.
---| '"xp_changed"' # XP changed; call s1.progress() to read the new values.
---| '"rank_up"' # The player advanced to a new tier or rank.
---| '"player_ready"' # The local player has spawned and s1.player() is ready.
---| '"player_died"' # The local player died.
---| '"player_revived"' # The local player revived.
---| '"trash_recycled"' # The local player finished recycling. The function receives the number of objects processed. A filled trash bag counts as one object.

---Names and identifies one Lua mod.
---@class ModOptions
---@field id string Stable lowercase ID such as yourname.my-first-mod.
---@field name string Name shown in logs and diagnostics.
---@field version? string Your mod version.
---@field author? string Your name or handle.
---@field description? string A short description of the mod.

---Controls how a clothing item is worn and displayed.
---@class ClothingOptions
---@field slot? string feet, bottom, waist, top, outerwear, hands, neck, eyes, head, or wrist. A clone keeps its original slot when omitted.
---@field application? string body_layer, face_layer, or accessory. A clone keeps its original application when omitted.
---@field asset? string Base-game clothing asset path to use or retexture. Required when the item does not clone clothing.
---@field texture? string Optional PNG inside this mod folder that replaces the clothing texture.
---@field colorable? boolean Whether the player can choose a clothing color.
---@field default_color? string Default color such as white, black, red, sky_blue, navy, purple, or hot_pink.
---@field blocked_slots? string[] Other clothing slots blocked while this item is equipped.

---Describes an item and where players can buy it.
---@class ItemOptions
---@field id string Local item ID. S1Lua prefixes it with the mod ID.
---@field clone? string Base-game item ID to clone. Recommended for first mods.
---@field name? string Display name. Required for items that do not clone another item.
---@field description? string Inventory description. A clone keeps its original description when omitted.
---@field category? string product, packaging, growing, tools, furniture, lighting, cash, consumable, equipment, ingredient, decoration, or clothing. Use the clothing table for equipable clothing settings.
---@field stack? integer Maximum quantity in one slot, from 1 to 999.
---@field price? number Purchase price, zero or greater.
---@field resell? number Resell fraction from 0 to 1.
---@field legal? boolean Whether the item is legal.
---@field icon? string PNG path inside this mod folder.
---@field clothing? ClothingOptions Equipable-clothing settings. Cloning an existing clothing item is the most reliable starting point.
---@field shops? string|string[] Use compatible for every suitable shop, or provide a list of shop names.

---A fixed position in the game world.
---@class Position
---@field x number World X coordinate.
---@field y number World Y coordinate.
---@field z number World Z coordinate.

---Describes a phone map marker at a fixed position or following an NPC.
---@class MarkerOptions
---@field id string Local marker ID. S1Lua prefixes it with the mod ID.
---@field label? string Text shown beside the marker.
---@field position? Position Fixed world position. Use this or npc, not both.
---@field npc? string NPC ID for a marker that follows that NPC. Use this or position, not both.
---@field icon? string PNG path inside this mod folder.
---@field text? "always"|"hover"|"off" When the marker label is shown.
---@field visible? boolean Whether the marker starts visible.

---Describes a phone call with one or more lines of dialogue.
---@class PhoneCallOptions
---@field caller? string Caller name when npc is not supplied.
---@field npc? string NPC ID whose name and portrait should be used.
---@field icon? string PNG caller portrait used when npc is not supplied.
---@field stages string[] One to twenty lines shown in order.

---Current details about an NPC.
---@class NpcInfo
---@field id string Stable in-game NPC ID.
---@field name string NPC full name.
---@field region string NPC's assigned region.
---@field relationship number Relationship from 0 for a stranger to 1 for the maximum.
---@field is_unlocked boolean Whether the NPC is unlocked.
---@field is_dead boolean Whether the NPC is dead.

---The current in-game day and time.
---@class TimeInfo
---@field day string Current weekday in lowercase.
---@field time integer Current 24-hour time such as 1330.
---@field formatted string Current time formatted for display.
---@field elapsed_days integer Number of elapsed game days.
---@field is_night boolean Whether it is currently nighttime.
---@field is_sleeping boolean Whether sleep is in progress.

---The current weather conditions.
---@class WeatherInfo
---@field primary string Weather component with the highest weight.
---@field sunny number Sunny weight from 0 to 1.
---@field cloudy number Cloudy weight from 0 to 1.
---@field rainy number Rainy weight from 0 to 1.
---@field stormy number Stormy weight from 0 to 1.
---@field snowy number Snowy weight from 0 to 1.
---@field foggy number Foggy weight from 0 to 1.
---@field windy number Windy weight from 0 to 1.
---@field hail number Hail weight from 0 to 1.
---@field sleet number Sleet weight from 0 to 1.

---The player's current balances.
---@class MoneyInfo
---@field cash number Cash currently carried by the player.
---@field online number Current online account balance.
---@field net_worth number Current total net worth reported by the game.

---The player's current rank and XP.
---@class ProgressInfo
---@field rank string Stable lowercase rank ID such as street_rat or shot_caller.
---@field tier integer Current tier within the rank.
---@field xp integer XP earned within the current tier.
---@field total_xp integer Total XP accumulated across all ranks.
---@field xp_to_next_tier number XP threshold for completing the current tier.

---The local player's current status and position.
---@class PlayerInfo
---@field name string Current player name.
---@field health number Current health.
---@field max_health number Maximum supported health.
---@field is_dead boolean Whether the player is dead.
---@field is_in_vehicle boolean Whether the player is currently in a vehicle.
---@field is_sleeping boolean Whether the player is currently sleeping.
---@field is_arrested boolean Whether the player is under arrest.
---@field region string Stable lowercase ID for the player's current region.
---@field position Position Current world position with x, y, and z coordinates.

---Basic details about a base-game quest.
---@class QuestInfo
---@field id string Normalized quest identifier.
---@field title string Quest title shown in game.

---@class Mod
local mod = {}

---@class Npc
local npc = {}

---@class Quest
local quest = {}

---@class Api
local s1 = {}

---Starts your mod and sets its identity and display information.
---@param options ModOptions
---@return Mod
function s1.mod(options) end

---Writes an informational line to the MelonLoader log.
---@param message string
function s1.log(message) end

---Writes a warning to the MelonLoader log.
---@param message string
function s1.warn(message) end

---Returns the current in-game day and time, or nil when no save is loaded.
---@return TimeInfo|nil
function s1.time() end

---Returns the current weather conditions, or nil until they are available.
---@return WeatherInfo|nil
function s1.weather() end

---Returns current cash, online balance, and net worth. Values are zero until balance information is available.
---@return MoneyInfo
function s1.money() end

---Returns the player's current rank and XP, or nil until this information is available.
---@return ProgressInfo|nil
function s1.progress() end

---Returns the local player's current status, or nil until the player has spawned.
---@return PlayerInfo|nil
function s1.player() end

---Creates an item and optionally adds it to shops.
---@param options ItemOptions
function mod:item(options) end

---Runs a function when a supported game event occurs.
---@overload fun(event: "game_loading", callback: fun())
---@overload fun(event: "game_loaded", callback: fun())
---@overload fun(event: "scene_changing", callback: fun())
---@overload fun(event: "before_save", callback: fun())
---@overload fun(event: "after_save", callback: fun())
---@overload fun(event: "hour_passed", callback: fun())
---@overload fun(event: "day_passed", callback: fun())
---@overload fun(event: "week_passed", callback: fun())
---@overload fun(event: "sleep_started", callback: fun())
---@overload fun(event: "sleep_ended", callback: fun(minutes_skipped: integer))
---@overload fun(event: "weather_changed", callback: fun())
---@overload fun(event: "balance_changed", callback: fun())
---@overload fun(event: "xp_changed", callback: fun())
---@overload fun(event: "rank_up", callback: fun())
---@overload fun(event: "player_ready", callback: fun())
---@overload fun(event: "player_died", callback: fun())
---@overload fun(event: "player_revived", callback: fun())
---@overload fun(event: "trash_recycled", callback: fun(item_count: integer))
---@param event S1EventName
---@param callback fun(value?: number)
function mod:on(event, callback) end

---Loads a helper Lua file from this mod folder.
---@param path string
---@return any
function mod:require(path) end

---Runs a function once after the chosen number of gameplay seconds.
---@param seconds number
---@param callback fun()
---@return integer
function mod:after(seconds, callback) end

---Runs a function repeatedly at the chosen interval.
---@param seconds number
---@param callback fun()
---@return integer
function mod:every(seconds, callback) end

---Stops a delayed or repeating function created by this mod.
---@param timer_id integer
---@return boolean
function mod:cancel(timer_id) end

---Reads a value previously saved by this mod.
---@param key string
---@param default? string|number|boolean|nil
---@return string|number|boolean|nil
function mod:get(key, default) end

---Saves a string, number, boolean, or nil in the current save file.
---@param key string
---@param value string|number|boolean|nil
function mod:set(key, value) end

---Asks the game to save now and returns whether the request was accepted.
---@return boolean
function mod:save() end

---Adds or removes carried cash after the game is loaded.
---@param amount number
---@param visualize? boolean
---@param sound? boolean
function mod:change_cash(amount, visualize, sound) end

---Adds XP and returns true when successful. In multiplayer, only the lobby host can add XP.
---@param amount integer
---@return boolean
function mod:add_xp(amount) end

---Finds an NPC by ID and provides functions for working with them.
---@param id string
---@return Npc
function mod:npc(id) end

---Creates a phone map marker after the save finishes loading.
---@param options MarkerOptions
---@return string
function mod:marker(options) end

---Starts a simple phone call. Returns false when the chosen NPC is unavailable.
---@param options PhoneCallOptions
---@return boolean
function mod:call(options) end

---Finds a base-game quest by title or ID so you can read its details and events.
---@param name string
---@return Quest
function mod:quest(name) end

---Returns the NPC's current details, or nil when that NPC is unavailable.
---@return NpcInfo|nil
function npc:info() end

---Shows temporary world-space text above the NPC.
---@param text string
---@param seconds? number
---@return boolean
function npc:say(text, seconds) end

---Sends the player a phone message from the NPC.
---@param message string
---@return boolean
function npc:text(message) end

---Changes the NPC's relationship by an amount from -5 to 5.
---@param amount number
---@return boolean
function npc:add_relationship(amount) end

---Unlocks the NPC as though the player met them directly.
---@return boolean
function npc:unlock() end

---Runs a function when the NPC's relationship changes, they are unlocked, or they die.
---@param event "relationship_changed"|"unlocked"|"died"
---@param callback function
function npc:on(event, callback) end

---Returns the quest's current details, or nil when the quest is unavailable.
---@return QuestInfo|nil
function quest:info() end

---Runs a function when the quest is completed or failed.
---@param event "completed"|"failed"
---@param callback fun()
function quest:on(event, callback) end

_G.s1 = s1
return s1
