using Bitbucket.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;
using Bitbucket.Helpers;
using Bitbucket.DI;
using Bitbucket.Models.Interfaces;
using Bitbucket.Models;
using Bitbucket.Services.Fabrics;
using Bitbucket.Workers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var version in ControllerVersionsHelper.GetControllerVersions())
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


builder.Services.AddAppDbContext(builder.Configuration);

    builder.Services.AddVersioning()
    .AddCustomHealthCheck(builder.Configuration)
    .AddServices();

builder.Services.AddHostedService<BloomFilterInitializerWorker>();
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
