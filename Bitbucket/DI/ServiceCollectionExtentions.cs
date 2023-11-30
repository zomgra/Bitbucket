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

        public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MSSQL");
            string dbConnectTryString = Environment.GetEnvironmentVariable("DB_CONNECT_TRY");
            int dbConnectTry = 3;
            if (dbConnectTryString != null)
                dbConnectTry = int.TryParse(dbConnectTryString, out var result) ? result : 3;
            
            var dbConnectDelayString = Environment.GetEnvironmentVariable("DB_CONNECT_DELAY");
            var dbConnectDelay = 10000;
            if(dbConnectDelayString != null)
                dbConnectDelay = int.TryParse(dbConnectTryString, out int result) ? result : 10000; 

            Log.Information("Connection string: {string}", connectionString);
            try
            {
                services.AddDbContext<AppDbContext>(x => x.UseSqlServer(connectionString, sqlServerOptionsAction: sql =>
                {
                    sql.EnableRetryOnFailure(
                        maxRetryCount: dbConnectTry, 
                        maxRetryDelay: TimeSpan.FromMilliseconds(dbConnectDelay),
                        errorNumbersToAdd: null);
                })) ;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to connect to the database1. Exception: {ex}");
                Environment.Exit(0);
            }

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
                .ForwardToPrometheus();

            return services;
        }


    }
}