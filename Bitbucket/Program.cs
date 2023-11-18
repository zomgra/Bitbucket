using Bitbucket.Data;
using Bitbucket.Middlewares;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using Bitbucket.HealthCheck;
using Prometheus;
using Bitbucket.Helpers;
using Bitbucket.Controllers.V1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddCheck<BloomFilterHealthCheck>("bloomfilter-healthcheck");

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

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

builder.Services.AddBloomFilter(setupAction =>
{
    setupAction.UseInMemory();
});

builder.Services.AddTransient<BarCodeGenerator>();
builder.Services.AddTransient<BloomFilterService>();
builder.Services.AddTransient<PrometheusService>();

builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    var versions = apiVersionDescriptionProvider.ApiVersionDescriptions
        .Select(x => x.ApiVersion)
        .Select(x => $"{x.MajorVersion}");

    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".yaml"] = "text/yaml";

    app.UseStaticFiles(new StaticFileOptions()
    {
        ContentTypeProvider = provider,
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Swagger")),
        RequestPath = "/CustomSwagger"
    });

    app.UseRouting();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var version in versions)
        {
            c.SwaggerEndpoint($"/CustomSwagger/SwaggerV{version}.yaml", $"v{version} (YAML)");
        }
    });
    app.UseEndpoints(end =>
    {
        foreach (var version in versions)
        {
            end.MapGet($"/CustomSwagger/SwaggerV{version}.yaml", async context =>
            {
                context.Response.ContentType = "application/yaml";
                var json = await File.ReadAllTextAsync($"SwaggerV{version}.yaml");
                await context.Response.WriteAsync(json);
            });
        }
    });
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
    options.AddCustomLabel("host", context => context.Request.Host.Host);
});

app.UseEndpoints(x =>
{
    x.MapMetrics();
});

var bloomFilterService = app.Services.CreateScope().ServiceProvider.GetRequiredService<BloomFilterService>();
//await bloomFilterService.InjectFromDB();

app.Run();
