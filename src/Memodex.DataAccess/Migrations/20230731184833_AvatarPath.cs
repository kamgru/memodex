using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memodex.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AvatarPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                schema: "mdx",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPath",
                schema: "mdx",
                table: "Profiles");
        }
    }
}
