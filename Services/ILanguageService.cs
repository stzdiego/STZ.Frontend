using STZ.Shared.Dtos;

namespace STZ.Frontend.Services;

public interface ILanguageService
{
    Task<ResourcesCultureDto> GetResourcesAsync(Guid cultureId);
}