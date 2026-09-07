using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebClient.Handlers;
using WebClient.Services;
using WebClient.Services.Implements;

namespace WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddHttpContextAccessor();

            builder.Services.AddTransient<AuthTokenHandler>();

            builder.Services.AddHttpClient("ApiClient", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Api:Url"]);
                //client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler<AuthTokenHandler>()
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<ISocialService, SocialService>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IEventValidatorService, EventValidatorService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            builder.Services.AddHttpClient<IVoucherService, VoucherService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Api:Url"]);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

            builder.Services.AddHttpClient<IUserVoucherService, UserVoucherService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Api:Url"]);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

            builder.Services.AddHttpClient<IEventStaffService, EventStaffService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Api:Url"]);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();
            builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
            builder.Services.AddScoped<IVnPayService, VnPayService>();
            builder.Services.AddScoped<IMomoService, MomoService>();
            builder.Services.AddScoped<IWithdrawalService, WithdrawalService>();
            builder.Services.AddScoped<IPhotoService, CloudinaryService>();
            builder.Services.AddScoped<ITicketApiClient, TicketApiClient>();

            // Add HttpClient for ChatService with Gateway URL
            builder.Services.AddHttpClient<IChatService, ChatService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Api:Url"]);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Khi user chưa đăng nhập mà vào trang kín -> Chuyển hướng về đây
                    options.LoginPath = "/Authentication/Login";

                    options.AccessDeniedPath = "/Authentication/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Dòng lệnh "Cứu mạng": Tự động cắt đứt vòng lặp cha-con
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

            builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.RootDirectory = "/Views";
    });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var hasToken = context.User.HasClaim(c => c.Type == "AccessToken");
                    var path = context.Request.Path.Value?.ToLower() ?? "";
                    if (!hasToken && !path.StartsWith("/authentication/logout") && !path.StartsWith("/authentication/login"))
                    {
                        await context.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                        context.Response.Redirect("/Authentication/Login");
                        return;
                    }
                }
                await next();
            });

            app.MapRazorPages();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            
            app.Run();
        }
    }
}