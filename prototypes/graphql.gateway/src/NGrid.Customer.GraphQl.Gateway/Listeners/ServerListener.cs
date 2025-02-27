using HotChocolate.AspNetCore.Instrumentation;

namespace NGrid.Customer.GraphQl.Gateway.Listeners;

public class ServerListener : ServerDiagnosticEventListener
{
    private readonly ILogger<ServerListener> _logger;

    public ServerListener(ILogger<ServerListener> logger)
    {
        _logger = logger;
    }

    public override IDisposable ExecuteHttpRequest(HttpContext context, HttpRequestKind kind)
    {
        _logger.LogInformation("HttpRequest: {Path}", context.Request.Path);
        return base.ExecuteHttpRequest(context, kind);
    }

    public override void HttpRequestError(HttpContext context, IError error)
    {
        error.LogError(_logger);
        base.HttpRequestError(context, error);
    }

    public override void HttpRequestError(HttpContext context, Exception exception)
    {
        _logger.LogInformation("{Method} - {Error}", nameof(HttpRequestError), exception.Message);
        base.HttpRequestError(context, exception);
    }
}