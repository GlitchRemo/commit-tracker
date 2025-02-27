using System.Net.Http.Headers;
using HotChocolate.Execution;
using HotChocolate.Stitching.Pipeline;

namespace NGrid.Customer.GraphQl.Gateway;

public class JwtInjector : HttpStitchingRequestInterceptor
{
    internal static string Jwt;

    public override ValueTask OnCreateRequestAsync(string targetSchema, IQueryRequest request, HttpRequestMessage requestMessage,
        CancellationToken cancellationToken = new CancellationToken())
    {
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Jwt);
        return base.OnCreateRequestAsync(targetSchema, request, requestMessage, cancellationToken);
    }
}