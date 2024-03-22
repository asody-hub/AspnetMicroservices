using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true);
builder.Services.AddOcelot()
                .AddCacheManager(options => options.WithDictionaryHandle());

//builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"))
//                .AddConsole()
//                .AddDebug();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
await app.UseOcelot();

app.Run();
