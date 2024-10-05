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
    // [Authorize(Roles = "Administrator")] // Only Admin can register new users
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
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userService.AuthenticateAsync(model.Email, model.Password);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = _tokenService.GenerateJwtToken(user);
        return Ok(new { token, role = user.Role });
    }

    // GET: api/user/{id}
    [HttpGet("{id}")]
    // [Authorize(Roles = "Administrator,CSR")] 
    public async Task<ActionResult<User>> GetUserById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    // PUT: api/User/{id}/update
    [HttpPut("{id}/update")]
    [Authorize(Roles = "Administrator,CSR")]  // Only Administrator and csr can update user details
    public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Explicitly set the isActive field
        user.IsActive = updatedUser.IsActive;

        await _userService.UpdateUserAsync(id, updatedUser);
        return Ok("User updated successfully.");
    }

    // PUT: api/user/{id}/deactivate
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Administrator,CSR")] // Admin and CSR can deactivate user accounts
    public async Task<ActionResult> DeactivateUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        user.IsActive = false;
        await _userService.UpdateUserAsync(id, user);
        return Ok("User deactivated successfully.");
    }

    // PUT: api/user/{id}/activate
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Administrator,CSR")] // Admin and CSR can activate user accounts
    public async Task<ActionResult> ActivateUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        user.IsActive = true;
        await _userService.UpdateUserAsync(id, user);
        return Ok("User activated successfully.");
    }

    // DELETE: api/user/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")] // Only Admin can delete users
    public async Task<ActionResult> DeleteUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        await _userService.DeleteUserAsync(id);
        return Ok("User deleted successfully");
    }

    //vendor
    // POST: api/user/register-vendor
    [HttpPost("register-vendor")]
    [Authorize(Roles = "Administrator")] // Only Admin can register vendors
    public async Task<ActionResult> RegisterVendor([FromBody] User user)
    {
        var existingUser = await _userService.GetUserByEmailAsync(user.Email);
        if (existingUser != null)
        {
            return BadRequest("User with the provided email already exists.");
        }

        // Assign the role of Vendor
        user.Role = "Vendor";
        await _userService.CreateUserAsync(user);
        return Ok("Vendor registered successfully");
    }

}
