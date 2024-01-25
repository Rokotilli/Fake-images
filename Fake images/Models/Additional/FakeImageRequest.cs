namespace Fake_images.Models.Additional
{
    public class FakeImageRequest
    {
        public string Name { get; set; }
        public IFormFile Photo {  get; set; }
        public IFormFile BackGround { get; set; }
    }
}
