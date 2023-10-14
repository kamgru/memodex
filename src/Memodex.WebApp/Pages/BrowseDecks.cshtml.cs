using System.Security.Claims;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class BrowseDecks : PageModel
{
    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public BrowseDecks(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    public record DeckItem(
        int Id,
        string Name,
        string Description,
        int ItemCount);

    public IReadOnlyList<DeckItem> Decks { get; set; } = new List<DeckItem>();

    public async Task<IActionResult> OnGetAsync()
    {
        BrowseDecksReader browseDecksReader = new(_sqliteConnectionFactory, User);
        Decks = await browseDecksReader.GetDecksAsync();
        return Page();
    }

    public class BrowseDecksReader
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public BrowseDecksReader(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task<IReadOnlyList<DeckItem>> GetDecksAsync()
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal);
            await connection.OpenAsync();

            await using SqliteCommand command = connection.CreateCommand(
                """
                SELECT id, name, description, flashcardCount
                FROM decks;
                """);

            List<DeckItem> decks = new();
            SqliteDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                decks.Add(new DeckItem(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetValue(2) as string ?? string.Empty,
                    reader.GetInt32(3)));
            }

            return decks;
        }
    }
}
