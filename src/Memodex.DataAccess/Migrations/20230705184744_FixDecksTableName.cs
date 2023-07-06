using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memodex.WebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDecksTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deck_Categories_CategoryId",
                table: "Deck");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Deck_DeckId",
                table: "Flashcards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deck",
                table: "Deck");

            migrationBuilder.RenameTable(
                name: "Deck",
                newName: "Decks");

            migrationBuilder.RenameIndex(
                name: "IX_Deck_CategoryId",
                table: "Decks",
                newName: "IX_Decks_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Decks",
                table: "Decks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Categories_CategoryId",
                table: "Decks",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Decks_DeckId",
                table: "Flashcards",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Categories_CategoryId",
                table: "Decks");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Decks_DeckId",
                table: "Flashcards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Decks",
                table: "Decks");

            migrationBuilder.RenameTable(
                name: "Decks",
                newName: "Deck");

            migrationBuilder.RenameIndex(
                name: "IX_Decks_CategoryId",
                table: "Deck",
                newName: "IX_Deck_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deck",
                table: "Deck",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deck_Categories_CategoryId",
                table: "Deck",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Deck_DeckId",
                table: "Flashcards",
                column: "DeckId",
                principalTable: "Deck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
