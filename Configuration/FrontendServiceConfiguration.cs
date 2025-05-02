using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using STZ.Frontend.Services;
using STZ.Shared.Bases;

namespace STZ.Frontend.Configuration;

public static class FrontendServiceConfiguration
{
    public static void AddSTZFrontendServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddScoped(typeof(ServiceBase<>));

        services.AddScoped<ILanguageService, LanguageService>();
        services.AddHttpClient<ILanguageService, LanguageService>(client =>
            {
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]
                                             ?? throw new InvalidOperationException("Base URL no configurada."));
            })
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var env = provider.GetRequiredService<IHostEnvironment>();
                if (env.IsDevelopment())
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }

                return new HttpClientHandler();
            });

        services.AddScoped<LocalizationService>();
    }
}