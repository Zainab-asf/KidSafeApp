using KidSafeApp.Backend.Services;
using KidSafeApp.Backend.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace KidSafeApp.Backend.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto, CancellationToken cancellationToken)
        {
            return BadRequest("Self-registration is disabled. Contact an administrator to create your account.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _userService.LoginAsync(dto, cancellationToken);
                return Ok(response);
            }
            catch (ServiceException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }
    }
}
