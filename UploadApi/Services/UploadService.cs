using Azure.Storage.Blobs;
using Domain;
using Domain.Models;
using MessageBus.Messages;
using UploadApi.Models.Additional;

namespace UploadApi.Services
{
    public class UploadService
    {
        private readonly FakeImagesDbContext _context;
        private readonly IConfiguration _configuration;

        public UploadService(FakeImagesDbContext fakeImagesDbContext, IConfiguration configuration)
        {
            _context = fakeImagesDbContext;
            _configuration = configuration;
        }

        public async Task<ImagesResizeEvent> Upload(FakeImageRequest fakeImageRequest, string userId)
        {
            try
            {
                var photoFileName = Path.GetFileName(fakeImageRequest.Photo.FileName);
                var backFileName = Path.GetFileName(fakeImageRequest.BackGround.FileName);

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

                var fakeImage = new FakeImage
                {
                    name = fakeImageRequest.Name,
                    author_id = int.Parse(userId),
                    original_photo_url = photoBlobClient.Uri.ToString(),
                    original_back_url = backBlobClient.Uri.ToString(),
                    upload_at = DateTime.Now,
                };

                _context.FakeImages.Add(fakeImage);
                await _context.SaveChangesAsync();

                return new ImagesResizeEvent() { fakeImage = fakeImage, BlobContainerName = containerName, BlobConnectionString = connectionString, Exception = null };
            }

            catch (Exception ex)
            {
                return new ImagesResizeEvent { fakeImage = null, Exception = ex.Message };
            }
        }
    }
}
