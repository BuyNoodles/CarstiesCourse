using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
// Add services to the container.

services.AddControllers();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();

services.AddMassTransit(config =>
{
    config.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

services.AddHttpClient<AuctionServiceHttpClient>()
    .AddPolicyHandler(GetPolicy());

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
});

await app.RunAsync();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));