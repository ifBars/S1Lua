local XP_PER_ITEM = 5

local mod = s1.mod {
    id = "s1lua.recycle-for-xp",
    name = "Recycle for XP",
    version = "1.0.0",
    author = "S1Lua"
}

mod:on("trash_recycled", function(item_count)
    local xp = item_count * XP_PER_ITEM
    if mod:add_xp(xp) then
        s1.log("Recycled " .. item_count .. " trash item(s) and requested " .. xp .. " XP.")
    else
        s1.warn("Trash was recycled, but progression is not ready yet.")
    end
end)
