using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();

services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
services.AddMassTransit(config =>
{
    config.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);

        o.UsePostgres();
        o.UseBusOutbox();
    });

    config.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

app.Run();