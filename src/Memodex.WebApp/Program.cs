using System.Reflection;
using Memodex.DataAccess;
using Memodex.WebApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

string connectionString = builder.Configuration.GetConnectionString("MemodexDb")
                           ?? throw new InvalidOperationException("Missing connection string for MemodexDb.");

builder.Services.AddDbContext<MemodexContext>(
    opt => opt.UseSqlServer(connectionString));
builder.Services.AddMediatR(opt => opt.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<IProfileProvider, ProfileProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<MediaPathProvider>();
builder.Services.AddSingleton<StaticFilesPathProvider>();
builder.Services.AddSingleton<Thumbnailer>();

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

string mediaRootPath = app.Configuration.GetSection("Media")
                           .GetValue<string>("Path")
                       ?? throw new InvalidOperationException("Missing configuration for Media:Path.");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRootPath),
    RequestPath = "/media"
});
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

//app.UseMiddleware<ProfileSessionMiddleware>();
app.Run();