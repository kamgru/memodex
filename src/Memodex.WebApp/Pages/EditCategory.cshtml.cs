using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class EditCategory : PageModel
{
    private readonly IMediator _mediator;

    public EditCategory(
        IMediator mediator,
        StaticFilesPathProvider staticFilesPathProvider,
        MediaPathProvider mediaPathProvider)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(
        int categoryId)
    {
        Category = await _mediator.Send(new GetCategoryRequest(categoryId));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        IFormFile? formFile)
    {
        await _mediator.Send(new UpdateCategoryRequest(
            Category!.Id,
            Category.Name,
            Category.Description,
            formFile));
        TempData["Notification"] = $"Category {Category.Name} updated.";
        return RedirectToPage("EditCategory", new { categoryId = Category.Id });
    }

    public async Task<IActionResult> OnPostDeleteCategoryAsync(
        [FromQuery]
        int categoryId)
    {
        await _mediator.Send(new DeleteCategoryRequest(categoryId));
        TempData["Notification"] = $"Category {Category!.Name} deleted.";
        return RedirectToPage("BrowseCategories");
    }

    [BindProperty]
    public CategoryItem? Category { get; set; }

    public record CategoryItem
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string ImagePath { get; init; } = string.Empty;
    }

    public record GetCategoryRequest(
        int Id) : IRequest<CategoryItem>;

    public record UpdateCategoryRequest(
        int Id,
        string Name,
        string? Description,
        IFormFile? FormFile) : IRequest;

    public record DeleteCategoryRequest(
        int CategoryId) : IRequest;

    public class GetCategoryHandler : IRequestHandler<GetCategoryRequest, CategoryItem>
    {
        private readonly MemodexContext _memodexContext;
        private readonly MediaPathProvider _mediaPathProvider;

        public GetCategoryHandler(
            MemodexContext memodexContext,
            MediaPathProvider mediaPathProvider)
        {
            _memodexContext = memodexContext;
            _mediaPathProvider = mediaPathProvider;
        }

        public async Task<CategoryItem> Handle(
            GetCategoryRequest request,
            CancellationToken cancellationToken)
        {
            CategoryItem? category = await _memodexContext.Categories
                .Where(item => item.Id == request.Id)
                .Select(item => new CategoryItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ImagePath = _mediaPathProvider.GetCategoryThumbnailPath(item.ImageFilename)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (category is null)
            {
                throw new InvalidOperationException($"Category with id {request.Id} not found.");
            }

            return category;
        }
    }

    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryRequest>
    {
        private readonly MemodexContext _memodexContext;
        private readonly Thumbnailer _thumbnailer;
        private readonly StaticFilesPathProvider _staticFilesPathProvider;

        public UpdateCategoryHandler(
            MemodexContext memodexContext,
            Thumbnailer thumbnailer,
            StaticFilesPathProvider staticFilesPathProvider)
        {
            _memodexContext = memodexContext;
            _thumbnailer = thumbnailer;
            _staticFilesPathProvider = staticFilesPathProvider;
        }

        public async Task Handle(
            UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            string? newFilename = null;
            if (request.FormFile is not null)
            {
                string filename = Path.GetFileName(request.FormFile.FileName);
                newFilename = $"c_{Guid.NewGuid():N}_{filename}";
                string physicalPath = _staticFilesPathProvider.GetCategoryPhysicalPath(newFilename);
                await using FileStream stream = new(physicalPath, FileMode.Create);
                await request.FormFile.CopyToAsync(stream, cancellationToken);
                await _thumbnailer.CreateThumbnailAsync(physicalPath);
            }

            Category? category = await _memodexContext.Categories
                .Where(item => item.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (category is null)
            {
                throw new InvalidOperationException($"Category with id {request.Id} not found.");
            }

            category.Name = request.Name;
            category.Description = request.Description ?? string.Empty;
            category.ImageFilename = newFilename ?? category.ImageFilename;

            await _memodexContext.SaveChangesAsync(cancellationToken);
        }

        public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryRequest>
        {
            private readonly MemodexContext _memodexContext;

            public DeleteCategoryHandler(
                MemodexContext memodexContext)
            {
                _memodexContext = memodexContext;
            }

            public async Task Handle(
                DeleteCategoryRequest request,
                CancellationToken cancellationToken)
            {
                Category? category = await _memodexContext.Categories
                    .Include(item => item.Decks)
                    .ThenInclude(item => item.Flashcards)
                    .Where(item => item.Id == request.CategoryId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (category is null)
                {
                    throw new InvalidOperationException($"Category with id {request.CategoryId} not found.");
                }

                List<int> deckIds = await _memodexContext.Decks
                    .Where(item => item.CategoryId == request.CategoryId)
                    .Select(item => item.Id)
                    .ToListAsync(cancellationToken);
                
                List<Challenge> challenges = await _memodexContext.Challenges
                    .Where(item => deckIds.Contains(item.DeckId))
                    .ToListAsync(cancellationToken);
                
                _memodexContext.Challenges.RemoveRange(challenges);
                _memodexContext.Categories.Remove(category);
                await _memodexContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}