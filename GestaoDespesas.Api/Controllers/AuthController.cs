using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GestaoDespesas.Api.DTOs;
using GestaoDespesas.Api.Services;

namespace GestaoDespesas.Api.Controllers;

// Handles user registration and authentication.
// Route: /api/auth/...
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    // Dependencies are injected by the built-in DI container.
    // UserManager is provided by ASP.NET Core Identity and handles
    // user creation, password hashing and validation.
    public AuthController(
        UserManager<IdentityUser> userManager,
        TokenService tokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    // POST /api/auth/register
    // Creates a new user account
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        // Prevent duplicate accounts for the same email
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return BadRequest("A user with this email already exists.");
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        // Identity hashes the password before storing it
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            // Return validation errors (e.g. password too weak)
            return BadRequest(result.Errors);
        }

        return Ok("User registered successfully.");
    }

    // POST /api/auth/login
    // Validates credentials and returns a JWT on success
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Use a generic message so we don't reveal whether the email exists
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = _tokenService.GenerateToken(user);
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"]!);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(expirationMinutes)
        });
    }
    // GET /api/auth/me
// A protected test endpoint: only accessible with a valid JWT.
// Returns the authenticated user's id and email, extracted from the token claims.
[HttpGet("me")]
[Microsoft.AspNetCore.Authorization.Authorize]
public IActionResult Me()
{
    var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
    var email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;

    return Ok(new { UserId = userId, Email = email });
}
}