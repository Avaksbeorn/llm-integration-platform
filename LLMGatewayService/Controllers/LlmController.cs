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
			client.Timeout = TimeSpan.FromMinutes(5);

			var llmEndpoint = _config["LLM:Endpoint"]; // e.g. "http://llm:8000/v1/completions"
			var payload = new
			{
				// <-- поправлено: теперь точно соответствует id из /v1/models
				model = "/models/open-llama-7B-open-instruct.ggmlv3.q4_0.bin",
				prompt = prompt,
				max_tokens = 512,
				temperature = 0.7
			};

			var llmResponse = await client.PostAsJsonAsync(llmEndpoint, payload);
			var body = await llmResponse.Content.ReadAsStringAsync();
			if (!llmResponse.IsSuccessStatusCode)
			{
				Console.Error.WriteLine($"[LLM ERROR] {(int)llmResponse.StatusCode}: {body}");
				throw new InvalidOperationException($"LLM returned {(int)llmResponse.StatusCode}: {body}");
			}

			using var doc = JsonDocument.Parse(body);
			var root = doc.RootElement;
			responseText = root
				.GetProperty("choices")[0]
				.GetProperty("text")
				.GetString()?
				.Trim() ?? "";
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[LLM EXCEPTION]\n{ex}");
			return StatusCode(500, new
			{
				jsonrpc = "2.0",
				id = request.id,
				// возвращаем реальное сообщение ошибки для отладки
				error = new { code = -32603, message = ex.Message }
			});
		}

		// Публикация события в RabbitMQ
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
		await _publishEndpoint.Publish(new LlmRequestEvent
		{
			UserId = userId,
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
