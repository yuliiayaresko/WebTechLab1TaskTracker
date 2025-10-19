using Microsoft.EntityFrameworkCore;
using WebTechLab1TaskTracker;
using Microsoft.AspNetCore.Identity;
using WebTechLab1TaskTracker.Models;
using WebTechLab1TaskTracker.Data;
using WebTechLab1TaskTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<TaskTrackerDbContext>();

builder.Services.AddScoped< INotificationService , TelegramNotificationService >();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Додаємо підтримку Razor Pages для Identity
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // Отримуємо конфігурацію з appsettings.json
        IConfigurationSection googleAuthNSection =
            builder.Configuration.GetSection("Authentication:Google");

        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// =======================================================
//      ОСЬ КЛЮЧОВЕ ВИПРАВЛЕННЯ! ПОРЯДОК ВАЖЛИВИЙ!
// =======================================================
app.UseAuthentication(); // Спочатку дізнаємося, ХТО користувач
app.UseAuthorization();  // Потім перевіряємо, що йому МОЖНА

// Правильне налаштування маршрутів
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Для сторінок Identity (Login, Register)

app.Run();