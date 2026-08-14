local mod = s1.mod {
    id = "s1lua.status-watcher",
    name = "Status Watcher",
    version = "1.0.0",
    author = "S1Lua"
}

mod:on("game_loaded", function()
    local player = s1.player()
    local progress = s1.progress()
    local money = s1.money()

    if player ~= nil then
        s1.log("Current region: " .. player.region .. "; health: " .. player.health)
    end
    if progress ~= nil then
        s1.log("Current rank: " .. progress.rank .. " tier " .. progress.tier)
    end
    s1.log("Cash on hand: " .. money.cash)
end)

mod:on("rank_up", function()
    local progress = s1.progress()
    if progress ~= nil then
        s1.log("Rank advanced to " .. progress.rank .. " tier " .. progress.tier .. "!")
    end
end)

mod:on("player_died", function()
    s1.warn("The local player died.")
end)

mod:on("player_revived", function()
    s1.log("The local player is back.")
end)
