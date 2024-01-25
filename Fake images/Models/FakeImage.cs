namespace Fake_images.Models
{
    public class FakeImage
    {
        public int id { get; set; }
        public int author_id { get; set; }
        public string name { get; set; }
        public string original_photo_url { get; set; }
        public string original_back_url { get; set; }
        public DateTime upload_at { get; set; } = DateTime.Now;
        public string resize_photo_url { get; set; }
        public string resize_back_url { get; set; }
        public DateTime resized_at { get; set; }
        public string no_back_photo_url { get; set; }
        public DateTime remove_bg_at { get; set; }
        public string result_photo_url { get; set; }
        public DateTime finish_at { get; set; }

        public User AuthorId {  get; set; }
    }
}
