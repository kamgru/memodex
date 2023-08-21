using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class EditFlashcards : PageModel
{
    private readonly IMediator _mediator;

    public EditFlashcards(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(
        int deckId,
        int itemsPerPage = 25)
    {
        PagedData<FlashcardItem> data = await _mediator.Send(new GetFlashcardsRequest(
            deckId,
            1,
            itemsPerPage));

        CurrentPageInfo = new PageInfo(
            deckId,
            itemsPerPage,
            data);

        return Page();
    }

    public async Task<IActionResult> OnGetFlashcardsAsync(
        int deckId,
        int pageNumber,
        int itemsPerPage)
    {
        PagedData<FlashcardItem> data = await _mediator.Send(new GetFlashcardsRequest(
            deckId,
            pageNumber,
            itemsPerPage));

        return data.Items.Any()
            ? new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new PageInfo(
                        deckId,
                        itemsPerPage,
                        data)
                },
                ViewName = "_FlashcardsListPartial"
            }
            : new EmptyResult();
    }

    public async Task<IActionResult> OnGetSingleFlashcardAsync(
        int flashcardId)
    {
        FlashcardItem flashcard = await _mediator.Send(new GetSingleFlashcardRequest(flashcardId));

        return new PartialViewResult
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = flashcard
            },
            ViewName = "_FlashcardItemPartial"
        };
    }

    public async Task<IActionResult> OnGetEditAsync(
        int flashcardId)
    {
        EditFlashcardItem flashcard = await _mediator.Send(new GetEditFlashcardRequest(
            flashcardId));

        return new PartialViewResult
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = flashcard
            },
            ViewName = "_EditFlashcardPartial"
        };
    }

    public async Task<IActionResult> OnPostUpdateFlashcardAsync(
        [FromForm]
        UpdateFlashcardRequest request)
    {
        FlashcardItem flashcard = await _mediator.Send(request);

        return new PartialViewResult
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = flashcard
            },
            ViewName = "_FlashcardItemPartial"
        };
    }

    public async Task<IActionResult> OnPostDeleteFlashcardAsync(
        [FromQuery] DeleteFlashcardRequest request)
    {
        await _mediator.Send(request);
        return new EmptyResult();
    }

    public record PageInfo(
        int DeckId,
        int ItemsPerPage,
        PagedData<FlashcardItem> Data);

    public PageInfo? CurrentPageInfo { get; set; }

    public record FlashcardItem(
        int Id,
        string Question,
        string Answer);

    public record EditFlashcardItem(
        int Id,
        int DeckId,
        string Question,
        string Answer);

    public record GetFlashcardsRequest(
        int DeckId,
        int Page,
        int Count) : IRequest<PagedData<FlashcardItem>>;

    public record GetSingleFlashcardRequest(
        int FlashcardId) : IRequest<FlashcardItem>;

    public record GetEditFlashcardRequest(
        int FlashcardId) : IRequest<EditFlashcardItem>;

    public record UpdateFlashcardRequest(
        int FlashcardId,
        string Question,
        string Answer) : IRequest<FlashcardItem>;

    public record DeleteFlashcardRequest(
        int FlashcardId) : IRequest;

    public class GetFlashcardsHandler : IRequestHandler<GetFlashcardsRequest, PagedData<FlashcardItem>>
    {
        private readonly MemodexContext _memodexContext;

        public GetFlashcardsHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<PagedData<FlashcardItem>> Handle(
            GetFlashcardsRequest request,
            CancellationToken cancellationToken)
        {
            IQueryable<Flashcard> query = _memodexContext.Flashcards
                .Where(flashcard => flashcard.DeckId == request.DeckId);

            int totalCount = await query.CountAsync(cancellationToken);

            List<FlashcardItem> items = await query
                .OrderBy(flashcard => flashcard.Id)
                .Skip((request.Page - 1) * request.Count)
                .Take(request.Count)
                .Select(flashcard => new FlashcardItem(
                    flashcard.Id,
                    flashcard.Question,
                    flashcard.Answer))
                .ToListAsync(cancellationToken);

            int totalPages = (int)Math.Ceiling((double)totalCount / request.Count);

            return new PagedData<FlashcardItem>
            {
                ItemCount = totalCount,
                Page = request.Page,
                TotalPages = totalPages,
                Items = items
            };
        }

        public class GetEditFlashcardHandler : IRequestHandler<GetEditFlashcardRequest, EditFlashcardItem>
        {
            private readonly MemodexContext _memodexContext;

            public GetEditFlashcardHandler(
                MemodexContext memodexContext)
            {
                _memodexContext = memodexContext;
            }

            public async Task<EditFlashcardItem> Handle(
                GetEditFlashcardRequest request,
                CancellationToken cancellationToken)
            {
                EditFlashcardItem flashcard = await _memodexContext.Flashcards
                                                  .Where(flashcard => flashcard.Id == request.FlashcardId)
                                                  .Select(flashcard => new EditFlashcardItem(
                                                      flashcard.Id,
                                                      flashcard.DeckId,
                                                      flashcard.Question,
                                                      flashcard.Answer))
                                                  .FirstOrDefaultAsync(cancellationToken)
                                              ?? throw new InvalidOperationException("Flashcard not found.");

                return flashcard;
            }
        }

        public class GetSingleFlashcardHandler : IRequestHandler<GetSingleFlashcardRequest, FlashcardItem>
        {
            private readonly MemodexContext _memodexContext;

            public GetSingleFlashcardHandler(
                MemodexContext memodexContext)
            {
                _memodexContext = memodexContext;
            }

            public async Task<FlashcardItem> Handle(
                GetSingleFlashcardRequest request,
                CancellationToken cancellationToken)
            {
                FlashcardItem flashcard = await _memodexContext.Flashcards
                                              .Where(flashcard => flashcard.Id == request.FlashcardId)
                                              .Select(flashcard => new FlashcardItem(
                                                  flashcard.Id,
                                                  flashcard.Question,
                                                  flashcard.Answer))
                                              .FirstOrDefaultAsync(cancellationToken)
                                          ?? throw new InvalidOperationException("Flashcard not found.");

                return flashcard;
            }
        }

        public class UpdateFlashcardHandler : IRequestHandler<UpdateFlashcardRequest, FlashcardItem>
        {
            private readonly MemodexContext _memodexContext;

            public UpdateFlashcardHandler(
                MemodexContext memodexContext)
            {
                _memodexContext = memodexContext;
            }

            public async Task<FlashcardItem> Handle(
                UpdateFlashcardRequest request,
                CancellationToken cancellationToken)
            {
                Flashcard flashcard = await _memodexContext.Flashcards
                                          .Where(flashcard => flashcard.Id == request.FlashcardId)
                                          .FirstOrDefaultAsync(cancellationToken)
                                      ?? throw new InvalidOperationException("Flashcard not found.");

                flashcard.Question = request.Question;
                flashcard.Answer = request.Answer;

                await _memodexContext.SaveChangesAsync(cancellationToken);

                return new FlashcardItem(
                    flashcard.Id,
                    flashcard.Question,
                    flashcard.Answer);
            }
        }

        public class DeleteFlashcardHandler : IRequestHandler<DeleteFlashcardRequest>
        {
            private readonly MemodexContext _memodexContext;

            public DeleteFlashcardHandler(
                MemodexContext memodexContext)
            {
                _memodexContext = memodexContext;
            }

            public async Task Handle(
                DeleteFlashcardRequest request,
                CancellationToken cancellationToken)
            {
                Flashcard flashcard = await _memodexContext.Flashcards
                                          .Where(flashcard => flashcard.Id == request.FlashcardId)
                                          .FirstOrDefaultAsync(cancellationToken)
                                      ?? throw new InvalidOperationException("Flashcard not found.");

                List<ChallengeStep> challengeSteps = await _memodexContext.ChallengeSteps
                    .Where(challengeStep => challengeStep.FlashcardId == flashcard.Id)
                    .ToListAsync(cancellationToken);

                _memodexContext.Flashcards.Remove(flashcard);
                _memodexContext.ChallengeSteps.RemoveRange(challengeSteps);

                await _memodexContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}