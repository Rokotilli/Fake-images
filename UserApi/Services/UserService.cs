using Domain;
using Domain.Models;
using UserApi.Models.Additional;

namespace UserApi.Services
{
    public class UserService
    {
        private readonly FakeImagesDbContext _context;
        private readonly JwtService _jwtService;

        public UserService(
            FakeImagesDbContext context,
            JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var check = _context.Users.FirstOrDefault(u => u.Name == model.Username && u.Password == model.Password);

            if (check == null)
            {
                throw new Exception("Username or password is incorrect");
            }

            var jwtToken = _jwtService.GenerateJwtToken(check);

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
