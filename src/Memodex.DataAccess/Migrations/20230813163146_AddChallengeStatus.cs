using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memodex.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinished",
                schema: "mdx",
                table: "Challenges");

            migrationBuilder.AddColumn<int>(
                name: "State",
                schema: "mdx",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                schema: "mdx",
                table: "Challenges");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                schema: "mdx",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
