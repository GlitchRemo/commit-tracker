using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution.Processing;
using HotChocolate.Resolvers;

namespace NGrid.Customer.GraphQl.Gateway.Listeners;

public class ExecutionListener : ExecutionDiagnosticEventListener
{
    private readonly ILogger<ExecutionListener> _logger;

    public ExecutionListener(ILogger<ExecutionListener> logger)
    {
        _logger = logger;
    }

    public override IDisposable ExecuteRequest(IRequestContext context)
    {
        _logger.LogInformation("Request - {Query} - {Document}", context.Request.Query, context.Document?.ToString());
        return base.ExecuteRequest(context);
    }

    public override IDisposable ResolveFieldValue(IMiddlewareContext context)
    {
        _logger.LogInformation("{Method} - {Path} - {Selection}", nameof(ResolveFieldValue), context.Path, context.Selection);
        return base.ResolveFieldValue(context);
    }

    public override void RequestError(IRequestContext context, Exception exception)
    {
        _logger.LogInformation("{Method} - {ErrorMessage}", nameof(RequestError), exception.Message);
    }

    public override void ResolverError(IMiddlewareContext context, IError error)
    {
        error.LogError(_logger);
    }
    public override void ResolverError(IRequestContext context, ISelection selection, IError error)
    {
        error.LogError(_logger);
        _logger.LogInformation($"Resolver Error: {context.Document} -> {selection.Field.Name}");
    }

    public override void TaskError(IExecutionTask task, IError error)
    {
        error.LogError(_logger);
    }
}