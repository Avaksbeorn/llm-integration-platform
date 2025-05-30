using System.Text;
using AuthService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение PostgreSQL
builder.Services.AddDbContext<AuthDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDbConnection")));

// 2. Настройка Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
	.AddEntityFrameworkStores<AuthDbContext>()
	.AddDefaultTokenProviders();

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
	});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
	db.Database.Migrate();
}

InitializeRolesAndAdminAsync(app.Services);

app.Run();


async Task InitializeRolesAndAdminAsync(IServiceProvider services)
{
	using var scope = services.CreateScope();

	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

	string[] roles = new[] { "Admin", "User" };

	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole(role));
		}
	}

	// Создание админа
	var adminEmail = "admin@example.com";
	var adminUser = await userManager.FindByEmailAsync(adminEmail);
	if (adminUser == null)
	{
		var admin = new IdentityUser { UserName = "admin", Email = adminEmail, EmailConfirmed = true };
		var result = await userManager.CreateAsync(admin, "AdminPassword123!");

		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(admin, "Admin");
		}
	}
}
