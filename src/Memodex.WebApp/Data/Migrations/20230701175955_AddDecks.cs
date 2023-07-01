using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memodex.WebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Categories_CategoryId",
                table: "Flashcards");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Flashcards",
                newName: "DeckId");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcards_CategoryId",
                table: "Flashcards",
                newName: "IX_Flashcards_DeckId");

            migrationBuilder.CreateTable(
                name: "Deck",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemCount = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deck_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deck_CategoryId",
                table: "Deck",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Deck_DeckId",
                table: "Flashcards",
                column: "DeckId",
                principalTable: "Deck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Deck_DeckId",
                table: "Flashcards");

            migrationBuilder.DropTable(
                name: "Deck");

            migrationBuilder.RenameColumn(
                name: "DeckId",
                table: "Flashcards",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcards_DeckId",
                table: "Flashcards",
                newName: "IX_Flashcards_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Categories_CategoryId",
                table: "Flashcards",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
