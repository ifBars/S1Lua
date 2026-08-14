using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using S1Lua.Generated;
using S1Lua.Hosting;
using S1Lua.Model;
using S1Lua.State;

namespace S1Lua.Scripting;

internal sealed class BeginnerApiBindings
{
    private const string ModUsage = "s1.mod(options)";
    private const string ItemUsage = "mod:item(options)";
    private const string MarkerUsage = "mod:marker(options)";
    private const string CallUsage = "mod:call(options)";
    private static readonly Regex IdentifierPattern = new(
        "^[a-z0-9][a-z0-9_.-]*$",
        RegexOptions.CultureInvariant);

    private readonly IS1LuaHost _host;
    private readonly ScriptModSession _session;

    internal BeginnerApiBindings(IS1LuaHost host, ScriptModSession session)
    {
        _host = host;
        _session = session;
    }

    internal DynValue CreateMod(ScriptExecutionContext context, CallbackArguments args)
    {
        Table options = LuaArguments.RequiredTable(args, 0, ModUsage);
        string id = LuaArguments.RequiredFieldString(options, "id", ModUsage).ToLowerInvariant();
        ValidateIdentifier(id, "mod ID", ModUsage);
        string name = LuaArguments.RequiredFieldString(options, "name", ModUsage);
        string version = LuaArguments.OptionalFieldString(options, "version", ModUsage) ?? "1.0.0";
        string? author = LuaArguments.OptionalFieldString(options, "author", ModUsage);
        string? description = LuaArguments.OptionalFieldString(options, "description", ModUsage);

        _session.Initialize(new ModMetadata(id, name, version, author, description));
        var table = new Table(_session.Script);
        table.Set("id", DynValue.NewString(id));
        table.Set("name", DynValue.NewString(name));
        table.Set("version", DynValue.NewString(version));
        GeneratedSurface.RegisterMod(table, this, _session);
        return DynValue.NewTable(table);
    }

    internal DynValue Log(ScriptExecutionContext context, CallbackArguments args) =>
        WriteLog(args, S1LuaLogLevel.Info, "s1.log(message)");

    internal DynValue Warn(ScriptExecutionContext context, CallbackArguments args) =>
        WriteLog(args, S1LuaLogLevel.Warning, "s1.warn(message)");

    internal DynValue GetTime(ScriptExecutionContext context, CallbackArguments args)
    {
        GameTimeSnapshot? snapshot = _host.GetGameTime();
        if (snapshot == null)
            return DynValue.Nil;

        var table = new Table(_session.Script);
        table.Set("day", DynValue.NewString(snapshot.Day));
        table.Set("time", DynValue.NewNumber(snapshot.Time));
        table.Set("formatted", DynValue.NewString(snapshot.Formatted));
        table.Set("elapsed_days", DynValue.NewNumber(snapshot.ElapsedDays));
        table.Set("is_night", DynValue.NewBoolean(snapshot.IsNight));
        table.Set("is_sleeping", DynValue.NewBoolean(snapshot.IsSleeping));
        return DynValue.NewTable(table);
    }

    internal DynValue GetWeather(ScriptExecutionContext context, CallbackArguments args)
    {
        WeatherSnapshot? snapshot = _host.GetWeather();
        if (snapshot == null)
            return DynValue.Nil;

        var table = new Table(_session.Script);
        table.Set("primary", DynValue.NewString(snapshot.Primary));
        table.Set("sunny", DynValue.NewNumber(snapshot.Sunny));
        table.Set("cloudy", DynValue.NewNumber(snapshot.Cloudy));
        table.Set("rainy", DynValue.NewNumber(snapshot.Rainy));
        table.Set("stormy", DynValue.NewNumber(snapshot.Stormy));
        table.Set("snowy", DynValue.NewNumber(snapshot.Snowy));
        table.Set("foggy", DynValue.NewNumber(snapshot.Foggy));
        table.Set("windy", DynValue.NewNumber(snapshot.Windy));
        table.Set("hail", DynValue.NewNumber(snapshot.Hail));
        table.Set("sleet", DynValue.NewNumber(snapshot.Sleet));
        return DynValue.NewTable(table);
    }

