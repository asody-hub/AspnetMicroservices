using Common.Logging;
using EventBus.Messages.Common;
using MassTransit;
using Ordering.API.EventBusConsumer;
using Ordering.API.Extensions;
using Ordering.API.Mapper;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddAutoMapper(typeof(OrderingProfile));
builder.Services.AddScoped<BasketCheckoutConsumer>();

builder.Services.AddMassTransit(mtconfig =>
    {
        mtconfig.AddConsumer<BasketCheckoutConsumer>();
        mtconfig.UsingRabbitMq((rmqcontext, rmqconfig) => {
            rmqconfig.Host(builder.Configuration["EventBusSettings:HostAddress"]);
            rmqconfig.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, reconfig =>
                reconfig.ConfigureConsumer<BasketCheckoutConsumer>(rmqcontext));
        });
    });
//builder.Services.AddMassTransitHostedService();

builder.Host.UseSerilog(Serilogger.Configure);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase<OrderContext>((context, services) =>
    {
        var logger = services.GetService<ILogger<OrderContextSeed>>();
        OrderContextSeed.SeedAsync(context, logger).Wait();
    });

app.Run();
