using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly IConfiguration _config;

	public AuthController(UserManager<IdentityUser> userManager, IConfiguration config)
	{
		_userManager = userManager;
		_config = config;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		var user = new IdentityUser { UserName = request.Username, Email = request.Email };
		var result = await _userManager.CreateAsync(user, request.Password);

		if (!result.Succeeded)
			return BadRequest(result.Errors);

		await _userManager.AddToRoleAsync(user, "User");
		return Ok(new { message = "Registered successfully" });
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		var user = await _userManager.FindByNameAsync(request.Username);
		if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
			return Unauthorized();

		// Берём настройки
		var jwtSection = _config.GetSection("JWT");
		var secretBase64 = jwtSection["Secret"]!;
		var keyBytes = Convert.FromBase64String(secretBase64);
		var key = new SymmetricSecurityKey(keyBytes);
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		// Собираем claims
		var claims = new Dictionary<string, object>
		{
			[ClaimTypes.NameIdentifier] = user.Id,
			[ClaimTypes.Name] = user.UserName!,
			[ClaimTypes.Role] = "User"
		};

		// Создаём дескриптор
		var descriptor = new SecurityTokenDescriptor
		{
			Claims = claims,
			NotBefore = DateTime.UtcNow,
			Expires = DateTime.UtcNow.AddHours(1),
			Issuer = jwtSection["Issuer"],
			Audience = jwtSection["Audience"],
			SigningCredentials = creds
		};

		// Генерируем Compact JWT с помощью JsonWebTokenHandler
		var handler = new JsonWebTokenHandler
		{
			SetDefaultTimesOnTokenCreation = false
		};
		var tokenString = handler.CreateToken(descriptor);

		return Ok(new { token = tokenString });
	}
}
