using Azure.Storage.Blobs;
using Domain;
using MassTransit;
using MessageBus.Messages;
using SkiaSharp;

namespace OverlayApi.Consumers
{
    public class ImagesOverlayConsumer : IConsumer<ImagesOverlayEvent>
    {
        private readonly FakeImagesDbContext _context;

        public ImagesOverlayConsumer(FakeImagesDbContext fakeImagesDbContext)
        {
            _context = fakeImagesDbContext;
        }

        public async Task Consume(ConsumeContext<ImagesOverlayEvent> consumeContext)
        {
            var photoFileName = consumeContext.Message.PhotoFileName;
            var backFileName = consumeContext.Message.BackGroundFileName;
            var userId = consumeContext.Message.UserId;
            var fakeImage = consumeContext.Message.fakeImage;

            var blobServiceClient = new BlobServiceClient(consumeContext.Message.BlobConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(consumeContext.Message.BlobContainerName);

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

            await consumeContext.RespondAsync(new ImagesOverlayEvent
            {
                fakeImage = fakeImage
            });
        }
    }
}
