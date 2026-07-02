using FluentValidation;
using KRT.Domain.Exceptions;
using System.Text.Json;

namespace KRT.API.Middlewares;

public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await WriteResponse(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage);
            await WriteResponse(context, StatusCodes.Status400BadRequest, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado: {Message}", ex.Message);
            await WriteResponse(context, StatusCodes.Status500InternalServerError, "Ocorreu um erro interno.");
        }
    }

    private static async Task WriteResponse(HttpContext context, int statusCode, object body)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { errors = body }));
    }
}
