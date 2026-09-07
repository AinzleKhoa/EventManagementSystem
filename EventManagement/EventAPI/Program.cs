using EventAPI.Background;
using EventAPI.Mappers;
using EventAPI.Models;
using EventAPI.Odata;
using EventAPI.Repositories;
using EventAPI.Repositories.Implement;
using EventAPI.Services;
using EventAPI.Services.Implement;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace EventAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<EventsDbContext>(options =>
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

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Event API", Version = "v1" });

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
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               var jwtSettings = builder.Configuration.GetSection("Jwt");
               var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,

                   ValidIssuer = jwtSettings["Issuer"],
                   ValidAudience = jwtSettings["Audience"],

                   IssuerSigningKey = new SymmetricSecurityKey(key),

                   ClockSkew = TimeSpan.Zero
               };
           });
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
            builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
            builder.Services.AddScoped<IEventStaffRepository, EventStaffRepository>();
            builder.Services.AddScoped<IEventStaffService, EventStaffService>();
            builder.Services.AddHttpClient<IChatApiClient, ChatApiClient>();
            builder.Services.AddHostedService<ValidatorCleanupBackgroundService>();
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
