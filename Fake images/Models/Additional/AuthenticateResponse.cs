using System.Text.Json.Serialization;

namespace Fake_images.Models.Additional
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }

        public AuthenticateResponse(User user, string jwtToken)
        {
            Id = user.Id;
            Username = user.Name;
            JwtToken = jwtToken;
        }
    }
}
