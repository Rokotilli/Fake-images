using Fake_images.Models.Additional;
using Fake_images.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fake_images.Models.Context;

namespace Fake_images.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly FakeImagesDbContext dbContext;
        private UsersService _userService;

        public UsersController(UsersService userService, FakeImagesDbContext dbContext)
        {
            this.dbContext = dbContext;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = dbContext.Users;
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = dbContext.Users.FirstOrDefault(user => user.Id == id);
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var response = await _userService.Authenticate(model);

            setTokenCookie(response.JwtToken);

            return Ok(response);
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Add(AuthenticateRequest model)
        {
            var response = await _userService.AddUser(model);

            if (!response)
                return BadRequest();

            var auth = await _userService.Authenticate(model);

            setTokenCookie(auth.JwtToken);

            return Ok();            
        }

        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("accessToken", token, cookieOptions);
        }
    }
}
