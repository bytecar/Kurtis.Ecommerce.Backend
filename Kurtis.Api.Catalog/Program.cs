using Kurtis.Common.Models;
using Kurtis.DAL;
using Kurtis.DAL.Dapper.Extensions;
using Kurtis.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<KurtisDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("KurtisDb")));
builder.Services.AddKurtisDal(builder.Configuration);
builder.Services.AddKurtisDapper(builder.Configuration);
builder.Services.AddIdentity<User, Role>().AddEntityFrameworkStores<KurtisDbContext>().AddDefaultTokenProviders();

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
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();    
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();