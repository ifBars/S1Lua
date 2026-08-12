using S1Lua.State;

namespace S1Lua.Runtime;

internal static class S1LuaState
{
    internal static InMemoryModStateStore Store { get; } = new();
}
