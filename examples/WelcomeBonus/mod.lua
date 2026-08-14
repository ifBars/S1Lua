local mod = s1.mod {
    id = "s1lua.welcome-bonus",
    name = "Welcome Bonus",
    version = "1.0.0",
    author = "S1Lua"
}

mod:on("game_loaded", function()
    if mod:get("bonus_paid", false) then
        return
    end

    mod:change_cash(250, true, true)
    mod:set("bonus_paid", true)
    mod:save()
    s1.log("Paid the one-time $250 welcome bonus.")
end)
