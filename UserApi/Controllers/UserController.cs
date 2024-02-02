using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApi.Models.Additional;
using UserApi.Services;

namespace UserApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly FakeImagesDbContext dbContext;
        private UserService _userService;

        public UserController(UserService userService, FakeImagesDbContext dbContext)
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

            return Ok(auth);
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
