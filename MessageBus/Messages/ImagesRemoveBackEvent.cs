using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus.Messages
{
    public class ImagesRemoveBackEvent : IntegrationBaseEvent
    {
        public int UserId { get; set; }
        public string PhotoFileName { get; set; }
        public string BackGroundFileName { get; set; }
        public FakeImage fakeImage { get; set; }
        public string BlobConnectionString { get; set; }
        public string BlobContainerName { get; set; }
    }
}
