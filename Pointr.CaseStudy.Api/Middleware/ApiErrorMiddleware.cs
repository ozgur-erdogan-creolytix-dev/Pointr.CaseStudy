using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Pointr.CaseStudy.Application.Common;

namespace Pointr.CaseStudy.Api.Middleware;

public sealed class ApiErrorMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundAppException ex)
        {
            await Write(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ValidationAppException ex)
        {
            await Write(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ConcurrencyAppException ex)
        {
            await Write(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await Write(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            await Write(context, HttpStatusCode.InternalServerError, "Unexpected error.", ex);
        }
    }

    private static Task Write(
        HttpContext ctx,
        HttpStatusCode code,
        string message,
        Exception? ex = null
    )
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)code;

        var payload = new
        {
            error = message,
            status = (int)code,
            traceId = ctx.TraceIdentifier,
            detail = ex?.ToString(),
        };

        return ctx.Response.WriteAsJsonAsync(payload);
    }
}
