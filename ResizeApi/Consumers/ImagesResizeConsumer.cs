using Azure.Storage.Blobs;
using Domain;
using MassTransit;
using MessageBus.Messages;
using SkiaSharp;

namespace ResizeApi.Consumers
{
    public class ImagesResizeConsumer : IConsumer<ImagesResizeEvent>
    {
        private readonly FakeImagesDbContext _context;
        private readonly IRequestClient<ImagesRemoveBackEvent> _requestClient;
        private const int maxWidth = 800;
        private const int maxHeight = 600;

        public ImagesResizeConsumer(FakeImagesDbContext fakeImagesDbContext, IRequestClient<ImagesRemoveBackEvent> requestClient)
        {
            _context = fakeImagesDbContext;
            _requestClient = requestClient;
        }

        public async Task Consume(ConsumeContext<ImagesResizeEvent> consumeContext)
        {
            BlobClient photoBlobClient = null;
            BlobClient backBlobClient = null;

            var blobServiceClient = new BlobServiceClient(consumeContext.Message.BlobConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(consumeContext.Message.BlobContainerName);

            var photoFileName = consumeContext.Message.PhotoFileName;
            var backFileName = consumeContext.Message.BackGroundFileName;
            var userId = consumeContext.Message.UserId;
            var fakeImage = consumeContext.Message.fakeImage;

            using (var memoryStreamForBack = new MemoryStream(consumeContext.Message.BackGroundContent))
            {
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

                    using (var memoryStreamForPhoto = new MemoryStream(consumeContext.Message.PhotoContent))
                    {
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

            ImagesRemoveBackEvent imagesRemoveBackEvent = new ImagesRemoveBackEvent()
            {
                UserId = userId,
                PhotoFileName = consumeContext.Message.PhotoFileName,
                BackGroundFileName = consumeContext.Message.BackGroundFileName,
                fakeImage = fakeImage,
                BlobConnectionString = consumeContext.Message.BlobConnectionString,
                BlobContainerName = consumeContext.Message.BlobContainerName
            };

            try
            {
                var request = _requestClient.Create(imagesRemoveBackEvent);
                var response = await request.GetResponse<ImagesRemoveBackEvent>();
                var result = response.Message.fakeImage;

                if (!response.Message.IsSuccess)
                {
                    await consumeContext.RespondAsync(new ImagesResizeEvent
                    {
                        IsSuccess = false,
                        Exception = response.Message.Exception
                    });
                    return;
                }

                await consumeContext.RespondAsync(new ImagesResizeEvent
                {
                    fakeImage = result
                });
            }
            catch
            {
                await consumeContext.RespondAsync(new ImagesResizeEvent
                {
                    IsSuccess = false,
                    Exception = "RemoveBackApi is not accessible"
                });
            }
        }
    }
}
