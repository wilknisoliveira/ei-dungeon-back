using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace ei_back.Infrastructure.Exceptions
{
    public class AppExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<AppExceptionHandler> _logger;

        public AppExceptionHandler(ILogger<AppExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            (int statusCode, string errorMessage) = exception switch
            {
                BadGatewayException badGatewayException => (StatusCodes.Status502BadGateway, badGatewayException.Message),
                BadRequestException badRequestException => (StatusCodes.Status400BadRequest, badRequestException.Message),
                NotFoundException notFoundException => (StatusCodes.Status404NotFound, notFoundException.Message),
                _ => (StatusCodes.Status500InternalServerError, "Something went wrong")
            };

            _logger.LogError(exception, $"{statusCode} - {exception.GetType().Name}: {errorMessage}. StackTrace: {exception.StackTrace}");

            ExceptionResponse exceptionResponse = new(statusCode, exception.GetType().Name, errorMessage);
            
            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(exceptionResponse, cancellationToken);

            return true;
        }
    }
}
