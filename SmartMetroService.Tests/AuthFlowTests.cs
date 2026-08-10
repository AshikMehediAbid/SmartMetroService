using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartMetroService.Storage.Sql;
using Xunit;

namespace SmartMetroService.Tests;

public class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_SetsRefreshTokenInHttpOnlyCookie_AndLogout_RevokesIt()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var loginPayload = new
        {
            phoneNumber = "+8801234567890",
            passWord = "StrongPass123!"
        };

        var registerResponse = await client.PostAsync("/api/account/register", new StringContent(
            JsonSerializer.Serialize(new
            {
                name = "Test User",
                email = "test@example.com",
                phoneNumber = "+8801234567890",
                password = "StrongPass123!",
                confirmPassword = "StrongPass123!"
            }),
            Encoding.UTF8,
            "application/json"));

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await client.PostAsync("/api/account/login", new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json"));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        Assert.Contains(cookies, c => c.Contains("refreshToken=", StringComparison.OrdinalIgnoreCase) && c.Contains("httponly", StringComparison.OrdinalIgnoreCase));

        var refreshCookie = cookies.Single(c => c.Contains("refreshToken="));
        var refreshToken = refreshCookie.Split("refreshToken=")[1].Split(';')[0];

        Assert.False(string.IsNullOrWhiteSpace(refreshToken));

        var logoutResponse = await client.PostAsync("/api/account/logout", new StringContent("", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        Assert.Contains(logoutResponse.Headers.GetValues("Set-Cookie"), c => c.Contains("refreshToken=", StringComparison.OrdinalIgnoreCase) && c.Contains("expires=", StringComparison.OrdinalIgnoreCase));
    }
}
