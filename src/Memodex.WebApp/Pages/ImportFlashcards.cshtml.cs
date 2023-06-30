using System.Text;
using MediatR;
using Memodex.WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class ImportFlashcards : PageModel
{
    private readonly IMediator _mediator;

    public ImportFlashcards(MemodexContext memodexContext, IMediator mediator)
    {
        _mediator = mediator;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IEnumerable<IFormFile> files)
    {
        await _mediator.Send(new ImportFlashcardsRequest
        {
            Files = files
        });
        
        return Page();
    }

    public class ImportFlashcardsRequest : IRequest<Unit>
    {
        public required IEnumerable<IFormFile> Files { get; set; }
    }

    public class ImportFlashcardsHandler : IRequestHandler<ImportFlashcardsRequest, Unit>
    {
        private readonly MemodexContext _memodexContext;

        public ImportFlashcardsHandler(MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<Unit> Handle(ImportFlashcardsRequest request, CancellationToken cancellationToken)
        {
            foreach (IFormFile formFile in request.Files)
            {
                using TextReader textReader = new StreamReader(formFile.OpenReadStream(), Encoding.UTF8);
                Dictionary<string, Category> categories = new();
                while (await textReader.ReadLineAsync(cancellationToken) is { } line)
                {
                    string[] parts = line.Split('|');
                    string question = parts[1];
                    string answer = parts[2];
                    string categoryName = parts[0];

                    categories.TryGetValue(categoryName, out Category? category);

                    if (category == null)
                    {
                        category = await _memodexContext.Categories.FirstOrDefaultAsync(
                                       item => item.Name == categoryName, cancellationToken)
                                   ??
                                   new Category
                                   {
                                       Name = categoryName,
                                       Flashcards = null!
                                   };
                        _memodexContext.Categories.Add(category);
                        categories.Add(categoryName, category);
                    }

                    category.ItemCount += 1;
                    
                    Flashcard flashcard = new()
                    {
                        Question = question,
                        Answer = answer,
                        Category = category
                    };
                    _memodexContext.Flashcards.Add(flashcard);
                }
            }

            await _memodexContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}