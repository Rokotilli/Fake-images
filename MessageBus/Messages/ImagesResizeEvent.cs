using Domain.Models;

namespace MessageBus.Messages
{
    public class ImagesResizeEvent : IntegrationBaseEvent
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public byte[] PhotoContent { get; set; }
        public string PhotoFileName { get; set; }
        public byte[] BackGroundContent { get; set; }
        public string BackGroundFileName { get; set; }
        public FakeImage fakeImage { get; set; }
        public string BlobConnectionString { get; set; }
        public string BlobContainerName { get; set; }
    }
}
