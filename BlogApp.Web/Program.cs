using BlogApp.Web.Data;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Infrastructure.Repositories;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Redis cache with fallback to in-memory cache
string redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
bool redisEnabled = !string.IsNullOrEmpty(redisConnectionString);

if (redisEnabled)
{
    try
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            //options.InstanceName = "YourAppName:";
        });
    }
    catch (Exception ex)
    {
        // Log the exception and fallback to in-memory caching
        Console.WriteLine($"Redis connection failed: {ex.Message}");
        builder.Services.AddDistributedMemoryCache(); // Fallback to in-memory cache
    }
}
else
{
    builder.Services.AddDistributedMemoryCache(); // Fallback to in-memory cache
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireUppercase = false;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(30); // Adjusted to a more reasonable timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Path to the login action
        options.AccessDeniedPath = "/Account/Login"; // Path for access denied
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Set cookie expiration to 30 days
        options.SlidingExpiration = true; // Enable sliding expiration
        options.Cookie.HttpOnly = true; // Protect the cookie from client-side scripts
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Require HTTPS for the cookie
        options.Cookie.IsEssential = true; // Mark the cookie as essential
    });

builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

// Configure Email Service
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Configure Seed
builder.Services.AddTransient<Seed>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseSession(); // Make sure this is called after UseRouting and before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<Seed>();
    await seeder.SeedRolesAsync();
    await seeder.SeedUsersAsync();
}

app.Run();
