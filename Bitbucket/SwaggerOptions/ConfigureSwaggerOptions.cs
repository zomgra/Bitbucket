using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bitbucket.SwaggerOptions
{
    public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>

    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(
            IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }
        public void Configure(string name, SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    CreateVersionInfo(description));
            };
        }

        public void Configure(SwaggerGenOptions options)
        {
            Configure(options);
        }
        private OpenApiInfo CreateVersionInfo(
            ApiVersionDescription desc)
        {
            var info = new OpenApiInfo()
            {
                Title = "Serdiuk Mykyta BitBucket",
                Version = desc.ApiVersion.ToString()
            };

            info.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0);

            if (desc.IsDeprecated)
            {
                info.Description += " Serdiuk Mykyta BitBucket";
            }
            return info;
        }
    }
}