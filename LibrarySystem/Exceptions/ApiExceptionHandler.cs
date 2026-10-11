using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Exceptions
{
    // Único lugar donde las excepciones de los servicios se traducen a códigos HTTP.
    public class ApiExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, code) = exception switch
            {
                InvalidInputException => (StatusCodes.Status400BadRequest, "invalid_input"),
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "invalid_credentials"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "forbidden"),
                NotFoundException => (StatusCodes.Status404NotFound, "not_found"),
                ConflictException => (StatusCodes.Status409Conflict, "conflict"),
                _ => (0, string.Empty)
            };

            // Cualquier otra excepción sigue el camino normal y responde 500.
            if (status == 0)
            {
                return false;
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = code,
                Detail = exception.Message
            };
            problem.Extensions["code"] = code;

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }
    }
}
