using Azure.Storage.Blobs;
using Fake_images.Models;
using Fake_images.Models.Additional;
using Fake_images.Models.Context;
using SkiaSharp;

namespace Fake_images.Services.FakeImageServices
{
    public class ResizeService
    {
        private readonly FakeImagesDbContext _context;
        private const int maxWidth = 800;
        private const int maxHeight = 600;

        public ResizeService(FakeImagesDbContext context)
        {
            _context = context;
        }

        public async Task<FakeImage> Resize(FakeImageRequest fakeImageRequest, BlobContainerClient blobContainerClient, FakeImage fakeImage, string userId)
        {
            BlobClient photoBlobClient = null;
            BlobClient backBlobClient = null;
            var photoFileName = Path.GetFileName(fakeImageRequest.Photo.FileName);
            var backFileName = Path.GetFileName(fakeImageRequest.BackGround.FileName);

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

            fakeImage.resize_photo_url = photoBlobClient.Uri.ToString();
            fakeImage.resize_back_url = backBlobClient.Uri.ToString();
            fakeImage.resized_at = DateTime.Now;

            _context.FakeImages.Update(fakeImage);
            await _context.SaveChangesAsync();

            return fakeImage;
        }
    }
}
