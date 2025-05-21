using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
		return Ok("Registered successfully");
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		var user = await _userManager.FindByNameAsync(request.Username);
		if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
			return Unauthorized();

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Name, user.UserName),
			new Claim(ClaimTypes.Role, "User")
		};

		var token = new JwtSecurityToken(
			issuer: _config["JWT:Issuer"],
			audience: _config["JWT:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddHours(1),
			signingCredentials: new SigningCredentials(
				new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"])),
				SecurityAlgorithms.HmacSha256)
			);

		return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
	}
}
