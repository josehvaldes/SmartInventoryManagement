using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;

namespace SmartInventory.API.Controllers
{
    [ApiController]
    [Route("api/versions")]
    [ApiVersionNeutral]
    public class VersionsController(IApiVersionDescriptionProvider provider) : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var versions = provider.ApiVersionDescriptions
                .OrderBy(v => v.ApiVersion)
                .Select(v => new
                {
                    version      = v.GroupName,
                    isDeprecated = v.IsDeprecated,
                    sunsetPolicy = v.SunsetPolicy?.Date?.ToString("yyyy-MM-dd")
                });

            return Ok(versions);
        }
    }
}
