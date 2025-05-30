using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// 3. JWT-аутентификация
var jwtKey = builder.Configuration["JWT:Secret"];
var key = new SymmetricSecurityKey(Convert.FromBase64String(jwtKey));

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidIssuer = builder.Configuration["JWT:Issuer"],
			ValidAudience = builder.Configuration["JWT:Audience"],
			IssuerSigningKey = key
		};
		options.Events = new JwtBearerEvents
		{
			OnAuthenticationFailed = context =>
			{
				Console.WriteLine($"[JWT ERROR] {context.Exception.Message}");
				return Task.CompletedTask;
			},
			OnTokenValidated = context =>
			{
				Console.WriteLine($"[JWT SUCCESS] Token validated");
				return Task.CompletedTask;
			}
		};
	});


builder.Services.AddAuthorization();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
	x.UsingRabbitMq((ctx, cfg) =>
	{
		cfg.Host("rabbitmq", "/", h =>
		{
			h.Username("guest");
			h.Password("guest");
		});
	});
});

// HttpClient для общения с LLM-контейнером
builder.Services.AddHttpClient();

builder.Services.AddControllers();
var app = builder.Build();


app.Use(async (context, next) =>
{
	var authHeader = context.Request.Headers["Authorization"].ToString();
	Console.WriteLine($"[LOG] Incoming Authorization header: {authHeader}");
	await next();
});


app.Use(async (context, next) =>
{
	var authHeader = context.Request.Headers["Authorization"].ToString();
	Console.WriteLine($"Incoming Authorization header: {authHeader}");
	Console.WriteLine($"JWT KEY length: {jwtKey.Length} | {Convert.ToBase64String(Convert.FromBase64String(jwtKey))}");

	await next();
});


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
