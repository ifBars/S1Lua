namespace S1Lua.Runtime;

internal sealed class CallbackSubscription : IDisposable
{
    private Action? _dispose;

    internal CallbackSubscription(Action dispose)
    {
        _dispose = dispose;
    }

    public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
}
