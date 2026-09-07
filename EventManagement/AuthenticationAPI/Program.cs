using AuthenticationAPI.DTOs;
using AuthenticationAPI.Helpers;
using AuthenticationAPI.Helpers.Implements;
using AuthenticationAPI.Mappers;
using AuthenticationAPI.Models;
using AuthenticationAPI.Repositories;
using AuthenticationAPI.Repositories.Implements;
using AuthenticationAPI.Services;
using AuthenticationAPI.Services.Implements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace AuthenticationAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. FIX LỖI CLAIM MAPPING: Tắt tính năng tự động đổi tên Claim của .NET
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Cấu hình Database
            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // 2. CẤU HÌNH SWAGGER CHUẨN (Tự động thêm Bearer)
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });

                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Dán Token vào đây (Không cần gõ chữ Bearer)",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[]{}
                    }
                });
            });

            // Dependency Injection
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddScoped<IPasswordHelper, PasswordHelper>();
            builder.Services.AddScoped<IJwtHelper, JwtHelper>();
            builder.Services.AddScoped<IOtpHelper, OtpHelper>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IOtpStorageService, OtpStorageService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            // 3. CẤU HÌNH AUTHENTICATION TRIỆT ĐỂ
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // FIX LỖI .NET 8: Bắt buộc dùng TokenValidator truyền thống để khớp với JwtHelper
                options.UseSecurityTokenValidators = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

                        var userIdClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub);
                        var tokenStamp = context.Principal?.FindFirst("SecurityStamp")?.Value;

                        if (userIdClaim == null || string.IsNullOrEmpty(tokenStamp))
                        {
                            context.Fail("Auth Error: Sub or SecurityStamp claim is missing.");
                            return;
                        }

                        var userId = userIdClaim.Value;
                        string cacheKey = $"security_stamp_{userId}";

                        if (!cache.TryGetValue(cacheKey, out string? dbStamp))
                        {
                            var user = await userRepository.GetById(int.Parse(userId));
                            if (user == null || user.IsActive != true)
                            {
                                context.Fail("Auth Error: User inactive or not found.");
                                return;
                            }
                            dbStamp = user.SecurityStamp;
                            cache.Set(cacheKey, dbStamp, TimeSpan.FromMinutes(5));
                        }

                        if (dbStamp != tokenStamp)
                        {
                            context.Fail($"Auth Error: Stamp mismatch. DB: {dbStamp}, Token: {tokenStamp}");
                        }
                    }
                };
            });

            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<ReadUserDTO>("UsersOData");

            builder.Services.AddControllers()
                .AddOData(options => options
                    .AddRouteComponents("api", odataBuilder.GetEdmModel()) 
                    .Select()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Count()
                    .SetMaxTop(100)
                );

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication(); // Phải đứng trước Authorization
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}