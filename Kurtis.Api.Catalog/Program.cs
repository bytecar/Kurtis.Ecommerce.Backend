using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Kurtis.DAL;
using Kurtis.DAL.Extensions;
using Kurtis.DAL.Dapper.Extensions;
using Microsoft.AspNetCore.Identity;
namespace Kurtis.Api.Catalog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kurtis Catalog API", Version = "v1" }); });

            builder.Services.AddDbContext<KurtisDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("KurtisDb")));
            builder.Services.AddKurtisDal(builder.Configuration);
            builder.Services.AddKurtisDapper(builder.Configuration);
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
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kurtis Catalog API");
            });
            app.UseAuthentication(); 
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}