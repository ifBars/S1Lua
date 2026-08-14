#if !IL2CPPMELON
using System.Reflection;
#endif
using MelonLoader;
using S1API.Lifecycle;
using S1API.Utils;

#if IL2CPPMELON
using S1Recycler = Il2CppScheduleOne.ObjectScripts.Recycler;
#else
using S1Recycler = ScheduleOne.ObjectScripts.Recycler;
#endif

namespace S1Lua.Runtime;

internal sealed class S1ApiRecyclingService
{
#if !IL2CPPMELON
    private static readonly MethodInfo? GetTrashMethod =
        typeof(S1Recycler).GetMethod("GetTrash", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
    private readonly List<RecyclerBinding> _bindings = new();
    private readonly Dictionary<int, PendingRecycling> _pending = new();
    private event Action<int>? TrashRecycled;
    private bool _attached;
    private bool _reportedCountFailure;

    internal IDisposable Subscribe(Action<int> callback)
    {
        TrashRecycled += callback;
        Attach();
        return new CallbackSubscription(() =>
        {
            TrashRecycled -= callback;
            if (TrashRecycled == null)
                Detach();
        });
    }

    private void Attach()
    {
        if (_attached)
            return;

        _attached = true;
        GameLifecycle.OnLoadComplete += BindRecyclers;
        GameLifecycle.OnPreSceneChange += UnbindRecyclers;
    }

    private void Detach()
    {
        if (!_attached)
            return;

        _attached = false;
        GameLifecycle.OnLoadComplete -= BindRecyclers;
        GameLifecycle.OnPreSceneChange -= UnbindRecyclers;
        UnbindRecyclers();
    }

    private void BindRecyclers()
    {
        UnbindRecyclers();
        foreach (S1Recycler recycler in UnityEngine.Object.FindObjectsOfType<S1Recycler>(includeInactive: true))
        {
            if (recycler == null || recycler.ButtonIntObj?.onInteractStart == null)
                continue;

            Action started = () => Begin(recycler);
            EventHelper.AddListener(started, recycler.ButtonIntObj.onInteractStart);
            _bindings.Add(new RecyclerBinding(recycler, started));
        }
    }

    private void UnbindRecyclers()
    {
        foreach (PendingRecycling pending in _pending.Values.ToArray())
            RemoveCompletionListener(pending);
        _pending.Clear();

        foreach (RecyclerBinding binding in _bindings)
        {
            try
            {
                if (binding.Recycler != null && binding.Recycler.ButtonIntObj?.onInteractStart != null)
                {
                    EventHelper.RemoveListener(
                        binding.Started,
                        binding.Recycler.ButtonIntObj.onInteractStart);
                }
            }
            catch
            {
                // Scene teardown can destroy the native recycler before S1Lua unbinds.
            }
        }
        _bindings.Clear();
    }

    private void Begin(S1Recycler recycler)
    {
        int count = GetTrashCount(recycler);
        if (count <= 0 || recycler.onStop == null)
            return;

        int instanceId = recycler.GetInstanceID();
        if (_pending.Remove(instanceId, out PendingRecycling? stale))
            RemoveCompletionListener(stale);

        Action completion = () => Complete(instanceId);
        var pending = new PendingRecycling(recycler, completion, count);
        _pending.Add(instanceId, pending);

        try
        {
            EventHelper.AddListener(completion, recycler.onStop);
        }
        catch (Exception ex)
        {
            _pending.Remove(instanceId);
            MelonLogger.Warning($"[S1Lua] Could not watch recycler completion: {ex.Message}");
        }
    }

    private int GetTrashCount(S1Recycler recycler)
    {
        try
        {
#if IL2CPPMELON
            return recycler.GetTrash()?.Length ?? 0;
#else
            object? trash = GetTrashMethod?.Invoke(recycler, null);
            return trash is Array array ? array.Length : 0;
#endif
        }
        catch (Exception ex)
        {
            if (!_reportedCountFailure)
            {
                _reportedCountFailure = true;
                MelonLogger.Warning($"[S1Lua] Could not count recycler contents: {ex.GetBaseException().Message}");
            }
            return 0;
        }
    }

    private void Complete(int instanceId)
    {
        if (!_pending.Remove(instanceId, out PendingRecycling? pending))
            return;

        RemoveCompletionListener(pending);
        TrashRecycled?.Invoke(pending.Count);
    }

    private static void RemoveCompletionListener(PendingRecycling pending)
    {
        try
        {
            if (pending.Recycler != null && pending.Recycler.onStop != null)
                EventHelper.RemoveListener(pending.Completion, pending.Recycler.onStop);
        }
        catch
        {
            // Scene teardown can destroy the native recycler before S1Lua unbinds.
        }
    }

    private sealed class RecyclerBinding
    {
        internal RecyclerBinding(S1Recycler recycler, Action started)
        {
            Recycler = recycler;
            Started = started;
        }

        internal S1Recycler Recycler { get; }
        internal Action Started { get; }
    }

    private sealed class PendingRecycling
    {
        internal PendingRecycling(S1Recycler recycler, Action completion, int count)
        {
            Recycler = recycler;
            Completion = completion;
            Count = count;
        }

        internal S1Recycler Recycler { get; }
        internal Action Completion { get; }
        internal int Count { get; }
    }
}
