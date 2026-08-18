using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SmartMetroService.Api.Configurations;

public static class DefaultAuthenticationConfig
{
    public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "SmartMetro";
                options.DefaultChallengeScheme = "SmartMetro";
            })
            .AddPolicyScheme(
                "SmartMetro",
                "SmartMetro JWT or Keycloak",
                options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authorization = context.Request.Headers.Authorization.FirstOrDefault();

                        if (authorization?.StartsWith("Bearer ") == true)
                        {
                            var token = authorization["Bearer ".Length..].Trim();

                            // Keycloak tokens contain the Keycloak issuer.
                            if (token.Contains("."))
                            {
                                var parts = token.Split('.');

                                if (parts.Length == 3)
                                {
                                    try
                                    {
                                        var payload = parts[1];

                                        var jsonBytes =
                                            Convert.FromBase64String(
                                                payload
                                                    .Replace('-', '+')
                                                    .Replace('_', '/')
                                                    .PadRight(
                                                        payload.Length +
                                                        (4 - payload.Length % 4) % 4,
                                                        '='));

                                        var json = System.Text.Json.JsonDocument.Parse(jsonBytes);

                                        if (json.RootElement.TryGetProperty("iss", out var issuer))
                                        {
                                            var issuerValue = issuer.GetString();

                                            if (issuerValue == configuration["Keycloak:Issuer"])
                                            {
                                                return "Keycloak";
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        // Let the appropriate JWT handler reject it.
                                    }
                                }
                            }
                        }

                        return "SmartMetroJwt";
                    };
                })

            // Manual Jwt
            .AddJwtBearer("SmartMetroJwt", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),

                    ClockSkew = TimeSpan.Zero
                };
            })
            // Keycloak JWT
            .AddJwtBearer("Keycloak", options =>
            {
                options.Authority = configuration["Keycloak:Issuer"];
                options.Audience = configuration["Keycloak:Audience"];

                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Keycloak:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = configuration["Keycloak:Audience"],

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ClockSkew = TimeSpan.Zero,

                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;

                        if (identity == null)
                            return Task.CompletedTask;

                        var realmAccessClaim =
                            context.Principal?.FindFirst("realm_access");

                        if (realmAccessClaim != null)
                        {
                            using var document =
                                JsonDocument.Parse(realmAccessClaim.Value);

                            if (document.RootElement.TryGetProperty(
                                    "roles",
                                    out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    identity.AddClaim(
                                        new Claim(
                                            ClaimTypes.Role,
                                            role.GetString()!));
                                }
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
