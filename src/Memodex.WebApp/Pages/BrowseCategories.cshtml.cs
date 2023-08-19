using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class BrowseCategories : PageModel
{
    private readonly IMediator _mediator;

    public BrowseCategories(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public IEnumerable<CategoryItem> Categories { get; set; } = Enumerable.Empty<CategoryItem>();
    
    public async Task<IActionResult>  OnGetAsync()
    {
        Categories = await _mediator.Send(new GetCategoriesRequest());

        return Page();    
    }
}

public record CategoryItem(
    int Id,
    string Name,
    string Description,
    string ImagePath,
    int DeckCount);

public record GetCategoriesRequest : IRequest<IEnumerable<CategoryItem>>;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesRequest, IEnumerable<CategoryItem>>
{
    private readonly MemodexContext _memodexContext;
    private readonly MediaPathProvider _mediaPathProvider;

    public GetCategoriesHandler(
        MemodexContext memodexContext,
        MediaPathProvider mediaPathProvider)
    {
        _memodexContext = memodexContext;
        _mediaPathProvider = mediaPathProvider;
    }

    public async Task<IEnumerable<CategoryItem>> Handle(
        GetCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        IEnumerable<CategoryItem> categoryItems = await _memodexContext
            .Categories
            .Include(category => category.Decks)
            .Select(category => new CategoryItem(
                category.Id,
                category.Name,
                category.Description,
                _mediaPathProvider.GetCategoryThumbnailPath(category.ImageFilename),
                category.Decks.Count))
            .ToListAsync(cancellationToken);

        return categoryItems;
    }
}