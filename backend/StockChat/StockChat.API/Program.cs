using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockChat.API.Hubs;
using StockChat.Application.Services;
using StockChat.Domain.Contracts.Repositories;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;
using StockChat.Infrastructure.DatabaseContext;
using StockChat.Infrastructure.Repositories;

namespace StockChat.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IChatService, ChatService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<ChatHub>("/chat");

            app.UseCors("AllowAll");

            app.Run();
        }
    }
}
