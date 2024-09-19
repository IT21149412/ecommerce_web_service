using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;

    public UserController(UserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    // GET: api/user
    [HttpGet]
    [Authorize(Roles = "Administrator")] // Only Admins can view all users
    public async Task<ActionResult> GetUsers()
    {
        var users = await _userService.GetUsersAsync();
        return Ok(users);
    }

    // POST: api/user/register
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] User user)
    {
        var existingUser = await _userService.GetUserByEmailAsync(user.Email);
        if (existingUser != null)
        {
            return BadRequest("User with the provided email already exists.");
        }

        await _userService.CreateUserAsync(user);
        return Ok("User registered successfully");
    }

    // POST: api/user/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userService.AuthenticateAsync(model.Email, model.Password);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = _tokenService.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    // GET: api/user/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator,CSR")] // Admin and CSR can view users
    public async Task<ActionResult<User>> GetUserById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
}
