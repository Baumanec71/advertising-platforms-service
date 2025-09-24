using advertising_platforms_service.Domain.ViewModels;
using advertising_platforms_service.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace advertising_platforms_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisingPlatformController : Controller
    {
        private readonly IAdvertisingPlatformService _advertisingPlatformService;
        private readonly IWorkToTxtFile _workToTxtFile;
        public AdvertisingPlatformController(IAdvertisingPlatformService advertisingPlatformService, IWorkToTxtFile workToTxtFile)
        {
            _advertisingPlatformService = advertisingPlatformService;
            _workToTxtFile = workToTxtFile;
        }

        [HttpGet("GetPlatforms")]
        public ActionResult GetPlatformsWithLocations([FromQuery] string? location = null, [FromQuery] int page = 1)
        {
            var filterModel = new AdvertisingPlatformFilterModel { Location = location };

            var platforms = _advertisingPlatformService.GetPlatforms(page, filterModel);

            if (platforms.StatusCode == HttpStatusCode.OK)
            {
                return Ok(platforms.Value);
            }

            return BadRequest(platforms.Description);
        }

        [HttpPut("UpdatePlatforms")]
        public async Task<IActionResult> UpdatePlatforms()
        {
            var readResult = await _workToTxtFile.ReadFile();

            if (readResult.StatusCode != HttpStatusCode.OK)
            {
                return StatusCode((int)readResult.StatusCode, readResult.Description);
            }

            var updateResult = _advertisingPlatformService
                .Update(readResult.Value!);

            if (updateResult.StatusCode == HttpStatusCode.OK)
            {
                return Ok(new
                {
                    description = updateResult.Description,
                });
            }

            return BadRequest(updateResult.Description);
        }

    }
}
