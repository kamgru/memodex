using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class EditDeck : PageModel
{
    private readonly IMediator _mediator;

    public EditDeck(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(
        int deckId)
    {
        Deck = await _mediator.Send(new GetDeckRequest(deckId));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _mediator.Send(new UpdateDeckRequest(
            Deck!.Id,
            Deck.Name,
            Deck.Description));
        TempData["Notification"] = $"Deck {Deck.Name} updated.";
        return RedirectToPage("EditDeck", new { deckId = Deck.Id });
    }

    public async Task<IActionResult> OnPostDeleteDeckAsync(
        [FromQuery]
        int deckId)
    {
        int categoryId = await _mediator.Send(new DeleteDeckRequest(deckId));
        TempData["Notification"] = $"Deck {Deck!.Name} deleted.";
        return RedirectToPage("BrowseDecks", new { categoryId });
    }

    [BindProperty]
    public DeckItem? Deck { get; set; }

    public record DeckItem(
        int Id,
        int CategoryId,
        string Name,
        string Description);

    public record GetDeckRequest(
        int DeckId) : IRequest<DeckItem?>;

    public record UpdateDeckRequest(
        int Id,
        string Name,
        string Description) : IRequest;

    public record DeleteDeckRequest(
        int Id) : IRequest<int>;

    public class GetDeckHandler : IRequestHandler<GetDeckRequest, DeckItem?>
    {
        private readonly MemodexContext _memodexContext;

        public GetDeckHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<DeckItem?> Handle(
            GetDeckRequest request,
            CancellationToken cancellationToken)
        {
            DeckItem? deck = await _memodexContext.Decks
                .Where(item => item.Id == request.DeckId)
                .Select(item => new DeckItem(
                    item.Id,
                    item.CategoryId,
                    item.Name,
                    item.Description))
                .FirstOrDefaultAsync(cancellationToken);

            return deck;
        }
    }

    public class UpdateDeckHandler : IRequestHandler<UpdateDeckRequest>
    {
        private readonly MemodexContext _memodexContext;

        public UpdateDeckHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task Handle(
            UpdateDeckRequest request,
            CancellationToken cancellationToken)
        {
            Deck? deck = await _memodexContext.Decks
                .Where(item => item.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (deck is null)
            {
                throw new InvalidOperationException($"Deck with id {request.Id} not found.");
            }

            deck.Name = request.Name;
            deck.Description = request.Description;

            await _memodexContext.SaveChangesAsync(cancellationToken);
        }
    }

    public class DeleteDeckHandler : IRequestHandler<DeleteDeckRequest, int>
    {
        private readonly MemodexContext _memodexContext;

        public DeleteDeckHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<int> Handle(
            DeleteDeckRequest request,
            CancellationToken cancellationToken)
        {
            Deck? deck = await _memodexContext.Decks
                .Where(item => item.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (deck is null)
            {
                throw new InvalidOperationException($"Deck with id {request.Id} not found.");
            }

            List<Challenge> challenges = await _memodexContext.Challenges
                .Where(item => item.DeckId == request.Id)
                .ToListAsync(cancellationToken);
            
            _memodexContext.Challenges.RemoveRange(challenges); 
            _memodexContext.Decks.Remove(deck);
            await _memodexContext.SaveChangesAsync(cancellationToken);
            return deck.CategoryId;
        }
    }
}