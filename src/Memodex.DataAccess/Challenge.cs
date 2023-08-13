namespace Memodex.DataAccess;

public class Challenge
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public int DeckId { get; set; }
    public Deck Deck { get; set; } = null!;
    public bool IsFinished { get; set; }
    public ICollection<ChallengeStep> ChallengeSteps { get; set; } = new List<ChallengeStep>();
    public int? CurrentStepIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}