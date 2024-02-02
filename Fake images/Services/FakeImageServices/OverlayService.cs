using Azure.Storage.Blobs;
using Domain;
using Domain.Models;
using Fake_images.Models.Additional;
using SkiaSharp;

namespace Fake_images.Services.FakeImageServices
{
    public class OverlayService
    {
        private readonly FakeImagesDbContext _context;

        public OverlayService(FakeImagesDbContext fakeImagesDbContext)
        {
            _context = fakeImagesDbContext;
        }

        public async Task<FakeImage> OverlayImage(FakeImageRequest fakeImageRequest, BlobContainerClient blobContainerClient, FakeImage fakeImage, string userId)
        {
            var photoFileName = Path.GetFileName(fakeImageRequest.Photo.FileName);
            var backFileName = Path.GetFileName(fakeImageRequest.BackGround.FileName);
            var backBlobContainer = blobContainerClient.GetBlobClient($"{userId}/resizedImages/resized_{backFileName}");
            var photoBlobContainer = blobContainerClient.GetBlobClient($"{userId}/removedBack/rb_{photoFileName}");
            var overlayedImageBlobContainer = blobContainerClient.GetBlobClient($"{userId}/overlayedImage/ov_{photoFileName}");

            var resizedBackGround = await backBlobContainer.DownloadAsync();
            var rbPhoto = await photoBlobContainer.DownloadAsync();

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
                            await overlayedImageBlobContainer.UploadAsync(stream, true);
                        }
                    }
                }
            }

            fakeImage.result_photo_url = overlayedImageBlobContainer.Uri.ToString();
            fakeImage.finish_at = DateTime.Now;

            _context.FakeImages.Update(fakeImage);
            await _context.SaveChangesAsync();

            return fakeImage;
        }
    }
}
