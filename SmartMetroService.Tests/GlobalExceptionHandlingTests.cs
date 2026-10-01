using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SmartMetroService.Api.Middleware;
using SmartMetroService.Api.Models;
using SmartMetroService.Application.Exceptions;
using Xunit;

namespace SmartMetroService.Tests;

public class GlobalExceptionHandlingTests
{
    [Fact]
    public async Task Middleware_Translates_ApiException_To_Consistent_Response()
    {
        var middleware = new ExceptionHandlingMiddleware(
            async context =>
            {
                throw new ValidationException("The request payload is invalid.");
            },
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<object>>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("The request payload is invalid.", response.Message);
        Assert.Null(response.Data);
    }
}
