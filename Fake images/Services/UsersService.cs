using Fake_images.Auth;
using Fake_images.Models;
using Fake_images.Models.Additional;
using Fake_images.Models.Context;

namespace Fake_images.Services
{
    public class UsersService
    {
        private readonly FakeImagesDbContext _context;
        private readonly JwtUtils _jwtUtils;

        public UsersService(
            FakeImagesDbContext context,
            JwtUtils jwtUtils)
        {
            _context = context;
            _jwtUtils = jwtUtils;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var check = _context.Users.FirstOrDefault(u => u.Name == model.Username && u.Password == model.Password);

            if (check == null)
            {
                throw new Exception("Username or password is incorrect");
            }

            var jwtToken = _jwtUtils.GenerateJwtToken(check);

            return new AuthenticateResponse(check, jwtToken);
        }

        public async Task<bool> AddUser(AuthenticateRequest model)
        {
            var newUser = new User { Name = model.Username, Email = model.Username + "@gmail.com", Password = model.Password };

            try
            {
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
