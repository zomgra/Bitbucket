using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace Bitbucket.DI
{
    public static class ApplicationBuilderExtentions
    {
        public static IApplicationBuilder UseStaticFileWithYAMLMapper(this IApplicationBuilder app)
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".yaml"] = "text/yaml";

            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider,
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Swagger")),
                RequestPath = "/CustomSwagger"
            });

            return app;
        }
        public static IApplicationBuilder UseSwaggerWithVersionsEndpoint(this IApplicationBuilder app)
        {
            var apiVersionDescriptionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

            var versions = apiVersionDescriptionProvider.ApiVersionDescriptions
                .Select(x => x.ApiVersion)
                .Select(x => $"{x.MajorVersion}");



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

            return app;
        }
    }
}