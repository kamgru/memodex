using Microsoft.EntityFrameworkCore;

namespace Memodex.DataAccess;

public class MemodexContext : DbContext 
{ 
    public MemodexContext(
        DbContextOptions<MemodexContext> options)
        : base(options)
    {
    } 
    
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeStep> ChallengeSteps => Set<ChallengeStep>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("mdx");
        
        modelBuilder.Entity<Flashcard>()
            .HasOne(flashcard => flashcard.Deck)
            .WithMany(category => category.Flashcards);

        modelBuilder.Entity<Deck>()
            .HasOne(deck => deck.Category)
            .WithMany(category => category.Decks);

        modelBuilder.Entity<Challenge>()
            .HasMany<ChallengeStep>(challenge => challenge.ChallengeSteps)
            .WithOne(challengeStep => challengeStep.Challenge);
    }
}