using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memodex.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mdx");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "mdx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                schema: "mdx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                schema: "mdx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemCount = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Decks_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "mdx",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                schema: "mdx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    DeckId = table.Column<int>(type: "int", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    CurrentStepIndex = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Challenges_Decks_DeckId",
                        column: x => x.DeckId,
                        principalSchema: "mdx",
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Challenges_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "mdx",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Flashcards",
                schema: "mdx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeckId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flashcards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flashcards_Decks_DeckId",
                        column: x => x.DeckId,
                        principalSchema: "mdx",
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeSteps",
                schema: "mdx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Index = table.Column<int>(type: "int", nullable: false),
                    NeedsReview = table.Column<bool>(type: "bit", nullable: false),
                    FlashcardId = table.Column<int>(type: "int", nullable: false),
                    ChallengeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeSteps_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalSchema: "mdx",
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_DeckId",
                schema: "mdx",
                table: "Challenges",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_ProfileId",
                schema: "mdx",
                table: "Challenges",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeSteps_ChallengeId",
                schema: "mdx",
                table: "ChallengeSteps",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_CategoryId",
                schema: "mdx",
                table: "Decks",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_DeckId",
                schema: "mdx",
                table: "Flashcards",
                column: "DeckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengeSteps",
                schema: "mdx");

            migrationBuilder.DropTable(
                name: "Flashcards",
                schema: "mdx");

            migrationBuilder.DropTable(
                name: "Challenges",
                schema: "mdx");

            migrationBuilder.DropTable(
                name: "Decks",
                schema: "mdx");

            migrationBuilder.DropTable(
                name: "Profiles",
                schema: "mdx");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "mdx");
        }
    }
}
