using Microsoft.AspNetCore.Mvc;
using InventoryAPI.Data;


namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}/username")]
    public async Task<IActionResult> GetUsername(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }
        return Ok(new { Username = user.Username });
    }
}