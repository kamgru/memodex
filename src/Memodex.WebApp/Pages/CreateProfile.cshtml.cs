using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class CreateProfile : PageModel
{
    private readonly MediaPathProvider _mediaPathProvider;

    public CreateProfile(
        MediaPathProvider mediaPathProvider)
    {
        _mediaPathProvider = mediaPathProvider;
        FormData = new CreateProfileRequest("Memodexer", "default.png");
    }

    [BindProperty]
    public CreateProfileRequest FormData { get; set; }

    public List<AvatarImage> Avatars { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Avatars = new List<AvatarImage>();
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT `id`, `name` from avatars;
            """);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Avatars.Add(new AvatarImage(
                reader.GetInt32(0),
                _mediaPathProvider.GetAvatarThumbnailPath(reader.GetString(1)),
                reader.GetString(1)));
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO profiles (userId, name, avatar)
            VALUES (@userId, @name, @avatar);
            """);
        command.Parameters.AddWithValue("@userId", 1);
        command.Parameters.AddWithValue("@name", FormData.Name);
        command.Parameters.AddWithValue("@avatar", FormData.Image);
        await command.ExecuteNonQueryAsync();

        return RedirectToPage("SelectProfile");
    }

    public record AvatarImage(
        int Id,
        string ImageUrl,
        string ImageName);


    public record CreateProfileRequest(
        string Name,
        string Image);
}
