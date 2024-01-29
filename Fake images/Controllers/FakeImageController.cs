using Fake_images.Models.Additional;
using Fake_images.Services;
using Fake_images.Services.FakeImageServices;
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
        private readonly UploadService _uploadService;
        private readonly ResizeService _resizeService;
        private readonly RemoveBackService _removeBackService;
        private readonly OverlayService _overlayService;

        public FakeImageController(UploadService uploadService, ResizeService resizeService, RemoveBackService removeBackService, OverlayService overlayService)
        {
            _uploadService = uploadService;
            _resizeService = resizeService;
            _removeBackService = removeBackService;
            _overlayService = overlayService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(FakeImageRequest fakeImageRequest)
        {
            if (fakeImageRequest.Photo == null || fakeImageRequest.BackGround == null)
            {
                return BadRequest("Invalid images.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var resultUpload = await _uploadService.Upload(fakeImageRequest, User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!resultUpload.IsSuccess)
            {
                return BadRequest(resultUpload.Error);
            }

            var resultResize = await _resizeService.Resize(fakeImageRequest, resultUpload.BlobContainerClient, resultUpload.FakeImage, userId);
            var resultRemoveBack = await _removeBackService.RemoveBackGround(resultUpload.BlobContainerClient, resultResize, userId, Path.GetFileName(fakeImageRequest.Photo.FileName));
            var resultOverlay = await _overlayService.OverlayImage(fakeImageRequest, resultUpload.BlobContainerClient, resultRemoveBack, userId);

            return Ok(resultOverlay);
        }
    }
}
