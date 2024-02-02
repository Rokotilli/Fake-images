using Azure.Storage.Blobs;
using Domain;
using MassTransit;
using MessageBus.Messages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace RemoveBackApi.Consumers
{
    public class ImagesRemoveBackConsumer : IConsumer<ImagesRemoveBackEvent>
    {
        private readonly HttpClient _httpClient;
        private readonly FakeImagesDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IRequestClient<ImagesOverlayEvent> _requestClient;

        public ImagesRemoveBackConsumer(FakeImagesDbContext fakeImagesDbContext, IConfiguration configuration, IHttpClientFactory httpClientFactory, IRequestClient<ImagesOverlayEvent> requestClient)
        {
            _httpClient = httpClientFactory.CreateClient();
            _context = fakeImagesDbContext;
            _configuration = configuration;
            _requestClient = requestClient;
        }

        public async Task Consume(ConsumeContext<ImagesRemoveBackEvent> consumeContext)
        {
            string apiUrl = $"{_configuration["AzureComputerVisionEndpoint"]}/computervision/imageanalysis:segment?api-version=2023-02-01-preview&mode=backgroundRemoval";
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configuration["Ocp-Apim-Subscription-Key"]);

            var blobServiceClient = new BlobServiceClient(consumeContext.Message.BlobConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(consumeContext.Message.BlobContainerName);
            var userId = consumeContext.Message.UserId;
            var photoFileName = consumeContext.Message.PhotoFileName;
            var fakeImage = consumeContext.Message.fakeImage;

            var photoBlobClient = blobContainerClient.GetBlobClient($"{userId}/resizedImages/resized_{photoFileName}");

            var contentObject = new
            {
                url = photoBlobClient.Uri.ToString()
            };

            var content = new StringContent(JsonConvert.SerializeObject(contentObject), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            HttpResponseMessage responseHttp = await _httpClient.PostAsync(apiUrl, content);

            if (!responseHttp.IsSuccessStatusCode)
            {
                await consumeContext.RespondAsync(new ImagesRemoveBackEvent
                {
                    IsSuccess = false,
                    Exception = "Something went wrong in \"Remove background api\""
                });
                return;
            }

            photoBlobClient = blobContainerClient.GetBlobClient($"{userId}/removedBack/rb_{photoFileName}");

            var fileBytes = await responseHttp.Content.ReadAsByteArrayAsync();

            using (var stream = new MemoryStream(fileBytes))
            {
                await photoBlobClient.UploadAsync(stream, true);
            }

            fakeImage.no_back_photo_url = photoBlobClient.Uri.ToString();
            fakeImage.remove_bg_at = DateTime.Now;

            _context.FakeImages.Update(fakeImage);
            await _context.SaveChangesAsync();

            ImagesOverlayEvent imagesOverlayEvent = new ImagesOverlayEvent()
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
                var request = _requestClient.Create(imagesOverlayEvent);
                var response = await request.GetResponse<ImagesOverlayEvent>();
                var result = response.Message.fakeImage;

                await consumeContext.RespondAsync(new ImagesRemoveBackEvent
                {
                    fakeImage = result
                });
            }
            catch
            {
                await consumeContext.RespondAsync(new ImagesRemoveBackEvent
                {
                    IsSuccess = false,
                    Exception = "OverlayApi is not accessible"
                });
            }            
        }
    }
}
