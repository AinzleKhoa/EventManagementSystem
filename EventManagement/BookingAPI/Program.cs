using BookingAPI.Background;
using BookingAPI.Helpers;
using BookingAPI.Mappers;
using BookingAPI.Models;
using BookingAPI.Odata;
using BookingAPI.Repositories;
using BookingAPI.Repositories.Implements;
using BookingAPI.Services;
using BookingAPI.Services.Implements;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace BookingAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Background Service
            builder.Services.AddHangfire(config =>
            config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHangfireServer();

            // Add services to the container.
            builder.Services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.Configure<VnPayConfig>(
            builder.Configuration.GetSection("VnPay"));

            builder.Services.Configure<MomoConfig>(
                builder.Configuration.GetSection("MomoAPI")
            );

            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderDetailsRepository, OrderDetailsRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();

            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IOrderDetailsService, OrderDetailsService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IVnPayService, VnPayService>();
            builder.Services.AddScoped<IMomoService, MomoService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddScoped<OrderBackgroundJob>();

            builder.Services.AddHttpClient<IChatApiClient, ChatApiClient>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthHeaderHandler>();

            builder.Services.AddHttpClient("GatewayAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["GatewayAPI:Url"]);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();
            builder.Services.AddControllers(options =>
            { }).AddOData(opt => opt
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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Booking API", Version = "v1" });

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
            builder.Services.AddAutoMapper(typeof(MappingProfile));
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
            builder.Services.AddAuthorization();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();
            
            app.UseHangfireDashboard();

            RecurringJob.AddOrUpdate<OrderBackgroundJob>(
            "cancel-expired-orders",
            x => x.CancelExpiredOrders(),
            Cron.Minutely);

            app.MapControllers();

            app.Run();
        }
    }
}
