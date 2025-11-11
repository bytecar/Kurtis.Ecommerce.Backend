
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Kurtis.DAL;
using Kurtis.DAL.Extensions;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kurtis Users API", Version = "v1" }); });

builder.Services.AddDbContext<KurtisDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("KurtisDb")));
builder.Services.AddKurtisDal(builder.Configuration);

builder.Services.AddIdentity<IdentityUser<int>, IdentityRole<int>>().AddEntityFrameworkStores<KurtisDbContext>().AddDefaultTokenProviders();

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
    var roles = new[] { "admin", "user" };
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    foreach (var r in roles) if (!roleManager.RoleExistsAsync(r).GetAwaiter().GetResult()) roleManager.CreateAsync(new IdentityRole<int>(r)).GetAwaiter().GetResult();
}

app.UseSwagger(); 
app.UseSwaggerUI();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.Run();
