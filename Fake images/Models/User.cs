namespace Fake_images.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime email_verified_at { get; set; }
        public string Password { get; set; }

        public List<FakeImage> FakeImages { get; set; }
    }
}
