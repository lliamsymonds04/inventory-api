public class JwtFromCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public JwtFromCookieMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _config = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get the cookie name
        var jwtCookieName = _config["JwtSettings:AuthTokenName"] ?? "jwt";

        // Check if the JWT cookie exists
        if (context.Request.Cookies.TryGetValue(jwtCookieName, out var jwt))
        {
            // Set the JWT in the Authorization header
            context.Request.Headers["Authorization"] = $"Bearer {jwt}";
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}