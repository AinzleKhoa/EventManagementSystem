using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace GatewayAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendMVC", policy =>
                {
                    policy.WithOrigins("https://localhost:7104", "http://localhost:5213")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Add services to the container.
            builder.Configuration
            .AddJsonFile("ocelot.Development.json", optional: false, reloadOnChange: true);
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Đăng ký Ocelot (Hỗ trợ cả WebSockets cho SignalR)
            builder.Services.AddOcelot(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // CORS phải đứng trước Ocelot
            app.UseCors("AllowFrontendMVC");


            app.Use(async (context, next) =>
            {
                var auth = context.Request.Headers["Authorization"].ToString();
                Console.WriteLine("GATEWAY RECEIVED Authorization:");
                Console.WriteLine(auth == "" ? "(EMPTY)" : auth);

                await next();
            });
            app.UseWebSockets();
            await app.UseOcelot();


            app.MapControllers();

            app.Run();
        }
    }
}