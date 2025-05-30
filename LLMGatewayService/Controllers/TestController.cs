using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LLMGatewayService.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
	[HttpGet("whoami")]
	//[Authorize]
	public IActionResult WhoAmI()
	{
		return Ok(new
		{
			userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
			name = User.Identity?.Name,
			roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value)
		});
	}
}
