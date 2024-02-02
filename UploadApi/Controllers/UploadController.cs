using Domain.Models;
using MassTransit;
using MessageBus.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UploadApi.Models.Additional;
using UploadApi.Services;

namespace UploadApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly UploadService _uploadService;
        //private readonly ResizeService _resizeService;
        //private readonly RemoveBackService _removeBackService;
        //private readonly OverlayService _overlayService;
        private readonly IRequestClient<ImagesResizeEvent> _requestClient;


        public UploadController(UploadService uploadService, //ResizeService resizeService, RemoveBackService removeBackService, OverlayService overlayService,
            IRequestClient<ImagesResizeEvent> requestClient)
        {
            _requestClient = requestClient;
            _uploadService = uploadService;
            //_resizeService = resizeService;
            //_removeBackService = removeBackService;
            //_overlayService = overlayService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(FakeImageRequest fakeImageRequest)
        {
            if (fakeImageRequest.Photo == null || fakeImageRequest.BackGround == null)
            {
                return BadRequest("Invalid images.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var resultUpload = await _uploadService.Upload(fakeImageRequest, userId);

            ImagesResizeEvent imagesResizeEvent = new ImagesResizeEvent()
            {
                UserId = int.Parse(userId),
                Name = fakeImageRequest.Name,
                PhotoFileName = Path.GetFileName(fakeImageRequest.Photo.FileName),
                BackGroundFileName = Path.GetFileName(fakeImageRequest.BackGround.FileName),
                fakeImage = resultUpload.fakeImage,
                BlobConnectionString = resultUpload.BlobConnectionString,
                BlobContainerName = resultUpload.BlobContainerName
            };

            using (var memoryStream = new MemoryStream())
            {
                fakeImageRequest.Photo.CopyTo(memoryStream);
                imagesResizeEvent.PhotoContent = memoryStream.ToArray();
            }
            using (var memoryStream = new MemoryStream())
            {
                fakeImageRequest.BackGround.CopyTo(memoryStream);
                imagesResizeEvent.BackGroundContent = memoryStream.ToArray();
            }

            try
            {
                var request = _requestClient.Create(imagesResizeEvent);
                var response = await request.GetResponse<ImagesResizeEvent>();

                if (!response.Message.IsSuccess)
                {
                    return BadRequest(response.Message.Exception);
                }

                var result = response.Message.fakeImage;

                return Ok(result);
            }
            catch
            {
                return BadRequest("ResizeApi is not accessible");
            }            
        }
    }
}
