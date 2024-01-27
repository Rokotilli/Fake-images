using Azure.Storage.Blobs;
using Fake_images.Models.Additional;
using Fake_images.Models;
using System.Security.Claims;
using Fake_images.Models.Context;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using SkiaSharp;

namespace Fake_images.Services
{
    public class FakeImageService
    {
        private readonly HttpClient _httpClient;
        private readonly FakeImagesDbContext _context;
        private readonly IConfiguration _configuration;
        private const int maxWidth = 800;
        private const int maxHeight = 600;

        public FakeImageService(FakeImagesDbContext fakeImagesDbContext, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _context = fakeImagesDbContext;
            _configuration = configuration;
        }

        public async Task<(FakeImage, Exception)> Upload(FakeImageRequest fakeImageRequest, ClaimsPrincipal User)
        {
            try
            {
                if (fakeImageRequest.Photo != null && fakeImageRequest.BackGround != null)
                {
                    var photoFileName = Path.GetFileName(fakeImageRequest.Photo.FileName);
                    var backFileName = Path.GetFileName(fakeImageRequest.BackGround.FileName);
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    var connectionString = _configuration["AzureStorageConnectionString"];
                    var containerName = _configuration["AzureStorageContainer"];

                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                    var photoBlobClient = blobContainerClient.GetBlobClient($"{userId}/{photoFileName}");
                    using (var photoStream = fakeImageRequest.Photo.OpenReadStream())
                    {
                        await photoBlobClient.UploadAsync(photoStream, true);
                    }

                    var backBlobClient = blobContainerClient.GetBlobClient($"{userId}/{backFileName}");
                    using (var backStream = fakeImageRequest.BackGround.OpenReadStream())
                    {
                        await backBlobClient.UploadAsync(backStream, true);
                    }

                    var uploadedAt = DateTime.Now;

                    var resizingResult = await Resize(fakeImageRequest, blobContainerClient, userId, photoFileName, backFileName);
                    var removeBackGroundResult = await RemoveBackGround(blobContainerClient, userId, photoFileName);
                    var overlayingResult = await OverlayImage(blobContainerClient, userId, backFileName, photoFileName);

                    if (removeBackGroundResult.removedBackPhotoBlobClient == null)
                    {
                        return (null, null);
                    }

                    var fakeImage = new FakeImage
                    {
                        name = fakeImageRequest.Name,
                        author_id = int.Parse(userId),
                        original_photo_url = photoBlobClient.Uri.ToString(),
                        original_back_url = backBlobClient.Uri.ToString(),
                        upload_at = uploadedAt,
                        resize_photo_url = resizingResult.resizedPhotoBlobClient.Uri.ToString(),
                        resize_back_url = resizingResult.resizedBackBlobClient.Uri.ToString(),
                        resized_at = resizingResult.resizedAt,
                        no_back_photo_url = removeBackGroundResult.removedBackPhotoBlobClient.Uri.ToString(),
                        remove_bg_at = removeBackGroundResult.removedBackAt,
                        result_photo_url = overlayingResult.resultImage.Uri.ToString(),
                        finish_at = overlayingResult.resultAt
                    };

                    _context.FakeImages.Add(fakeImage);
                    await _context.SaveChangesAsync();

                    return (fakeImage, null);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        public async Task<(BlobClient resizedPhotoBlobClient, BlobClient resizedBackBlobClient, DateTime resizedAt)> Resize(FakeImageRequest fakeImageRequest, BlobContainerClient blobContainerClient, string userId, string photoFileName, string backFileName)
        {
            BlobClient photoBlobClient = null;
            BlobClient backBlobClient = null;

            using (var memoryStreamForBack = new MemoryStream())
            {
                await fakeImageRequest.BackGround.CopyToAsync(memoryStreamForBack);
                using (var originalBack = SKBitmap.Decode(memoryStreamForBack.ToArray()))
                {
                    var ratioXBack = (double)maxWidth / originalBack.Width;
                    var ratioYBack = (double)maxHeight / originalBack.Height;
                    var ratioBack = Math.Min(ratioXBack, ratioYBack);
                    var newWidthBack = (int)(originalBack.Width * ratioBack);
                    var newHeightBack = (int)(originalBack.Height * ratioBack);

                    var resizedBack = originalBack.Resize(new SKImageInfo(newWidthBack, newHeightBack), SKFilterQuality.High);
                    backBlobClient = blobContainerClient.GetBlobClient($"{userId}/resizedImages/resized_{backFileName}");

                    using (var image = SKImage.FromBitmap(resizedBack))
                    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 90))
                    using (var stream = new MemoryStream(data.ToArray()))
                    {
                        await backBlobClient.UploadAsync(stream, true);
                    }

                    using (var memoryStreamForPhoto = new MemoryStream())
                    {
                        await fakeImageRequest.Photo.CopyToAsync(memoryStreamForPhoto);
                        using (var originalPhoto = SKBitmap.Decode(memoryStreamForPhoto.ToArray()))
                        {
                            photoBlobClient = blobContainerClient.GetBlobClient($"{userId}/resizedImages/resized_{photoFileName}");

                            var ratioXPhoto = (double)newWidthBack / originalPhoto.Width;
                            var ratioYPhoto = (double)newHeightBack / originalPhoto.Height;
                            var ratioPhoto = Math.Min(ratioXPhoto, ratioYPhoto);
                            var newWidthPhoto = (int)(originalPhoto.Width * ratioPhoto);
                            var newHeightPhoto = (int)(originalPhoto.Height * ratioPhoto);

                            var resizedPhoto = originalPhoto.Resize(new SKImageInfo(newWidthPhoto, newHeightPhoto), SKFilterQuality.High);

                            using (var image = SKImage.FromBitmap(resizedPhoto))
                            using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 90))
                            using (var stream = new MemoryStream(data.ToArray()))
                            {
                                await photoBlobClient.UploadAsync(stream, true);
                            }
                        }
                    }
                }
            }

