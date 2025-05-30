using Common.Contracts;
using LoggingService.Data;
using MassTransit;

namespace LoggingService.Consumers;

public class LlmRequestEventConsumer : IConsumer<LlmRequestEvent>
{
	private readonly LogsDbContext _db;

	public LlmRequestEventConsumer(LogsDbContext db)
	{
		_db = db;
	}

	public async Task Consume(ConsumeContext<LlmRequestEvent> context)
	{
		var message = context.Message;

		var logEntry = new LlmLogEntry
		{
			UserId = message.UserId,
			Prompt = message.Prompt,
			Response = message.Response,
			Timestamp = message.Timestamp
		};

		_db.LlmLogs.Add(logEntry);
		await _db.SaveChangesAsync();

		//Console.WriteLine($"[Log Saved] User: {message.UserId}, Prompt: {message.Prompt}");
	}
}
