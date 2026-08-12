local mod = s1.mod {
    id = "s1lua.golden-cuke",
    name = "Golden Cuke",
    version = "1.0.0",
    author = "S1Lua"
}

mod:item {
    id = "golden_cuke",
    clone = "cuke",
    name = "Golden Cuke",
    description = "A suspiciously expensive energy drink.",
    price = 250,
    resell = 0.5,
    stack = 10,
    legal = true,
    shops = "compatible"
}

mod:on("game_loaded", function()
    local times_loaded = mod:get("times_loaded", 0) + 1
    mod:set("times_loaded", times_loaded)
    s1.log("Golden Cuke has loaded " .. times_loaded .. " time(s) in this save.")
end)