    internal DynValue GetMoney(ScriptExecutionContext context, CallbackArguments args)
    {
        MoneySnapshot snapshot = _host.GetMoney();
        var table = new Table(_session.Script);
        table.Set("cash", DynValue.NewNumber(snapshot.Cash));
        table.Set("online", DynValue.NewNumber(snapshot.Online));
        table.Set("net_worth", DynValue.NewNumber(snapshot.NetWorth));
        return DynValue.NewTable(table);
    }

    internal DynValue GetProgress(ScriptExecutionContext context, CallbackArguments args)
    {
        ProgressSnapshot? snapshot = _host.GetProgress();
        if (snapshot == null)
            return DynValue.Nil;

        var table = new Table(_session.Script);
        table.Set("rank", DynValue.NewString(snapshot.Rank));
        table.Set("tier", DynValue.NewNumber(snapshot.Tier));
        table.Set("xp", DynValue.NewNumber(snapshot.Xp));
        table.Set("total_xp", DynValue.NewNumber(snapshot.TotalXp));
        table.Set("xp_to_next_tier", DynValue.NewNumber(snapshot.XpToNextTier));
        return DynValue.NewTable(table);
    }

    internal DynValue GetPlayer(ScriptExecutionContext context, CallbackArguments args)
    {
        PlayerSnapshot? snapshot = _host.GetPlayer();
        if (snapshot == null)
            return DynValue.Nil;

        var table = new Table(_session.Script);
        table.Set("name", DynValue.NewString(snapshot.Name));
        table.Set("health", DynValue.NewNumber(snapshot.Health));
        table.Set("max_health", DynValue.NewNumber(snapshot.MaxHealth));
        table.Set("is_dead", DynValue.NewBoolean(snapshot.IsDead));
        table.Set("is_in_vehicle", DynValue.NewBoolean(snapshot.IsInVehicle));
        table.Set("is_sleeping", DynValue.NewBoolean(snapshot.IsSleeping));
        table.Set("is_arrested", DynValue.NewBoolean(snapshot.IsArrested));
        table.Set("region", DynValue.NewString(snapshot.Region));
        PositionSnapshot positionSnapshot = snapshot.Position ?? new PositionSnapshot(0, 0, 0);
        var position = new Table(_session.Script);
        position.Set("x", DynValue.NewNumber(positionSnapshot.X));
        position.Set("y", DynValue.NewNumber(positionSnapshot.Y));
        position.Set("z", DynValue.NewNumber(positionSnapshot.Z));
        table.Set("position", DynValue.NewTable(position));
        return DynValue.NewTable(table);
    }

    internal DynValue DeclareItem(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args)
    {
        ModMetadata metadata = RequireMetadata(session, ItemUsage);
        int offset = LuaArguments.ModOffset(args);
        Table options = LuaArguments.RequiredTable(args, offset, ItemUsage);
        string localId = LuaArguments.RequiredFieldString(options, "id", ItemUsage).ToLowerInvariant();
        ValidateIdentifier(localId, "item ID", ItemUsage);

        string id = $"{metadata.Id}:{localId}";
        string? clone = LuaArguments.OptionalFieldString(options, "clone", ItemUsage);
        string? name = LuaArguments.OptionalFieldString(options, "name", ItemUsage);
        if (clone == null && name == null)
            throw new ScriptRuntimeException($"{ItemUsage}: 'name' is required when 'clone' is not provided.");

        string? description = LuaArguments.OptionalFieldString(options, "description", ItemUsage);
        string? category = LuaArguments.OptionalFieldString(options, "category", ItemUsage)?.ToLowerInvariant();
        int? stack = LuaArguments.OptionalFieldInteger(options, "stack", 1, 999, ItemUsage);
        double? price = LuaArguments.OptionalFieldNumber(options, "price", 0, 1_000_000, ItemUsage);
        double? resell = LuaArguments.OptionalFieldNumber(options, "resell", 0, 1, ItemUsage);
        bool? legal = LuaArguments.OptionalFieldBoolean(options, "legal", ItemUsage);
        string? icon = LuaArguments.OptionalFieldString(options, "icon", ItemUsage);
        ClothingDeclaration? clothing = ReadClothing(options, clone, ref category);
        ShopSelection shops = ReadShops(options);

        session.AddItem(new ItemDeclaration(
            metadata.Id,
            localId,
            id,
            session.SourceDirectory,
            clone,
            name,
            description,
            category,
            stack,
            price,
            resell,
            legal,
            icon,
            clothing,
            shops));

        return DynValue.NewString(id);
    }

