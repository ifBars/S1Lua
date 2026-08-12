# Security boundaries

S1Lua is designed to limit accidental complexity and common script mistakes. It is not a security sandbox for untrusted hostile files.

Each mod receives its own MoonSharp `Script` using the hard-sandbox module preset. S1Lua registers only generated callbacks and never registers CLR userdata. Standard Lua file, operating-system, and dynamic .NET access are unavailable through the supported surface.

Additional boundaries are intentionally narrow:

- startup code and event callbacks stop after a one-second execution budget;
- state accepts only finite numbers, strings, booleans, and `nil`;
- state is namespaced by mod ID and stored with the current game save;
- item icon paths are confined to the script's own folder and PNG files;
- scripts are isolated, so a load or callback error is logged without stopping siblings.

The execution budget cannot interrupt time spent inside a future long-running native host call. Keep host callbacks short and non-blocking. MoonSharp and the host process are still ordinary native/.NET software, so install scripts only from people you trust and treat newly exposed host functions as security-sensitive API changes.

S1Lua does not download code, open network access, evaluate .NET assemblies, or hot reload scripts.
