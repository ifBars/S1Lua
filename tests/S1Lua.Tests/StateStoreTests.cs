using S1Lua.State;

namespace S1Lua.Tests;

public sealed class StateStoreTests
{
    [Fact]
    public void AttachedDictionaryRemainsThePersistenceBackingStore()
    {
        var store = new InMemoryModStateStore();
        var backing = new Dictionary<string, Dictionary<string, StoredValue>>(StringComparer.Ordinal);
        Dictionary<string, Dictionary<string, StoredValue>> attached = store.Attach(backing, preserveExisting: false);

        store.Set("alex.mod", "enabled", StoredValue.FromBoolean(true));

        Assert.True(attached["alex.mod"]["enabled"].Boolean);
    }

    [Fact]
    public void PreserveExistingMergesValuesSetBeforeSaveableDiscovery()
    {
        var store = new InMemoryModStateStore();
        store.Set("alex.mod", "count", StoredValue.FromNumber(3));

        Dictionary<string, Dictionary<string, StoredValue>> attached = store.Attach(
            new Dictionary<string, Dictionary<string, StoredValue>>(StringComparer.Ordinal),
            preserveExisting: true);

        Assert.Equal(3d, attached["alex.mod"]["count"].Number);
    }
}
