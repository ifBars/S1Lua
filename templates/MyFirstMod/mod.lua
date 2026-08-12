local mod = s1.mod {
    id = "yourname.my-first-mod",
    name = "My First Mod",
    version = "1.0.0",
    author = "Your Name"
}

mod:item {
    id = "golden_cuke",
    clone = "cuke",
    name = "Golden Cuke",
    description = "A suspiciously expensive energy drink.",
    price = 250,
    stack = 10,
    shops = "compatible"
}

mod:on("game_loaded", function()
    s1.log("My First Mod is ready!")
end)
