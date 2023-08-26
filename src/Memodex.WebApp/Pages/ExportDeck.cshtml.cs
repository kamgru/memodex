using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class ExportDeck : PageModel
{
    private readonly IMediator _mediator;

    public ExportDeck(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult>  OnGetAsync(int deckId)
    {
        DeckItem deckItem = await _mediator.Send(new GetDeckRequest(deckId));

        string content = JsonSerializer.Serialize(deckItem, new JsonSerializerOptions()
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

    public record GetDeckRequest(
        int DeckId) : IRequest<DeckItem>;
    
    public class GetDeckHandler : IRequestHandler<GetDeckRequest, DeckItem>
    {
        private readonly MemodexContext _memodexContext;

        public GetDeckHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<DeckItem> Handle(
            GetDeckRequest request,
            CancellationToken cancellationToken)
        {
            Deck deck = await _memodexContext.Decks
                            .Include(item => item.Flashcards)
                            .FirstOrDefaultAsync(item => item.Id == request.DeckId, cancellationToken)
                        ?? throw new InvalidOperationException("Deck not found.");

            DeckItem deckItem = new(
                deck.Name,
                deck.Description,
                deck.Flashcards.Select(flashcard => new FlashcardItem(
                    flashcard.Question,
                    flashcard.Answer)));

            return deckItem;
        }
    }
}