    internal DynValue Subscribe(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args)
    {
        RequireMetadata(session, "mod:on(event, callback)");
        int offset = LuaArguments.ModOffset(args);
        string eventName = LuaArguments.RequiredString(args, offset, "mod:on(event, callback)").ToLowerInvariant();
        if (!GeneratedSurface.IsKnownEvent(eventName))
            throw new ScriptRuntimeException($"mod:on: unknown event '{eventName}'. See the Lua API reference for supported events.");

        DynValue callback = LuaArguments.At(args, offset + 1, "mod:on(event, callback)");
        if (callback.Type is not (DataType.Function or DataType.ClrFunction))
            throw new ScriptRuntimeException("mod:on: callback must be a function.");
        session.Subscribe(eventName, callback);
        return DynValue.Nil;
    }

    internal DynValue RequireModule(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args)
    {
        RequireMetadata(session, "mod:require(path)");
        int offset = LuaArguments.ModOffset(args);
        string path = LuaArguments.RequiredString(args, offset, "mod:require(path)");
        return session.RequireModule(path);
    }

    internal DynValue ScheduleAfter(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args) =>
        ScheduleTimer(session, args, repeat: false, "mod:after(seconds, callback)");

    internal DynValue ScheduleEvery(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args) =>
        ScheduleTimer(session, args, repeat: true, "mod:every(seconds, callback)");

    internal DynValue CancelTimer(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args)
    {
        RequireMetadata(session, "mod:cancel(timer_id)");
        int offset = LuaArguments.ModOffset(args);
        DynValue value = LuaArguments.At(args, offset, "mod:cancel(timer_id)");
        if (value.Type != DataType.Number || value.Number < 1 || value.Number > int.MaxValue ||
            value.Number != Math.Truncate(value.Number))
        {
            throw new ScriptRuntimeException("mod:cancel: timer_id must be a positive whole number.");
        }

        return DynValue.NewBoolean(session.CancelTimer((int)value.Number));
    }

    internal DynValue GetState(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args)
    {
        ModMetadata metadata = RequireMetadata(session, "mod:get(key, default)");
        int offset = LuaArguments.ModOffset(args);
        string key = ReadStateKey(args, offset, "mod:get(key, default)");
        if (_host.State.TryGet(metadata.Id, key, out StoredValue? value) && value != null)
            return ToDynValue(value);
        return args.Count > offset + 1 ? args[offset + 1] : DynValue.Nil;
    }

    internal DynValue SetState(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args)
    {
        ModMetadata metadata = RequireMetadata(session, "mod:set(key, value)");
        int offset = LuaArguments.ModOffset(args);
        string key = ReadStateKey(args, offset, "mod:set(key, value)");
        DynValue value = LuaArguments.At(args, offset + 1, "mod:set(key, value)");
        if (LuaArguments.IsNil(value))
        {
            _host.State.Remove(metadata.Id, key);
            return DynValue.Nil;
        }

        _host.State.Set(metadata.Id, key, ToStoredValue(value));
        return value;
    }

