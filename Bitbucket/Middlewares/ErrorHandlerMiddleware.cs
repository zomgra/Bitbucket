using Bitbucket.Exceptions;
using Bitbucket.Responces;
using Bitbucket.Services;
using Prometheus;

namespace Bitbucket.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<ErrorHandlerMiddleware> logger, PrometheusService prometheusService)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch(DomainException ex)
            {
                prometheusService.ErrorCounter.Inc();
                context.Response.StatusCode = ex.StatusCode;
                await context.Response.WriteAsJsonAsync(new DomainError
                {
                    Message = ex.Message 
                });
            }
            catch
            {
                prometheusService.ErrorCounter.Inc();
            }
        }
    }
}