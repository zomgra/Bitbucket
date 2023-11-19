using Bitbucket.Middlewares;
using Bitbucket.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;
using Bitbucket.Helpers;
using Bitbucket.Controllers.V1;
using Bitbucket.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var version in ControllerVersions.GetControllerVersions(typeof(ShipmentsController)))
    {
        options.SwaggerDoc($"v{version} (YAML)", new()
        {
            Version = version,
        });
    }

}).AddSwaggerGenNewtonsoftSupport();

builder.Services.AddAppDbContext(builder.Configuration)
    .AddVersioning()
    .AddCustomHealthCheck(builder.Configuration)
    .AddServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseStaticFileWithYAMLMapper();
    app.UseSwaggerWithVersionsEndpoint();
}
app.UseMiddleware<ErrorHandlerMiddleware>();

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

var bloomFilterService = app.Services.CreateScope().ServiceProvider.GetRequiredService<BloomFilterService>();
await bloomFilterService.InjectFromDB();

app.Run();
