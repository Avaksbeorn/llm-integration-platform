using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Common.Contracts;
using LLMGatewayService.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLMGatewayService.Controllers;

[ApiController]
[Route("llm")]
[Authorize]
public class LlmController : ControllerBase
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IPublishEndpoint _publishEndpoint;
	private readonly IConfiguration _config;

	public LlmController(IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint, IConfiguration config)
	{
		_httpClientFactory = httpClientFactory;
		_publishEndpoint = publishEndpoint;
		_config = config;
	}

	[HttpPost]
	public async Task<IActionResult> PostRpcRequest([FromBody] RpcRequest request)
	{
		if (request.jsonrpc != "2.0" || request.method != "GenerateText")
			return BadRequest(new
			{
				jsonrpc = "2.0",
				id = request.id,
				error = new { code = -32601, message = "Method not found" }
			});

		string prompt = request.@params.GetProperty("prompt").GetString() ?? "";

		string responseText;

		try
		{
			var client = _httpClientFactory.CreateClient();
			var llmUrl = _config["LLM:Endpoint"];
			var llmResponse = await client.PostAsJsonAsync(llmUrl, new { prompt });
			llmResponse.EnsureSuccessStatusCode();

			var resultJson = await llmResponse.Content.ReadFromJsonAsync<JsonElement>();
			responseText = resultJson.GetProperty("result").GetString() ?? "";
		}
		catch
		{
			return StatusCode(500, new
			{
				jsonrpc = "2.0",
				id = request.id,
				error = new { code = -32603, message = "Internal error calling LLM" }
			});
		}

		// Публикация события в RabbitMQ
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		await _publishEndpoint.Publish(new LlmRequestEvent
		{
			UserId = userId!,
			Prompt = prompt,
			Response = responseText,
			Timestamp = DateTime.UtcNow
		});

		return Ok(new
		{
			jsonrpc = "2.0",
			id = request.id,
			result = new { text = responseText }
		});
	}
}
