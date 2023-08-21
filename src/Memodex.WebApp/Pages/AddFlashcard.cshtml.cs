using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class AddFlashcard : PageModel
{
    private readonly IMediator _mediator;

    public AddFlashcard(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public IActionResult OnGet(
        int deckId)
    {
        Input = new AddFlashcardRequest
        {
            DeckId = deckId,
            Question = string.Empty,
            Answer = string.Empty
        };

        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _mediator.Send(Input!);

        return RedirectToPage("EditFlashcards", new { deckId = Input!.DeckId });
    }
    
    [BindProperty]
    public AddFlashcardRequest? Input { get; set; }
    
    public class AddFlashcardRequest : IRequest
    {
        public int DeckId { get; init; }
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }

    public class AddFlashcardHandler : IRequestHandler<AddFlashcardRequest>
    {
        private readonly MemodexContext _memodexContext;

        public AddFlashcardHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task Handle(
            AddFlashcardRequest request,
            CancellationToken cancellationToken)
        {
            Flashcard flashcard = new()
            {
                DeckId = request.DeckId,
                Question = request.Question,
                Answer = request.Answer
            };

            _memodexContext.Flashcards.Add(flashcard);
            await _memodexContext.SaveChangesAsync(cancellationToken);
        }
    }
}