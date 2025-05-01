using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STZ.Frontend.Services;
using STZ.Shared.Bases;

namespace STZ.Frontend.Configuration;

public static class FrontendServiceConfiguration
{
    public static void AddFrontendServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddScoped(typeof(ServiceBase<>));
        
        services.AddScoped<ICultureService, CultureService>();
        services.AddHttpClient<ICultureService, CultureService>(client =>
        {
            client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]
                                         ?? throw new InvalidOperationException("Base URL no configurada."));
        });
        services.AddScoped<LocalizationService>();
    }
}