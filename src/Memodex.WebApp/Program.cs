using System.Reflection;
using Memodex.DataAccess;
using Memodex.WebApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDbContext<MemodexContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("MemodexDb")));
builder.Services.AddMediatR(opt => opt.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        app.Configuration.GetSection("Media").GetValue<string>("Path")),
    RequestPath = "/media"
});
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.UseMiddleware<ProfileSessionMiddleware>();
app.Run();