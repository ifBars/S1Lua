local mod = s1.mod {
    id = "bars.modular-timer",
    name = "Modular Timer",
    version = "1.0.0",
    author = "Bars",
    description = "Demonstrates safe modules and delayed callbacks."
}

local messages = mod:require("messages")

mod:on("game_loaded", function()
    mod:after(1, function()
        s1.log(messages.loaded())
    end)
end)
