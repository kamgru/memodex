using System.Data.Common;
using System.Security.Claims;
using Memodex.WebApp.Infrastructure;

namespace Memodex.WebApp.Data;

public class UserDatabase
{
    private readonly SqliteConnectionFactory _sqliteConnectionFactory;
    private readonly ILogger<UserDatabase> _logger;

    public UserDatabase(
        SqliteConnectionFactory sqliteConnectionFactory,
        ILogger<UserDatabase> logger)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
        _logger = logger;
    }

    public async Task CreateAsync(
        ClaimsPrincipal user)
    {
        await using SqliteConnection userDbConnection = _sqliteConnectionFactory.CreateForUser(user, true, true);

        await userDbConnection.OpenAsync();
        await using DbTransaction userDbTransaction = await userDbConnection.BeginTransactionAsync();

        await using SqliteCommand createDbCmd = userDbConnection.CreateCommand(
            """
            create table if not exists main.decks
            (
                id             integer not null
                    constraint decks_pk primary key autoincrement,
                name           text    not null,
                description    text,
                flashcardCount integer not null default 0
            );

            create table if not exists main.flashcards
            (
                id          integer not null
                        constraint flashcards_pk
                            primary key autoincrement,
                ordinalNumber integer not null default 1,
                question    text    not null,
                answer      text    not null,
                deckId      integer not null
                        constraint flashcards_decks_id_fk references decks (id) on delete cascade
            );

            create table if not exists main.challenges
            (
                id integer not null
                    constraint challenge_pk
                        primary key autoincrement,
                deckId integer not null
                    constraint challenge_decks_id_fk references decks(id) on delete cascade,
                state integer not null default 0,
                currentStepIndex integer not null default 0,
                stepCount integer not null default 0,
                createdAt text not null default CURRENT_TIMESTAMP,
                updatedAt text not null default CURRENT_TIMESTAMP
            );

            create table if not exists main.steps
            (
                id integer not null
                    constraint step_pk
                        primary key autoincrement,
                stepIndex integer not null,
                needsReview integer not null default 0,
                flashcardId integer not null
                    constraint step_flashcards_id_fk references flashcards(id) on delete cascade,
                challengeId integer not null
                    constraint step_challenges_id_fk references challenges(id) on delete cascade
            );

            create table if not exists main.preferences
            (
                key text not null
                    constraint preferences_pk
                        primary key,
                value text not null
            );
            """);
        await createDbCmd.ExecuteNonQueryAsync();

        await userDbTransaction.CommitAsync();
        await userDbConnection.CloseAsync();
        _logger.LogInformation($"Created user database: {userDbConnection.DataSource}");
    }
}
