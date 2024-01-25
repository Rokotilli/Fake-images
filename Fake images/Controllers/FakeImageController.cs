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
        private readonly FakeImagesDbContext _context;
        private readonly IConfiguration _configuration;
        private FakeImageService _fakeImageService;

        public FakeImageController(FakeImagesDbContext fakeImagesDbContext, IConfiguration configuration, FakeImageService fakeImageService)
        {
            _context = fakeImagesDbContext;
            _configuration = configuration;
            _fakeImageService = fakeImageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(FakeImageRequest fakeImageRequest)
        {
            var result = await _fakeImageService.Upload(fakeImageRequest, User);

            if (result)
            {
                return Ok("Images uploaded successfully.");
            }

            return BadRequest("Invalid files.");
        }
    }
}