    internal DynValue RequestSave(ScriptModSession session, ScriptExecutionContext context, CallbackArguments args)
    {
        RequireMetadata(session, "mod:save()");
        return DynValue.NewBoolean(_host.RequestSave());
    }

    internal DynValue ChangeCash(
        ScriptModSession session,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        RequireMetadata(session, "mod:change_cash(amount, visualize, sound)");
        int offset = LuaArguments.ModOffset(args);
        DynValue amount = LuaArguments.At(args, offset, "mod:change_cash(amount, visualize, sound)");
        if (amount.Type != DataType.Number || double.IsNaN(amount.Number) || double.IsInfinity(amount.Number) ||
            amount.Number < -1_000_000_000 || amount.Number > 1_000_000_000)
        {
            throw new ScriptRuntimeException(
                "mod:change_cash: amount must be a finite number from -1000000000 to 1000000000.");
        }

        bool visualize = OptionalBooleanArgument(
            args,
            offset + 1,
            true,
            "mod:change_cash(amount, visualize, sound)");
        bool sound = OptionalBooleanArgument(
            args,
            offset + 2,
            false,
            "mod:change_cash(amount, visualize, sound)");
        _host.ChangeCash(amount.Number, visualize, sound);
        return DynValue.Nil;
    }

    internal DynValue AddXp(
        ScriptModSession session,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        RequireMetadata(session, "mod:add_xp(amount)");
        int offset = LuaArguments.ModOffset(args);
        DynValue amount = LuaArguments.At(args, offset, "mod:add_xp(amount)");
        if (amount.Type != DataType.Number || double.IsNaN(amount.Number) || double.IsInfinity(amount.Number) ||
            amount.Number < 1 || amount.Number > 1_000_000 || amount.Number != Math.Truncate(amount.Number))
        {
            throw new ScriptRuntimeException(
                "mod:add_xp: amount must be a whole number from 1 to 1000000.");
        }

        return DynValue.NewBoolean(_host.AddXp((int)amount.Number));
    }

    internal DynValue CreateNpcProxy(
        ScriptModSession session,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        RequireMetadata(session, "mod:npc(id)");
        int offset = LuaArguments.ModOffset(args);
        string npcId = LuaArguments.RequiredString(args, offset, "mod:npc(id)").ToLowerInvariant();
        var table = new Table(session.Script);
        table.Set("id", DynValue.NewString(npcId));
        GeneratedSurface.RegisterNpc(table, this, session, npcId);
        return DynValue.NewTable(table);
    }

    internal DynValue GetNpcInfo(
        ScriptModSession session,
        string npcId,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        NpcSnapshot? snapshot = _host.GetNpc(npcId);
        if (snapshot == null)
            return DynValue.Nil;

        var table = new Table(session.Script);
        table.Set("id", DynValue.NewString(snapshot.Id));
        table.Set("name", DynValue.NewString(snapshot.Name));
        table.Set("region", DynValue.NewString(snapshot.Region));
        table.Set("relationship", DynValue.NewNumber(snapshot.Relationship));
        table.Set("is_unlocked", DynValue.NewBoolean(snapshot.IsUnlocked));
        table.Set("is_dead", DynValue.NewBoolean(snapshot.IsDead));
        return DynValue.NewTable(table);
    }

    internal DynValue ShowNpcText(
        ScriptModSession session,
        string npcId,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        int offset = LuaArguments.ModOffset(args);
        string text = LuaArguments.RequiredString(args, offset, "npc:say(text, seconds)");
        double duration = OptionalNumberArgument(args, offset + 1, 0.25, 60, 4, "npc:say(text, seconds)");
        return DynValue.NewBoolean(_host.ShowNpcText(npcId, text, duration));
    }

    internal DynValue SendNpcMessage(
        ScriptModSession session,
        string npcId,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        int offset = LuaArguments.ModOffset(args);
        string message = LuaArguments.RequiredString(args, offset, "npc:text(message)");
        return DynValue.NewBoolean(_host.SendNpcMessage(npcId, message));
    }

