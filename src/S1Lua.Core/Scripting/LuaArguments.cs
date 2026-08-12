using MoonSharp.Interpreter;

namespace S1Lua.Scripting;

internal static class LuaArguments
{
    internal static Table RequiredTable(CallbackArguments args, int index, string usage)
    {
        DynValue value = At(args, index, usage);
        if (value.Type != DataType.Table)
            throw new ScriptRuntimeException($"{usage}: expected a table for argument {index + 1}.");
        return value.Table;
    }

    internal static int ModOffset(CallbackArguments args) =>
        args.Count > 0 && args[0].Type == DataType.Table ? 1 : 0;

    internal static string RequiredString(CallbackArguments args, int index, string usage)
    {
        DynValue value = At(args, index, usage);
        if (value.Type != DataType.String || string.IsNullOrWhiteSpace(value.String))
            throw new ScriptRuntimeException($"{usage}: argument {index + 1} must be a non-empty string.");
        return value.String.Trim();
    }

    internal static DynValue At(CallbackArguments args, int index, string usage)
    {
        if (index < 0 || index >= args.Count)
            throw new ScriptRuntimeException($"{usage}: missing argument {index + 1}.");
        return args[index];
    }

    internal static DynValue Field(Table table, string name) => table.Get(name);

    internal static string RequiredFieldString(Table table, string name, string usage)
    {
        DynValue value = Field(table, name);
        if (value.Type != DataType.String || string.IsNullOrWhiteSpace(value.String))
            throw new ScriptRuntimeException($"{usage}: '{name}' must be a non-empty string.");
        return value.String.Trim();
    }

    internal static string? OptionalFieldString(Table table, string name, string usage)
    {
        DynValue value = Field(table, name);
        if (IsNil(value))
            return null;
        if (value.Type != DataType.String || string.IsNullOrWhiteSpace(value.String))
            throw new ScriptRuntimeException($"{usage}: '{name}' must be a non-empty string when provided.");
        return value.String.Trim();
    }

    internal static double RequiredFieldNumber(
        Table table,
        string name,
        double minimum,
        double maximum,
        string usage)
    {
        double? value = OptionalFieldNumber(table, name, minimum, maximum, usage);
        if (!value.HasValue)
            throw new ScriptRuntimeException($"{usage}: '{name}' must be provided.");
        return value.Value;
    }

    internal static double? OptionalFieldNumber(Table table, string name, double minimum, double maximum, string usage)
    {
        DynValue value = Field(table, name);
        if (IsNil(value))
            return null;
        if (value.Type != DataType.Number || double.IsNaN(value.Number) || double.IsInfinity(value.Number) ||
            value.Number < minimum || value.Number > maximum)
        {
            throw new ScriptRuntimeException($"{usage}: '{name}' must be a number from {minimum} to {maximum}.");
        }
        return value.Number;
    }

    internal static int? OptionalFieldInteger(Table table, string name, int minimum, int maximum, string usage)
    {
        double? value = OptionalFieldNumber(table, name, minimum, maximum, usage);
        if (!value.HasValue)
            return null;
        if (Math.Abs(value.Value - Math.Round(value.Value)) > double.Epsilon)
            throw new ScriptRuntimeException($"{usage}: '{name}' must be a whole number.");
        return (int)value.Value;
    }

    internal static bool? OptionalFieldBoolean(Table table, string name, string usage)
    {
        DynValue value = Field(table, name);
        if (IsNil(value))
            return null;
        if (value.Type != DataType.Boolean)
            throw new ScriptRuntimeException($"{usage}: '{name}' must be true or false.");
        return value.Boolean;
    }

    internal static bool IsNil(DynValue value) => value.Type is DataType.Nil or DataType.Void;
}
