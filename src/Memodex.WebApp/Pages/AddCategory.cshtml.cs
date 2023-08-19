using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class AddCategory : PageModel
{
    public IActionResult OnGet()
    {
        Category = new CategoryItem();
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(
        [FromServices] IMediator mediator)
    {
        int categoryId = await mediator.Send(new AddCategoryRequest(
            Category!.Name));
        TempData["Notification"] = $"Category {Category.Name} added.";
        return RedirectToPage("EditCategory", new { categoryId });
    }
    
    [BindProperty]
    public CategoryItem? Category { get; set; }
    
    public class CategoryItem
    {
        public string Name { get; set; } = string.Empty;
    }
    
    public record AddCategoryRequest(
        string Name) : IRequest<int>;
    
    public class AddCategoryHandler : IRequestHandler<AddCategoryRequest, int>
    {
        private readonly MemodexContext _memodexContext;

        public AddCategoryHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<int> Handle(
            AddCategoryRequest request,
            CancellationToken cancellationToken)
        {
            Category category = new()
            {
                Name = request.Name,
                Description = string.Empty,
                ImageFilename = "default.png",
                Decks = new List<Deck>()
            };
            
            _memodexContext.Categories.Add(category);
            await _memodexContext.SaveChangesAsync(cancellationToken);
            return category.Id;
        }
    }
}