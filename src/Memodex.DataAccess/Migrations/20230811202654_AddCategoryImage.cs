using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memodex.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFilename",
                schema: "mdx",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFilename",
                schema: "mdx",
                table: "Categories");
        }
    }
}