    internal DynValue AddNpcRelationship(
        ScriptModSession session,
        string npcId,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        int offset = LuaArguments.ModOffset(args);
        DynValue value = LuaArguments.At(args, offset, "npc:add_relationship(amount)");
        if (value.Type != DataType.Number || double.IsNaN(value.Number) || double.IsInfinity(value.Number) ||
            value.Number < -5 || value.Number > 5)
        {
            throw new ScriptRuntimeException("npc:add_relationship: amount must be a number from -5 to 5.");
        }
        return DynValue.NewBoolean(_host.AddNpcRelationship(npcId, value.Number));
    }

    internal DynValue UnlockNpc(
        ScriptModSession session,
        string npcId,
        ScriptExecutionContext context,
        CallbackArguments args) =>
        DynValue.NewBoolean(_host.UnlockNpc(npcId));

    internal DynValue SubscribeNpc(
        ScriptModSession session,
        string npcId,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        int offset = LuaArguments.ModOffset(args);
        string eventName = LuaArguments.RequiredString(args, offset, "npc:on(event, callback)").ToLowerInvariant();
        DynValue callback = RequiredCallback(args, offset + 1, "npc:on(event, callback)");
        NpcSubscriptionEvent parsed = eventName switch
        {
            "relationship_changed" => NpcSubscriptionEvent.RelationshipChanged,
            "unlocked" => NpcSubscriptionEvent.Unlocked,
            "died" => NpcSubscriptionEvent.Died,
            _ => throw new ScriptRuntimeException(
                $"npc:on: unknown event '{eventName}'. Choose relationship_changed, unlocked, or died.")
        };
        session.SubscribeNpc(npcId, parsed, callback);
        return DynValue.Nil;
    }

    internal DynValue DeclareMarker(
        ScriptModSession session,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        ModMetadata metadata = RequireMetadata(session, MarkerUsage);
        int offset = LuaArguments.ModOffset(args);
        Table options = LuaArguments.RequiredTable(args, offset, MarkerUsage);
        string localId = LuaArguments.RequiredFieldString(options, "id", MarkerUsage).ToLowerInvariant();
        ValidateIdentifier(localId, "marker ID", MarkerUsage);
        string id = $"{metadata.Id}:{localId}";
        string label = LuaArguments.OptionalFieldString(options, "label", MarkerUsage) ?? localId;
        string? npcId = LuaArguments.OptionalFieldString(options, "npc", MarkerUsage)?.ToLowerInvariant();
        string? icon = LuaArguments.OptionalFieldString(options, "icon", MarkerUsage);
        string visibility = LuaArguments.OptionalFieldString(options, "text", MarkerUsage)?.ToLowerInvariant() ?? "always";
        if (visibility is not ("always" or "hover" or "off"))
            throw new ScriptRuntimeException($"{MarkerUsage}: 'text' must be always, hover, or off.");
        bool visible = LuaArguments.OptionalFieldBoolean(options, "visible", MarkerUsage) ?? true;

        DynValue positionValue = LuaArguments.Field(options, "position");
        double? x = null;
        double? y = null;
        double? z = null;
        if (!LuaArguments.IsNil(positionValue))
        {
            if (positionValue.Type != DataType.Table)
                throw new ScriptRuntimeException($"{MarkerUsage}: 'position' must be a table with x, y, and z.");
            x = LuaArguments.RequiredFieldNumber(positionValue.Table, "x", -100_000, 100_000, MarkerUsage);
            y = LuaArguments.RequiredFieldNumber(positionValue.Table, "y", -100_000, 100_000, MarkerUsage);
            z = LuaArguments.RequiredFieldNumber(positionValue.Table, "z", -100_000, 100_000, MarkerUsage);
        }

        if (npcId == null && !x.HasValue)
            throw new ScriptRuntimeException($"{MarkerUsage}: provide either 'npc' or 'position'.");
        if (npcId != null && x.HasValue)
            throw new ScriptRuntimeException($"{MarkerUsage}: use either 'npc' or 'position', not both.");

        session.AddMarker(new MapMarkerRequest(
            id,
            session.SourceDirectory,
            label,
            x,
            y,
            z,
            npcId,
            icon,
            visibility,
            visible));
        return DynValue.NewString(id);
    }

