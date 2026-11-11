using SmartInventory.Application.Common.Interfaces;
using System.Security.Claims;

namespace SmartInventory.API.Services
{
    public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
    {
        public string Username => accessor.HttpContext?.User.Identity?.Name ?? throw new InvalidOperationException("No username found in the current context.");
        public string UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("No user ID found in the current context.");
    }
}
