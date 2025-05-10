using System.Security.Claims;

namespace STZ.Frontend.Authorization;

public interface IPermissionService
{
    Task<bool> HasAccessAsync(ClaimsPrincipal user, string feature, string action);
}