    internal DynValue QueuePhoneCall(
        ScriptModSession session,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        RequireMetadata(session, CallUsage);
        int offset = LuaArguments.ModOffset(args);
        Table options = LuaArguments.RequiredTable(args, offset, CallUsage);
        string? npcId = LuaArguments.OptionalFieldString(options, "npc", CallUsage)?.ToLowerInvariant();
        string caller = LuaArguments.OptionalFieldString(options, "caller", CallUsage) ?? "Unknown Caller";
        string? icon = LuaArguments.OptionalFieldString(options, "icon", CallUsage);
        IReadOnlyList<string> stages = ReadStringList(options, "stages", CallUsage);
        return DynValue.NewBoolean(_host.QueuePhoneCall(new PhoneCallRequest(
            session.SourceDirectory,
            caller,
            npcId,
            icon,
            stages)));
    }

    internal DynValue CreateQuestProxy(
        ScriptModSession session,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        RequireMetadata(session, "mod:quest(name)");
        int offset = LuaArguments.ModOffset(args);
        string questName = LuaArguments.RequiredString(args, offset, "mod:quest(name)");
        var table = new Table(session.Script);
        table.Set("id", DynValue.NewString(questName));
        GeneratedSurface.RegisterQuest(table, this, session, questName);
        return DynValue.NewTable(table);
    }

    internal DynValue GetQuestInfo(
        ScriptModSession session,
        string questName,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        QuestSnapshot? snapshot = _host.GetQuest(questName);
        if (snapshot == null)
            return DynValue.Nil;
        var table = new Table(session.Script);
        table.Set("id", DynValue.NewString(snapshot.Id));
        table.Set("title", DynValue.NewString(snapshot.Title));
        return DynValue.NewTable(table);
    }

    internal DynValue SubscribeQuest(
        ScriptModSession session,
        string questName,
        ScriptExecutionContext context,
        CallbackArguments args)
    {
        int offset = LuaArguments.ModOffset(args);
        string eventName = LuaArguments.RequiredString(args, offset, "quest:on(event, callback)").ToLowerInvariant();
        DynValue callback = RequiredCallback(args, offset + 1, "quest:on(event, callback)");
        QuestSubscriptionEvent parsed = eventName switch
        {
            "completed" => QuestSubscriptionEvent.Completed,
            "failed" => QuestSubscriptionEvent.Failed,
            _ => throw new ScriptRuntimeException("quest:on: event must be completed or failed.")
        };
        session.SubscribeQuest(questName, parsed, callback);
        return DynValue.Nil;
    }

    private DynValue WriteLog(CallbackArguments args, S1LuaLogLevel level, string usage)
    {
        string message = LuaArguments.RequiredString(args, 0, usage);
        _host.Log(level, _session.DisplayName, message);
        return DynValue.Nil;
    }

    private static ModMetadata RequireMetadata(ScriptModSession session, string usage) =>
        session.Metadata ?? throw new ScriptRuntimeException($"{usage}: call s1.mod {{ ... }} first.");

    private static string ReadStateKey(CallbackArguments args, int index, string usage)
    {
        string key = LuaArguments.RequiredString(args, index, usage);
        if (key.Length > 64)
            throw new ScriptRuntimeException($"{usage}: storage keys may not exceed 64 characters.");
        return key;
    }

    private static StoredValue ToStoredValue(DynValue value)
    {
        return value.Type switch
        {
            DataType.String => StoredValue.FromString(value.String),
            DataType.Number when !double.IsNaN(value.Number) && !double.IsInfinity(value.Number) =>
                StoredValue.FromNumber(value.Number),
            DataType.Boolean => StoredValue.FromBoolean(value.Boolean),
            _ => throw new ScriptRuntimeException("mod:set: values must be a string, finite number, boolean, or nil.")
        };
    }

