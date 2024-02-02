using Azure.Storage.Blobs;
using Domain;
using Domain.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Fake_images.Services.FakeImageServices
{
    public class RemoveBackService
    {
        private readonly HttpClient _httpClient;
        private readonly FakeImagesDbContext _context;
        private readonly IConfiguration _configuration;

        public RemoveBackService(FakeImagesDbContext fakeImagesDbContext, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _context = fakeImagesDbContext;
            _configuration = configuration;
        }

        public async Task<FakeImage> RemoveBackGround(BlobContainerClient blobContainerClient, FakeImage fakeImage, string userId, string photoFileName)
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
                return null;
            }

            photoBlobClient = blobContainerClient.GetBlobClient($"{userId}/removedBack/rb_{photoFileName}");

            var fileBytes = await response.Content.ReadAsByteArrayAsync();

            using (var stream = new MemoryStream(fileBytes))
            {
                await photoBlobClient.UploadAsync(stream, true);
            }

            fakeImage.no_back_photo_url = photoBlobClient.Uri.ToString();
            fakeImage.remove_bg_at = DateTime.Now;

            _context.FakeImages.Update(fakeImage);
            await _context.SaveChangesAsync();

            return fakeImage;
        }
    }
}
