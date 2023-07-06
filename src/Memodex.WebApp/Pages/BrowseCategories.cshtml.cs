using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Common;
using Memodex.WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class BrowseCategories : PageModel
{
    private readonly IMediator _mediator;

    public BrowseCategories(IMediator mediator)
    {
        _mediator = mediator;
    }

    public PagedData<CategoryItem> Data { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int p = 1, int n = 25, string? s = null)
    {
        Data = await _mediator.Send(new GetCategoriesRequest
        {
            Count = n,
            Page = p,
            SearchTerm = s
        });
        return Page();
    }

    public record CategoryItem(int Id, string Name, int ItemCount);

    public class GetCategoriesRequest : IRequest<PagedData<CategoryItem>>
    {
        public string? SearchTerm { get; init; }
        public int Page { get; init; }
        public int Count { get; init; }
    }
    
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesRequest, PagedData<CategoryItem>>
    {
        private readonly MemodexContext _memodexContext;

        public GetCategoriesHandler(MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<PagedData<CategoryItem>> Handle(GetCategoriesRequest request, CancellationToken cancellationToken)
        {
            IQueryable<Category> query = _memodexContext.Categories;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(item => item.Name.Contains(request.SearchTerm));
            }

            int page = request.Page >= 1
                ? request.Page
                : 1;

            int count = request.Count is > 5 and < 100
                ? request.Count
                : 10;

            query = query.OrderBy(item => item.Name)
                .Skip((page - 1) * count)
                .Take(count);

            int totalItems = await _memodexContext.Categories.CountAsync(cancellationToken);

            return new PagedData<CategoryItem>
            {
                Items = await query.Select(item => new CategoryItem(item.Id, item.Name, item.ItemCount))
                    .ToListAsync(cancellationToken),
                ItemCount = totalItems,
                Page = page,
                TotalPages = totalItems / count + 1
            };
        }
    }
}
