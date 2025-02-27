using NGrid.Customer.GraphQl.FusionGateway;

var builder = WebApplication.CreateBuilder(args);



builder.Services
    .AddCors()
    .AddSingleton<GatewayConfigurationProvider>()
    .AddHeaderPropagation(c =>
    {
        c.Headers.Add("GraphQL-Preflight");
        c.Headers.Add("Authorization");
    });

builder.Services
    .AddHttpClient("Fusion")
    .AddHeaderPropagation();

builder.Services
    .AddFusionGatewayServer()
    //.RegisterGatewayConfiguration(sp => sp.GetRequiredService<GatewayConfigurationProvider>())
    .ConfigureFromFile("gateway-config.json");

var app = builder.Build();

app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseHeaderPropagation();
app.MapGraphQL();
app.Run();