    private static DynValue ToDynValue(StoredValue value)
    {
        return value.Kind switch
        {
            StoredValueKind.String => DynValue.NewString(value.String ?? string.Empty),
            StoredValueKind.Number => DynValue.NewNumber(value.Number),
            StoredValueKind.Boolean => DynValue.NewBoolean(value.Boolean),
            _ => DynValue.Nil
        };
    }

    private static ShopSelection ReadShops(Table options)
    {
        DynValue value = LuaArguments.Field(options, "shops");
        if (LuaArguments.IsNil(value))
            return ShopSelection.None;
        if (value.Type == DataType.String)
        {
            if (string.Equals(value.String, "compatible", StringComparison.OrdinalIgnoreCase))
                return ShopSelection.Compatible;
            if (string.IsNullOrWhiteSpace(value.String))
                throw new ScriptRuntimeException($"{ItemUsage}: 'shops' cannot be empty.");
            return new ShopSelection(ShopSelectionKind.Named, new[] { value.String.Trim() });
        }
        if (value.Type != DataType.Table)
            throw new ScriptRuntimeException($"{ItemUsage}: 'shops' must be 'compatible' or a list of shop names.");

        var names = new List<string>();
        foreach (DynValue shop in value.Table.Values)
        {
            if (shop.Type != DataType.String || string.IsNullOrWhiteSpace(shop.String))
                throw new ScriptRuntimeException($"{ItemUsage}: every shop name must be a non-empty string.");
            names.Add(shop.String.Trim());
        }
        if (names.Count == 0)
            throw new ScriptRuntimeException($"{ItemUsage}: 'shops' cannot be an empty list.");
        return new ShopSelection(ShopSelectionKind.Named, names);
    }

    private static ClothingDeclaration? ReadClothing(Table options, string? clone, ref string? category)
    {
        DynValue value = LuaArguments.Field(options, "clothing");
        if (LuaArguments.IsNil(value))
            return null;
        if (value.Type != DataType.Table)
            throw new ScriptRuntimeException($"{ItemUsage}: 'clothing' must be a table.");
        if (category != null && !string.Equals(category, "clothing", StringComparison.OrdinalIgnoreCase))
            throw new ScriptRuntimeException($"{ItemUsage}: 'category' must be 'clothing' when clothing options are provided.");

        category = "clothing";
        Table clothing = value.Table;
        string? asset = LuaArguments.OptionalFieldString(clothing, "asset", ItemUsage);
        if (clone == null && asset == null)
            throw new ScriptRuntimeException($"{ItemUsage}: clothing without a clone must provide an 'asset' Resources path.");

        return new ClothingDeclaration(
            LuaArguments.OptionalFieldString(clothing, "slot", ItemUsage)?.ToLowerInvariant(),
            LuaArguments.OptionalFieldString(clothing, "application", ItemUsage)?.ToLowerInvariant(),
            asset,
            LuaArguments.OptionalFieldString(clothing, "texture", ItemUsage),
            LuaArguments.OptionalFieldBoolean(clothing, "colorable", ItemUsage),
            LuaArguments.OptionalFieldString(clothing, "default_color", ItemUsage)?.ToLowerInvariant(),
            ReadOptionalStringList(clothing, "blocked_slots", 10));
    }

    private static IReadOnlyList<string> ReadOptionalStringList(Table options, string field, int maximumCount)
    {
        DynValue value = LuaArguments.Field(options, field);
        if (LuaArguments.IsNil(value))
            return Array.Empty<string>();
        if (value.Type != DataType.Table)
            throw new ScriptRuntimeException($"{ItemUsage}: clothing '{field}' must be a list of strings.");

        var values = new List<string>();
        for (int index = 1; index <= value.Table.Length; index++)
        {
            DynValue entry = value.Table.Get(index);
            if (entry.Type != DataType.String || string.IsNullOrWhiteSpace(entry.String))
                throw new ScriptRuntimeException($"{ItemUsage}: every clothing '{field}' entry must be a non-empty string.");
            values.Add(entry.String.Trim().ToLowerInvariant());
        }
        if (values.Count > maximumCount)
            throw new ScriptRuntimeException($"{ItemUsage}: clothing '{field}' may contain at most {maximumCount} entries.");
        return values;
    }

