using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Common;

public static class PageModelExtensions
{
    public static void AddNotification(
        this PageModel pageModel,
        NotificationType type,
        string message)
    {
        pageModel.TempData["Notification"] = new Notification
            {
                Type = type,
                Message = message
            }
            .Serialize();
    }
}
