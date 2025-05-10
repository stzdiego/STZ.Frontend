using System.Net.Http.Json;
using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STZ.Shared.Bases;
using STZ.Shared.Entities;

namespace STZ.Frontend.Configuration;

public static class AuthenticationConfiguration
{
    public static void STZAuth0(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddAuthorization();
        services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = configuration["Auth0:Domain"];
            options.ClientId = configuration["Auth0:ClientId"];
            options.ClientSecret = configuration["Auth0:ClientSecret"];
            options.CallbackPath = configuration["Auth0:CallbackPath"];
            options.Scope = "openid profile email";
        });
        services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/authentication/login";
            options.LogoutPath = "/authentication/logout";
            options.AccessDeniedPath = "/authentication/access-denied";

            options.Events = new CookieAuthenticationEvents()
            {
                OnSignedIn = async context =>
                {
                    var identity = (ClaimsIdentity)context.Principal!.Identity;
                    var email = identity.FindFirst(ClaimTypes.Email)?.Value;
                    var firstName = identity.FindFirst(ClaimTypes.GivenName)?.Value;
                    var lastName = identity.FindFirst(ClaimTypes.Surname)?.Value;

                    if (!string.IsNullOrEmpty(email))
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<ServiceBase<User>>();

                        var newUser = new User
                        {
                            Email = email,
                            FirstName = firstName ?? string.Empty,
                            LastName = lastName ?? string.Empty
                        };
                        
                        try
                        {
                            if (!await userService.ExistsAsync("Email", email))
                            {
                                await userService.AddAsync(newUser);
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            // Log o manejo si ya existe, dependiendo del comportamiento de tu API
                            Console.WriteLine($"Error al registrar usuario: {ex.Message}");
                        }
                    }
                }
            };
        });
    }
}