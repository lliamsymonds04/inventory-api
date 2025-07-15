using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
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
    private readonly AuthService _authService;

    public AuthController(AppDbContext context, IConfiguration configuration, AuthService authService)
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

        // var token = GenerateJwtToken(request.Username, user.Role);
        var token = _authService.GenerateJwtToken(request.Username, user.Role);
        return Ok(new
        {
            Token = token,
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
            Role = "warehouse"
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // var token = GenerateJwtToken(newUser.Username, newUser.Role);
        var token = _authService.GenerateJwtToken(newUser.Username, newUser.Role);
        return Ok(new
        {
            Token = token,
            role = newUser.Role,
        });
    }
}