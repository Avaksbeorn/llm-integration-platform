using LoggingService.Consumers;
using LoggingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<LogsDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("LogsDb")));

builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<LlmRequestEventConsumer>();

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host("rabbitmq", "/", h =>
		{
			h.Username("guest");
			h.Password("guest");
		});

		cfg.ReceiveEndpoint("llm-logs-queue", e =>
		{
			e.ConfigureConsumer<LlmRequestEventConsumer>(context);
		});
	});
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<LogsDbContext>();
	db.Database.Migrate();
}

await app.RunAsync();
