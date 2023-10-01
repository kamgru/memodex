using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class MyProfile : PageModel
{
    public record UpdateTheme(
        string Theme);

    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public MyProfile(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    public async Task<IActionResult> OnPostUpdateThemeAsync(
        [FromBody]
        UpdateTheme updateTheme)
    {
        await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(User);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            UPDATE preferences
            SET value= @theme
            WHERE key = 'preferredTheme';
            """);
        command.Parameters.AddWithValue("@theme", updateTheme.Theme);
        await command.ExecuteNonQueryAsync();

        HttpContext.Session.SetString("preferredTheme", updateTheme.Theme);

        return new OkResult();
    }
}
