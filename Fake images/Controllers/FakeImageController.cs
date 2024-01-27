using Azure.Storage.Blobs;
using Fake_images.Models;
using Fake_images.Models.Additional;
using Fake_images.Models.Context;
using Fake_images.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fake_images.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FakeImageController : ControllerBase
    {        
        private FakeImageService _fakeImageService;

        public FakeImageController(FakeImageService fakeImageService)
        {            
            _fakeImageService = fakeImageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(FakeImageRequest fakeImageRequest)
        {
            var result = await _fakeImageService.Upload(fakeImageRequest, User);

            if (result.Item1 != null)
            {
                return Ok(result.Item1);
            }

            if (result.Item1 == null && result.Item2 == null)
            {
                return BadRequest("Invalid images.");
            }

            return BadRequest(result.Item2);
        }
    }
}
