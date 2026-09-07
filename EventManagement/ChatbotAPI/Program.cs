using ChatbotAPI.Services;
using ChatbotAPI.Services.Implements;

namespace ChatbotAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendMVC", policy =>
                {
                    policy.WithOrigins("https://localhost:7104", "http://localhost:5213")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            builder.Services.AddHttpClient("GatewayAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7158/");
            });
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IGroqService, GroqService>();
 
            builder.Services.AddHttpClient("GroqClient", client =>
            {
                client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
                var apiKey = builder.Configuration["AI:ApiKey"];
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            });
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowFrontendMVC");
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
