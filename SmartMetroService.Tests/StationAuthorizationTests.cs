using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartMetroService.Application.Models;
using SmartMetroService.Storage.Sql;
using Xunit;

namespace SmartMetroService.Tests;

public class StationAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StationAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private string GenerateJwtToken(string role)
    {
        var config = _factory.Services.GetRequiredService<IConfiguration>();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.MobilePhone, "+8801234567890"),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(3),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task CreateStation_WithoutToken_Returns401()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();

        var stationDto = new StationCreationDto
        {
            StationName = "Test Station",
            InsertAfter = 0
        };

        var response = await client.PostAsync(
            "/api/station/create",
            new StringContent(JsonSerializer.Serialize(stationDto), Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateStation_WithUserRole_Returns403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();
        var userToken = GenerateJwtToken("User");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

        var stationDto = new StationCreationDto
        {
            StationName = "Test Station",
            InsertAfter = 0
        };

        var response = await client.PostAsync(
            "/api/station/create",
            new StringContent(JsonSerializer.Serialize(stationDto), Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateStation_WithAdminRole_Returns200()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();
        var adminToken = GenerateJwtToken("Admin");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        var stationDto = new StationCreationDto
        {
            StationName = "Test Station",
            InsertAfter = 0
        };

        var response = await client.PostAsync(
            "/api/station/create",
            new StringContent(JsonSerializer.Serialize(stationDto), Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStation_WithoutToken_Returns401()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();

        var stationUpdateDto = new StationUpdateDto
        {
            StationName = "Updated Station",
            InsertAfter = 0
        };

        var response = await client.PutAsync(
            "/api/station/1",
            new StringContent(JsonSerializer.Serialize(stationUpdateDto), Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStation_WithUserRole_Returns403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();
        var userToken = GenerateJwtToken("User");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

        var stationUpdateDto = new StationUpdateDto
        {
            StationName = "Updated Station",
            InsertAfter = 0
        };

        var response = await client.PutAsync(
            "/api/station/1",
            new StringContent(JsonSerializer.Serialize(stationUpdateDto), Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteStation_WithoutToken_Returns401()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();

        var response = await client.DeleteAsync("/api/station/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteStation_WithUserRole_Returns403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();
        var userToken = GenerateJwtToken("User");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

        var response = await client.DeleteAsync("/api/station/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAllStations_WithoutToken_Returns200()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/station/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetStationFare_WithoutToken_Returns200()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/station/fare?fromStationId=1&toStationId=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
