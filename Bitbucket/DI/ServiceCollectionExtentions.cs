using Bitbucket.Data;
using Bitbucket.HealthCheck;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;

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

        public static async Task<IServiceCollection> AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MSSQL");
            var dbConnectTry = int.Parse(Environment.GetEnvironmentVariable("DB_CONNECT_TRY"));
            var dbConnectDelay = int.Parse(Environment.GetEnvironmentVariable("DB_CONNECT_DELAY"));
            
            for (int i = 0; i < dbConnectTry; i++)
            {
                try
                {
                    Log.Warning("Connect to DB Attention: {i}", i);
                    services.AddDbContext<AppDbContext>(x => x.UseSqlServer(connectionString));
                    return services;
                }
                catch(Exception ex)
                {
                    Log.Warning($"Failed to connect to the database. Exception: {ex}");
                    await Task.Delay(dbConnectDelay);
                }
            }
            Log.Fatal("Connect to DB fatal error");
            Environment.Exit(1);
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