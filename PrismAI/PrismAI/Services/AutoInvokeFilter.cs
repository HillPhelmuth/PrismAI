using Microsoft.SemanticKernel;

namespace PrismAI.Services;

public class AutoInvokeFilter : IAutoFunctionInvocationFilter
{
    public event Action<AutoFunctionInvocationContext, string>? AutoFunctionInvocationStarted;
    public event Action<AutoFunctionInvocationContext, string>? AutoFunctionInvocationCompleted;

    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        var connectionId = context.Kernel.Data["connectionId"] as string;
        Console.WriteLine($"\n=============================\nFunction Called: {context.Function.Name}\n=============================\nconnection: {connectionId}\n=============================\n");
        
        AutoFunctionInvocationStarted?.Invoke(context, connectionId ?? "");
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var value = $"An error has occurred. Move on to a different request.\n\n{ex}";
            Console.WriteLine(value);
            context.Result = new FunctionResult(context.Function, value);
        }
        //if (!string.IsNullOrEmpty(connectionId))
        AutoFunctionInvocationCompleted?.Invoke(context, connectionId ?? "");
        
    }
}