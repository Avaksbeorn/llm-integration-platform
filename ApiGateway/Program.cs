using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowReactDev", policy =>
	{
		policy
			.WithOrigins("http://localhost:3000")  // URL вашего React dev-сервера
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();                   // если вы используете куки/credentials
	});
});

var jwtKey = builder.Configuration["JWT:Secret"];
var key = new SymmetricSecurityKey(Convert.FromBase64String(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
	});

builder.Services.AddAuthorization();

builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("AllowReactDev");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();
