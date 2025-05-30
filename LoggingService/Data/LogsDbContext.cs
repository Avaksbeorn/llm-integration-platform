using Microsoft.EntityFrameworkCore;

namespace LoggingService.Data;

public class LogsDbContext : DbContext
{
	public LogsDbContext(DbContextOptions<LogsDbContext> options) : base(options) { }

	public DbSet<LlmLogEntry> LlmLogs { get; set; }
}

public class LlmLogEntry
{
	public int Id { get; set; }
	public string UserId { get; set; }
	public string Prompt { get; set; }
	public string Response { get; set; }
	public DateTime Timestamp { get; set; }
}
