using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
using HotChocolate.Stitching.Pipeline;
using Microsoft.AspNetCore.HeaderPropagation;
using NGrid.Customer.GraphQl.Gateway;
using NGrid.Customer.GraphQl.Gateway.Listeners;
using StackExchange.Redis;

var superGraphConfig = new SuperGraphConfig();
var opaOptions = new OpaOptions
{
    BaseAddress = new Uri(superGraphConfig.GetBaseUrl("accountlink")),
    // PolicyResultHandlers =
    // {
    //     { "foo", opaResponse =>
    //     {
    //         opaResponse.
    //     }}
    // }
};
var builder = WebApplication.CreateBuilder(args);
var useRedis = false;
if (useRedis)
{
    builder.Services.AddSingleton(ConnectionMultiplexer.Connect("localhost:7000"));
}

JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap = new Dictionary<string, string>
{
    { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "sub" }
};
var tokenHandler = new JwtSecurityTokenHandler();
var claim = new ClaimsIdentity(new List<Claim> { new(ClaimTypes.NameIdentifier, "660ef6fa-fb12-4291-9d0d-3c0ca793e7d6")});
var token = tokenHandler.CreateJwtSecurityToken("god", "mankind", claim);
var jwt = tokenHandler.WriteToken(token);

var graphQl =
    builder.Services
        .AddHeaderPropagation(options => options.Headers = new HeaderPropagationEntryCollection { "Authorization" })
        .Configure<SuperGraphConfig>(_ => { })
        .AddGraphQLServer()
        .AddDiagnosticEventListener<ExecutionListener>()
        .AddDiagnosticEventListener<ServerListener>();

foreach (var subGraph in superGraphConfig.SchemaToEndpointMap)
{
    if (useRedis)
    {
        graphQl.AddRemoteSchemasFromRedis(subGraph.Key, sp => sp.GetRequiredService<ConnectionMultiplexer>());
    }
    else
    {
        graphQl.AddRemoteSchema(subGraph.Key);
    }
    builder.Services.AddHttpClient(subGraph.Key, c =>
    {
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        c.BaseAddress = new Uri(subGraph.Value);
    });
}

var webApplication = builder.Build();
webApplication
    .UseRouting()
    .UseHeaderPropagation()
    .UseEndpoints(endpoints => endpoints.MapGraphQL());

webApplication.Run();