using Azure.Storage.Blobs;
using Fake_images.Models.Additional;
using Fake_images.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Fake_images.Models.Context;

namespace Fake_images.Services
{
    public class FakeImageService
    {
        private readonly FakeImagesDbContext _context;
        private readonly IConfiguration _configuration;

        public FakeImageService(FakeImagesDbContext fakeImagesDbContext, IConfiguration configuration)
        {
            _context = fakeImagesDbContext;
            _configuration = configuration;
        }

        public async Task<bool> Upload(FakeImageRequest fakeImageRequest, ClaimsPrincipal User)
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

                    var fakePhoto = new FakeImage
                    {
                        name = fakeImageRequest.Name,
                        author_id = int.Parse(userId),
                        original_photo_url = photoBlobClient.Uri.ToString(),
                        original_back_url = backBlobClient.Uri.ToString()
                    };

                    _context.FakeImages.Add(fakePhoto);
                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
