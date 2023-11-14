using Bitbucket.Data;
using Bitbucket.Middlewares;
using Bitbucket.Services;
using Bitbucket.SwaggerOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Bitbucket.Memory;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers(options =>
{
});

builder.Services.AddHealthChecks()
    .AddProcessAllocatedMemoryHealthCheck( 200, name: "200mb-memory-use", tags: new string[] {"More 200mb using"})
    .AddCheck<MemoryHealthCheck>("memory", tags: new string[] {"memory"});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    //opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
    //                                                new HeaderApiVersionReader("x-api-version"),
    //                                                new MediaTypeApiVersionReader("x-api-version"));
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
builder.Services.AddTransient<ShipmentService>();

builder.Services.AddDbContext<AppDbContext>(x=>x.UseInMemoryDatabase("BitBucket"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {

            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
        }
    });
}
app.UseRouting();
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseAuthorization();



app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});
app.Run();
