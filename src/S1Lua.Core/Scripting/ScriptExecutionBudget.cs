using System.Diagnostics;
using MoonSharp.Interpreter;

namespace S1Lua.Scripting;

internal static class ScriptExecutionBudget
{
    private const int AutoYieldInstructions = 10_000;
    private static readonly TimeSpan MaximumExecutionTime = TimeSpan.FromSeconds(1);

    internal static DynValue RunSource(Script script, string source, string sourcePath)
    {
        DynValue function = script.LoadString(source, null, sourcePath);
        return RunFunction(script, function, $"{Path.GetFileName(sourcePath)} startup");
    }

    internal static DynValue RunFunction(
        Script script,
        DynValue function,
        string label,
        params DynValue[] arguments)
    {
        if (function.Type == DataType.ClrFunction)
        {
            return script.Call(function, arguments);
        }

        DynValue coroutineValue = script.CreateCoroutine(function);
        Coroutine coroutine = coroutineValue.Coroutine;
        coroutine.AutoYieldCounter = AutoYieldInstructions;
        var stopwatch = Stopwatch.StartNew();
        DynValue result = DynValue.Nil;

        bool firstResume = true;
        while (coroutine.State != CoroutineState.Dead)
        {
            result = coroutine.Resume(firstResume ? arguments : Array.Empty<DynValue>());
            firstResume = false;
            if (stopwatch.Elapsed > MaximumExecutionTime)
            {
                throw new ScriptRuntimeException(
                    $"{label} ran for more than {MaximumExecutionTime.TotalSeconds:0} second. " +
                    "Check for a loop that never ends; long-running work should wait for a future event.");
            }
        }

        return result;
    }
}
