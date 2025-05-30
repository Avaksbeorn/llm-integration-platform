namespace Common.Contracts;

public class LlmRequestEvent
{
	public string UserId { get; set; }
	public string Prompt { get; set; }
	public string Response { get; set; }
	public DateTime Timestamp { get; set; }
}
