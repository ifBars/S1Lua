---
title: S1Lua documentation
description: Make Schedule I mods with small, approachable Lua scripts powered by S1API.
_layout: landing
---

<section class="s1lua-hero">
  <p class="s1lua-eyebrow">Schedule I modding, without the framework ceremony</p>
  <h1>Change the game with one Lua file.</h1>
  <p class="s1lua-lead">S1Lua turns a focused, beginner-friendly Lua API into safe S1API calls. Create an item, react to the game world, or make an NPC send a message without setting up a C# project.</p>
  <div class="s1lua-actions">
    <a class="s1lua-action s1lua-action-primary" href="guides/getting-started.md">Make your first mod</a>
    <a class="s1lua-action" href="api/reference.md">Browse the Lua API</a>
  </div>
</section>

<section class="s1lua-section" aria-labelledby="what-you-can-make">
  <p class="s1lua-kicker">A deliberately small surface</p>
  <h2 id="what-you-can-make">Start with useful changes, not engine internals.</h2>
  <div class="s1lua-grid">
    <a class="s1lua-card" href="guides/getting-started.md">
      <span class="s1lua-card-number">01</span>
      <h3>Create items</h3>
      <p>Clone a base-game item, change its presentation and pricing, then place it in compatible or named shops.</p>
    </a>
    <a class="s1lua-card" href="api/reference.md#events">
      <span class="s1lua-card-number">02</span>
      <h3>React to the world</h3>
      <p>Run Lua callbacks when saves load, time passes, sleep ends, weather changes, or the game saves.</p>
    </a>
    <a class="s1lua-card" href="api/reference.md#npcinfo">
      <span class="s1lua-card-number">03</span>
      <h3>Work with people</h3>
      <p>Read NPC state, show dialogue, send texts, adjust relationships, and listen for NPC or quest events.</p>
    </a>
  </div>
</section>

<section class="s1lua-example" aria-labelledby="one-file-example">
  <div>
    <p class="s1lua-kicker">One folder. One script.</p>
    <h2 id="one-file-example">Readable before you know Lua.</h2>
    <p>Each mod lives in its own folder under <code>Mods/S1Lua</code>. S1Lua handles registration timing, runtime differences, and namespacing behind the scenes.</p>
  </div>

```lua
local mod = s1.mod {
    id = "yourname.golden-cuke",
    name = "Golden Cuke"
}

mod:item {
    id = "golden_cuke",
    clone = "cuke",
    name = "Golden Cuke",
    price = 250,
    shops = "compatible"
}
```
</section>

<section class="s1lua-next">
  <div>
    <p class="s1lua-kicker">Ready when you are</p>
    <h2>Build the smallest mod that makes you smile.</h2>
  </div>
  <a class="s1lua-action s1lua-action-primary" href="guides/getting-started.md">Open the walkthrough</a>
</section>
