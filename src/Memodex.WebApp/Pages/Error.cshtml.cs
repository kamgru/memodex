using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string? ExceptionMessage { get; set; }

    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(
        ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogWarning("An unhandled exception occurred.");
        
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        IExceptionHandlerPathFeature? exceptionHandlerPathFeature =
            HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error is null)
        {
            return;
        }

        ExceptionMessage = exceptionHandlerPathFeature.Error.Message;
        _logger.LogError(exceptionHandlerPathFeature?.Error, "An unhandled exception occurred.");
    }
}
