using Bitbucket.Data;
using Bitbucket.HealthCheck;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace Bitbucket.DI
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {

            services.AddBloomFilter(setupActio =>
            {
                setupActio.UseInMemory();
            })
            .AddTransient<BarCodeGenerator>()
            .AddTransient<BloomFilterService>()
            .AddTransient<PrometheusService>();


            return services;
        }

        public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MSSQL");

            services.AddDbContext<AppDbContext>(x => x.UseSqlServer(connectionString));
            return services;
        }
        public static IServiceCollection AddVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MSSQL");

            services.AddHealthChecks()
                .AddCheck<BloomFilterHealthCheck>("bloomfilter-healthcheck")
                .AddSqlServer(connectionString)
                .ForwardToPrometheus();

            return services;
        }


    }
}