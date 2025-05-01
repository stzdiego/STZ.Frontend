using STZ.Shared.Dtos;

namespace STZ.Frontend.Services;

public interface ICultureService
{
    Task<ResourcesCultureDto> GetResourcesAsync(Guid cultureId);
}