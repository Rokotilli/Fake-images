using Azure.Storage.Blobs;
using Fake_images.Models.Additional;
using Domain.Models;
using Domain;

namespace Fake_images.Services.FakeImageServices
{
    public class UploadResult
    {
        public FakeImage FakeImage { get; set; }
        public BlobContainerClient BlobContainerClient { get; set; }
        public string Error { get; set; }
        public bool IsSuccess => FakeImage != null;
    }

    public class UploadService
    {
        private readonly FakeImagesDbContext _context;
        private readonly IConfiguration _configuration;

        public UploadService(FakeImagesDbContext fakeImagesDbContext, IConfiguration configuration)
        {
            _context = fakeImagesDbContext;
            _configuration = configuration;
        }

        public async Task<UploadResult> Upload(FakeImageRequest fakeImageRequest, string userId)
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

                return new UploadResult() { FakeImage = fakeImage, BlobContainerClient = blobContainerClient, Error = null };
            }

            catch (Exception ex)
            {
                return new UploadResult { FakeImage = null, BlobContainerClient = null, Error = ex.Message };
            }
        }
    }
}
