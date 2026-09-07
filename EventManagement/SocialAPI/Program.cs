
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialAPI.Models;
using Microsoft.OpenApi.Models;
using SocialAPI.Repositories.Implement;
using SocialAPI.Repositories;
using SocialAPI.Services.Implement;
using SocialAPI.Services;
using SocialAPI.Mappers;
using Microsoft.AspNetCore.OData;
using SocialAPI.Odata;
using FluentValidation.AspNetCore;
using FluentValidation;
using SocialAPI.Hubs;

namespace SocialAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            builder.Services.AddDbContext<SocialDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // --- 2. Cấu hình Authentication ---
            builder.Services.AddAuthentication(options =>
            {
                // Ép buộc sử dụng JWT Bearer làm mặc định cho mọi thứ
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    // Cho phép chênh lệch thời gian server một chút (thường là 5 phút)
                    ClockSkew = TimeSpan.Zero
                };

                // --- 3. DEBUG LOGGING (Cực mạnh) ---
                options.Events = new JwtBearerEvents
                {
                    // Sự kiện 1: Khi Server nhận được Request, chưa kịp check gì cả
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault();
                        Console.WriteLine($"\n>> [1] Đã nhận Request! Header Auth: {token}");
                        return Task.CompletedTask;
                    },
                    // Sự kiện 2: Khi Token bị lỗi (Hết hạn, sai key, sai audience...)
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($">> [2] AUTH FAILED: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    // Sự kiện 3: Khi Token hợp lệ hoàn toàn
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($">> [3] TOKEN HỢP LỆ! User: {context.Principal?.Identity?.Name}");
                        // In thử các claim để xem có gì bên trong
                        foreach (var c in context.Principal!.Claims)
                        {
                            Console.WriteLine($"   - Claim: {c.Type} = {c.Value}");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<ISocialService, SocialService>();
            builder.Services.AddSignalR();
            builder.Services.AddHttpClient("GatewayAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7158/");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();
            builder.Services.AddControllers(options =>
            {
            }).AddOData(opt => opt
                .AddRouteComponents(
                    "odata",
                    EdmModelBuilder.GetEdmModel()
                ).Select()
                .Filter()
                .OrderBy()
                .Expand()
                .Count()
                .SetMaxTop(100)
);

            builder.Services.AddEndpointsApiExplorer();
            // Cấu hình Swagger nhập Token
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SocialAPI", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Nhập token vào đây (Không cần chữ Bearer, chỉ cần token)",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http, 
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", b => b
                    .WithOrigins("https://localhost:7104", "https://localhost:7158")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication(); // Check Token
            app.UseAuthorization();  // Check Quyền (Role)

            app.MapControllers();
            app.MapHub<ReviewHub>("/reviewHub");

            app.Run();
        }
    }
}
