using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using STZ.Shared.Bases;
using STZ.Shared.Dtos;
using STZ.Shared.Entities;

namespace STZ.Frontend.Services;

public class LanguageService : ServiceBase<Culture>, ILanguageService
{
    public LanguageService(HttpClient httpClient, IConfiguration configuration) 
        : base(httpClient, configuration) {}

    public async Task<ResourcesCultureDto> GetResourcesAsync(Guid cultureId)
    {
        try
        {
            var response =
                await HttpClient.GetFromJsonAsync<ResourcesCultureDto>($"{Endpoint}/{cultureId.ToString()}/resources");

            if (response is null)
                throw new Exception("Error al obtener los recursos de la cultura");

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResourcesCultureDto()
            {
                CultureCode = "",
                Date = DateTime.MinValue,
                Resources = []
            };
        }
    }
}