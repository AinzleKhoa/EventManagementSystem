using ChatAPI.Hubs;
using ChatAPI.Mappers;
using ChatAPI.Models;
using ChatAPI.Odata;
using ChatAPI.Repositories;
using ChatAPI.Repositories.Implements;
using ChatAPI.Services;
using ChatAPI.Services.Implements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ChatAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                })
                .AddOData(opt => opt
                    .AddRouteComponents("odata", EdmModelBuilder.GetEdmModel())
                    .Select()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Count()
                    .SetMaxTop(100)
                );

            builder.Services.AddSignalR();
            builder.Services.AddHttpClient("GatewayAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7158/");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWebClient", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();  
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat API", Version = "v1" });

                // 1. Cấu hình nút Authorize (Ổ khóa) - Kiểu HTTP Bearer (Chuẩn nhất)
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token", // Không cần hướng dẫn gõ chữ Bearer nữa
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http, // <--- Đổi từ ApiKey sang Http
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                // 2. Yêu cầu bảo mật
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

                option.CustomSchemaIds(type => type.FullName);
            });
            builder.Services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               var jwtSettings = builder.Configuration.GetSection("Jwt");
               var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = false,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(key),
                   ClockSkew = TimeSpan.FromMinutes(5)
               };

               options.Events = new JwtBearerEvents
               {
                   OnMessageReceived = context =>
                   {
                       var accessToken = context.Request.Query["access_token"];
                       var path = context.HttpContext.Request.Path;
                       if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                       {
                           context.Token = accessToken;
                       }
                       return Task.CompletedTask;
                   },
                   OnAuthenticationFailed = context =>
                   {
                       Console.WriteLine($"🔥 ChatAPI Auth Fail: {context.Exception?.Message}");
                       return Task.CompletedTask;
                   }
               };
           });
            builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<IConversationParticipantRepository, ConversationParticipantRepository>();

            builder.Services.AddScoped<IConversationService, ConversationService>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IChatAuthorizationService, ChatAuthorizationService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowWebClient");

            app.UseAuthentication();

            app.UseAuthorization();
            app.MapControllers();

            app.MapHub<ChatHub>("/hubs/chat").RequireCors("AllowWebClient");

            app.Run();
        }
    }
}

