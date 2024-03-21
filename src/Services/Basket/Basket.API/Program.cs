using Basket.API.GrpcServices;
using Basket.API.Mapper;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using MassTransit;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString"));

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddAutoMapper(typeof(BasketProfile));

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options => 
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]));
builder.Services.AddScoped<DiscountGrpcService>();

builder.Services.AddMassTransit(config => {
    config.UsingRabbitMq((context, configurator) => {
        configurator.Host(builder.Configuration["EventBusSettings:HostAddress"]);
    });
});
//builder.Services.AddMassTransitHostedService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
