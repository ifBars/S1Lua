namespace S1Lua.State;

public interface IModStateStore
{
    bool TryGet(string modId, string key, out StoredValue? value);
    void Set(string modId, string key, StoredValue value);
    bool Remove(string modId, string key);
}
