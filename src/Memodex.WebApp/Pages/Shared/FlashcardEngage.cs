namespace Memodex.WebApp.Pages.Shared;

public record FlashcardEngage(
    int Id,
    string Question,
    string Answer,
    string DeckTitle,
    int DeckItemCount,
    int CurrentStep);