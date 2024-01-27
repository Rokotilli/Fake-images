using Fake_images.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fake_images.Auth
{
    public class JwtUtils
    {
        private readonly IConfiguration _appSettings;

        public JwtUtils(
            IConfiguration appSettings)
        {
            _appSettings = appSettings;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings["JwtSecurityKey"]);

            var jwt = new JwtSecurityToken(
                issuer: _appSettings["JwtIssuer"],
                audience: _appSettings["JwtAudience"],
                claims: new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) },
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature));

            return tokenHandler.WriteToken(jwt);
        }
    }
}