            return (photoBlobClient, backBlobClient, DateTime.Now);
        }




        public async Task<(BlobClient removedBackPhotoBlobClient, DateTime removedBackAt)> RemoveBackGround(BlobContainerClient blobContainerClient, string userId, string photoFileName)
        {
            string apiUrl = $"{_configuration["AzureComputerVisionEndpoint"]}/computervision/imageanalysis:segment?api-version=2023-02-01-preview&mode=backgroundRemoval";
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configuration["Ocp-Apim-Subscription-Key"]);
            var photoBlobClient = blobContainerClient.GetBlobClient($"{userId}/resizedImages/resized_{photoFileName}");

            var contentObject = new
            {
                url = photoBlobClient.Uri.ToString()
            };

            var content = new StringContent(JsonConvert.SerializeObject(contentObject), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return (null, DateTime.Now);
            }

            photoBlobClient = blobContainerClient.GetBlobClient($"{userId}/removedBack/rb_{photoFileName}");

            var fileBytes = await response.Content.ReadAsByteArrayAsync();

            using (var stream = new MemoryStream(fileBytes))
            {
                await photoBlobClient.UploadAsync(stream, true);
            }

            return (photoBlobClient, DateTime.Now);
        }

        public async Task<(BlobClient resultImage, DateTime resultAt)> OverlayImage(BlobContainerClient blobContainerClient, string userId, string backFileName, string photoFileName)
        {
            var baseImagePath = blobContainerClient.GetBlobClient($"{userId}/resizedImages/resized_{backFileName}");
            var overlayImagePath = blobContainerClient.GetBlobClient($"{userId}/removedBack/rb_{photoFileName}");
            var outputPath = blobContainerClient.GetBlobClient($"{userId}/overlayedImage/ov_{photoFileName}");

            var resizedBackGround = await baseImagePath.DownloadAsync();
            var rbPhoto = await overlayImagePath.DownloadAsync();

            using (var streamForRbPhoto = new MemoryStream())
            using (var streamForBackGround = new MemoryStream())
            {
                await rbPhoto.Value.Content.CopyToAsync(streamForRbPhoto);
                await resizedBackGround.Value.Content.CopyToAsync(streamForBackGround);

                using (var overlayImage = SKBitmap.Decode(streamForRbPhoto.ToArray()))
                using (var baseImage = SKBitmap.Decode(streamForBackGround.ToArray()))
                {
                    using (var surface = SKSurface.Create(new SKImageInfo(baseImage.Width, baseImage.Height)))
                    {
                        var canvas = surface.Canvas;
                        canvas.DrawBitmap(baseImage, new SKPoint(0, 0));

                        var overlayX = (baseImage.Width - overlayImage.Width) / 2;
                        var overlayY = (baseImage.Height - overlayImage.Height) / 2;

                        canvas.DrawBitmap(overlayImage, new SKPoint(overlayX, overlayY));

                        using (var finalImage = surface.Snapshot())
                        using (var data = finalImage.Encode(SKEncodedImageFormat.Jpeg, 90))
                        using (var stream = new MemoryStream(data.ToArray()))
                        {
                            await outputPath.UploadAsync(stream, true);
                        }
                    }
                }
            }


            return (outputPath, DateTime.Now);
        }
    }
}
