using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Klahisa.Web.Features.Auth;

namespace Klahisa.Web.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureClients(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IAuthApiClient, AuthApiClient>();
        services.AddScoped<AuthService>();
        services.AddScoped<CorretoraService>();
        return services;
    }

    public static IServiceCollection AddFeatureViews(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
        {
            options.ViewLocationFormats.Add("/Features/{1}/Views/{0}.cshtml");
            options.ViewLocationFormats.Add("/Features/{1}/Views/Shared/{0}.cshtml");
        });
        return services;
    }

    /// <summary>Cookie + política global: exige sessão em QUALQUER rota (exceto [AllowAnonymous]).</summary>
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/Login";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });
        return services;
    }
}
