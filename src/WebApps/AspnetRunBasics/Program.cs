using AspnetRunBasics.Services;
using Common.Logging;
using Serilog;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Host.UseSerilog(Serilogger.Configure);
builder.Services.AddTransient<LoggingDelegatingHandler>();

//Add http client services at ConfigureServices(IServiceCollection services)
builder.Services.AddHttpClient<ICatalogService, CatalogService>(client =>
                client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayUrl"]))
    .AddHttpMessageHandler<LoggingDelegatingHandler>();
builder.Services.AddHttpClient<IBasketService, BasketService>(client =>
                client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayUrl"]))
    .AddHttpMessageHandler<LoggingDelegatingHandler>();
builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
                client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayUrl"]))
    .AddHttpMessageHandler<LoggingDelegatingHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

