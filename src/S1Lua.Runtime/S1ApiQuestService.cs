using System.Reflection;
using S1API.Quests;
using S1API.Quests.Identifiers;
using S1Lua.Hosting;

namespace S1Lua.Runtime;

internal sealed class S1ApiQuestService
{
    private static readonly MethodInfo GetQuestMethod = typeof(QuestManager)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method => method.Name == nameof(QuestManager.Get) && method.IsGenericMethodDefinition);

    private readonly Dictionary<string, Type> _identifiers = BuildIdentifierCatalog();

    internal QuestSnapshot? Get(string name)
    {
        (Type Type, QuestWrapper Quest)? resolved = Resolve(name);
        return resolved.HasValue
            ? new QuestSnapshot(Normalize(resolved.Value.Type.Name), resolved.Value.Quest.Title)
            : null;
    }

    internal IDisposable? SubscribeCompleted(string name, Action callback)
    {
        (Type Type, QuestWrapper Quest)? resolved = Resolve(name);
        if (!resolved.HasValue)
            return null;
        QuestWrapper quest = resolved.Value.Quest;
        quest.OnComplete += callback;
        return new CallbackSubscription(() => quest.OnComplete -= callback);
    }

    internal IDisposable? SubscribeFailed(string name, Action callback)
    {
        (Type Type, QuestWrapper Quest)? resolved = Resolve(name);
        if (!resolved.HasValue)
            return null;
        QuestWrapper quest = resolved.Value.Quest;
        quest.OnFail += callback;
        return new CallbackSubscription(() => quest.OnFail -= callback);
    }

    private (Type Type, QuestWrapper Quest)? Resolve(string name)
    {
        if (!_identifiers.TryGetValue(Normalize(name), out Type? identifierType))
            return null;
        var method = GetQuestMethod.MakeGenericMethod(identifierType);
        return method.Invoke(null, null) is QuestWrapper quest ? (identifierType, quest) : null;
    }

    private static Dictionary<string, Type> BuildIdentifierCatalog()
    {
        var result = new Dictionary<string, Type>(StringComparer.Ordinal);
        foreach (Type type in typeof(IQuestIdentifier).Assembly.GetTypes()
                     .Where(type => !type.IsAbstract && typeof(IQuestIdentifier).IsAssignableFrom(type)))
        {
            result[Normalize(type.Name)] = type;
            QuestNameAttribute? attribute = type.GetCustomAttribute<QuestNameAttribute>();
            if (attribute != null)
                result[Normalize(attribute.Name)] = type;
        }
        return result;
    }

    private static string Normalize(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}
