
using Kurtis.Common.Models;
using Kurtis.DAL;
using Kurtis.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


builder.Services.AddDbContext<KurtisDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("KurtisDb"));
    //opt.LogTo(Console.WriteLine, LogLevel.Information);    
});
builder.Services.AddKurtisDal(builder.Configuration);

builder.Services.AddIdentity<User,Role>().AddEntityFrameworkStores<KurtisDbContext>().AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddOpenApi(opt =>
{
    opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
var jwtKey = builder.Configuration["Jwt:Key"] ?? "VerySecret_JWT_Key_ChangeThis";
var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// Seed roles & admin user idempotently
using (var scope = app.Services.CreateScope())
{
    string[] roles = ["Admin", "User", "ContentCreator", "customer"];
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    foreach (var r in roles)
    { 
        if (!roleManager.RoleExistsAsync(r).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new Role(r)).GetAwaiter().GetResult();
        }
    }
}

app.UseCors("AllowFrontend");

// Create default Admin user
using (var scope = app.Services.CreateScope())
{
    var adminEmail = "admin@myapp.com";
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(adminUser, "Welcome$123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();