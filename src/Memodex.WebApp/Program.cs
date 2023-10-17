using Memodex.WebApp.Data;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using NLog.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddScoped<ProfileProvider>();
builder.Services.AddSingleton<MediaPathProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MediaPhysicalPath>();
builder.Services.AddScoped<SqliteConnectionFactory>();
builder.Services.AddScoped<UserDatabase>();
builder.Services.AddScoped<MemodexDatabase>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.AccessDeniedPath = "/Forbidden";
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews()
        .AddRazorRuntimeCompilation();
}

WebApplication app = builder.Build();

using IServiceScope serviceScope = app.Services.CreateScope();
IServiceProvider serviceProvider = serviceScope.ServiceProvider;
MemodexDatabase memodexDatabase = serviceProvider.GetRequiredService<MemodexDatabase>();
await memodexDatabase.EnsureExistsAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

MediaPhysicalPath mediaPhysicalPath = new(builder.Configuration);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaPhysicalPath.ToString()),
    RequestPath = "/media"
});
app.UseSession();
app.UseRouting();

CookiePolicyOptions cookiePolicyOptions = new()
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
};
app.UseCookiePolicy(cookiePolicyOptions);
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages()
    .RequireAuthorization(options => { options.RequireAuthenticatedUser(); });

app.Run();
