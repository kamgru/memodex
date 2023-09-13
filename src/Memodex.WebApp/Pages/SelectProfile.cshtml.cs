using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class SelectProfile : PageModel
{
    public class ProfileItem
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string AvatarPath { get; set; }
    }

    private readonly IConfiguration _configuration;

    public SelectProfile(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<ProfileItem> Profiles { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Profiles = new List<ProfileItem>();

        string? rootPath = _configuration.GetSection("Media")
            .GetSection("Avatars")
            .GetValue<string>("Path");

        if (string.IsNullOrEmpty(rootPath))
        {
            throw new InvalidOperationException("Missing configuration for Media:Avatars:Path.");
        }

        await using SqliteConnection connection = SqliteConnectionFactory.Create("memodex_test.sqlite");
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT `id`, `name`, `avatar` FROM profiles;
            """);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            string avatar = reader.GetString(2);

            Profiles.Add(new ProfileItem
            {
                Id = id,
                Name = name,
                AvatarPath = Path.Combine("media", rootPath, $"t_{avatar}")
            });
        }

        switch (Profiles.Count)
        {
            case 0:
                return RedirectToPage("CreateProfile");
            case 1:
                HttpContext.Session.SetInt32(Common.Constants.SelectedProfileSessionKey, Profiles.First()
                    .Id);
                return RedirectToPage("Index");
        }

        return Page();
    }

    public IActionResult OnPostAsync(
        ProfileItem profileItem)
    {
        HttpContext.Session.SetInt32(Common.Constants.SelectedProfileSessionKey, profileItem.Id);
        return RedirectToPage("Index");
    }
}
