using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NGrid.Customer.Framework.FunctionalTests.Base.Models;
using NGrid.Customer.Framework.StreamingServiceHost.Streaming;
using NGrid.Customer.Framework.WebClient;

namespace NGrid.Customer.Framework.FunctionalTests.Base.Streaming;

public class RequestOutput<T> : IStreamOutput<T>
{
    private readonly IHttpClientCache _httpClientCache;
    private readonly ILogger<RequestOutput<T>> _logger;
    private readonly IOptions<WebCallConfig> _webCallConfig;

    public RequestOutput(IHttpClientCache httpClientCache, ILogger<RequestOutput<T>> logger, IOptions<WebCallConfig> webCallConfig)
    {
        _httpClientCache = httpClientCache;
        _logger = logger;
        _webCallConfig = webCallConfig;
    }

    private ILogger Logger => _logger;

    public virtual async Task Handle(T itemToHandle)
    {
        var client = _httpClientCache.Get(_webCallConfig.Value.BaseUrl);
        Logger.LogInformation("HTTP Post");
        await client.PostAsync(_webCallConfig.Value.Path, itemToHandle);
    }
}