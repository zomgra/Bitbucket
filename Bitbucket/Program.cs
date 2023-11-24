using Bitbucket.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;
using Bitbucket.Helpers;
using Bitbucket.Controllers.V1;
using Bitbucket.DI;
using Bitbucket.Models.Interfaces;
using Bitbucket.Models;
using Bitbucket.Services.Fabrics;
using Microsoft.Extensions.Configuration;
using Bitbucket.Workers;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var version in ControllerVersionsHelper.GetControllerVersions(typeof(ShipmentsController)))
    {
        options.SwaggerDoc($"v{version} (YAML)", new()
        {
            Version = version,
        });
    }

}).AddSwaggerGenNewtonsoftSupport();

builder.Services.AddTransient<IShipmentRepository<Shipment>, ShipmentService>();
builder.Services.AddTransient<IBloomFilterRepository<Shipment>, BloomFilterService>();
builder.Services.AddSingleton<BloomFilterServiceFactory>();

builder.Services.Configure<HealthCheckOptions>(builder.Configuration.GetSection("HealthCheckOptions"));
builder.Services.AddHostedService<BloomFilterInitializerWorker>();

builder.Services.AddAppDbContext(builder.Configuration)
    .AddVersioning()
    .AddCustomHealthCheck(builder.Configuration)
    .AddServices();

var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

if (isDocker)
{
    builder.Logging.AddFilter((category, level) =>
            level == LogLevel.Information
        ? false
        : true).AddConsole();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseStaticFileWithYAMLMapper();
    app.UseSwaggerWithVersionsEndpoint();
}
app.MapControllers();


app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.UseHttpMetrics(options =>
{
    options.ReduceStatusCodeCardinality();
});


app.UseEndpoints(x =>
{
    x.MapMetrics();
});

app.Run();
