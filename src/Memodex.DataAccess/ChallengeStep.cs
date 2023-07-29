namespace Memodex.DataAccess;

public class ChallengeStep
{
    public int Id { get; set; }
    public int Index { get; set; }
    public bool NeedsReview { get; set; }
    public int FlashcardId { get; set; }
    public required Challenge Challenge { get; set; }
}