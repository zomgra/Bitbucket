using Bitbucket.Exceptions;
using Bitbucket.Responces;

namespace Bitbucket.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<ErrorHandlerMiddleware> logger)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch(DomainException ex)
            {
                logger.LogError("Error on the domain: {message}", ex.Message);
                context.Response.StatusCode = ex.StatusCode;
                await context.Response.WriteAsJsonAsync(new DomainError
                {
                    Message = ex.Message 
                });
            }
        }
    }
}