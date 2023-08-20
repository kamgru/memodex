using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class AddDeck : PageModel
{
    private readonly IMediator _mediator;

    public AddDeck(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public DeckItem Deck { get; set; } = new();

    
    public IActionResult OnGetAsync(
        int categoryId)
    {
        Deck.CategoryId = categoryId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        int deckId = await _mediator.Send(new AddDeckRequest(
            Deck.CategoryId,
            Deck.Name));
        return RedirectToPage("EditDeck", new { deckId });
    }
    
    public class DeckItem
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public record AddDeckRequest(
        int CategoryId,
        string Name) : IRequest<int>;
    
    public class AddDeckHandler : IRequestHandler<AddDeckRequest, int>
    {
        private readonly MemodexContext _memodexContext;

        public AddDeckHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<int> Handle(
            AddDeckRequest request,
            CancellationToken cancellationToken)
        {
            Deck deck = new()
            {
                CategoryId = request.CategoryId,
                Name = request.Name,
                Description = string.Empty,
                Flashcards = new List<Flashcard>()
            };
            
            _memodexContext.Decks.Add(deck);
            await _memodexContext.SaveChangesAsync(cancellationToken);

            return deck.Id;
        }
    }
}