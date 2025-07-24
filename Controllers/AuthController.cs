using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;
using InventoryAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private static readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();
    private readonly IAuthService _authService;

    public AuthController(AppDbContext context, IConfiguration configuration, IAuthService authService)
    {
        _configuration = configuration;
        _context = context;
        _authService = authService;
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        // hash the incoming password and compare it with the stored hash
        var result = _passwordHasher.VerifyHashedPassword(request.Username, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid username or password.");
        }

        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = HandleToken(user);

        return Ok(new
        {
            role = user.Role,
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (existingUser != null)
        {
            return BadRequest("Username already exists.");
        }

        // hash the password before saving
        var passwordHash = _passwordHasher.HashPassword(request.Username, request.Password);

        var newUser = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            Role = "Warehouse"
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var token = HandleToken(newUser);

        return Ok(new
        {
            role = newUser.Role,
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear the JWT cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1) // Set to a past date to clear the cookie
        };

        var cookieName = _configuration["JwtSettings:AuthTokenName"] ?? "auth_token";
        Response.Cookies.Append(cookieName, "", cookieOptions);

        return Ok("Logged out successfully.");
    }

    [HttpGet("check")]
    public IActionResult Check()
    {
        var cookieName = _configuration["JwtSettings:AuthTokenName"] ?? "auth_token";
        if (Request.Cookies.TryGetValue(cookieName, out var token))
        {
            var principal = _authService.ValidateJwtToken(token);
            if (principal != null)
            {
                return Ok("User is authenticated.");
            }
        }
        return Unauthorized("User is not authenticated.");
    }

    private string HandleToken(User user)
    {
        var token = _authService.GenerateJwtToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpiryTime"))
        };

        var cookieName = _configuration["JwtSettings:AuthTokenName"] ?? "auth_token";
        Response.Cookies.Append(cookieName, token, cookieOptions);

        return token;
    }
}