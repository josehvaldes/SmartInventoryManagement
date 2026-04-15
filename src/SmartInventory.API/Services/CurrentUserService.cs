using SmartInventory.Application.Common.Interfaces;
using System.Security.Claims;

namespace SmartInventory.API.Services
{
    public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
    {
        public string Username => accessor.HttpContext?.User.Identity?.Name ?? "Unknown";
        public string UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
    }
}
