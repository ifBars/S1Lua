---@meta
-- Generated from surface/s1lua.surface.json. Do not edit by hand.
-- Surface version 0.2.1; S1API 3.1.15.

---Names and identifies one Lua mod.
---@class S1LuaModOptions
---@field id string Stable lowercase ID such as yourname.my-first-mod.
---@field name string Name shown in logs and diagnostics.
---@field version? string Your mod version.
---@field author? string Your name or handle.
---@field description? string A short description of the mod.

---Declares an item. S1Lua registers it at the safe S1API lifecycle stage.
---@class S1LuaItemOptions
---@field id string Local item ID. S1Lua prefixes it with the mod ID.
---@field clone? string Base-game item ID to clone. Recommended for first mods.
---@field name? string Display name. Required for items that do not clone another item.
---@field description? string Inventory description. A clone keeps its original description when omitted.
---@field category? string product, packaging, growing, tools, furniture, lighting, cash, consumable, equipment, ingredient, decoration, or clothing.
---@field stack? integer Maximum quantity in one slot, from 1 to 999.
---@field price? number Purchase price, zero or greater.
---@field resell? number Resell fraction from 0 to 1.
---@field legal? boolean Whether the item is legal.
---@field icon? string PNG path inside this mod folder.
---@field shops? string|string[] Use compatible or a list of in-game shop names.

---A fixed position in the game world.
---@class S1LuaPosition
---@field x number World X coordinate.
---@field y number World Y coordinate.
---@field z number World Z coordinate.

---Declares a phone-map marker at a position or following an NPC.
---@class S1LuaMarkerOptions
---@field id string Local marker ID. S1Lua prefixes it with the mod ID.
---@field label? string Text shown beside the marker.
---@field position? S1LuaPosition Fixed world position. Use this or npc, not both.
---@field npc? string NPC ID for a marker that follows that NPC. Use this or position, not both.
---@field icon? string PNG path inside this mod folder.
---@field text? "always"|"hover"|"off" When the marker label is shown.
---@field visible? boolean Whether the marker starts visible.

---Queues a simple staged phone call.
---@class S1LuaPhoneCallOptions
---@field caller? string Caller name when npc is not supplied.
---@field npc? string NPC ID whose name and portrait should be used.
---@field icon? string PNG caller portrait used when npc is not supplied.
---@field stages string[] One to twenty lines shown in order.

---A primitive snapshot of an NPC. It contains no Unity or CLR objects.
---@class S1LuaNpcInfo
---@field id string Stable in-game NPC ID.
---@field name string NPC full name.
---@field region string NPC's assigned region.
---@field relationship number Normalized relationship from 0 to 1.
---@field is_unlocked boolean Whether the NPC is unlocked.
---@field is_dead boolean Whether the NPC is dead.

---A primitive snapshot of the current game time.
---@class S1LuaTimeInfo
---@field day string Current weekday in lowercase.
---@field time integer Current 24-hour time such as 1330.
---@field formatted string Current time formatted for display.
---@field elapsed_days integer Number of elapsed game days.
---@field is_night boolean Whether it is currently nighttime.
---@field is_sleeping boolean Whether sleep is in progress.

---A primitive snapshot of the current weather weights.
---@class S1LuaWeatherInfo
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

---A primitive snapshot of a known base-game quest.
---@class S1LuaQuestInfo
---@field id string Normalized quest identifier.
---@field title string Quest title shown in game.

---@class S1LuaMod
local mod = {}

---@class S1LuaNpc
local npc = {}

---@class S1LuaQuest
local quest = {}

---@class S1LuaApi
local s1 = {}

---Creates the single mod declared by this script.
---@param options S1LuaModOptions
---@return S1LuaMod
function s1.mod(options) end

---Writes an informational line to the MelonLoader log.
---@param message string
function s1.log(message) end

---Writes a warning to the MelonLoader log.
---@param message string
function s1.warn(message) end

---Returns the current game-time snapshot, or nil outside a loaded game.
---@return S1LuaTimeInfo|nil
function s1.time() end

---Returns current weather weights, or nil before weather is available.
---@return S1LuaWeatherInfo|nil
function s1.weather() end

---Declares a beginner-friendly S1API item without exposing builders or Unity objects.
---@param options S1LuaItemOptions
function mod:item(options) end

---Runs a function when a supported game event occurs.
---@param event string
---@param callback fun(value?: number)
function mod:on(event, callback) end

---Reads a saved string, number, or boolean for this mod.
---@param key string
---@param default? string|number|boolean|nil
---@return string|number|boolean|nil
function mod:get(key, default) end

---Stores a string, number, boolean, or nil in this save.
---@param key string
---@param value string|number|boolean|nil
function mod:set(key, value) end

---Asks the game to save now and returns whether the request was accepted.
---@return boolean
function mod:save() end

---Creates a reload-safe proxy for an existing NPC ID.
---@param id string
---@return S1LuaNpc
function mod:npc(id) end

---Declares a phone-map marker created after the game loads.
---@param options S1LuaMarkerOptions
---@return string
function mod:marker(options) end

---Queues a simple phone call and returns false when its NPC caller is unavailable.
---@param options S1LuaPhoneCallOptions
---@return boolean
function mod:call(options) end

---Creates a read-only proxy for a known base-game quest title or compact ID.
---@param name string
---@return S1LuaQuest
function mod:quest(name) end

---Returns a primitive NPC snapshot, or nil while that NPC is unavailable.
---@return S1LuaNpcInfo|nil
function npc:info() end

---Shows temporary world-space text above the NPC.
---@param text string
---@param seconds? number
---@return boolean
function npc:say(text, seconds) end

---Sends a networked phone message from the NPC.
---@param message string
---@return boolean
function npc:text(message) end

---Adds a networked relationship delta from -5 to 5.
---@param amount number
---@return boolean
function npc:add_relationship(amount) end

---Unlocks the NPC using S1API's direct-approach default.
---@return boolean
function npc:unlock() end

---Listens for relationship_changed, unlocked, or died and rebinds after loads.
---@param event "relationship_changed"|"unlocked"|"died"
---@param callback function
function npc:on(event, callback) end

---Returns a primitive quest snapshot, or nil before the quest is available.
---@return S1LuaQuestInfo|nil
function quest:info() end

---Listens for completed or failed and rebinds after loads.
---@param event "completed"|"failed"
---@param callback fun()
function quest:on(event, callback) end

_G.s1 = s1
return s1
