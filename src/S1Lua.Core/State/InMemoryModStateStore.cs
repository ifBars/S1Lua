namespace S1Lua.State;

public sealed class InMemoryModStateStore : IModStateStore
{
    private readonly object _gate = new();
    private Dictionary<string, Dictionary<string, StoredValue>> _values =
        new(StringComparer.Ordinal);

    public bool TryGet(string modId, string key, out StoredValue? value)
    {
        lock (_gate)
        {
            if (_values.TryGetValue(modId, out Dictionary<string, StoredValue>? modValues) &&
                modValues.TryGetValue(key, out StoredValue? stored))
            {
                value = Clone(stored);
                return true;
            }

            value = null;
            return false;
        }
    }

    public void Set(string modId, string key, StoredValue value)
    {
        if (string.IsNullOrWhiteSpace(modId))
            throw new ArgumentException("Mod ID cannot be empty.", nameof(modId));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("State key cannot be empty.", nameof(key));
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        lock (_gate)
        {
            if (!_values.TryGetValue(modId, out Dictionary<string, StoredValue>? modValues))
            {
                modValues = new Dictionary<string, StoredValue>(StringComparer.Ordinal);
                _values.Add(modId, modValues);
            }

            modValues[key] = Clone(value);
        }
    }

    public bool Remove(string modId, string key)
    {
        lock (_gate)
        {
            return _values.TryGetValue(modId, out Dictionary<string, StoredValue>? modValues) &&
                   modValues.Remove(key);
        }
    }

    public Dictionary<string, Dictionary<string, StoredValue>> Snapshot()
    {
        lock (_gate)
        {
            return CloneValues(_values);
        }
    }

    public void ReplaceWith(Dictionary<string, Dictionary<string, StoredValue>>? values)
    {
        lock (_gate)
        {
            _values = values == null
                ? new Dictionary<string, Dictionary<string, StoredValue>>(StringComparer.Ordinal)
                : Normalize(values);
        }
    }

    public Dictionary<string, Dictionary<string, StoredValue>> Attach(
        Dictionary<string, Dictionary<string, StoredValue>>? values,
        bool preserveExisting)
    {
        lock (_gate)
        {
            Dictionary<string, Dictionary<string, StoredValue>> attached = values == null
                ? new Dictionary<string, Dictionary<string, StoredValue>>(StringComparer.Ordinal)
                : Normalize(values);

            if (preserveExisting)
            {
                foreach ((string modId, Dictionary<string, StoredValue> modValues) in _values)
                {
                    if (!attached.TryGetValue(modId, out Dictionary<string, StoredValue>? target))
                    {
                        target = new Dictionary<string, StoredValue>(StringComparer.Ordinal);
                        attached.Add(modId, target);
                    }

                    foreach ((string key, StoredValue value) in modValues)
                        target[key] = Clone(value);
                }
            }

            _values = attached;
            return _values;
        }
    }

    private static Dictionary<string, Dictionary<string, StoredValue>> Normalize(
        Dictionary<string, Dictionary<string, StoredValue>> values)
    {
        var normalized = new Dictionary<string, Dictionary<string, StoredValue>>(StringComparer.Ordinal);
        foreach ((string modId, Dictionary<string, StoredValue> modValues) in values)
        {
            var normalizedMod = new Dictionary<string, StoredValue>(StringComparer.Ordinal);
            foreach ((string key, StoredValue value) in modValues)
                normalizedMod[key] = Clone(value);
            normalized[modId] = normalizedMod;
        }
        return normalized;
    }

    private static Dictionary<string, Dictionary<string, StoredValue>> CloneValues(
        Dictionary<string, Dictionary<string, StoredValue>> values) => Normalize(values);

    private static StoredValue Clone(StoredValue value) => new()
    {
        Kind = value.Kind,
        String = value.String,
        Number = value.Number,
        Boolean = value.Boolean
    };
}