    private static DynValue RequiredCallback(CallbackArguments args, int index, string usage)
    {
        DynValue callback = LuaArguments.At(args, index, usage);
        if (callback.Type is not (DataType.Function or DataType.ClrFunction))
            throw new ScriptRuntimeException($"{usage}: callback must be a function.");
        return callback;
    }

    private static DynValue ScheduleTimer(
        ScriptModSession session,
        CallbackArguments args,
        bool repeat,
        string usage)
    {
        RequireMetadata(session, usage);
        int offset = LuaArguments.ModOffset(args);
        DynValue seconds = LuaArguments.At(args, offset, usage);
        if (seconds.Type != DataType.Number || double.IsNaN(seconds.Number) || double.IsInfinity(seconds.Number) ||
            seconds.Number < 0.05 || seconds.Number > 86_400)
        {
            throw new ScriptRuntimeException($"{usage}: seconds must be a finite number from 0.05 to 86400.");
        }

        DynValue callback = RequiredCallback(args, offset + 1, usage);
        int id = session.ScheduleTimer(seconds.Number, repeat, callback);
        return DynValue.NewNumber(id);
    }

    private static double OptionalNumberArgument(
        CallbackArguments args,
        int index,
        double minimum,
        double maximum,
        double defaultValue,
        string usage)
    {
        if (args.Count <= index || LuaArguments.IsNil(args[index]))
            return defaultValue;
        DynValue value = args[index];
        if (value.Type != DataType.Number || double.IsNaN(value.Number) || double.IsInfinity(value.Number) ||
            value.Number < minimum || value.Number > maximum)
        {
            throw new ScriptRuntimeException($"{usage}: argument {index + 1} must be a number from {minimum} to {maximum}.");
        }
        return value.Number;
    }

    private static bool OptionalBooleanArgument(
        CallbackArguments args,
        int index,
        bool defaultValue,
        string usage)
    {
        if (args.Count <= index || LuaArguments.IsNil(args[index]))
            return defaultValue;
        DynValue value = args[index];
        if (value.Type != DataType.Boolean)
            throw new ScriptRuntimeException($"{usage}: argument {index + 1} must be true or false.");
        return value.Boolean;
    }

    private static IReadOnlyList<string> ReadStringList(Table options, string field, string usage)
    {
        DynValue value = LuaArguments.Field(options, field);
        if (value.Type != DataType.Table)
            throw new ScriptRuntimeException($"{usage}: '{field}' must be a list of text lines.");
        var values = new List<string>();
        for (int index = 1; index <= value.Table.Length; index++)
        {
            DynValue entry = value.Table.Get(index);
            if (entry.Type != DataType.String || string.IsNullOrWhiteSpace(entry.String))
                throw new ScriptRuntimeException($"{usage}: every '{field}' entry must be non-empty text.");
            values.Add(entry.String.Trim());
        }
        if (values.Count == 0)
            throw new ScriptRuntimeException($"{usage}: '{field}' must contain at least one line.");
        if (values.Count > 20)
            throw new ScriptRuntimeException($"{usage}: '{field}' may contain at most 20 lines.");
        return values;
    }

    private static void ValidateIdentifier(string value, string label, string usage)
    {
        if (value.Length > 80 || !IdentifierPattern.IsMatch(value))
        {
            throw new ScriptRuntimeException(
                $"{usage}: {label} '{value}' must use lowercase letters, numbers, dots, underscores, or hyphens and start with a letter or number.");
        }
    }
}
