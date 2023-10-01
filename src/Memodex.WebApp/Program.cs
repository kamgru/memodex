using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddScoped<ProfileProvider>();
builder.Services.AddSingleton<MediaPathProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<MediaPhysicalPath>();
builder.Services.AddSingleton<SqliteConnectionFactory>();

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

MediaPhysicalPath mediaPhysicalPath = app.Services.GetRequiredService<MediaPhysicalPath>();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaPhysicalPath.ToString()),
    RequestPath = "/media"
});
app.UseSession();
app.UseRouting();

CookiePolicyOptions cookiePolicyOptions = new()
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    Secure = CookieSecurePolicy.Always
};
app.UseCookiePolicy(cookiePolicyOptions);
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages()
    .RequireAuthorization(options => { options.RequireAuthenticatedUser(); });


if (!File.Exists("memodex.db"))
{
    SqliteConnectionFactory sqliteConnectionFactory = app.Services.GetRequiredService<SqliteConnectionFactory>();
    await using SqliteConnection mdxDbConnection = sqliteConnectionFactory.CreateForApp();
    await mdxDbConnection.OpenAsync();
    await using SqliteCommand createDbCmd = mdxDbConnection.CreateCommand(
        """
        create table if not exists users
        (
            id              integer not null
                constraint users_pk
                    primary key autoincrement,
            username        varchar(255) not null,
            userId          varchar(255) not null,
            passwordHash    varchar(255) not null
        );
        """);
    await createDbCmd.ExecuteNonQueryAsync();
}


app.Run();
