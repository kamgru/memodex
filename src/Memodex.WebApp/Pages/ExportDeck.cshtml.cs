using System.Data.Common;
using System.Text;
using System.Text.Json;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class ExportDeck : PageModel
{
    public async Task<IActionResult>  OnGetAsync(int deckId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT name, description 
            FROM decks 
            WHERE id = @deckId
            LIMIT 1;
            """);

        command.Parameters.AddWithValue("@deckId", deckId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException($"Deck {deckId} not found");
        }
        string deckName = reader.GetString(0);
        string deckDescription = reader.GetString(1);

        await using SqliteCommand flashcardsCommand = connection.CreateCommand(
            """
            SELECT question, answer 
            FROM flashcards 
            WHERE deckId = @deckId;
            """);
        flashcardsCommand.Parameters.AddWithValue("@deckId", deckId);
        await using SqliteDataReader flashcardsReader = await flashcardsCommand.ExecuteReaderAsync();
        List<FlashcardItem> flashcards = new();
        while (await flashcardsReader.ReadAsync())
        {
            flashcards.Add(new FlashcardItem(
                flashcardsReader.GetString(0),
                flashcardsReader.GetString(1)));
        }
        DeckItem deckItem = new(deckName, deckDescription, flashcards);

        string content = JsonSerializer.Serialize(deckItem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        return File(Encoding.UTF8.GetBytes(content), "application/json", $"{deckItem.Name}.json");
    }

    public record FlashcardItem(
        string Question,
        string Answer);

    public record DeckItem(
        string Name,
        string Description,
        IEnumerable<FlashcardItem> Flashcards);
}
