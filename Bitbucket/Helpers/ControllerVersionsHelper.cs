using Bitbucket.Controllers.V1;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Bitbucket.Helpers
{
    public static class ControllerVersionsHelper
    {
        public static IEnumerable<string> GetControllerVersions<T>(T foundedControllerType) where T : Type
        {
            var assembly = Assembly.GetExecutingAssembly();
            var controllers = assembly.GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);

            var apiVersions = controllers
                .SelectMany(type => type.GetCustomAttributes<ApiVersionAttribute>().SelectMany(attr => attr.Versions))
                .Distinct()
                .OrderBy(version => version)
                .Select(version => version.ToString())
                .ToList();

            return apiVersions;
        }
    }
}