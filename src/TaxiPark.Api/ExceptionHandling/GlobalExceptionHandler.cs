using Microsoft.AspNetCore.Diagnostics;
using TaxiPark.Application.Exceptions;
namespace TaxiPark.Api.ExceptionHandling;
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken ct)
    {
        if (ex is not AuthException auth) return false;
        context.Response.StatusCode = auth.Status;
        await Results.Problem(statusCode: auth.Status, title: auth.Message,
            extensions: new Dictionary<string, object?> { ["code"] = auth.Code }).ExecuteAsync(context);
        return true;
    }
}
