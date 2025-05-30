using System.Text.Json;

namespace LLMGatewayService.Models;

public class RpcRequest
{
	public string jsonrpc { get; set; }
	public string method { get; set; }
	public JsonElement @params { get; set; }
	public string id { get; set; }
}
