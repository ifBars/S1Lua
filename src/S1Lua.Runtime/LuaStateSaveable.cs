using S1API.Internal.Abstraction;
using S1API.Saveables;
using S1Lua.State;

namespace S1Lua.Runtime;

internal sealed class LuaStateSaveable : Saveable
{
    [SaveableField("s1lua_state")]
    private Dictionary<string, Dictionary<string, StoredValue>> _state = new(StringComparer.Ordinal);

    private LuaStateSaveable()
    {
        _state = S1LuaState.Store.Attach(_state, preserveExisting: true);
    }

    protected override void OnLoaded()
    {
        _state = S1LuaState.Store.Attach(_state, preserveExisting: false);
    }
}
